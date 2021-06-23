using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Report.Utils
{

    public class AES
    {
   
       // private static SecretKeySpec secretKey;
        private static byte[] key;
        public static string tr( string text)
        {
            
                System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
                AesManaged tdes = new AesManaged();
                tdes.Key = UTF8.GetBytes("");
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform crypt = tdes.CreateEncryptor();
                byte[] plain = Encoding.UTF8.GetBytes(text);
                byte[] cipher = crypt.TransformFinalBlock(plain, 0, plain.Length);
                String encryptedText = Convert.ToBase64String(cipher);
            return encryptedText;
        }
    }
}