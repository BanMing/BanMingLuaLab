using System;
using System.Collections;
using UnityEngine;
public class ScriptThread : MonoBehaviour
{
    private static GameObject mInstanceGameObject;
    private static ScriptThread mInstance;
    public static ScriptThread Instance
    {
        get
        {
            if (ScriptThread.mInstance == null)
            {
                ScriptThread.CreateInstance();
            }
            return ScriptThread.mInstance;
        }
    }

    public static ScriptThread CreateInstance()
    {
        if (ScriptThread.mInstance == null)
        {
            ScriptThread.mInstanceGameObject = new GameObject(typeof(ScriptThread).Name);
            ScriptThread.mInstance = ScriptThread.mInstanceGameObject.AddComponent<ScriptThread>();
        }
        return ScriptThread.mInstance;
    }

    private void Awake()
    {
        if (ScriptThread.mInstance != null)
        {
            UnityEngine.Object.Destroy(base.gameObject);
            return;
        }
        UnityEngine.Object.DontDestroyOnLoad(this);
        ScriptThread.mInstance = this;
    }

    public static Coroutine Start(IEnumerator routine)
    {
        return ScriptThread.Instance.StartCoroutine(routine);
    }

    public static void StopAll()
    {
        ScriptThread.Instance.StopAllCoroutines();
    }

    public static void Stop(IEnumerator routine)
    {
        ScriptThread.Instance.StopCoroutine(routine);
    }


    public static void DoAction(Action action, float time)
    {
        Instance.StartCoroutine(DoActionImp(action, time));
    }

    private static IEnumerator DoActionImp(Action action, float time)
    {
        yield return new WaitForSeconds(time);
        action();
    }

    public static void DoActionInNextFrame(Action action)
    {
        Instance.StartCoroutine(DoActionInNextFrameImp(action));
    }

    private static IEnumerator DoActionInNextFrameImp(Action action)
    {
        yield return null;
        action();
    }

    public static void DoActionEverySecond(Func<bool> func)
    {
        Instance.StartCoroutine(DoActionEverySecondImp(func));
    }

    private static IEnumerator DoActionEverySecondImp(Func<bool> func)
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            bool result = func();
            if (result)
            {
                break;
            }
        }

    }

    public static void RepeatDoAction(Func<bool> func, float time)
    {
        Instance.StartCoroutine(RepeatDoActionImp(func, time));
    }

    private static IEnumerator RepeatDoActionImp(Func<bool> func, float time)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            bool result = func();
            if (result)
            {
                break;
            }
        }

    }

    public static void RepeatDoAction(Action func, float time)
    {
        Instance.StartCoroutine(RepeatDoActionImp(func, time));
    }

    private static IEnumerator RepeatDoActionImp(Action func, float time)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            func();
        }
    }
}
