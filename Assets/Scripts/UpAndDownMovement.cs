using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpAndDownMovement : MonoBehaviour
{
    [SerializeField]
    private float maximumHeight = 250f;

    private float minimumHeight = 0f;

    [SerializeField]
    private float step = 1.5f;


    void Update()
    {
        Vector3 transition = new Vector3(0f, step, 0f);
        transform.Translate(transition * Time.deltaTime);
        minimumHeight += step;
        if (minimumHeight >= maximumHeight)
            step = -step;
        else if (minimumHeight <= 0f)
            step = -step;
    }
}
