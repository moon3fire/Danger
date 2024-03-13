using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using TMPro;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    // game state
    private bool blueLevelHandled = false, helpTextShowed = false, loadingLevel = false;
    private bool nextLevelStarted = false;
    private int gameState = 0; // 0 weapon, 1 door, 2 pictures, 3 blue door
    //ending
    public GameObject whiteBG, endingText;

    [SerializeField] 
    private bool isUIVisible = false;
    private bool spaceKeyPressed = false;

    public Image wasdImage;
    public TextMeshProUGUI wasdTextUGUI, controlsTextUGUI;
    //UI
    public GameObject background;
    public Button yesButton;
    public Button noButton;
    public GameObject helpText;

    //Game state -- Level 1
    private static int weaponsTaked = 0;
    private static bool level1IntroEnded = false;
    //weapon
    public bool weaponPartEnded = false;
    public GameObject weaponPickingQuestionText;
    //door
    public TextMeshProUGUI doorOpeningQuestionText;
    private bool doorPartEnded = false, doorResetInProcess = false;
    private bool nearRedOpenedDoor = false;
    private bool nearBlueOpenedDoor = false;
    //pictures
    public TextMeshProUGUI symbolTakeQuestionText, symbolReplaceQuestionText;
    public bool picturesPartEnded = false, askedToPickAPicture = false, askInHold = false, animationEnd = false;
    GameObject pickedLetterObject;
    GameObject replacingLetterObject;
    string pickedText = string.Empty;
    string replacementText = string.Empty;
    public GameObject rightHandObject;
    public Material rightHandEmissive, rightHandDefault;

    //utility
    public bool movementDisabled = false;
    public CinemachineFreeLook flCam;
    public GameObject rulesText;
    private Color weaponMaterialBaseColor;
    public Material weaponBaseMaterial, weaponEmissiveMaterial;
    //GameObjects
    public float triggerDistance = 6.0f; 

    public bool isWeaponNear = false;
    public bool isDoorNear = false;

    public Transform player, weapon, door, dontOpenText3D;
    public List<GameObject> pictures = new List<GameObject>();
    public List<Transform> picturePlaceholders = new List<Transform>();
    public List<Transform> candidatePlaceholders = new List<Transform>();
    public Material doorMaterial;

    public AudioSource playerASDeath;

    public static GameManager Instance;

    // end game
    public GameObject bgImage, text1, text2;

    // Methods
    void Start()
    {
        wasdImage.gameObject.SetActive(false);
        wasdTextUGUI.gameObject.SetActive(false);
        controlsTextUGUI.gameObject.SetActive(false);

        Debug.Log("Intro status: " + level1IntroEnded);
        if (level1IntroEnded)
        {
            bgImage.gameObject.SetActive(false);
            text1.gameObject.SetActive(false);
            text2.gameObject.SetActive(false);
        }
        Instance = this;

        // optional
        doorMaterial.SetColor("_EmissionColor", new Vector4(0f, 0f));
    }
    private void Update()
    {
        if (!level1IntroEnded)
            StartCoroutine(Level1Intro());

        if (Input.GetKey(KeyCode.Space))
            spaceKeyPressed = true;
        else
            spaceKeyPressed = false;

        if (!isUIVisible)
        {
            if (spaceKeyPressed)
                CheckRadiuses();
        }

        if (picturesPartEnded && doorPartEnded && weaponPartEnded)
        {
            // call function for this
            rulesText.GetComponent<TextMeshPro>().text = "need to do ";
            if (3 - weaponsTaked > 1)
                rulesText.GetComponent<TextMeshPro>().text += (3 - weaponsTaked) + " things to get out";
            else if (3 - weaponsTaked == 1)
                rulesText.GetComponent<TextMeshPro>().text += "1 thing to get out";
            rulesText.SetActive(true);
            if (!door.gameObject.activeSelf)
            {
                StartCoroutine(DoorEmiting());
            }
            door.gameObject.SetActive(true);
        }

        // do check if rules are correct
        if (!picturesPartEnded)
            CheckIfTextIsCorrect();

        if (!loadingLevel)
        {
            if (spaceKeyPressed && nearRedOpenedDoor)
                StartCoroutine("LoadLabyrinthLevel");
            if (spaceKeyPressed && nearBlueOpenedDoor)
                StartCoroutine("HandleBlueDoor");
        }

        if (weaponsTaked == 3)
        {
            PersistentData.levelInfo = 3;
            SceneManager.LoadSceneAsync(2);
        }
    }

    void CheckRadiuses()
    {
        //Weapon part
        if (!isWeaponNear && !weaponPartEnded)
        {
            float weaponDistance = Vector3.Distance(player.position, weapon.position);
            if (weaponDistance < triggerDistance)
            {
                isWeaponNear = true;
                WeaponPickingHandler();
                return;
            }
        }

        //Door part
        if (!isDoorNear && !doorPartEnded)
        {
            float doorDistance = Vector3.Distance(player.position, door.position);
            if (doorDistance < triggerDistance)
            {
                isDoorNear = true;
                DoorOpeningHandler();
                return;
            }
        }
        else if (isDoorNear)
        {
            StartCoroutine(ResetDoorNearState());
        }

        //Pictures part
        if (!picturesPartEnded && weaponPartEnded && doorPartEnded)
        {
            if (!askInHold)
            {
                if (!askedToPickAPicture)
                {
                    foreach (Transform ph in picturePlaceholders)
                    {
                        float phDistance = Vector3.Distance(player.position, ph.position);
                        if (phDistance < triggerDistance + 1.25f) // added additional value due to bad objects
                        {
                            pickedLetterObject = ph.GetChild(0).GetChild(0).gameObject;
                            AskToPickALetter();
                            //erase the picked element for some time from the list and then add it after replacement
                            break;
                        }
                    }
                }
                else if (askedToPickAPicture)
                {
                    foreach (Transform ph in candidatePlaceholders)
                    {
                        float candidateDistance = Vector3.Distance(player.position, ph.position);
                        if (candidateDistance < triggerDistance + 1.25f)
                        {
                            replacingLetterObject = ph.GetChild(0).GetChild(0).gameObject;
                            AskToReplaceALetter();
                            break;
                        }
                    }
                }
            }
        }
    }

    // Weapon Part
    void WeaponPickingHandler()
    {
        showQuestionUI();
        weaponPickingQuestionText.SetActive(true);
        StartCoroutine("DoButtonPressedForWeapon");
    }
    IEnumerator DoButtonPressedForWeapon()
    {
        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y)) //yes pressed
        {
            weaponsTaked++;
            //player.gameObject.GetComponent<Animator>().SetBool("dead", true);
            //playerASDeath.Play();
            //StartCoroutine("RestartScene");
            level1IntroEnded = true;
            StartCoroutine("KillPlayer");
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            StartCoroutine("DestroyWeapon");
        }

        gameState++;
        weaponPickingQuestionText.SetActive(false);
        weaponPartEnded = true;
        hideQuestionUI();
        yield return new WaitForSeconds(.1f);
    }

    IEnumerator KillPlayer()
    {
        weapon.gameObject.GetComponent<UpAndDownMovement>().enabled = false;
        FadeGameObject weaponFade = weapon.gameObject.GetComponent<FadeGameObject>();
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<Animator>().enabled = false;
        movementDisabled = true;
        weaponFade.FadeOut();

        yield return new WaitForSeconds(weaponFade.fadeDuration);

        weapon.position = new Vector3(player.position.x - 2.1f, 7.3f, player.position.z + 2.1f);
        weapon.Rotate(-135f, 0f, 0f);

        weaponFade.FadeIn();
        yield return new WaitForSeconds(weaponFade.fadeDuration - 2f);

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

        weaponMaterialBaseColor = weaponBaseMaterial.GetColor("_EmissionColor");
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

        yield return new WaitForSeconds(3f);

        StartCoroutine(RestartScene());

        yield return null;
    }

    IEnumerator DestroyWeapon()
    {
        Rigidbody weaponRB = weapon.gameObject.AddComponent<Rigidbody>();
        weaponRB.useGravity = true;
        weaponRB.mass = 1.0f;
        weaponRB.AddForce(0f, 20f, 0f, ForceMode.Impulse);
        yield return new WaitForSeconds(1);
        Destroy(weapon.gameObject);
    }

    // Door Part
    IEnumerator ResetDoorNearState()
    {
        if (doorResetInProcess)
        {
            yield return null;
        }
        else
        {
            doorResetInProcess = true;
            yield return new WaitForSeconds(1f);
            isDoorNear = false;
            doorResetInProcess = false;
        }

    }
    void DoorOpeningHandler()
    {
        if (!weaponPartEnded)
        {
            //play some sound
            door.GetComponent<AudioSource>().Play();
            return;
        }
        doorOpeningQuestionText.gameObject.SetActive(true);
        showQuestionUI();
        StartCoroutine("DoButtonPressedForDoor");
    }
    IEnumerator DoButtonPressedForDoor()
    {
        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));


        if (Input.GetKey(KeyCode.Y) == true) // yes pressed
        {
            StartCoroutine("DoorEmiting");
            nearRedOpenedDoor = true;
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            nearBlueOpenedDoor = true;
        }

        doorPartEnded = true;
        doorOpeningQuestionText.gameObject.SetActive(false);
        SpawnPictures();
        hideQuestionUI();
        yield return new WaitForSeconds(.1f);
    }

    IEnumerator DoorFadeOut()
    {
        float elapsedTime = 0f;

        Color baseColor = door.GetComponent<Renderer>().material.color;
        Color startColor = baseColor;

        while (elapsedTime < 3f)
        {
            float t = elapsedTime / 3f;
            Color newColor = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));
            door.GetComponent<Renderer>().material.color = newColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(3f);

        door.gameObject.SetActive(false);
        door.GetComponent<Renderer>().material.color = baseColor;
    }

    // Pictures part
    void AskToPickALetter()
    {
        symbolTakeQuestionText.gameObject.SetActive(true);
        showQuestionUI();
        StartCoroutine("DoButtonPressedForPicking");
    }

    void AskToReplaceALetter()
    {
        symbolReplaceQuestionText.gameObject.SetActive(true);
        showQuestionUI();
        StartCoroutine("DoButtonPressedForReplacement");
    }

    IEnumerator DoButtonPressedForPicking()
    {
        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y) == true) // yes pressed
        {
            //animating right hand
            rightHandObject.GetComponent<Renderer>().material = rightHandEmissive;
            animationEnd = false;
            askedToPickAPicture = true;
            askInHold = true;
            rightHandObject.GetComponent<AudioSource>().Play();
            StartCoroutine("AnimateRightHand");

            pickedText = pickedLetterObject.GetComponent<TextMeshPro>().text;
            pickedLetterObject.GetComponent<TextMeshPro>().SetText("");

            foreach (Transform ph in picturePlaceholders)
            {
                if (ph.transform.GetChild(0).GetChild(0).name != pickedLetterObject.name)
                {
                    candidatePlaceholders.Add(ph);
                }
            }
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            Debug.Log("I dont want to pick a picture");
            // start corountine for 5 sec
            askedToPickAPicture = false;
            askInHold = false;
        }

        symbolTakeQuestionText.gameObject.SetActive(false);
        StartCoroutine("AskingInHoldState");
        hideQuestionUI();
        yield return new WaitForSeconds(.1f);
    }

    IEnumerator DoButtonPressedForReplacement()
    {
        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y) == true) // yes pressed
        {
            rightHandObject.GetComponent<AudioSource>().Stop();
            rightHandObject.GetComponent<Renderer>().material = rightHandDefault;
            replacementText = replacingLetterObject.GetComponent<TextMeshPro>().text;
            replacingLetterObject.GetComponent<TextMeshPro>().SetText(pickedText);
            pickedLetterObject.GetComponent<TextMeshPro>().SetText(replacementText);
            askedToPickAPicture = false;
            candidatePlaceholders.Clear();
            pickedText = string.Empty;
            replacementText = string.Empty;
            animationEnd = true;
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            Debug.Log("I dont want to replace the text");
            // start corountine for 5 sec
        }

        askInHold = true;
        StartCoroutine("AskingInHoldState");
        symbolReplaceQuestionText.gameObject.SetActive(false);
        hideQuestionUI();
        yield return new WaitForSeconds(.1f);
    }

    IEnumerator AskingInHoldState()
    {
        yield return new WaitForSeconds(1f);
        askInHold = false;
    }

    // utility
    IEnumerator RestartScene()
    {
        Debug.Log("restarting the scene");
        weapon.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(4.65f);
        //crunch, some quote if player dies
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
    void showQuestionUI()
    {
        isUIVisible = true;
        Time.timeScale = 0.1f;
        flCam.m_XAxis.m_MaxSpeed /= 10;
        background.SetActive(true);
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        // camera freeze
    }

    void hideQuestionUI()
    {
        isUIVisible = false;
        Time.timeScale = 1f;
        flCam.m_XAxis.m_MaxSpeed *= 10;
        background.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        // camera unfreeze
    }
    IEnumerator DoorEmiting()
    {
        Debug.Log("Door started emiting");
        float elapsedTime = 0f;
        doorMaterial.EnableKeyword("_EMISSION");
        float initialIntensity = 0f;
        float targetIntensity = 2.5f;
        doorMaterial.SetColor("_EmissionColor", new Vector4(15.050f, 2.357f, 2.357f, 0));
        Color currentColor = doorMaterial.GetColor("_EmissionColor");
        while (elapsedTime < 3.0f) // duration
        {
            float newIntensity = Mathf.Lerp(initialIntensity, targetIntensity, elapsedTime / 3f);
            Color newColor = currentColor * (newIntensity / 2);
            doorMaterial.SetColor("_EmissionColor", newColor);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator DoorEmiting2()
    {
        Debug.Log("Door started emiting");
        float elapsedTime = 0f;
        doorMaterial.EnableKeyword("_EMISSION");
        float initialIntensity = 0f;
        float targetIntensity = 2.5f;
        doorMaterial.SetColor("_EmissionColor", new Vector4(2.357f, 2.357f, 15.050f, 0));
        Color currentColor = doorMaterial.GetColor("_EmissionColor");
        while (elapsedTime < 3.0f) // duration
        {
            float newIntensity = Mathf.Lerp(initialIntensity, targetIntensity, elapsedTime / 3f);
            Color newColor = currentColor * (newIntensity / 2);
            doorMaterial.SetColor("_EmissionColor", newColor);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator AnimateRightHand()
    {
        while (!animationEnd)
        {
            float elapsedTime1 = 0f, elapsedTime2 = 0f;
            rightHandEmissive.EnableKeyword("_EMISSION");
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
        //rightHandEmissive.SetColor("_EmissionColor", Color.black);
    }

    void SpawnPictures()
    {
        foreach (GameObject obj in pictures)
        {
            obj.SetActive(true);
        }
    }

    void CheckIfTextIsCorrect()
    {
        string currentWord = string.Empty;
        foreach (GameObject picture in pictures)
        {
            currentWord += picture.transform.GetChild(0).GetComponent<TextMeshPro>().text;
        }

        if (currentWord == "RULES")
        {
            Debug.Log("Collected the letters");
            picturesPartEnded = true;
        }
    }

    IEnumerator Level1Intro()
    {
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<Animator>().enabled = false;
        yield return new WaitForSeconds(9f);
        StartCoroutine(ShowControls());
        level1IntroEnded = true;
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<Animator>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        gameState = 0;
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator ShowControls()
    {
        wasdImage.gameObject.SetActive(true);
        wasdTextUGUI.gameObject.SetActive(true);
        controlsTextUGUI.gameObject.SetActive(true);

        float timer = 0f;
        Color wasdColor = wasdImage.GetComponent<Image>().color;
        Color textColor = wasdTextUGUI.color;
        Color controlsColor = controlsTextUGUI.color;

        while (timer < 5f)
        {
            float alpha1 = Mathf.Lerp(wasdColor.a, 0f, timer / 5f);
            float alpha2 = Mathf.Lerp(textColor.a, 0f, timer / 5f);
            float alpha3 = Mathf.Lerp(controlsColor.a, 0f, timer / 5f);
            Color currentColor = new Color(wasdColor.r, wasdColor.g, wasdColor.b, alpha1);
            Color currentColor2 = new Color(textColor.r, textColor.g, textColor.b, alpha2);
            Color currentColor3 = new Color(controlsColor.r, controlsColor.g, controlsColor.b, alpha3);
            wasdImage.GetComponent<Image>().color = currentColor;
            wasdTextUGUI.color = currentColor2;
            controlsTextUGUI.color = currentColor3;
            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator StartNextLevel()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Level 2");
    }

    IEnumerator LoadLabyrinthLevel()
    {
        float distance = Vector3.Distance(player.gameObject.transform.position, door.transform.position);
        if (distance > 5f)
        {
            yield return null;
        }
        else
        {
            if (!picturesPartEnded)
            {
                door.GetComponent<AudioSource>().Play();
                yield return null;
            }
            else
            {
                StartCoroutine(DoorEmiting());
                //play some sound effects loading 
                blueLevelHandled = true;
                PersistentData.levelInfo = 1;
                StartCoroutine(AnimateWhiteBG2());
            }
        }
    }

    IEnumerator HandleBlueDoor()
    {
        float distance = Vector3.Distance(player.gameObject.transform.position, door.transform.position);
        if (distance > 5f)
        {
            yield return null;
        }
        else
        {
            if (!picturesPartEnded)
            {
                door.GetComponent<AudioSource>().Play();
                yield return null;
            }
            else if (!blueLevelHandled)
            {
                StartCoroutine(DoorEmiting2());
                //play some sound effects loading 
                blueLevelHandled = true;
                StartCoroutine(AnimateWhiteBG1());
            }
        }
    }

    // scene loading animations

    IEnumerator AnimateWhiteBG1()
    {
        endingText.SetActive(true);
        whiteBG.SetActive(true);
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
        yield return new WaitForSeconds(.1f);
        endingText.GetComponent<TextMeshProUGUI>().text = "You walk, and at the end of the maze, is a prize\n" +
                                                          "just waiting to be discovered";
        StartCoroutine(TextFadeOut(endingText));
        yield return new WaitForSeconds(5f);
        StartCoroutine(TextFadeIn(endingText));
        yield return new WaitForSeconds(5f);
        endingText.GetComponent<TextMeshProUGUI>().text = "All you have to do is find your way through.\n" +
                                                          "Can you see the maze? Its walls and floors,\n" +
                                                          "its twists and turns?";
        StartCoroutine(TextFadeOut(endingText));
        yield return new WaitForSeconds(5f);
        StartCoroutine(TextFadeIn(endingText));
        yield return new WaitForSeconds(5f);
        endingText.GetComponent<TextMeshProUGUI>().text = "Good, because the maze you've created in your mind\n" +
                                                          "is itself the maze. There is no desert, no rock or sand.\n" +
                                                          "There is the only idea of it.";
        StartCoroutine(TextFadeOut(endingText));
        yield return new WaitForSeconds(5f);
        StartCoroutine(TextFadeIn(endingText));
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(1);
    }

    IEnumerator AnimateWhiteBG2()
    {
        endingText.SetActive(true);
        whiteBG.SetActive(true);
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
        yield return new WaitForSeconds(.1f);
        endingText.GetComponent<TextMeshProUGUI>().text = "There is a maze in the desert carved from sand and rock";
        StartCoroutine(TextFadeOut(endingText));
        yield return new WaitForSeconds(5f);
        StartCoroutine(TextFadeIn(endingText));
        yield return new WaitForSeconds(5f);
        endingText.GetComponent<TextMeshProUGUI>().text = "A vast labyrinth of pathways and corridors, a hundred miles long";
        StartCoroutine(TextFadeOut(endingText));
        yield return new WaitForSeconds(5f);
        StartCoroutine(TextFadeIn(endingText));
        yield return new WaitForSeconds(5f);
        endingText.GetComponent<TextMeshProUGUI>().text = "Full of twists and dead ends. Picture it - the maze";
        StartCoroutine(TextFadeOut(endingText));
        yield return new WaitForSeconds(5f);
        StartCoroutine(TextFadeIn(endingText));
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(2);
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
