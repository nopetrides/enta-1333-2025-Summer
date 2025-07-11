using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton that fades a full-screen CanvasGroup in/out.
/// </summary>
public class ScreenFader : Singleton<ScreenFader>
{
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private float _defaultDuration = 0.5f;

    /// <summary>Fade to black, then back to transparent.</summary>
    public IEnumerator FadeOutIn(float duration = -1f)
    {
        if (duration < 0f) duration = _defaultDuration;
        yield return Fade(0f, 1f, duration);   // fade-out
        yield return null;                      // 1-frame buffer
        yield return Fade(1f, 0f, duration);   // fade-in
    }

    /// <summary>Generic alpha lerp.</summary>
    public IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        _group.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        _group.alpha = to;
    }
}
