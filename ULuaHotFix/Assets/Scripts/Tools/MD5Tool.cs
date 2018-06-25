using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class MD5Tool
{
    public static string Get(string input)
    {
        return Get(Encoding.Default.GetBytes(input));
    }

    public static string Get(byte[] input)
    {
        MD5 md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(input);
        return GetImp(data);
    }

    public static string Get(Stream stream)
    {
        MD5 md5 = MD5.Create();
        byte[] data = md5.ComputeHash(stream);
        return GetImp(data);
    }

    public static string GetByFilePath(string filePath)
    {
        byte[] data = File.ReadAllBytes(filePath);
        return Get(data);
    }

    private static string GetImp(byte[] data)
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (var c in data)
            stringBuilder.Append(c.ToString("x2"));

        return stringBuilder.ToString();
    }

    public static bool Verify(string input, string hash)
    {
        string hashOfInput = Get(input);
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        return (0 == comparer.Compare(hashOfInput, hash));
    }

    public static bool Verify(byte[] input, string hash)
    {
        string hashOfInput = Get(input);
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        return (0 == comparer.Compare(hashOfInput, hash));
    }

    public static bool Verify(Stream input, string hash)
    {
        string hashOfInput = Get(input);
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        return (0 == comparer.Compare(hashOfInput, hash));
    }

    public static bool VerifyFile(string firstFile, string secondFile)
    {
        string firstMd5 = GetByFilePath(firstFile);
        string secondMd5 = GetByFilePath(secondFile);

        return (string.Compare(firstMd5, secondMd5, true) == 0);
    }

    public static string GetUpperMD5(string str)
    {
        string md5 = Get(str);
        return md5.ToUpper();
    }

    //byte转字符串时用的"x"
    public static string GetUpperMD5WithFormatOne(string strText)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(strText));
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < result.Length; i++)
        {
            stringBuilder.Append(result[i].ToString("x"));
        }

        return stringBuilder.ToString().ToUpper();
    }
}