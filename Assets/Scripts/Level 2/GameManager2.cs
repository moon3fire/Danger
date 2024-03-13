using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager2 : MonoBehaviour
{
    //state
    public bool swordTouched = false;
    public GameObject whiteBG;
    private float triggerDistance = 6.0f;
    private bool spaceKeyPressed = false;

    private bool weaponAlreadyPicked = false;
    private bool doorAlreadyPicked = false;
    private bool pictureAlreadyPicked = false, symbolTaked = false, lastPictureAlreadyPicked = false, lastSymbolTaked;
    private bool rightHandAnimationGoing = false;
    private bool replacingStarted = false, lastReplacingStarted;
    // game objects
    public CinemachineFreeLook flCam;
    public GameObject weapon, door, wall, player, rightHandObject;

    public Material weaponBaseMaterial, weaponEmissiveMaterial, rightHandEmissive, rightHandBase;

    public List<GameObject> pictures = new List<GameObject>();
    public GameObject lastPicture;

    public List<Transform> picturePlaceholders = new List<Transform>();
    public List<Transform> lastPicturePlaceholders = new List<Transform>();
    public List<Transform> candidatePlaceholders = new List<Transform>();
    public List<Transform> lastCandidatePlaceholders = new List<Transform>();
    public List<TextMeshPro> letters = new List<TextMeshPro>();
    public List<TextMeshPro> lastLetters = new List<TextMeshPro>();

    private TextMeshPro pickedPictureText, replacingPictureText;
    private string pickedText, replacingText;
    // ui
    public GameObject questionImageBG, yesButtonText, noButtonText;
    public GameObject weaponPickText, symbolPickText, symbolReplaceText;

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
        if (!weaponAlreadyPicked)
        {
            float distance = Vector3.Distance(player.transform.position, weapon.transform.position);
            if (distance < triggerDistance)
            {
                weaponAlreadyPicked = true;
                StartCoroutine(PickTheWeapon());
            }
        }

        if (!doorAlreadyPicked && !swordTouched)
        {
            float distance = Vector3.Distance(player.transform.position, door.transform.position);
            if (distance < triggerDistance)
            {
                doorAlreadyPicked = true;
                StartCoroutine(PickTheDoor());
            }
        }

        if (!pictureAlreadyPicked && !swordTouched)
        {
            foreach (Transform ph in picturePlaceholders)
            {
                float phDistance = Vector3.Distance(player.transform.position, ph.position);
                if (phDistance < triggerDistance + 1.25f) // added additional value due to bad objects
                {
                    pictureAlreadyPicked = true;
                    pickedPictureText = ph.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
                    StartCoroutine(AskToPickALetter());
                    break;
                }
            }
        }
        else if (swordTouched && !lastPictureAlreadyPicked)
        {
            foreach (Transform ph in lastPicturePlaceholders)
            {
                float phDistance = Vector3.Distance(player.transform.position, ph.position);
                if (phDistance < triggerDistance + 1.25f) // added additional value due to bad objects
                {
                    lastPictureAlreadyPicked = true;
                    pickedPictureText = ph.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
                    StartCoroutine(AskToPickALastLetter()); // new one
                    break;
                }
            }
        }

        if (symbolTaked && !replacingStarted && !swordTouched)
        {
            foreach (Transform ph in candidatePlaceholders)
            {
                float phDistance = Vector3.Distance(player.transform.position, ph.position);
                if (phDistance < triggerDistance + 1.25f)
                {
                    replacingStarted = true;
                    replacingPictureText = ph.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
                    StartCoroutine(AskToReplaceALetter());
                    break;
                }
            }
        }
        else if (swordTouched && !lastReplacingStarted && lastSymbolTaked)
        {
            foreach (Transform ph in lastCandidatePlaceholders)
            {
                float phDistance = Vector3.Distance(player.transform.position, ph.position);
                if (phDistance < triggerDistance + 1.25f)
                {
                    lastReplacingStarted = true;
                    replacingPictureText = ph.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
                    StartCoroutine(AskToReplaceALastLetter()); // new one
                    break;
                }
            }
        }
       
    }

    IEnumerator PickTheWeapon()
    {
        showQuestionUI();
        weaponPickText.GetComponent<TextMeshProUGUI>().text = "Do you want to pick up the weapon?";
        weaponPickText.SetActive(true);

        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y)) //yes pressed
        {
            weaponPickText.SetActive(false);
            hideQuestionUI();
            weapon.GetComponent<UpAndDownMovement>().enabled = false;
            Renderer[] children = weapon.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in children)
            {
                var mats = new Material[rend.materials.Length];
                for (int j = 0; j < rend.materials.Length; j++)
                {
                    mats[j] = weaponEmissiveMaterial;
                }
                rend.materials = mats;
            }

            Color weaponMaterialBaseColor = weaponBaseMaterial.GetColor("_EmissionColor");
            weaponBaseMaterial = weaponEmissiveMaterial;
            float elapsedTime = 0f;
            float initialIntensity = 0f;
            float targetIntensity = 3.1f;
            weaponEmissiveMaterial.SetColor("_EmissionColor", new Vector4(15.050f, 2.357f, 2.357f, 0));
            Color currentColor = weaponEmissiveMaterial.GetColor("_EmissionColor");
            while (elapsedTime < 3.0f) // duration
            {
                float t = elapsedTime / 3.0f;
                float newIntensity = Mathf.Lerp(initialIntensity, targetIntensity, t);
                Color newColor = currentColor * (newIntensity / 2);
                weaponEmissiveMaterial.SetColor("_EmissionColor", newColor);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            weaponPickText.SetActive(false);
            hideQuestionUI();
            weaponAlreadyPicked = false;
        }
    }

    IEnumerator PickTheDoor()
    {
        showQuestionUI();
        weaponPickText.GetComponent<TextMeshProUGUI>().text = "Do you want to open the door?";
        weaponPickText.SetActive(true);

        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y)) //yes pressed
        {
            weaponPickText.SetActive(false);
            hideQuestionUI();

            foreach (GameObject obj in pictures)
            {
                obj.SetActive(true);
            }
          
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            weaponPickText.SetActive(false);
            hideQuestionUI();
            doorAlreadyPicked = false;
        }
    }

    //pictures
    IEnumerator AskToPickALetter()
    {
        symbolPickText.SetActive(true);
        showQuestionUI();

        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y) == true) // yes pressed
        {
            rightHandAnimationGoing = true;
            StartCoroutine(AnimateRightHand());
            symbolTaked = true;
            rightHandObject.GetComponent<Renderer>().material = rightHandEmissive;
            rightHandObject.GetComponent<AudioSource>().Play();
            pickedText = pickedPictureText.text;
            pickedPictureText.SetText("");

            foreach (Transform ph in picturePlaceholders)
            {
                if (ph.transform.GetChild(0).GetChild(0).name != pickedPictureText.gameObject.name)
                {
                    candidatePlaceholders.Add(ph);
                }
            }
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            pictureAlreadyPicked = false;
        }

        symbolPickText.gameObject.SetActive(false);
        hideQuestionUI();
    }

    IEnumerator AskToReplaceALetter()
    {
        showQuestionUI();
        symbolReplaceText.SetActive(true);

        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y) == true) // yes pressed
        {
            pictureAlreadyPicked = false;
            rightHandAnimationGoing = false;
            symbolTaked = false;
            rightHandObject.GetComponent<AudioSource>().Stop();
            rightHandObject.GetComponent<Renderer>().material = rightHandBase;
            replacingText = replacingPictureText.text;
            pickedPictureText.GetComponent<TextMeshPro>().SetText(replacingText);
            replacingPictureText.GetComponent<TextMeshPro>().SetText(pickedText);
            candidatePlaceholders.Clear();
            pickedText = string.Empty;
            replacingText = string.Empty;
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            Debug.Log("I dont want to replace the text");
            // start corountine for 5 sec
        }

        CheckPeace();
        replacingStarted = false;
        symbolReplaceText.SetActive(false);
        hideQuestionUI();
        yield return new WaitForSeconds(.1f);
    }
    // Utility

    void showQuestionUI()
    {
        player.GetComponent<MovementInLabyrinth>().enabled = false;
        Time.timeScale = 0.1f;
        flCam.m_XAxis.m_MaxSpeed /= 10;
        questionImageBG.SetActive(true);
        yesButtonText.gameObject.SetActive(true);
        noButtonText.gameObject.SetActive(true);
        // camera freeze
    }

    void hideQuestionUI()
    {
        player.GetComponent<MovementInLabyrinth>().enabled = true;
        Time.timeScale = 1f;
        flCam.m_XAxis.m_MaxSpeed *= 10;
        questionImageBG.SetActive(false);
        yesButtonText.gameObject.SetActive(false);
        noButtonText.gameObject.SetActive(false);
        // camera unfreeze
    }

    IEnumerator AnimateRightHand()
    {
        rightHandEmissive.EnableKeyword("_EMISSION");

        while (rightHandAnimationGoing)
        {
            float elapsedTime1 = 0f, elapsedTime2 = 0f;
            float initialIntensity = 0f;
            float targetIntensity = 2.2f;
            rightHandEmissive.SetColor("_EmissionColor", new Vector4(2.357f, 2.357f, 15.050f, 0));
            Color currentColor = rightHandEmissive.GetColor("_EmissionColor");

            while (elapsedTime1 < 3.5f)
            {
                float t = elapsedTime1 / 3.5f;
                float newIntensity = Mathf.Lerp(initialIntensity, targetIntensity, t);
                Color newColor = currentColor * newIntensity;
                rightHandEmissive.SetColor("_EmissionColor", newColor);

                elapsedTime1 += Time.deltaTime;
                yield return null;
            }
            rightHandEmissive.SetColor("_EmissionColor", currentColor * targetIntensity);

            yield return new WaitForSeconds(0.1f);

            while (elapsedTime2 < 3f)
            {
                float t = elapsedTime2 / 3f;
                float newIntensity = Mathf.Lerp(targetIntensity, initialIntensity, t);
                Color newColor = currentColor * newIntensity;
                rightHandEmissive.SetColor("_EmissionColor", newColor);

                elapsedTime2 += Time.deltaTime;
                yield return null;
            }
        }
    }

    void CheckPeace()
    {
        string word = string.Empty;
        foreach (TextMeshPro letter in letters)
        {
            word += letter.text;
        }

        if (word == "PEACE")
        {
            wall.SetActive(false);
        }
    }

    IEnumerator AskToPickALastLetter()
    {
        symbolPickText.SetActive(true);
        showQuestionUI();

        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y) == true) // yes pressed
        {
            rightHandAnimationGoing = true;
            StartCoroutine(AnimateRightHand());
            lastSymbolTaked = true;
            rightHandObject.GetComponent<Renderer>().material = rightHandEmissive;
            rightHandObject.GetComponent<AudioSource>().Play();
            pickedText = pickedPictureText.text;
            pickedPictureText.SetText("");

            foreach (Transform ph in lastPicturePlaceholders)
            {
                if (ph.transform.GetChild(0).GetChild(0).name != pickedPictureText.gameObject.name)
                {
                    lastCandidatePlaceholders.Add(ph);
                }
            }
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            lastPictureAlreadyPicked = false;
        }

        symbolPickText.gameObject.SetActive(false);
        hideQuestionUI();
    }

    IEnumerator AskToReplaceALastLetter()
    {
        showQuestionUI();
        symbolReplaceText.SetActive(true);

        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y) == true) // yes pressed
        {
            lastPictureAlreadyPicked = false;
            rightHandAnimationGoing = false;
            lastSymbolTaked = false;
            rightHandObject.GetComponent<AudioSource>().Stop();
            rightHandObject.GetComponent<Renderer>().material = rightHandBase;
            replacingText = replacingPictureText.text;
            pickedPictureText.GetComponent<TextMeshPro>().SetText(replacingText);
            replacingPictureText.GetComponent<TextMeshPro>().SetText(pickedText);
            lastCandidatePlaceholders.Clear();
            pickedText = string.Empty;
            replacingText = string.Empty;
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            Debug.Log("I dont want to replace the text");
        }

        lastReplacingStarted = false;
        symbolReplaceText.SetActive(false);
        hideQuestionUI();
        CheckEscape();
        yield return new WaitForSeconds(.1f);
    }

    void CheckEscape()
    {
        string word = string.Empty;
        foreach (TextMeshPro letter in lastLetters)
        {
            word += letter.text;
        }

        if (word == "ESCAPE")
        {
            StartCoroutine(SolvedMaze());
        }
    }

    IEnumerator SolvedMaze()
    {
        float timer = 0f;
        Color bgColor = whiteBG.GetComponent<Image>().color;
        while (timer < 3f)
        {
            float alpha1 = Mathf.Lerp(0f, 2f, timer / 3f);
            Color currentColor = new Color(bgColor.r, bgColor.g, bgColor.b, alpha1);
            whiteBG.GetComponent<Image>().color = currentColor;
            timer += Time.deltaTime;
            yield return null;
        }

        PersistentData.levelInfo = 2;
        SceneManager.LoadScene(2);
    }
}
