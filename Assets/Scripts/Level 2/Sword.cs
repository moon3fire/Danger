using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Sword : MonoBehaviour
{
    public GameManager2 manager;
    public List<GameObject> objectsToShow = new List<GameObject>();
    public GameObject playerObj, background, lastPicture;
    public TextMeshProUGUI youNeedEscapeText;
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
        if (distance < 100.0f)
        {
            isUsed = true;
            StartCoroutine(HandleSword());
        }
    }

    IEnumerator HandleSword()
    {
        float timer = 0f;
        Color bgColor = background.GetComponent<Image>().color;
        while (timer < 3f)
        {
            float alpha1 = Mathf.Lerp(0f, 2f, timer / 3f);
            Color currentColor = new Color(bgColor.r, bgColor.g, bgColor.b, alpha1);
            background.GetComponent<Image>().color = currentColor;
            timer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(4f);
        youNeedEscapeText.text = "Are you sure?";
        StartCoroutine(TextFadeOut(youNeedEscapeText.gameObject));
        yield return new WaitForSeconds(5f);
        StartCoroutine(TextFadeIn(youNeedEscapeText.gameObject));
        yield return new WaitForSeconds(5f);
        playerObj.transform.position = new Vector3(-5, 0, 0);

        foreach (GameObject obj in objectsToShow)
        {
            obj.SetActive(true);
        }
        lastPicture.SetActive(true);
        manager.swordTouched = true;
        background.SetActive(false);
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
