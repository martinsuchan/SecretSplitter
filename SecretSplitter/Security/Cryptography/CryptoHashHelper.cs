#if NETFX_CORE
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography.Core;
#else
using System.Security.Cryptography;
#endif

namespace Moserware.Security.Cryptography
{
    
    public static class CryptoHashHelper
    {
#if NETFX_CORE
        private static readonly HashAlgorithmProvider function =
            HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
#endif

        public static byte[] ComputeSHA1Hash(byte[] data)
        {
#if NETFX_CORE
            CryptographicHash hash = function.CreateHash();
            hash.Append(data.AsBuffer());
            return hash.GetValueAndReset().ToArray();
#else
            return new SHA1Managed().ComputeHash(data);
#endif
        }
    }
}