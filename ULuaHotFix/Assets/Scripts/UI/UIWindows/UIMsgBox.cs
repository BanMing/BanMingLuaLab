using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIMsgBox : SingletonGameObject<UIMsgBox>
{
    private Text m_TextTitle = null;
    private Text m_TextContent = null;

    private GameObject m_BtnOnlyOK = null;
    private Text m_TextBtnOnlyOK = null;

    private GameObject m_BtnOK = null;
    private Text m_TextBtnOK = null;

    private GameObject m_BtnCancel = null;
    private Text m_TextCancelOK = null;

    private Action<bool> m_ClickOKCancelAction = null;
    private Action m_ClickOnlyOKAction = null;

    private bool m_IsHideWhenClick = false;
    private bool m_IsDestroy = true; //点击按钮后 true:删除窗口 false:隐藏窗口

    //------------------------------------------------------------------------//

    new static public string ResPath = "UI/PanelMsgBox";

    // Use this for initialization
    override public void Init ()
    {
        MyUnityTool.SetUIParentWithLocalInfo(transform, UIManager.Instance.UIRoot);

        m_BtnOnlyOK = MyUnityTool.FindChild(transform, "BtnOnlyOK").gameObject;
        m_BtnOK = MyUnityTool.FindChild(transform, "BtnOK").gameObject;
        m_BtnCancel = MyUnityTool.FindChild(transform, "BtnCancel").gameObject;

        UIEventListener.Get(m_BtnOnlyOK).mOnClick = OnClickBtnOnlyOK;
        UIEventListener.Get(m_BtnOK).mOnClick = OnClickBtnOK;
        UIEventListener.Get(m_BtnCancel).mOnClick = OnClickBtnCancel;
        
        m_TextTitle = MyUnityTool.FindChild(transform, "Title").GetComponent<Text>();
        m_TextContent = MyUnityTool.FindChild(transform, "Content").GetComponent<Text>();
        m_TextBtnOnlyOK = MyUnityTool.FindChild(transform, "BtnOnlyOKText").GetComponent<Text>();
        m_TextBtnOK = MyUnityTool.FindChild(transform, "BtnOKText").GetComponent<Text>();
        m_TextCancelOK = MyUnityTool.FindChild(transform, "BtnCancelText").GetComponent<Text>();

        m_BtnOnlyOK.SetActive(false);
        m_BtnOK.SetActive(false);
        m_BtnCancel.SetActive(false);
    }


    void OnDisable()
    {
        m_ClickOKCancelAction = null;
        m_ClickOnlyOKAction = null;

        m_IsHideWhenClick = false;

        m_BtnOnlyOK.SetActive(false);
        m_BtnOK.SetActive(false);
        m_BtnCancel.SetActive(false);
    }

    //------------------------------------------------------------------------//

    void OnClickBtnOnlyOK()
    {
        if (m_ClickOnlyOKAction != null)
        {
            m_ClickOnlyOKAction();
        }

        if (m_IsHideWhenClick)
        {
            HideMsgBox();
        }
    }

    void OnClickBtnOK()
    {
        if(m_ClickOKCancelAction != null)
        {
            m_ClickOKCancelAction(true);
        }

        if (m_IsHideWhenClick)
        {
            if(m_IsDestroy)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    void OnClickBtnCancel()
    {
        if (m_ClickOKCancelAction != null)
        {
            m_ClickOKCancelAction(false);
        }

        if (m_IsHideWhenClick)
        {
            if (m_IsDestroy)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    //------------------------------------------------------------------------//

    //clickAction: true 点击确定，false 点击取消
    public void ShowMsgBoxOKCancel(string title, string text, Action<bool> clickAction, bool isAutoHideWhenClickBtn = true)
    {
        gameObject.SetActive(true);
        m_BtnOK.SetActive(true);
        m_BtnCancel.SetActive(true);

        m_TextTitle.text = title;
        m_TextContent.text = text;
        // m_TextBtnOK.text = "确定";
        m_TextBtnOK.text = LanguageConfig.WordConfirm;
        // m_TextCancelOK.text = "取消";
        m_TextCancelOK.text = LanguageConfig.WordCancel;

        m_ClickOKCancelAction = clickAction;
        m_IsHideWhenClick = isAutoHideWhenClickBtn;
    }

    //clickAction: true 点击确定，false 点击取消
    public void ShowMsgBoxOKCancel(string title, string text, string okBtn, string cancelBtn, Action<bool> clickAction, bool isAutoHideWhenClickBtn = true)
    {
        gameObject.SetActive(true);
        m_BtnOK.SetActive(true);
        m_BtnCancel.SetActive(true);

        m_TextTitle.text = title;
        m_TextContent.text = text;
        m_TextBtnOK.text = okBtn;
        m_TextCancelOK.text = cancelBtn;

        m_ClickOKCancelAction = clickAction;
        m_IsHideWhenClick = isAutoHideWhenClickBtn;
    }

    public void ShowMsgBoxOK(string title, string text, string okBtn, Action okAction, bool isAutoHideWhenClickBtn = true)
    {
        
        gameObject.SetActive(true);
        m_BtnOnlyOK.SetActive(true);
        m_TextTitle.text = title;
        m_TextContent.text = text;
        m_TextBtnOnlyOK.text = okBtn;

        m_ClickOnlyOKAction = okAction;
        m_IsHideWhenClick = isAutoHideWhenClickBtn;
    }

    public void HideMsgBox()
    {
        if (m_IsDestroy)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
