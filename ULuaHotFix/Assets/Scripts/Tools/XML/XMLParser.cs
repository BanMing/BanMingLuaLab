using UnityEngine;
using System;
using System.IO;
using System.Security;
using System.Collections.Generic;
using Mono.Xml;

public class XMLParser
{
    public static Boolean LoadXmlFile(String fileName, out Dictionary<Int32, Dictionary<String, String>> map)
    {
        string data = MyFileUtil.ReadConfigData(fileName);
        return LoadText(data, fileName, out map);
    }

    public static Boolean LoadXmlFileFromResources(String fileName, out Dictionary<Int32, Dictionary<String, String>> map)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);
        return LoadText(textAsset.text, fileName, out map);
    }

    //fileName用来做错误提示
    public static Boolean LoadText(String xmlContent, String fileName, out Dictionary<Int32, Dictionary<String, String>> map)
    {
        try
        {
            SecurityParser securityParser = new SecurityParser();
            securityParser.LoadXml(xmlContent);
            SecurityElement xml = securityParser.ToXml();

            map = LoadXmlImp(xml, fileName);
            return true;
        }
        catch (Exception ex)
        {
            map = null;
            Debug.LogException(ex);
            return false;
        }
    }

    private static Dictionary<Int32, Dictionary<String, String>> LoadXmlImp(SecurityElement xml, String fileName)
    {
        var result = new Dictionary<Int32, Dictionary<String, String>>();

        var index = 0;
        foreach (SecurityElement subMap in xml.Children)
        {
            index++;
            if (subMap.Children == null || subMap.Children.Count == 0)
            {
                Debug.LogWarning("empty row in row NO." + index + " of " + fileName);
                continue;
            }
 
            var children = new Dictionary<String, String>();
            for (int i = 0; i < subMap.Children.Count; i++)
            {
                var node = subMap.Children[i] as SecurityElement;

                //对属性名称部分后缀进行裁剪
                string tag;
                if (node.Tag.Length < 3)
                {
                    tag = node.Tag;
                }
                else
                {
                    var tagTial = node.Tag.Substring(node.Tag.Length - 2, 2);
                    if (tagTial == "_i" || tagTial == "_s" || tagTial == "_f" || tagTial == "_l" || tagTial == "_k" || tagTial == "_m")
                        tag = node.Tag.Substring(0, node.Tag.Length - 2);
                    else
                        tag = node.Tag;
                }
                
                if(tag == GameData.ID)
                {
                    Int32 key = Int32.Parse(node.Text.Trim());
                    if (result.ContainsKey(key))
                    {
                        Debug.LogWarning(String.Format("Key {0} already exist in {1}", key, fileName));
                        continue;
                    }
                    result.Add(key, children);
                }
                else if (!children.ContainsKey(tag))
                {
                    if (String.IsNullOrEmpty(node.Text))
                        children.Add(tag, "");
                    else
                        children.Add(tag, node.Text.Trim());
                }
                else
                    Debug.LogWarning(String.Format("Key {0} already exist, index {1} of {2}.", node.Tag, i, node.ToString()));
            }
        }
        return result;
    }

    /*
    private static Dictionary<Int32, Dictionary<String, String>> OldLoadXmlImp(SecurityElement xml, String fileName)
    {
        var result = new Dictionary<Int32, Dictionary<String, String>>();

        var index = 0;
        foreach (SecurityElement subMap in xml.Children)
        {
            index++;
            if (subMap.Children == null || subMap.Children.Count == 0)
            {
                Debug.LogWarning("empty row in row NO." + index + " of " + fileName);
                continue;
            }
            Int32 key = Int32.Parse((subMap.Children[0] as SecurityElement).Text);
            if (result.ContainsKey(key))
            {
                Debug.LogWarning(String.Format("Key {0} already exist in {1}", key, fileName));
                continue;
            }

            var children = new Dictionary<String, String>();
            result.Add(key, children);
            for (int i = 1; i < subMap.Children.Count; i++)
            {
                var node = subMap.Children[i] as SecurityElement;
                //对属性名称部分后缀进行裁剪
                string tag;
                if (node.Tag.Length < 3)
                {
                    tag = node.Tag;
                }
                else
                {
                    var tagTial = node.Tag.Substring(node.Tag.Length - 2, 2);
                    if (tagTial == "_i" || tagTial == "_s" || tagTial == "_f" || tagTial == "_l" || tagTial == "_k" || tagTial == "_m")
                        tag = node.Tag.Substring(0, node.Tag.Length - 2);
                    else
                        tag = node.Tag;
                }

                if (node != null && !children.ContainsKey(tag))
                {
                    if (String.IsNullOrEmpty(node.Text))
                        children.Add(tag, "");
                    else
                        children.Add(tag, node.Text.Trim());
                }
                else
                    Debug.LogWarning(String.Format("Key {0} already exist, index {1} of {2}.", node.Tag, i, node.ToString()));
            }
        }
        return result;
    }
    */
}
