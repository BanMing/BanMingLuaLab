/******************************************************************
 * 参考
 * https://msdn.microsoft.com/zh-cn/library/system.text.encoding(VS.80).aspx
 * http://www.cnblogs.com/guyun/p/4262587.html
*******************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 文本导入工具
/// </summary>
public class TextImporter : AssetPostprocessor
{
    enum TextType
    {
        Default,
        UTF8_Without_BOM,
        UTF8_BOM,
        Unicode,
        Big_Unicode,
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        for (int i = 0; i < importedAssets.Length; ++i)
        {
            string filePath = MyFileUtil.GetParentDir(Application.dataPath) + importedAssets[i];
            ConvertToUTF8(filePath);
        }
    }

    public static void ConvertToUTF8(string filePath)
    {
        filePath = filePath.ToLower();

        if (filePath.EndsWith(".meta"))
        {
            return;
        }

        if (filePath.EndsWith(".txt") || filePath.EndsWith(".csv") || filePath.EndsWith(".xml") || filePath.EndsWith(".lua"))
        {
            byte[] data = File.ReadAllBytes(filePath);
            TextType textType = GetEncodingType(data);
            switch (textType)
            {
                case TextType.Default: data = Encoding.Convert(Encoding.Default, Encoding.UTF8, data); break;
                case TextType.UTF8_Without_BOM: return;
                case TextType.UTF8_BOM:
                    {
                        BufferWriter writer = new BufferWriter(data.Length - 3);
                        writer.Write(data, 3, data.Length - 3);
                        data = writer.GetBuffer();
                    }
                    break;
                case TextType.Unicode: data = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, data); break;
                case TextType.Big_Unicode: data = Encoding.Convert(Encoding.BigEndianUnicode, Encoding.UTF8, data); break;
            }

            File.WriteAllBytes(filePath, data);
        }
    }

    private static TextType GetEncodingType(byte[] data)
    {
        Encoding encoding = Encoding.Default;

        if (IsUTF8WithBOM(data))
        {
            return TextType.UTF8_BOM;
        }

        else if (IsUTF8WithOutBOM(data))
        {
            return TextType.UTF8_Without_BOM;
        }

        else if (IsUnicode(data))
        {
            return TextType.Unicode;
        }
        else if (IsBigUnicode(data))
        {
            return TextType.Big_Unicode;
        }
      
        return TextType.Default;
    }

    private static bool IsUTF8WithBOM(byte[] data)
    {
        byte[] feature = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM 
        return FeatureCompare(data, feature);
    }

    private static bool IsUTF8WithOutBOM(byte[] data)
    {
        int charByteCounter = 1;	        //计算当前正分析的字符应还有的字节数
        byte curByte;                       //当前分析的字节.
        for (int i = 0; i < data.Length; i++)
        {
            curByte = data[i];
            if (charByteCounter == 1)
            {
                if (curByte >= 0x80)
                {
                    //判断当前
                    while (((curByte <<= 1) & 0x80) != 0)
                    {
                        charByteCounter++;
                    }
                    //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　
                    if (charByteCounter == 1 || charByteCounter > 6)
                    {
                        return false;
                    }
                }
            }
            else
            {
                //若是UTF-8 此时第一位必须为1
                if ((curByte & 0xC0) != 0x80)
                {
                    return false;
                }
                charByteCounter--;
            }
        }

        if (charByteCounter > 1)
        {
            Debug.LogError("非预期的byte格式");
        }

        return true;
    }

    private static bool IsUnicode(byte[] data)
    {
        byte[] feature = new byte[] { 0xFF, 0xFE, 0x41 }; 
        return FeatureCompare(data, feature);
    }

    private static bool IsBigUnicode(byte[] data)
    {
        byte[] feature = new byte[] { 0xFE, 0xFF, 0x00 }; 
        return FeatureCompare(data, feature);
    }

    private static bool FeatureCompare(byte[] data, byte[] feature)
    {
        if (data.Length < feature.Length)
        {
            return false;
        }

        if (feature[0] == data[0] && feature[1] == data[1] && feature[2] == data[2])
        {
            return true;
        }

        return false;
    }
}
