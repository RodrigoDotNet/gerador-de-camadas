using System;
using System.IO;
using System.Security.Cryptography;

namespace DataDrain.Library.Cryptography
{
    public sealed class HashSha1
    {
        public HashSha1()
        {

            Key = Convert.FromBase64String("mnTPxTzHcmf8q1wmVSM4fTTtXvhdSxVKI84GCXqRwdk=");
            IV = Convert.FromBase64String("vc/I/y1IHlhqx0SeVKjJig==");

        }

        public byte[] Key { get; private set; }

        public byte[] IV { get; private set; }

        public string Criptografa(string texto)
        {
            // Check arguments.
            if (texto == null || texto.Length <= 0)
                return "";

            // Declare the stream used to encrypt to an in memory
            // array of bytes.
            MemoryStream msEncrypt;

            // Declare the RijndaelManaged object
            // used to encrypt the data.
            RijndaelManaged aesAlg = null;

            try
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged { Key = Key, IV = IV };

                // Create an encryptor to perform the stream transform.
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                msEncrypt = new MemoryStream();
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {

                        //Write all data to the stream.
                        swEncrypt.Write(texto);
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return BitConverter.ToString(msEncrypt.ToArray()).Replace("-", "");
        }

        public string Descriptografa(string texto)
        {
            var textoCripto = HexToString(texto);
            // Check arguments.
            if (textoCripto == null || textoCripto.Length <= 0)
                return "";


            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext;

            try
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged { Key = Key, IV = IV };

                // Create a decrytor to perform the stream transform.
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(textoCripto))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Não foi possivel recuperar os dados", ex);
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        private static byte[] HexToString(string hexa)
        {
            string[] arr;
            if (!hexa.Contains("-"))
            {
                arr = new string[hexa.Length / 2];
                for (var i = 0; i < hexa.Length / 2; i++)
                {
                    arr[i] = hexa.Substring(i * 2, 2);
                }
            }
            else
            {
                arr = hexa.Split('-');
            }

            var array = new byte[arr.Length];
            for (var i = 0; i < arr.Length; i++)
            {
                array[i] = Convert.ToByte(arr[i], 16);
            }
            return array;

        }
    }
}
