using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public bool yesOrNot = false;
    public bool buttonPressed = false;

    //UI
    public GameObject background;
    public Button yesButton;
    public Button noButton;


    //GameObjects
    public float triggerDistance = 5.0f;

    public bool isWeaponPicked = false;
    public bool isDoorOpened = false;

    public Transform player, weapon, door, dontOpenText3D;
    public Material doorMaterial;

    public AudioSource playerASDeath;

    public UnityEvent onYesPressed = new UnityEvent();
    public UnityEvent onNoPressed = new UnityEvent();

    public static GameManager Instance;
    void Start()
    {
        Instance = this;
        onYesPressed.AddListener(OnYesButtonPressed);
        onNoPressed.AddListener(OnNoButtonPressed);
    }

    public void OnYesButtonPressed()
    {
        yesOrNot = true;
        buttonPressed = true;
    }
    public void OnNoButtonPressed()
    {
        yesOrNot = false;
        buttonPressed = true;
    }

    private void Update()
    {
        CheckRadiuses();
        CheckInput();
    }

    void CheckInput()
    {
        if (Input.GetKey(KeyCode.Y))
        {
            onYesPressed.Invoke();
        }
        else if(Input.GetKey(KeyCode.N))
        {
            onNoPressed.Invoke();
        }
    }
    void CheckRadiuses()
    {
        if (!isWeaponPicked)
        {
            float distance1 = Vector3.Distance(player.position, weapon.position);
            if (distance1 < triggerDistance)
            {
                isWeaponPicked = true;
                WeaponPicked();
                return;
            }
        }

        if (!isDoorOpened)
        {
            float distance2 = Vector3.Distance(player.position, door.position);
            if (distance2 < triggerDistance)
            {
                isDoorOpened = true;
                DoorOpened();
                return;
            }
        }
    }

    void WeaponPicked()
    {
        Time.timeScale = 0.1f;
        background.SetActive(true);
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        // camera freeze
        StartCoroutine("DoButtonPressed1");
    }

    IEnumerator DoButtonPressed1()
    {
        yield return new WaitUntil(() => buttonPressed);

        //yes pressed
        if (yesOrNot == true)
        {
            player.gameObject.GetComponent<Animator>().SetBool("dead", true);
            playerASDeath.Play();
            //player.gameObject.GetComponent<AudioSource>().Play();
            //gameends...


            //death animation and sound
            Debug.Log("Pressed Yes");

            StartCoroutine("DestroyWeapon");
            StartCoroutine("RestartScene");
        }
        else // no pressed
        {
            StartCoroutine("DestroyWeapon");
            Debug.Log("Pressed No");
        }
        Time.timeScale = 1f;
        background.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        buttonPressed = false;
        yield return new WaitForSeconds(.1f);
    }
   
    IEnumerator DestroyWeapon()
    {
        Rigidbody weaponRB = weapon.gameObject.AddComponent<Rigidbody>();
        weaponRB.useGravity = true;
        weaponRB.mass = 1.0f;
        weaponRB.AddForce(3f, 0f, 0f, ForceMode.Impulse);
        yield return new WaitForSeconds(5);
        Destroy(weapon.gameObject);
    }


    IEnumerator RestartScene()
    {
        yield return new WaitForSeconds(10);
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    void DoorOpened()
    {
        Time.timeScale = 0.1f;
        background.SetActive(true);
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        // camera freeze
        StartCoroutine("DoButtonPressed2");
    }
    IEnumerator DoButtonPressed2()
    {
        yield return new WaitUntil(() => buttonPressed);

        if (yesOrNot == true)
        {
            Debug.Log("Pressed Yes on the door");
            StartCoroutine("DoorEmiting");
            //StartCoroutine("RestartScene");
        }
        else // no pressed
        {
            Debug.Log("Pressed No on the door");
        }
        Time.timeScale = 1f;
        background.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        buttonPressed = false;
        yield return new WaitForSeconds(.1f);
    }
    IEnumerator DoorEmiting()
    {
        float elapsedTime = 0f;

        doorMaterial.EnableKeyword("_EMISSION");
        Color currentColor = doorMaterial.GetColor("_EmissionColor");
        Debug.Log(currentColor);
        float initialIntensity = -10f;
        float targetIntensity = 4.5f;

        while (elapsedTime < 5.0f) // duration
        {
            float t = elapsedTime / 5.0f;
            float newIntensity = Mathf.Lerp(initialIntensity, targetIntensity, t);
            //initialIntensity += newIntensity;
            Color newColor = currentColor * newIntensity;

            doorMaterial.SetColor("_EmissionColor", newColor);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

}
