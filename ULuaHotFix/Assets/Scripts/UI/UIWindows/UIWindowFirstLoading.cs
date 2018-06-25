

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIWindowFirstLoading : SingletonGameObject<UIWindowFirstLoading>
{
    public const float StartProgressValue = 0.2f;
    public const float InitResProgressValue = 0.5f;
    public const float FinishResProgressValue = 1f;
    public const float FinishLuaProgressValue = 1f;
    public const float FullProgressValue = 1;

    public float m_TargetProgress = 0;
    public float m_CurProgress = 0;

    private Text m_Text = null;
    private Slider m_Slider = null;
    // private Transform m_Dice = null;
    // private Transform m_RotateCircle = null;

    //------------------------------------------------------------------------//

    new static public string ResPath = "UI/PanelFirstLoading";

    override public void Init()
    {
        MyUnityTool.SetUIParentWithLocalInfo(transform, UIManager.Instance.UIRoot);
        transform.SetAsFirstSibling();

        //m_Slider = MyUnityTool.FindChild(transform, "Slider").GetComponent<Slider>();
        m_Text = MyUnityTool.FindChild(transform, "Tips").GetComponent<Text>();

        //m_Slider.value = 0;
        m_Text.text = LanguageConfig.GetText(9);    //正在准备资源
        // m_Text.text="正在准备资源";
        // m_Dice = MyUnityTool.FindChild(transform, "animation");
        // m_RotateCircle = MyUnityTool.FindChild(transform, "rotateRoot");
    }

    //------------------------------------------------------------------------//

    // Use this for initialization
    void Start ()
    {

    }

    void Update()
    {
        UpdateProgress();
    }

    //------------------------------------------------------------------------//

    public void ShowText(string text)
    {
        m_Text.text = text;
    }

    public void SetTargetProgress(float progress)
    {
        m_TargetProgress = progress;
    }
    public void UpdateProgress()
    {
        if(m_CurProgress < m_TargetProgress)
        {
            m_CurProgress += 0.01f;
            //m_Slider.value = m_CurProgress;
			//Debug.LogError(m_CurProgress);
            // m_Text.text=(int)(m_CurProgress*100)+"%";
			m_Text.text = LanguageConfig.GetText(9)+(int)(m_CurProgress*100)+"%";
        }
    }

    public void UpdateProgress(float progress)
    {
        m_Slider.value = progress;
    }

    public void UpdateProgress(float progress, string text)
    {
        m_Text.text = text;
        m_Slider.value = progress;
    }

    static public void Close()
    {
        if (m_Instance != null)
        {
            Destroy(Instance.gameObject);
        }
    }

    static public void Show()
    {
        if (m_Instance != null)
        {
            MyUnityTool.SetActive(Instance.transform, true);
        }else{
            
        }

    }

    static public void Hide()
    {
        if (m_Instance != null)
        {
            MyUnityTool.SetActive(Instance.transform, false);
        }
    }

    static public void HideUpdateItems()
    {
        if (m_Instance!=null)
        {
            // MyUnityTool.SetActive(Instance.m_Dice, false);
            MyUnityTool.SetActive(Instance.m_Text.transform, false);
            // MyUnityTool.SetActive(Instance.m_RotateCircle, false);
        }
    }
}
