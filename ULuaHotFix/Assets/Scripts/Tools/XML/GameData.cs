using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

public abstract class GameData
{
    public const string ID = "id";

    /// <summary>
    /// 配置项id
    /// </summary>
    public int id { get; protected set; }

    protected static Dictionary<int, T> GetDataMap<T>()
    {
        Dictionary<int, T> dataMap;
        var type = typeof(T);
        var fileNameField = type.GetField("fileName");
        if (fileNameField != null)
        {
            var fileName = fileNameField.GetValue(null) as String;
            dataMap = GetDataMap<T>(fileName);
        }
        else
        {
            dataMap = new Dictionary<int, T>();
        }

        return dataMap;
    }

    protected static Dictionary<int, T> GetDataMap<T>(string fileName)
    {
        Dictionary<int, T> dataMap;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var type = typeof(T);
        var result = GameDataLoader.LoadXMLDataFromFile(fileName, typeof(Dictionary<int, T>), type);
        dataMap = result as Dictionary<int, T>;

        sw.Stop();
        return dataMap;
    }

    protected static Dictionary<int, T> GetDataMapFromText<T>(string content, string fileName = "")
    {
        Dictionary<int, T> dataMap;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var type = typeof(T);
        var result = GameDataLoader.LoadXMLData(content, fileName, typeof(Dictionary<int, T>), type);
        dataMap = result as Dictionary<int, T>;

        sw.Stop();
        return dataMap;
    }
}

public abstract class GameData<T> : GameData where T : GameData<T>
{
    private static Dictionary<int, T> m_DataMap;

    public static Dictionary<int, T> dataMap
    {
        get
        {
            if (m_DataMap == null)
                m_DataMap = GetDataMap<T>();
            return m_DataMap;
        }
        set { m_DataMap = value; }
    }

    public static void Load(string fileName)
    {
        m_DataMap = GetDataMap<T>(fileName);
    }

    //fileName用来做错误提示
    public static void LoadFromText(string content, string fileName = "")
    {
        m_DataMap = GetDataMapFromText<T>(content, fileName);
    }

    public static void Reload()
    {
        m_DataMap = GetDataMap<T>();
    }

    public static Dictionary<int, T> ParseDataFromFile(string fileName = "")
    {
        return GetDataMap<T>(fileName);
    }

    public static Dictionary<int, T> ParseDataFromText(string content, string fileName = "")
    {
        return GetDataMapFromText<T>(content, fileName);
    }

    public static T GetValue(int key)
    {
        if(dataMap.ContainsKey(key))
        {
            return dataMap[key];
        }

        string str = string.Format("GameData.GetValue:{0}获取Key为{1}的值失败", typeof(T).ToString(), key);
        UnityEngine.Debug.LogError(str);
        return null;
    }
}

public class GameDataLoader
{
    static public object LoadXMLDataFromFile(string fileName, Type dicType, Type type)
    {
        Dictionary<Int32, Dictionary<String, String>> map;//int32为id, string为属性名, string为属性值;
        if (XMLParser.LoadXmlFile(fileName, out map))
        {
            return LoadXMLDataImp(ref map, dicType, type);
        }

        return null;
    }

    //fileName用来做错误提示
    static public object LoadXMLData(string xmlContent, string fileName, Type dicType, Type type)
    {
        Dictionary<Int32, Dictionary<String, String>> map;//int32为id, string为属性名, string为属性值;
        if (XMLParser.LoadText(xmlContent, fileName, out map))
        {
            return LoadXMLDataImp(ref map, dicType, type);
        }
        return null;
    }

    static private object LoadXMLDataImp(ref Dictionary<Int32, Dictionary<String, String>> map, Type dicType, Type type)
    {
        object result = null;
        try
        {
            result = dicType.GetConstructor(Type.EmptyTypes).Invoke(null);
            var props = type.GetProperties();//获取实体属性

            foreach (var item in map)
            {
                var t = type.GetConstructor(Type.EmptyTypes).Invoke(null);//构造实体实例
                foreach (var prop in props)
                {
                    if (prop.Name == GameData.ID)
                    {
                        prop.SetValue(t, item.Key, null);
                    }
                    else
                    {
                        if (item.Value.ContainsKey(prop.Name))
                        {
                            try
                            {
                                var value = XMLUtils.GetValue(item.Value[prop.Name], prop.PropertyType);
                                prop.SetValue(t, value, null);
                            }
                            catch (System.Exception ex)
                            {
                                UnityEngine.Debug.LogException(ex);
                                string str = string.Format("PropertyName:{0:s} Value:{1:s}", prop.Name, item.Value[prop.Name]);
                                UnityEngine.Debug.LogError(str);
                            }
                        }
                    }
                }
                dicType.GetMethod("Add").Invoke(result, new object[] { item.Key, t });
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }

        return result;
    }
}