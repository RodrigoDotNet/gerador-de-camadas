using System;
using System.Runtime.InteropServices;
using System.Security;

namespace DataDrain.ORM.Interfaces
{
    public static class EmSecureString
    {
        public static string ConvertToString(this SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        /// <summary>
        /// Verifica se a SecureString é igual a SecureString alvo 
        /// </summary>
        /// <param name="ssOriginal">SecureString original</param>
        /// <param name="ssComparada">SecureString alvo</param>
        /// <returns></returns>
        public static bool SecureStringEqual(this SecureString ssOriginal, SecureString ssComparada)
        {
            if (ssOriginal == null)
            {
                throw new ArgumentNullException("ssOriginal");
            }

            if (ssComparada == null)
            {
                throw new ArgumentNullException("ssComparada");
            }

            if (ssOriginal.Length != ssComparada.Length)
            {
                return false;
            }

            var ssBstr1Ptr = IntPtr.Zero;
            var ssBstr2Ptr = IntPtr.Zero;

            try
            {
                ssBstr1Ptr = Marshal.SecureStringToBSTR(ssOriginal);
                ssBstr2Ptr = Marshal.SecureStringToBSTR(ssComparada);

                var str1 = Marshal.PtrToStringBSTR(ssBstr1Ptr);
                var str2 = Marshal.PtrToStringBSTR(ssBstr2Ptr);

                return str1.Equals(str2);
            }
            finally
            {
                if (ssBstr1Ptr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ssBstr1Ptr);
                }

                if (ssBstr2Ptr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ssBstr2Ptr);
                }
            }
        }
    }
}
