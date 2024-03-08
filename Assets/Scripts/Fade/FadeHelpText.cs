using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeHelpText : MonoBehaviour
{
    public float fadeDuration = 3.0f;
    public TextMeshProUGUI textMeshPro;

    public void FadeOut()
    {
        StartCoroutine(FadeOutBegin());
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInBegin());
    }

    IEnumerator FadeOutBegin()
    {
        float timer = 0f;
        Color startColor = textMeshPro.color;

        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            float newValue = startColor.a - alpha;
            textMeshPro.color = new Color(startColor.r, startColor.g, startColor.b, newValue);
            timer += Time.deltaTime;
            yield return null;
        }

        textMeshPro.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }

    IEnumerator FadeInBegin()
    {
        float timer = 0f;
        Color startColor = textMeshPro.color;

        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            textMeshPro.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
