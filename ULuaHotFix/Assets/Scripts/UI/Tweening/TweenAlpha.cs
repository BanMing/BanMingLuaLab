//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's alpha. Works with both UI widgets as well as renderers.
/// </summary>

[AddComponentMenu("NGUI/Tween/Tween Alpha")]
public class TweenAlpha : UITweener
{
    [Range(0f, 1f)]
    public float from = 1f;
    [Range(0f, 1f)]
    public float to = 1f;

    bool mCached = false;
    CanvasRenderer mCanvasRenderer;
    SpriteRenderer mSr;
    Material mMat;

    [System.Obsolete("Use 'value' instead")]
    public float alpha { get { return this.value; } set { this.value = value; } }

    void Cache()
    {
        mCached = true;

        mCanvasRenderer = GetComponent<CanvasRenderer>();
        if (mCanvasRenderer != null) return;

        mSr = GetComponent<SpriteRenderer>();
        if (mCanvasRenderer != null) return;

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
		Renderer ren = renderer;
#else
        Renderer ren = GetComponent<Renderer>();
#endif
        if (ren != null)
        {
#if UNITY_EDITOR
            mMat = ren.sharedMaterial;
#else
            mMat = ren.material;
#endif
            return;
        }
    }

    /// <summary>
    /// Tween's current value.
    /// </summary>

    public float value
    {
        get
        {
            if (!mCached) Cache();
            if (mCanvasRenderer != null) return mCanvasRenderer.GetAlpha();
            
            if (mSr != null) return mSr.color.a;
            return mMat != null ? mMat.color.a : 1f;
        }
        set
        {
            if (!mCached) Cache();

            if (mCanvasRenderer != null)
            {
                mCanvasRenderer.SetAlpha(value);
            }
            else if (mSr != null)
            {
                Color c = mSr.color;
                c.a = value;
                mSr.color = c;
            }
            else if (mMat != null)
            {
                Color c = mMat.color;
                c.a = value;
                mMat.color = c;
            }
        }
    }

    /// <summary>
    /// Tween the value.
    /// </summary>

    protected override void OnUpdate(float factor, bool isFinished) { value = Mathf.Lerp(from, to, factor); }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenAlpha Begin(GameObject go, float duration, float alpha)
    {
        TweenAlpha comp = UITweener.Begin<TweenAlpha>(go, duration);
        comp.from = comp.value;
        comp.to = alpha;

        if (duration <= 0f)
        {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }

    public override void SetStartToCurrentValue() { from = value; }
    public override void SetEndToCurrentValue() { to = value; }
}
