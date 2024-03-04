using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeTextMeshPro : MonoBehaviour
{
    public float fadeDuration = 5.0f;
    public GameObject dangerText, background;
    private TextMeshProUGUI textMeshPro;

    private void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        FadeIn();
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutBegin());
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInBegin());
    }

    public void ShowDangerText()
    {
        StartCoroutine(ShowDangerBegin());
    }

    IEnumerator ShowDangerBegin()
    {
        yield return new WaitForSeconds(1.5f);
        dangerText.SetActive(true);

        StartCoroutine(DangerAndBackgroundFade());
    }

    IEnumerator DangerAndBackgroundFade()
    {
        float timer = 0f;
        Color backgroundStartColor = background.gameObject.GetComponent<Image>().color;
        Color dangerTextColor = dangerText.GetComponent<TextMeshProUGUI>().color;
   
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            Color currentColor1 = new Color(backgroundStartColor.r, backgroundStartColor.g, backgroundStartColor.b, alpha);
            Color currentColor2 = new Color(dangerTextColor.r, dangerTextColor.g, dangerTextColor.b, alpha);
            background.gameObject.GetComponent<Image>().color = currentColor1;
            dangerText.GetComponent<TextMeshProUGUI>().color = currentColor2;
            timer += Time.deltaTime;
            yield return null;
        }

        background.gameObject.SetActive(false);
        dangerText.gameObject.SetActive(false);
    }

    IEnumerator FadeOutBegin()
    {
        float timer = 0f;
        Color startColor = textMeshPro.color;

        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            textMeshPro.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        textMeshPro.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        ShowDangerText();
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

        textMeshPro.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
        FadeOut();
    }
}
