using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Piano : MonoBehaviour
{
    public GameObject textToShow, playerObj, sphereObj;
    public GameObject YAITMtext;
    public Material YAITMnewMaterial;
    public GameObject animateBG, animateText1, animateText2, animateText3;
    private bool isUsed = false;

    void Update()
    {
        if (!isUsed)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                CheckPlayerRadius();
            }
        }
    }

    void CheckPlayerRadius()
    {
        float distance = Vector3.Distance(gameObject.transform.position, playerObj.transform.position);
        if (distance < 7.0f)
        {
            isUsed = true;
            textToShow.SetActive(true);
            sphereObj.SetActive(true);
            StartCoroutine(PianoHandler());
        }
    }

    IEnumerator PianoHandler()
    {
        animateBG.SetActive(true); // optional 
        float timer = 0f;
        Color bgColor = animateBG.GetComponent<Image>().color;
        while (timer < 5f)
        {
            float alpha1 = Mathf.Lerp(0f, 2f, timer / 5f);
            Color currentColor = new Color(bgColor.r, bgColor.g, bgColor.b, alpha1);
            animateBG.GetComponent<Image>().color = currentColor;
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(5f);
        animateText1.GetComponent<TextMeshProUGUI>().text = "Do you know how long you've been here?";
        StartCoroutine(TextFadeOut(animateText1));
        yield return new WaitForSeconds(5f);
        StartCoroutine(TextFadeIn(animateText1));
        yield return new WaitForSeconds(5f);
        animateText2.GetComponent<TextMeshProUGUI>().text = "Weeks? Months?";
        StartCoroutine(TextFadeOut(animateText2));
        yield return new WaitForSeconds(5f);
        StartCoroutine(TextFadeIn(animateText2));
        yield return new WaitForSeconds(5f);
        animateText3.GetComponent<TextMeshProUGUI>().text = "Do you know why you're here?";
        StartCoroutine(TextFadeOut(animateText3));
        yield return new WaitForSeconds(3f);
        StartCoroutine(TextFadeIn(animateText3));
        yield return new WaitForSeconds(3.5f);
        timer = 0f;
        bgColor = animateBG.GetComponent<Image>().color;
        while (timer < 5f)
        {
            float alpha1 = Mathf.Lerp(2f, 0f, timer / 5f);
            Color currentColor = new Color(bgColor.r, bgColor.g, bgColor.b, alpha1);
            animateBG.GetComponent<Image>().color = currentColor;
            timer += Time.deltaTime;
            yield return null;
        }

        YAITMtext.GetComponent<Renderer>().material = YAITMnewMaterial;
        yield return new WaitForSeconds(5f);
    }

    IEnumerator TextFadeOut(GameObject obj)
    {
        float timer = 0f;
        Color text1Color = obj.GetComponent<TextMeshProUGUI>().color;
        while (timer < 5f)
        {
            float alpha1 = Mathf.Lerp(0f, 2f, timer / 5f);
            Color currentColor = new Color(text1Color.r, text1Color.g, text1Color.b, alpha1);
            obj.GetComponent<TextMeshProUGUI>().color = currentColor;
            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator TextFadeIn(GameObject obj)
    {
        float timer = 0f;
        Color text1Color = obj.GetComponent<TextMeshProUGUI>().color;
        while (timer < 5f)
        {
            float alpha1 = Mathf.Lerp(2f, 0f, timer / 5f);
            Color currentColor = new Color(text1Color.r, text1Color.g, text1Color.b, alpha1);
            obj.GetComponent<TextMeshProUGUI>().color = currentColor;
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
