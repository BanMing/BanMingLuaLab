
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIWindowUpdate : SingletonGameObject<UIWindowUpdate>
{
    private Text m_Text = null;

    private WWW m_WWW = null;
    private int m_TotalCount = 0;
    private int m_Current = 0;

    private int m_CurSize = 0;

    //------------------------------------------------------------------------//

    new static public string ResPath = "UI/PanelUpdate";

    override public void Init()
    {
        MyUnityTool.SetUIParentWithLocalInfo(transform, UIManager.Instance.UIRoot);

        m_Text = MyUnityTool.FindChild(transform, "Tips").GetComponent<Text>();

        m_Text.text = LanguageConfig.GetText(9);    //正在检查更新
        // m_Text.text="正在检查更新";
    }

    static public void Show()
    {
        if (m_Instance != null)
        {
            MyUnityTool.SetActive(Instance.transform, true);
        }
    }
    void Update()
    {
        if (m_WWW != null && string.IsNullOrEmpty(m_WWW.error) == true)
        {
            string downloaded = (m_CurSize * m_WWW.progress * 1.0 / 1024 / 1024).ToString("N2");
            string total = (m_CurSize * 1.0 / 1024 / 1024).ToString("N2");
            string str = string.Format(LanguageConfig.GetText(7), m_TotalCount, m_Current, downloaded, total);
            m_Text.text = str;
        }
    }

    public void ShowDownloadTips(int totalCout, int current, string resName, WWW www, int resSize)
    {
        m_TotalCount = totalCout;
        m_Current = current;
        m_CurSize = resSize;
        m_WWW = www;
        string downloaded = "0";
        string total = (resSize * 1.0 / 1024 / 1024).ToString("N2");
        string str = string.Format(LanguageConfig.GetText(7), totalCout, current, downloaded, total);
        m_Text.text = str;
    }

    public void ShowVerifyTips()
    {
        m_Text.text = LanguageConfig.GetText(10); //正在校验文件
        m_WWW = null;
    }

    public void ShowUnZipTips()
    {
        m_Text.text = LanguageConfig.GetText(8);
        m_WWW = null;
    }

    public void ShowTips(string tips)
    {
        m_Text.text = tips;
    }

    static public void Close()
    {
        if (m_Instance != null)
        {
            Destroy(Instance.gameObject);
        }
    }
}
