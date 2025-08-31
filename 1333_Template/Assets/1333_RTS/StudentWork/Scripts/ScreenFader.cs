using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup faderGroup; 
    [SerializeField] private float fadeDuration = 1.0f;
    public float FadeDuration => fadeDuration;

    private Coroutine currentRoutine;

    void Awake()
    {
       
        faderGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void FadeOut()
    {
        gameObject.SetActive(true);
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(0f, 1f));
    }

    public void FadeIn()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(1f, 0f));
    }

    private IEnumerator FadeRoutine(float from, float to)
    {
        float timer = 0f;
        faderGroup.alpha = from;
        gameObject.SetActive(true);

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; 
            faderGroup.alpha = Mathf.Lerp(from, to, timer / fadeDuration);
            yield return null;
        }

        faderGroup.alpha = to;

       
        if (to <= 0.01f) gameObject.SetActive(false);
    }
}
