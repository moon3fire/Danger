using UnityEngine;

public class PersistentData : MonoBehaviour
{
    public static int levelInfo = 0;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        DontDestroyOnLoad(gameObject);
    }
}
