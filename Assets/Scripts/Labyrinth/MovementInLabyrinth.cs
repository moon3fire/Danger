using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementInLabyrinth : MonoBehaviour
{
    public AudioSource footstepAS;
    public Transform cameraTransform;
    [SerializeField]
    CharacterController characterController;
    [SerializeField]
    private Animator characterAnimator;
    [SerializeField]
    private bool isWalking = false;
    [SerializeField]
    private float moveSpeed = 15f;

    [SerializeField]
    private float turnSmoothTime = 0.3f;
    [SerializeField]
    private float turnSmoothVelocity;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        characterAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            isWalking = true;
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            if (footstepAS.isPlaying == false)
                footstepAS.Play();
            characterController.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
        else
        {
            footstepAS.Stop();
            isWalking = false;
        }

        characterAnimator.SetBool("isWalking", isWalking);
    
    }
}