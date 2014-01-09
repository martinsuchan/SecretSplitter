#if NETFX_CORE
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using System.Runtime.InteropServices.WindowsRuntime;
#else
using System.Security.Cryptography;
#endif

namespace Moserware.Security.Cryptography
{
    public static class RandomNumberHelper
    {
        public static void GetBytes(byte[] data)
        {
#if NETFX_CORE
            IBuffer randomBuffer = CryptographicBuffer.GenerateRandom((uint)data.Length);
            randomBuffer.CopyTo(data);
#else
            RandomNumberGenerator rng = new RNGCryptoServiceProvider();
            rng.GetBytes(data);
#endif
        }
    }
}