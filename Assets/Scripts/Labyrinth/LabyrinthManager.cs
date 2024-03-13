using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LabyrinthManager : MonoBehaviour
{
    private bool teleported = false;
    public GameObject exit;
    public GameObject door;
    public GameObject player;
    public GameObject text;

    bool introShowed = false, solved = false;

    private void Start()
    {
        int levelBefore = PersistentData.levelInfo;
        door.transform.position = player.transform.position + new Vector3(0f, 3.68f, -4.5f);
        Debug.Log("Level before equals: " + levelBefore);
        if (levelBefore == 1)
        {
            StartCoroutine(ShowIntro1());
        }
        else if (levelBefore == 2)
            StartCoroutine(ShowIntro2());
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && !solved)
        {
            float distance = Vector3.Distance(door.transform.position, player.transform.position);
            if (distance < 6.0f)
                StartCoroutine(TurnBack());
        }

        if (!teleported && PersistentData.levelInfo == 3)
            TeleportToExit();
    }

    IEnumerator ShowIntro1()
    {
        Debug.Log("Intro 1 showed");
        yield return null;
    }

    IEnumerator ShowIntro2()
    {
        Debug.Log("Intro 2 showed");
        yield return null;
    }

    void TeleportToExit()
    {
        player.transform.position = exit.transform.position;
        player.transform.Translate(5f, -2.5f, 0f);
        teleported = true;
    }

    IEnumerator TurnBack()
    {
        solved = true;
        text.GetComponentInChildren<TextMeshProUGUI>().text = "Can't tell dreams from reality anymore";
        text.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(0);
    }
}
