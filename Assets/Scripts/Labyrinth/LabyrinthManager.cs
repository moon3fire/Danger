using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LabyrinthManager : MonoBehaviour
{
    public GameObject door;
    public GameObject player;
    public GameObject text;

    bool introShowed = false, solved = false;

    void Update()
    {
        if (!introShowed)
            StartCoroutine(ShowIntro());

        if (Input.GetKey(KeyCode.Space) && !solved)
        {
            float distance = Vector3.Distance(door.transform.position, player.transform.position);
            if (distance < 6.0f)
                StartCoroutine("SolveLabyrinth");
        }
    }

    IEnumerator ShowIntro()
    {
        introShowed = true;
        text.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        text.GetComponent<TextMeshProUGUI>().text = "Weeks?";
        yield return new WaitForSeconds(3f);
        text.GetComponent<TextMeshProUGUI>().text = "Months?";
        yield return new WaitForSeconds(3f);
        text.GetComponent<TextMeshProUGUI>().text = "Why am I here?";
        yield return new WaitForSeconds(10f);
        text.gameObject.SetActive(false);
    }

    IEnumerator SolveLabyrinth()
    {
        solved = true;
        text.GetComponentInChildren<TextMeshProUGUI>().text = "Can't tell dreams from reality anymore";
        text.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(1);
    }
}
