using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameFinish : MonoBehaviour
{
    [SerializeField]
    public GameObject whiteBG, finishText;
    public AudioSource finishAS;
    private GameObject player;
    private bool used = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        finishAS = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && !used)
        {
            float distance = Vector3.Distance(gameObject.transform.position, player.transform.position);
            if (distance < 5f)
            {
                used = true;
                StartCoroutine(Finish());
            }
        }
    }

    IEnumerator Finish()
    {
        finishAS.Play();
        whiteBG.SetActive(true); // optional 
        float timer = 0f;
        Color bgColor = whiteBG.GetComponent<Image>().color;
        while (timer < 5f)
        {
            float alpha1 = Mathf.Lerp(0f, 2f, timer / 5f);
            Color currentColor = new Color(bgColor.r, bgColor.g, bgColor.b, alpha1);
            whiteBG.GetComponent<Image>().color = currentColor;
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;
        Color text1Color = finishText.GetComponent<TextMeshProUGUI>().color;
        while (timer < 5f)
        {
            float alpha1 = Mathf.Lerp(0f, 2f, timer / 5f);
            Color currentColor = new Color(text1Color.r, text1Color.g, text1Color.b, alpha1);
            finishText.GetComponent<TextMeshProUGUI>().color = currentColor;
            timer += Time.deltaTime;
            yield return null;
        }
        
        yield return new WaitForSeconds(15f);
        Application.Quit();
    }
}
