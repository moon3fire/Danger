using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager2 : MonoBehaviour
{
    //state
    private bool spaceKeyPressed = false;

    private bool weaponNear = false, weaponAlreadyPicked = false;
    private bool doorNear = false, doorAlreadyPicked = false;

    // game objects
    public CinemachineFreeLook flCam;
    public GameObject weapon, door, wall, player;

    // ui
    public GameObject questionImageBG, yesButtonText, noButtonText;

    private void Start()
    {
        door.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Vector4(0f, 0f));
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
            spaceKeyPressed = true;
        else
            spaceKeyPressed = false;

        if (spaceKeyPressed)
            CheckRadiuses();
    }

    void CheckRadiuses()
    {
        if (!weaponNear && !weaponAlreadyPicked)
        {
            float distance = Vector3.Distance(player.transform.position, weapon.transform.position);
            weaponAlreadyPicked = true;
            StartCoroutine(PickTheWeapon());
        }
    }

    IEnumerator PickTheWeapon()
    {
        showQuestionUI();

        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y)) //yes pressed
        {
            Debug.Log("Do smth");
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            Debug.Log("Do other smth");
        }


        hideQuestionUI();
    }


    // Utility

    void showQuestionUI()
    {
        Time.timeScale = 0.1f;
        flCam.m_XAxis.m_MaxSpeed /= 10;
        questionImageBG.SetActive(true);
        yesButtonText.gameObject.SetActive(true);
        noButtonText.gameObject.SetActive(true);
        // camera freeze
    }

    void hideQuestionUI()
    {
        Time.timeScale = 1f;
        flCam.m_XAxis.m_MaxSpeed *= 10;
        questionImageBG.SetActive(false);
        yesButtonText.gameObject.SetActive(false);
        noButtonText.gameObject.SetActive(false);
        // camera unfreeze
    }

}
