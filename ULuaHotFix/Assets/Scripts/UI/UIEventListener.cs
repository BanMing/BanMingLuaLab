/******************************************************************
** 文件名:	
** 版  权:	(C)  
** 创建人:  Liange
** 日  期:	2015.4.27
** 描  述: 	UIEventListener继承了多个接口，在滑动列表中一个列表项如果通过UIEventListener绑定了点击事件
            则会造成滑动失败，所以单独提供接口UIClickListener接口

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/
using UnityEngine;
using UnityEngine.EventSystems;

[AddComponentMenu("UIExt/Event Listener")]
public class UIEventListener : UnityEngine.EventSystems.EventTrigger
{
    public delegate void VoidEventDelegate();
    public delegate void VoidDelegate(GameObject go, PointerEventData eventData);
    public delegate void BaseEventDelegate(GameObject go, BaseEventData eventData);
    public delegate void AxisEventDelegate(GameObject go, AxisEventData eventData);
    public delegate void BoolDelegate(GameObject go, bool press);

    public VoidDelegate mOnPointerEnter = null;
    public VoidDelegate mOnPointerExit = null;
    public VoidDelegate mOnPointerDown = null;
    public VoidDelegate mOnPointerUp = null;
    public VoidDelegate mOnPointerClick = null;
    public VoidDelegate mOnDoublePointerClick = null;
    public VoidEventDelegate mOnClick = null;
    public BoolDelegate mOnPress = null;

    public VoidDelegate mOnDrag = null;
    public VoidDelegate mOnDrop = null;
    public VoidDelegate mOnBeginDrag = null;
    public VoidDelegate mOnEndDrag = null;
    public VoidDelegate mOnScroll = null;

    public BaseEventDelegate mOnSelect = null;
    public BaseEventDelegate mOnDeselect = null; 

    public AxisEventDelegate mOnMove = null;
    public BaseEventDelegate mOnUpdateSelected = null;
    public VoidDelegate mOnInitializePotentialDrag = null;

    public BaseEventDelegate mOnSubmit = null;
    public BaseEventDelegate mOnCancel = null;

    public System.Object mParam = null;

    override public void OnPointerEnter(PointerEventData eventData)
    {
        if (mOnPointerEnter != null)
        {
            mOnPointerEnter(gameObject, eventData);
        }
    }

    override public void OnPointerExit(PointerEventData eventData)
    {
        if (mOnPointerExit != null)
        {
            mOnPointerExit(gameObject, eventData);
        }
    }

    override public void OnPointerDown(PointerEventData eventData)
    {
        if (mOnPointerDown != null)
        {
            mOnPointerDown(gameObject, eventData);
        }

        if(mOnPress != null)
        {
            mOnPress(gameObject, true);
        }
    }

    override public void OnPointerUp(PointerEventData eventData)
    {
        if (mOnPointerUp != null)
        {
            mOnPointerUp(gameObject, eventData);
        }

        if (mOnPress != null)
        {
            mOnPress(gameObject, false);
        }
    }

    override public void OnPointerClick(PointerEventData eventData)
    {
        if (mOnPointerClick != null && eventData.clickCount == 1)
        {
            mOnPointerClick(gameObject, eventData);
        }

        if (mOnDoublePointerClick != null && eventData.clickCount == 2)
        {
            mOnDoublePointerClick(gameObject, eventData);
        }

        if (mOnClick != null)
        {
            mOnClick();
        }
    }

    override public void OnDrag(PointerEventData eventData)
    {
        if (mOnDrag != null)
        {
            mOnDrag(gameObject, eventData);
        }
    }

    override public void OnBeginDrag(PointerEventData eventData)
    {
        if (mOnBeginDrag != null)
        {
            mOnBeginDrag(gameObject, eventData);
        }
    }

    override public void OnEndDrag(PointerEventData eventData)
    {
        if (mOnEndDrag != null)
        {
            mOnEndDrag(gameObject, eventData);
        }
    }

    override public void OnDrop(PointerEventData eventData)
    {
        if (mOnDrop != null)
        {
            mOnDrop(gameObject, eventData);
        }
    }

    override public void OnScroll(PointerEventData eventData)
    {
        if (mOnScroll != null)
        {
            mOnScroll(gameObject, eventData);
        }
    }

    override public void OnSelect(BaseEventData eventData)
    {
        if (mOnSelect != null)
        {
            mOnSelect(gameObject, eventData);
        }
    }

    override public void OnDeselect(BaseEventData eventData)
    {
        if (mOnDeselect != null)
        {
            mOnDeselect(gameObject, eventData);
        }
    }

    override public void OnUpdateSelected(BaseEventData eventData)
    {
        if (mOnUpdateSelected != null)
        {
            mOnUpdateSelected(gameObject, eventData);
        }
    }

    override public void OnMove(AxisEventData eventData)
    {
        if (mOnMove != null)
        {
            mOnMove(gameObject, eventData);
        }
    }

    override public void OnSubmit(BaseEventData eventData)
    {
        if (mOnSubmit != null)
        {
            mOnSubmit(gameObject, eventData);
        }
    }

    override public void OnCancel(BaseEventData eventData)
    {
        if (mOnCancel != null)
        {
            mOnCancel(gameObject, eventData);
        }
    }

    override public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (mOnInitializePotentialDrag != null)
        {
            mOnInitializePotentialDrag(gameObject, eventData);
        }
    }

    static public UIEventListener Get(GameObject go)
    {
        UIEventListener listener = go.GetComponent<UIEventListener>();
        if (listener == null) listener = go.AddComponent<UIEventListener>();
        return listener;
    }

    static public UIEventListener Get(Transform parent, string childName)
    {
        Transform tran = FindChild(parent, childName);
        return Get(tran.gameObject);
    }

    static private Transform FindChild(Transform parent, string childName)
    {
        if (childName.Contains("/"))
        {
            return parent.Find(childName);
        }

        Transform child = parent.Find(childName);
        if (child != null)
        {
            return child;
        }

        Transform[] tranArray = parent.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < tranArray.Length; ++i)
        {
            Transform tran = tranArray[i];
            if (tran.name == childName)
            {
                return tran;
            }
        }

        return null;
    }
}
