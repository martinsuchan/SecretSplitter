using System;
using System.Linq;

namespace Moserware.Security.Cryptography {
    public static class HexadecimalPasswordGenerator {
        public static string GeneratePasswordOfBitSize(int bitSize) {
            return GeneratePassword(bitSize/4);
        }

        private static string GeneratePassword(int length) {
            int byteLength = (length + 1)/2;
            var totalBytes = new byte[byteLength];
            RandomNumberHelper.GetBytes(totalBytes);
            var result = String.Join("", totalBytes.Select(b => b.ToString("x2")));
            return result.Substring(0, length);
        }
    }
}
