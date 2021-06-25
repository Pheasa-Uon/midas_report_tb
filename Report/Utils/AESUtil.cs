using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LMSReport.Utils
{
    public class AESUtil
    {
        private static string KeyString = "c113bc8b3d03e1135309df7567d351dd";
        private static string IVString = "9c0534f194d6e5892d1c6768d11070eb";

        public static string EncryptString(string message)
        {
            byte[] Key = UTF8Encoding.UTF8.GetBytes(KeyString);
            byte[] IV = UTF8Encoding.UTF8.GetBytes(IVString);
            string encrypted = null;
            RijndaelManaged rj = new RijndaelManaged();
            rj.KeySize = 128;
            rj.BlockSize = 128;
            rj.Key = Key;
            rj.IV = IV;
            rj.Mode = CipherMode.CBC;
            rj.Padding = PaddingMode.PKCS7;
            try
            {
                MemoryStream ms = new MemoryStream();

                using (CryptoStream cs = new CryptoStream(ms, rj.CreateEncryptor(rj.Key, rj.IV), CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(message);
                        sw.Close();
                    }
                    cs.Close();
                }
                byte[] encoded = ms.ToArray();
                encrypted = Convert.ToBase64String(encoded);
                ms.Close();
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("A file error occurred: {0}", e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: {0}", e.Message);
            }
            finally
            {
                rj.Clear();
            }
            return encrypted;
        }
    }
}