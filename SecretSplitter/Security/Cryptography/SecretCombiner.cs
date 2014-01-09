﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using Moserware.Algebra;
using Moserware.Numerics;

namespace Moserware.Security.Cryptography {
    // REVIEW: Keep this as static for simple API usage?
    public static class SecretCombiner {
        private static readonly Diffuser DefaultDiffuser = new SsssDiffuser();
        
        public static CombinedSecret Combine(string allShares) {
            return Combine(Regex.Matches(allShares, SecretShare.RegexPattern).OfType<Match>().Select(m => SecretShare.Parse(m.Value)), DefaultDiffuser);
        }

        public static CombinedSecret Combine(IEnumerable<string> allShares) {
            return Combine(allShares, DefaultDiffuser);
        }

        public static CombinedSecret Combine(IEnumerable<string> allShares, Diffuser diffuser) {
            return Combine(allShares.Select(share => Regex.Match(share, SecretShare.RegexPattern).Value).Select(SecretShare.Parse), diffuser);
        }
        
        private static CombinedSecret Combine(IEnumerable<SecretShare> shares, Diffuser diffuser) {
            var allShares = shares.ToArray();

            if(allShares.Length == 0) {
                throw new SecretSplitterException("You must provide at least one secret share (piece).");
            }

            var expectedShareType = allShares[0].ShareType;
            if(!allShares.All(s => s.ShareType == expectedShareType)) {
                throw new SecretSplitterException("Secret shares (pieces) must be be of the same type.");
            }

            var firstInvalidShare = allShares.FirstOrDefault(s => (s.ShareType != SecretShareType.Unknown) && !s.HasValidChecksum);
            if(firstInvalidShare != null) {
                throw new InvalidChecksumShareException(firstInvalidShare.ParsedValue);
            }

            var secretCoefficient = LagrangeInterpolator.EvaluateAtZero(allShares.Select(s => s.Point));
            var scrambledValue = secretCoefficient.PolynomialValue;
            var unscrambledValue = diffuser.Unscramble(scrambledValue, scrambledValue.ToByteArray().Length);
            var recoveredSecret = unscrambledValue.ToUnsignedBigEndianBytes();
            
            return new CombinedSecret(allShares[0].ShareType, recoveredSecret);
        }
    }

    public class InvalidChecksumShareException : SecretSplitterException {
        public InvalidChecksumShareException(string invalidShare) : base("The secret share '" + invalidShare + "' has an invalid checksum.") {
            InvalidShare = invalidShare;
        }

        public string InvalidShare { get; private set; }
    }

    public class CombinedSecret {
        public CombinedSecret(SecretShareType shareType, byte[] recoveredBytes) {
            ShareType = shareType;
            // TODO: Use ProtectedData.Protect, but that'd pull in another DLL
            RecoveredBytes = recoveredBytes;
        }

        public SecretShareType ShareType { get; private set; }

        // SECREVIEW: technically should copy array to prevent callers from altering array, but I don't want lots of copies floating around
        public byte[] RecoveredBytes { get; private set; }
        public string RecoveredTextString {
            get { return SecretEncoder.DecodeString(RecoveredBytes); }
        }

        public string RecoveredHexString {
            get { return RecoveredBytes.ToHexString(); }
        }

        public Stream Decrypt(Stream inputStream, out string originalFileName, out DateTime originalDateTime)
        {
#if NETFX_CORE || SILVERLIGHT
            throw new NotImplementedException("Not yet implemented for Windows Phone and WinRT");
#else
            if (ShareType != SecretShareType.File) {
                throw new InvalidOperationException("Secret must be of the File type to decrypt file");
            }

            var passPhrase = new SecureString();

            foreach(var currentNibble in RecoveredHexString) {
                passPhrase.AppendChar(currentNibble);
            }

            return OpenPgp.DecryptSingleFile(inputStream, passPhrase, out originalFileName, out originalDateTime);
#endif
        }
    }
}