using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Sphere : MonoBehaviour
{
    public GameObject fence, playerObj, road;
    public Material newMaterial;

    public GameObject textToHide1, textToHide2, textToChange3;
    public Material textToChangeMaterial;

    private bool isUsed = false;
    public GameObject animateBG, animateText1, animateText2, animateText3;

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
        if (distance < 14.0f)
        {
            isUsed = true;
            fence.GetComponent<BoxCollider>().enabled = false;
            road.SetActive(true);
            fence.GetComponent<Renderer>().material = newMaterial;
            StartCoroutine(SphereHandler());
        }
    }

    IEnumerator SphereHandler()
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

        yield return new WaitForSeconds(3f);
        animateText1.GetComponent<TextMeshProUGUI>().text = "What kind of delusion is this?";
        StartCoroutine(TextFadeOut(animateText1));
        yield return new WaitForSeconds(3f);
        StartCoroutine(TextFadeIn(animateText1));
        yield return new WaitForSeconds(3.5f);
        animateText2.GetComponent<TextMeshProUGUI>().text = "What's the last thing I remember?";
        StartCoroutine(TextFadeOut(animateText2));
        yield return new WaitForSeconds(3f);
        StartCoroutine(TextFadeIn(animateText2));
        yield return new WaitForSeconds(3.5f);
        animateText3.GetComponent<TextMeshProUGUI>().text = "I should beware the ideas that are not my own";
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
        
        textToHide1.SetActive(false);
        textToHide2.SetActive(false);
        textToChange3.GetComponent<Renderer>().material = textToChangeMaterial;
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
