using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Data;
using TMPro;

public class GameManager : MonoBehaviour
{
    // game state
    [SerializeField] 
    private bool isUIVisible = false;

    public bool yesOrNot = false;

    //UI
    public GameObject background;
    public Button yesButton;
    public Button noButton;

    //Game state -- Level 1
    //weapon
    public bool weaponPartEnded = false;
    //door
    public bool doorPartEnded = false, doorResetInProcess = false;
    //pictures
    public bool picturesPartEnded = false, askedToPickAPicture = false, askInHold = false, animationEnd = false;
    GameObject pickedLetterObject;
    GameObject replacingLetterObject;
    string pickedText = string.Empty;
    string replacementText = string.Empty;

    public GameObject rightHandObject;
    public Material rightHandEmissive, rightHandDefault;

    //GameObjects
    public float triggerDistance = 5.0f; 

    public bool isWeaponNear = false;
    public bool isDoorNear = false;

    public Transform player, weapon, door, dontOpenText3D;
    public List<GameObject> pictures = new List<GameObject>();
    public List<Transform> picturePlaceholders = new List<Transform>();
    public List<Transform> candidatePlaceholders = new List<Transform>();
    public Material doorMaterial;

    public AudioSource playerASDeath;

    public static GameManager Instance;
    
    // Methods
    void Start()
    {
        Instance = this;

        // optional
        doorMaterial.SetColor("_EmissionColor", new Vector4(0f, 0f));
    }
    private void Update()
    {
        if (!isUIVisible)
        {
            CheckRadiuses();
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
        StartCoroutine("DoButtonPressedForWeapon");
    }
    IEnumerator DoButtonPressedForWeapon()
    {
        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y)) //yes pressed
        {
            player.gameObject.GetComponent<Animator>().SetBool("dead", true);
            playerASDeath.Play();
            StartCoroutine("DestroyWeapon");
            StartCoroutine("RestartScene");
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            StartCoroutine("DestroyWeapon");
        }

        weaponPartEnded = true;
        hideQuestionUI();
        yield return new WaitForSeconds(.1f);
    }
    IEnumerator DestroyWeapon()
    {
        Rigidbody weaponRB = weapon.gameObject.AddComponent<Rigidbody>();
        weaponRB.useGravity = true;
        weaponRB.mass = 1.0f;
        weaponRB.AddForce(3f, 5f, 0f, ForceMode.Impulse);
        yield return new WaitForSeconds(5);
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
            yield return new WaitForSeconds(10f);
            isDoorNear = false;
            doorResetInProcess = false;
        }

    }
    void DoorOpeningHandler()
    {
        if (!weaponPartEnded)
        {
            //play some sound
            Debug.Log("Weapon is not picked up");
            return;
        }
        showQuestionUI();
        StartCoroutine("DoButtonPressedForDoor");
    }
    IEnumerator DoButtonPressedForDoor()
    {
        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));


        if (Input.GetKey(KeyCode.Y) == true) // yes pressed
        {
            StartCoroutine("DoorEmiting");
            StartCoroutine("RestartScene");
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            Debug.Log("Pressed No on the door");
        }

        doorPartEnded = true;
        SpawnPictures();
        hideQuestionUI();
        yield return new WaitForSeconds(.1f);
    }

    // Pictures part
    void AskToPickALetter()
    {
        showQuestionUI();
        StartCoroutine("DoButtonPressedForPicking");
    }

    void AskToReplaceALetter()
    {
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
            StartCoroutine("AnimateRightHand");

            pickedText = pickedLetterObject.GetComponent<TextMeshPro>().text;
            pickedLetterObject.GetComponent<TextMeshPro>().SetText("");
            Debug.Log("Picked text is: " + pickedText);

            foreach (Transform ph in picturePlaceholders)
            {
                if (ph.transform.GetChild(0).GetChild(0).name != pickedLetterObject.name)
                {
                    Debug.Log(ph.transform.GetChild(0).GetChild(0).name + " VS " + pickedLetterObject.name);
                    candidatePlaceholders.Add(ph);
                }
            }
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            Debug.Log("I dont want to pick a picture");
            // start corountine for 5 sec
        }

        askedToPickAPicture = true;
        askInHold = true;
        StartCoroutine("AskingInHoldState");
        hideQuestionUI();
        yield return new WaitForSeconds(.1f);
    }

    IEnumerator DoButtonPressedForReplacement()
    {
        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y) == true) // yes pressed
        {
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
        hideQuestionUI();
        yield return new WaitForSeconds(.1f);
    }

    IEnumerator AskingInHoldState()
    {
        yield return new WaitForSeconds(2f);
        askInHold = false;
    }

    // utility
    IEnumerator RestartScene()
    {
        yield return new WaitForSeconds(10);
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
    void showQuestionUI()
    {
        isUIVisible = true;
        Time.timeScale = 0.1f;
        background.SetActive(true);
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        // camera freeze
    }

    void hideQuestionUI()
    {
        isUIVisible = false;
        Time.timeScale = 1f;
        background.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        // camera unfreeze
    }
    IEnumerator DoorEmiting()
    {
        float elapsedTime = 0f;
        doorMaterial.EnableKeyword("_EMISSION");
        float initialIntensity = 0f;
        float targetIntensity = 4.0f;
        doorMaterial.SetColor("_EmissionColor", new Vector4(15.050f, 2.357f, 2.357f, 0));
        Color currentColor = doorMaterial.GetColor("_EmissionColor");
        while (elapsedTime < 7.0f) // duration
        {
            float t = elapsedTime / 7.0f;
            float newIntensity = Mathf.Lerp(initialIntensity, targetIntensity, t);
            Color newColor = currentColor * newIntensity;

            doorMaterial.SetColor("_EmissionColor", newColor);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        //StartCoroutine("DoorFade");
        Destroy(door.gameObject);
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
}
