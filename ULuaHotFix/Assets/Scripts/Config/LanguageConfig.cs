

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LanguageConfig : GameData<LanguageConfig>
{
    static public readonly string fileName = "LanguageConfig.xml";

    public string text { get; set; }

    static public string GetText(int id)
    {
        if(dataMap.ContainsKey(id))
        {
            return dataMap[id].text;
        }

        string str = string.Format("LanguageConfig.GetText:获取id{0}的文字失败", id);
        Debug.LogError(str);
        return "*";
    }

    const int confirm = 0; //确定
    const int cancel = 1; //取消
    const int update = 2; //更新

    static public string WordConfirm
    {
        get
        {
            return GetText(confirm);
        }
    }

    static public string WordCancel
    {
        get
        {
            return GetText(cancel);
        }
    }

    static public string WordUpdate
    {
        get
        {
            return GetText(update);
        }
    }
}
