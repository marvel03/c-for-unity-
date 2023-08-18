using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movements : MonoBehaviour
{
    public bool canMove { get; private set; } = true;
    [Header("functional options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canCrouch=true;
    [SerializeField] private bool canJump=true;

    [Header("Controls")]
    [SerializeField] private KeyCode crouchKey=KeyCode.F;
    [SerializeField] private KeyCode jumpKey=KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Movement parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField, Range(1, 20)] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed=1.5f;

    [Header("jump parameters")] 
    [SerializeField] private float jumpForce=8.0f;
    [SerializeField,Range(0,100)] private float gravity = 30.0f;

    [Header("crouch parameters")]
    [SerializeField] private float crouchHieght=0.5f;
    [SerializeField] private float standingHeight=2f;
    [SerializeField] private float timeToCrouch=0.25f;
    [SerializeField] private Vector3 crouchingCenter=new Vector3(0,0.5f,0);
    [SerializeField] private Vector3 standingCenter =new Vector3(0,0,0);
    private bool isCrouching ;
    private bool duringCrouchingAnimation;


    [Header("look parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;

    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool shouldJump =>characterController.isGrounded && Input.GetKeyDown(jumpKey); 
    private bool shouldCrouch=> Input.GetKeyDown(crouchKey) && characterController.isGrounded && !duringCrouchingAnimation;
    private Camera playerCam;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;
    private float rotationX = 0;
    void Awake()
    {
        playerCam = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }
    void Update()
    {
        if (canMove)
        {
            HandleMouseMove();
            HandleMovementInput();
            if(canCrouch){
                HandleCrouch();
            }
            if(canJump){
                HandleJump();
            }
            ApplyFinalMovemts();
        }
    }

    private void HandleMovementInput()
    {   crouchSpeed=walkSpeed/2;
        currentInput = new Vector2((isCrouching?crouchSpeed:IsSprinting?sprintSpeed:walkSpeed) * Input.GetAxis("Vertical"), (isCrouching?crouchSpeed:IsSprinting?sprintSpeed:walkSpeed) * Input.GetAxis("Horizontal"));
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }
    private void HandleMouseMove()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);//to limit ur forward rotaion value
        playerCam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }
    private void ApplyFinalMovemts()// this will allow to finally apply all the values calculated in ur controller
    {
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;// this will apply gravity to the player, it will mainly usefull whem u apply jump to this script ;)
        }
        characterController.Move(moveDirection * Time.deltaTime);
    }
    private void HandleJump(){
        if(shouldJump){
            moveDirection.y=jumpForce;
        }
    }
    private void HandleCrouch(){
        if(shouldCrouch){
            StartCoroutine(CrouchStand());
        }
    }
    private IEnumerator CrouchStand(){
        if(isCrouching&& Physics.Raycast(playerCam.transform.position,Vector3.up,2f)) yield break;

        duringCrouchingAnimation=true;
        float timeElapsed=0;
        float targetHeight=isCrouching? standingHeight:crouchHieght;
        float currentHeight=characterController.height;
        Vector3 targetCenter=isCrouching?standingCenter:crouchingCenter;
        Vector3 currentCenter=characterController.center;
        while(timeElapsed<timeToCrouch){
            characterController.height=Mathf.Lerp(currentHeight,targetHeight,timeElapsed/timeToCrouch);
            characterController.center=Vector3.Lerp(currentCenter,targetCenter,timeElapsed/timeToCrouch);
            timeElapsed+=Time.deltaTime;
            yield return null;
        }
        characterController.height=targetHeight;
        characterController.center=targetCenter;
        isCrouching= !isCrouching;
        duringCrouchingAnimation=false;

    }
}
