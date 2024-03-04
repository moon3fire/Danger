using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeGameObject : MonoBehaviour
{
    public float fadeDuration = 3.0f;
    Renderer objRenderer;

    private void Start()
    {
        objRenderer = GetComponent<Renderer>();
    }

    public void FadeOut()
    {
        StartCoroutine("FadeOutBegin");
    }
    public void FadeIn()
    {
        StartCoroutine("FadeInBegin");
    }

    IEnumerator FadeOutBegin()
    {
        float timer = 0f;
        Color startColor = objRenderer.material.color;

        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            objRenderer.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        objRenderer.material.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }

    IEnumerator FadeInBegin()
    {
        float timer = 0f;
        Color startColor = objRenderer.material.color;

        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            objRenderer.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        objRenderer.material.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
    }
}
