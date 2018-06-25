using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// DESº”√‹
/// </summary>
public static class DESCrypto
{
    public static void Encrypt(string filePath, byte[] Key)
    {
        byte[] data = File.ReadAllBytes(filePath);
        data = Encrypt(data, Key);
        File.WriteAllBytes(filePath, data);
    }

    public static Byte[] Encrypt(Byte[] ToEncrypt, byte[] Key)
    {
        DESCryptoServiceProvider des = new DESCryptoServiceProvider() { Key = Key, IV = Key };
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

    public static void Decrypt(string filePath, byte[] Key)
    {
        byte[] data = File.ReadAllBytes(filePath);
        data = Decrypt(data, Key);
        File.WriteAllBytes(filePath, data);
    }

    public static Byte[] Decrypt(Byte[] ToDecrypt, byte[] Key)
    {
        DESCryptoServiceProvider des = new DESCryptoServiceProvider() { Key = Key, IV = Key };
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

    public static void DecryptAsync(Byte[] ToDecrypt, byte[] Key, Action<Byte[]> callback)
    {
        Loom.RunAsync(() =>
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider() { Key = Key, IV = Key };
            Byte[] decrypted;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(ToDecrypt, 0, ToDecrypt.Length);
                    cs.FlushFinalBlock();
                    decrypted = ms.ToArray();
                    Loom.QueueOnMainThread(() =>{
                        if (callback != null)
                        {
                            callback(decrypted);
                        }
                    });
                }
            }
        });
    }

    public static String GenerateKeyString()
    {
        var desCrypto = DESCryptoServiceProvider.Create();
        return ASCIIEncoding.ASCII.GetString(desCrypto.Key);
    }

    public static byte[] GenerateKey()
    {
        var desCrypto = DESCryptoServiceProvider.Create();
        return desCrypto.Key;
    }
}