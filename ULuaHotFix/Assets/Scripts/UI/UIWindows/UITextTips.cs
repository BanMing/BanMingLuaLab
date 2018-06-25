
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UITextTips : SingletonGameObject<UITextTips>
{
    private Queue<string> mQueueData = new Queue<string>();
    private Queue<Text> mQueueLabel = new Queue<Text>();

    //冷却时间间隔，此时间范围内只增加一个tips
    private const float mCoolTimeInterval = 1f;
    private float mLeftCoolTime = 0;

    //------------------------------------------------------------------------//

    new static public string ResPath = "UI/PanelTextTips";

    override public void Init()
    {
        MyUnityTool.SetUIParentWithLocalInfo(transform, UIManager.Instance.UIRoot);

        Text[] textList = GetComponentsInChildren<Text>();
        if (textList != null && textList.Length > 0)
        {
            for (int i = 0; i < textList.Length; ++i)
            {
                textList[i].gameObject.SetActive(false);
                mQueueLabel.Enqueue(textList[i]);
            }
        }
    }
    //------------------------------------------------------------------------//

    public void ShowText(string data)
    {
        mQueueData.Enqueue(data);
     
        ShowImp();
    }

    void ShowImp()
    {
        if (mLeftCoolTime > 0)
        {
            return;
        }

        if (mQueueData.Count == 0 || mQueueLabel.Count == 0)
        {
            return;
        }

        mLeftCoolTime = mCoolTimeInterval;
        StartCoroutine(CoolDown());

        //
        Text text = mQueueLabel.Dequeue();
        text.gameObject.SetActive(true);
        text.text = mQueueData.Dequeue();

        TweenPosition tp = text.GetComponent<TweenPosition>();
        TweenAlpha ta = text.GetComponent<TweenAlpha>();

        EventDelegate.Callback cb = delegate()
        {
            text.gameObject.SetActive(false);
            mQueueLabel.Enqueue(text);
        };

        tp.SetOnFinished(cb);
        tp.ResetToBeginning();
        tp.PlayForward();

        ta.ResetToBeginning();
        ta.PlayForward();
    }

    IEnumerator CoolDown()
    {
        while (mLeftCoolTime > 0)
        {
            yield return null;
            mLeftCoolTime -= Time.deltaTime;
        }

        ShowImp();
    }
}
