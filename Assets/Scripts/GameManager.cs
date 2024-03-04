using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Data;

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
    public bool weaponPartEnded = false;
    public bool doorPartEnded = false, doorResetInProcess = false;
    public bool picturesPartEnded = false, askedToPickAPicture = false;

    //GameObjects
    public float triggerDistance = 5.0f; 

    public bool isWeaponNear = false;
    public bool isDoorNear = false;

    public Transform player, weapon, door, dontOpenText3D;
    public List<GameObject> pictures = new List<GameObject>();
    public List<Transform> picturePlaceholders = new List<Transform>();
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
            foreach (Transform ph in picturePlaceholders)
            {
                float placeHolderDistance = Vector3.Distance(player.position, ph.transform.position);
                if (placeHolderDistance < triggerDistance + 1.25f && !askedToPickAPicture) // added additional value due to bad objects
                {
                    AskToPickALetter();
                }
                else if (placeHolderDistance < triggerDistance + 1.25f && askedToPickAPicture)
                {
                    Debug.Log("SOMETHING!))))) :)");
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
        StartCoroutine("DoButtonPressedForAPicture");
    }

    IEnumerator DoButtonPressedForAPicture()
    {
        yield return new WaitUntil(() => Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.N));

        if (Input.GetKey(KeyCode.Y) == true) // yes pressed
        {
            Debug.Log("I pick a picture!");
        }
        else if (Input.GetKey(KeyCode.N)) // no pressed
        {
            Debug.Log("I dont want to pick a picture");
        }

        hideQuestionUI();
        yield return new WaitForSeconds(.1f);
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
            //initialIntensity += newIntensity; useless
            Color newColor = currentColor * newIntensity;

            doorMaterial.SetColor("_EmissionColor", newColor);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        //StartCoroutine("DoorFade");
        Destroy(door.gameObject);
    }

    void SpawnPictures()
    {
        foreach (GameObject obj in pictures)
        {
            obj.SetActive(true);
        }
    }
}
