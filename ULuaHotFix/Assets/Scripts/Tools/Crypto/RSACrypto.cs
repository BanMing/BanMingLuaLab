using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// RSAº”√‹¿‡
/// </summary>
public static class RSACrypto
{
    public static Byte[] Encrypt(Byte[] ToEncrypt, String Key)
    {
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        des.Key = ASCIIEncoding.ASCII.GetBytes(Key);
        des.IV = ASCIIEncoding.ASCII.GetBytes(Key);
        Byte[] encrypted;
        using (MemoryStream ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(ToEncrypt, 0, ToEncrypt.Length);
                cs.FlushFinalBlock();
                encrypted = ms.ToArray();
            }
        }
        return encrypted;
    }

    public static Byte[] Decrypt(Byte[] ToDecrypt, String Key)
    {
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        des.Key = ASCIIEncoding.ASCII.GetBytes(Key);
        des.IV = ASCIIEncoding.ASCII.GetBytes(Key);
        Byte[] decrypted;
        using (MemoryStream ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(ToDecrypt, 0, ToDecrypt.Length);
                cs.FlushFinalBlock();
                decrypted = ms.ToArray();
            }
        }
        return decrypted;
    }

    public static void GenerateKey()
    {
        RSACryptoServiceProvider rcp = new RSACryptoServiceProvider();
        string keyString = rcp.ToXmlString(true);
    }
}