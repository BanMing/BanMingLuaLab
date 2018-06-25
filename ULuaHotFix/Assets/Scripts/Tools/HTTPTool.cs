
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class HTTPTool
{
    static public void GetTexture(string url, System.Action<UnityEngine.Texture> onLoad)
    {
        ScriptThread.Instance.StartCoroutine(GetTextureImp(url, onLoad));
    }

    static public void GetSprite(string url, System.Action<Sprite> onLoad)
    {
		ScriptThread.Instance.StartCoroutine(GetSpriteImp(url, onLoad));
    }

    static private IEnumerator GetTextureImp(string url, System.Action<UnityEngine.Texture> onLoad)
    {
        WWW www = GetWWW(url);
        yield return www;

        try
        {
            if (string.IsNullOrEmpty(www.error) == false)
            {
                string str = string.Format("HTTPTool.GetTextureImp:访问URL {0} 失败，错误信息{1}", url, www.error);
                Debug.LogError(str);
                onLoad(null);
            }
            else
            {
                onLoad(www.texture);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

	static private IEnumerator GetSpriteImp(string url, System.Action<UnityEngine.Sprite> onLoad)
	{
		WWW www = GetWWW(url);
		yield return www;

		try
		{
			if (string.IsNullOrEmpty(www.error) == false)
			{
				string str = string.Format("HTTPTool.GetTextureImp:访问URL {0} 失败，错误信息{1}", url, www.error);
				Debug.LogError(str);
				onLoad(null);
			}
			else
			{
				UnityEngine.Texture2D tex2d = www.texture;
				Rect rect = new Rect(0, 0, tex2d.width, tex2d.height);
				Sprite sprite = Sprite.Create(tex2d, rect, new Vector2(0.5f, 0.5f));
				onLoad(sprite);
				Resources.UnloadAsset(www.assetBundle);
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}


    //--------------------------------------------------------------------------//

    static public void GetText(string url, System.Action<string> onLoad)
    {
        ScriptThread.Instance.StartCoroutine(GetTextImp(url, onLoad));
    }

    static private IEnumerator GetTextImp(string url, System.Action<string> onLoad)
    {
        WWW www = GetWWW(url);
        yield return www;

        try
        {
            if (string.IsNullOrEmpty(www.error) == false)
            {
                string str = string.Format("HTTPTool.GetTextImp:访问URL {0} 失败，错误信息{1}", url, www.error);
                Debug.LogError(str);
                onLoad(null);
            }
            else
            {
                onLoad(www.text);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    //--------------------------------------------------------------------------//

    static public void GetBytes(string url, System.Action<byte[]> onLoad)
    {
        ScriptThread.Instance.StartCoroutine(GetBytesImp(url, onLoad));
    }

    static private IEnumerator GetBytesImp(string url, System.Action<byte[]> onLoad)
    {
        WWW www = GetWWW(url);
        yield return www;

        try
        {
            if (string.IsNullOrEmpty(www.error) == false)
            {
                string str = string.Format("HTTPTool.GetBytesImp:访问URL {0} 失败，错误信息{1}", url, www.error);
                Debug.LogError(str);
                onLoad(null);
            }
            else
            {
                onLoad(www.bytes);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    //--------------------------------------------------------------------------//

    static public void DownLoadFile(string url, string localFilePath, bool isAbsolutePath, Action<bool> finishAction)
    {
        try
        {
            System.Action<byte[]> onLoad = delegate (byte[] data)
            {
                if (data != null)
                {
                    MyFileUtil.WriteFile(localFilePath, data, isAbsolutePath);
                    finishAction(true);
                }
                else
                {
                    finishAction(false);
                }
            };
            GetBytes(url, onLoad);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            finishAction(false);
        }
    }

    //--------------------------------------------------------------------------//

    static public string GetURL(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }
        string str = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss_fff_");
        long tick = System.DateTime.Now.Ticks;
        System.Random random = new System.Random((int)tick);
        int num = random.Next(0, 10000);

        if (url.Contains("?"))
        {
            url = string.Format("{0}&p={1}{2}", url, str, num);
        }
        else
        {
            url = string.Format("{0}?p={1}{2}", url, str, num);
        }
      
        //Debug.Log("HTTPTool.GetURL:" + url);
        return url;
    }

    static public WWW GetWWW(string url)
    {
        url = GetURL(url);
        WWW www = new WWW(url);
        return www;
    }
}
