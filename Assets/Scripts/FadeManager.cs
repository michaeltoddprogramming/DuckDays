using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
        if (!fadeImage)
        {
            var canvas = new GameObject("FadeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            fadeImage = new GameObject("FadeImage", typeof(Image)).GetComponent<Image>();
            fadeImage.transform.SetParent(canvas.transform, false);
            fadeImage.rectTransform.anchorMin = Vector2.zero;
            fadeImage.rectTransform.anchorMax = Vector2.one;
            fadeImage.rectTransform.offsetMin = Vector2.zero;
            fadeImage.rectTransform.offsetMax = Vector2.zero;
            fadeImage.color = new Color(0, 0, 0, 0);
        }
    }

    public IEnumerator FadeOut()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 1);
    }

    public IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeImage.color = new Color(0, 0, 0, 1 - Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);
    }
}