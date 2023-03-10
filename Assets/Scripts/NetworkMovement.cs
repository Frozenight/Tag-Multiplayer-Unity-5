using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Cinemachine;
using TMPro;
using UnityEngine.InputSystem;

public class NetworkMovement : NetworkBehaviour
{
    private Animator m_Animator;

    private GameObject gameManager;
    private GameObject menu;

    public CharacterController controller;
    public float controllerDefaultHeight = 1.45f;
    public float controllerWallClimbHeight = 1f;
    public Vector3 controllerDefaultCenter = new Vector3(0, 0.8f, 0);
    public Vector3 controllerWallClimbCenter = new Vector3(0, 1.2f, 0);
    public Transform cam;

    private float speed = 0;
    private float runningSpeed = 6;
    private float walkingSpeed = 3;
    private float climbOnTopOfTheWallSpeedUp = 2f;
    private float climbOnTopOfTheWallSpeedForward = 1.8f;
    public float gravity = -9.81f;
    public float jumpHeight = 7f;

    public Transform groundCheck;
    public float groundDistance = 0.3f;
    [SerializeField]
    private LayerMask groundMask;
    [SerializeField]
    private LayerMask wallMask;
    [SerializeField]
    private Image wallIdentication;
    private Button jumpButton;
    private Image jumpButtonImage;
    private TMP_Text jumpText;

    bool isGrounded;
    bool running = false;
    bool walking = false;
    bool falling = false;
    bool jumping = false;
    bool onWall = false;
    bool climbingWall = false;
    bool fallingFromTheWall = false;
    bool readyToClimbOnTheWallTop = false;

    public float turnSmoothTime = 0.9f;
    Vector3 velocity;
    float turnSmoothVelocity;

    public bool isOnWall
    {
        get { return onWall; }
        set
        {
            if (value != onWall)
            {
                onWall = value;
                if (onWall)
                {
                    controller.height = controllerWallClimbHeight;
                    controller.center = controllerWallClimbCenter;
                }
                else
                {
                    if (climbUpTheWall)
                        return;
                    controller.height = controllerDefaultHeight;
                    controller.center = controllerDefaultCenter;
                    fallingFromTheWall = true;
                }
            }
        }
    }

    public bool IsJumpAvailable
    {
        get { return isJumpAvalable; }
        set
        {
            if (value != isJumpAvalable)
            {
                isJumpAvalable = value;
                if (isJumpAvalable)
                {
                    StartJumpCD();
                }
            }
        }
    }
    public bool climbUpTheWall
    {
        get { return climbUpWall; }
        set
        {
            if (value != climbUpWall)
            {
                climbUpWall = value;
                if (climbUpWall)
                {
                    StartCoroutine(GetComponent<ClimbUpWallTop>().ClimbUpTheWall());
                }
            }
        }
    }

    private bool mobilePress = false;

    private bool climbUpWall;
    private bool isJumpAvalable;
    private float jumpCooldown = 0.5f;
    private bool canJump = true;
    private bool isJumping;

    public static Vector3 respawn_point = new Vector3(-4, 0.5f, 0);
    private float respawn_Height = -10f;

    private PlayerInput playerInput;

    [SerializeField]
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();


    RaycastHit forwardHit;
    RaycastHit forwardHitLowerBody;
    RaycastHit rightHit;
    RaycastHit leftHit;
    int wallSideIndex;
    public enum PlayerState
    {
        isFalling,
        isWalking,
        isRunning,
        isGrounded,
        isJumping,
        isLanding,
        isIdle,
        isOnWall,
        isClimbingWall,
        isFallingOfTheWall,
        isClimbingOnTopOfTheWall
    }

    private void Start()
    {
        //Cursor.lockState =  CursorLockMode.Locked;
        GameObject camera = GameObject.Find("Main Camera");
        cam = camera.transform;
        m_Animator = gameObject.GetComponent<Animator>();
        GameObject wall = GameObject.Find("WallIdentication");
        wallIdentication = wall.GetComponent<Image>();

        GameObject gManager = GameObject.Find("GameManager");
        gameManager = gManager;

        GameObject Menu = GameObject.Find("Canvas");
        menu = Menu;
        playerInput = GetComponent<PlayerInput>();

        GameObject jump = GameObject.Find("Jump_button");
        jumpButton = jump.GetComponent<Button>();
        jumpButtonImage = jump.GetComponent<Image>();
        GameObject text = GameObject.Find("JumpText");
        Debug.Log(text);
        jumpText = text.GetComponent<TMP_Text>();
        jumpButton.enabled = false;
        jumpButtonImage.enabled = false;
        jumpText.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner && IsClient)
        {
            ClientCursor();
            if (climbUpTheWall)
            {
               Client_ClimbOnTopOfTheWall();
            }
            else if (isOnWall)
                Client_WallClimb();
            else
                Client_Movement();

            Client_CheckForWall();
            SetVisualsOfTheClientToTheServer();
        }
        ClientVisuals();
    }

    void ClientCursor()
    {
        if (Input.GetKey(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;
    }

    void Client_ClimbOnTopOfTheWall()
    {
        if (wallSideIndex == 1)
            controller.Move(new Vector3(0, climbOnTopOfTheWallSpeedUp, climbOnTopOfTheWallSpeedForward) * Time.deltaTime);
        else if (wallSideIndex == 2)
            controller.Move(new Vector3(climbOnTopOfTheWallSpeedForward, climbOnTopOfTheWallSpeedUp, 0) * Time.deltaTime);
        else if (wallSideIndex == 3)
            controller.Move(new Vector3(0, climbOnTopOfTheWallSpeedUp, -climbOnTopOfTheWallSpeedForward) * Time.deltaTime);
        else if (wallSideIndex == 4)
            controller.Move(new Vector3(-climbOnTopOfTheWallSpeedForward, climbOnTopOfTheWallSpeedUp, 0) * Time.deltaTime);
    }

    void Client_Movement()
    {
        climbingWall = false;
        isOnWall = false;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        // on Ground
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            isGrounded = true;
            readyToClimbOnTheWallTop = false;
            jumping = false;
            isJumping = false;
            IsJumpAvailable = true;
            falling = false;
            fallingFromTheWall = false;
        }
        // not Touching Ground
        else
        {
            isGrounded = false;
            isJumping = false;
            IsJumpAvailable = false;
            climbingWall = false;

            if (((isJumping) && velocity.y < 0) || velocity.y < -2)
            {
                falling = true;
            }
        }
        // Respawn
        if (this.transform.position.y < respawn_Height)
        {
            controller.enabled = false;
            transform.position = respawn_point;
            controller.enabled = true;
        }
        // Jump
        if (Input.GetButtonUp("Jump") && isGrounded)
        {
            Jump();
        }
        Vector3 direction = new Vector3(0, 0, 0);
        // Mobile
        if (menu.GetComponent<Menu>().mobile)
        {
            Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();

            direction = new Vector3(input.x, 0f, input.y).normalized;
        }
        // PC
        else
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");


            direction = new Vector3(horizontal, 0f, vertical).normalized;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        // Jump
        if ((Input.GetButtonUp("Jump")) && isGrounded)
        {
            Jump();
        }
        //speed
        currentSpeed();

        if (direction.magnitude >= 0.1f)
        {
            sprint();
            walking = true;

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            running = false;
            walking = false;
        }
    }

    void Client_CheckForWall()
    {
        if (Physics.Raycast(transform.position + new Vector3(0, 1.3f), transform.forward, out forwardHit, 0.5f, wallMask))
        {
            if (!isOnWall)
            {
                wallIdentication.enabled = true;
                if (menu.GetComponent<Menu>().mobile)
                {
                    jumpButton.enabled = false;
                    jumpButtonImage.enabled = false;
                    jumpText.enabled = false;
                }
            }
            CheckForWallPart2();
        }
        else if (Physics.Raycast(transform.position + new Vector3(0, 1f), transform.forward, out forwardHitLowerBody, 1, wallMask) && (readyToClimbOnTheWallTop))
        {
            climbUpTheWall = true;
        }
        else
        {
            isOnWall = false;
            wallIdentication.enabled = false;
            if (menu.GetComponent<Menu>().mobile)
            {
                jumpButton.enabled = true;
                jumpButtonImage.enabled = true;
                jumpText.enabled = true;
            }
        }
    }

    public void CheckForWallPart2()
    {
        if ((Input.GetKeyDown(KeyCode.F) || mobilePress) && isGrounded)
        {
            bool leftRay = Physics.Raycast(transform.position + new Vector3(0, 1.4f), -transform.right, out rightHit, 0.5f, wallMask);
            bool rightRay = Physics.Raycast(transform.position + new Vector3(0, 1.4f), transform.right, out leftHit, 0.5f, wallMask);
            mobilePress = false;
            if (isOnWall)
                isOnWall = false;
            else
            {
                Client_TurnToWall(leftRay, rightRay, leftHit, rightHit, forwardHit);
                wallIdentication.enabled = false;
                if (menu.GetComponent<Menu>().mobile)
                {
                    jumpButton.enabled = true;
                    jumpButtonImage.enabled = true;
                    jumpText.enabled = true;
                }
            }
        }
    }
    public void CheckForWallPart2Mobile()
    {
        Debug.Log("Pressed");
        mobilePress = true;
    }

    void Client_WallClimb()
    {
        Vector3 direction = new Vector3(0, 0, 0);
        float horizontal = 0f;
        float vertical = 0f;
        // Mobile
        if (menu.GetComponent<Menu>().mobile)
        {
            Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();
            horizontal = input.x;
            vertical = input.y;

            direction = new Vector3(input.x, 0f, input.y).normalized;
        }
        // PC
        else
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");


            direction = new Vector3(horizontal, 0f, vertical).normalized;
        }

        readyToClimbOnTheWallTop = true;
        if (wallSideIndex == 1)
            controller.Move(new Vector3(horizontal, vertical, 0) * Time.deltaTime);
        else if (wallSideIndex == 2)
            controller.Move(new Vector3(0, vertical, -horizontal) * Time.deltaTime);
        else if (wallSideIndex == 3)
            controller.Move(new Vector3(-horizontal, vertical, 0) * Time.deltaTime);
        else if (wallSideIndex == 4 )
            controller.Move(new Vector3(0, vertical, horizontal) * Time.deltaTime);
        if (direction.magnitude >= 0.1f)
        {
            climbingWall = true;
        }
        else
            climbingWall = false;
    }

    void Client_TurnToWall(bool leftRay, bool rightRay, RaycastHit leftHit, RaycastHit rightHit, RaycastHit forwardHit)
    {
        int adjustmentInDegrees = 0;

        if (rightRay && leftRay)
        {
            if ((rightHit.distance < forwardHit.distance) || (leftHit.distance < forwardHit.distance))
            {
                if (rightHit.distance < leftHit.distance)
                    adjustmentInDegrees = 90;
                else
                    adjustmentInDegrees = -90;
            }
        }
        else if (leftRay)
        {
            if (leftHit.distance < forwardHit.distance)
                adjustmentInDegrees = -90;
        }
        else if (rightRay)
            if (rightHit.distance < forwardHit.distance)
                adjustmentInDegrees = 90;

        float wallRotation = 0;

        if ((transform.localRotation.eulerAngles.y > 0) && (transform.localRotation.eulerAngles.y <= 45))
        {
            wallRotation = -transform.localRotation.eulerAngles.y;
            wallSideIndex = 1;
        }
        else if ((transform.localRotation.eulerAngles.y > 45) && (transform.localRotation.eulerAngles.y <= 90))
        {
            wallRotation = 90 - transform.localRotation.eulerAngles.y;
            wallSideIndex = 2;
        }
        else if ((transform.localRotation.eulerAngles.y > 90) && (transform.localRotation.eulerAngles.y <= 135))
        {
            wallRotation = 90 - transform.localRotation.eulerAngles.y;
            wallSideIndex = 2;
        }
        else if ((transform.localRotation.eulerAngles.y > 135) && (transform.localRotation.eulerAngles.y <= 180))
        {
            wallRotation = 180 - transform.localRotation.eulerAngles.y;
            wallSideIndex = 3;
        }
        else if ((transform.localRotation.eulerAngles.y > 180) && (transform.localRotation.eulerAngles.y <= 225))
        {
            wallRotation = 180 - transform.localRotation.eulerAngles.y;
            wallSideIndex = 3;
        }
        else if ((transform.localRotation.eulerAngles.y > 225) && (transform.localRotation.eulerAngles.y <= 270))
        {
            wallRotation = 270 - transform.localRotation.eulerAngles.y;
            wallSideIndex = 4;
        }
        else if ((transform.localRotation.eulerAngles.y > 270) && (transform.localRotation.eulerAngles.y <= 315))
        {
            wallRotation = 270 - transform.localRotation.eulerAngles.y;
            wallSideIndex = 4;
        }
        else
        {
            wallRotation = 360 - transform.localRotation.eulerAngles.y;
            wallSideIndex = 1;
        }

        transform.Rotate(0, wallRotation + adjustmentInDegrees, 0);


        isOnWall = true;
    }

    void SetVisualsOfTheClientToTheServer()
    {
        if (isGrounded && walking)
            UpdatePlayerStateServerRpc(PlayerState.isWalking);
        if (isGrounded && running)
            UpdatePlayerStateServerRpc(PlayerState.isRunning);
        if (jumping && !falling)
            UpdatePlayerStateServerRpc(PlayerState.isJumping);
        if (falling)
            UpdatePlayerStateServerRpc(PlayerState.isFalling);
        if (isGrounded && !walking && !running)
            UpdatePlayerStateServerRpc(PlayerState.isLanding);
        if (isGrounded && !walking && !running)
            UpdatePlayerStateServerRpc(PlayerState.isIdle);
        if (climbingWall)
            UpdatePlayerStateServerRpc(PlayerState.isClimbingWall);
        if (isOnWall && !climbingWall)
            UpdatePlayerStateServerRpc(PlayerState.isOnWall);
        if (fallingFromTheWall)
            UpdatePlayerStateServerRpc(PlayerState.isFallingOfTheWall);
        if (climbUpTheWall)
            UpdatePlayerStateServerRpc(PlayerState.isClimbingOnTopOfTheWall);
    }

    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState newState)
    {
        networkPlayerState.Value = newState;
    }

    void ClientVisuals()
    {
        if (networkPlayerState.Value == PlayerState.isWalking)
        {
            m_Animator.SetBool("isWalking", true);
            m_Animator.SetBool("isRunning", false);
            m_Animator.SetBool("isFalling", false);
            m_Animator.SetBool("isGrounded", true);
            m_Animator.SetBool("isFallingOfTheWall", false);
            m_Animator.SetBool("isOnWall", false);
            m_Animator.SetBool("isWallClimbing", false);
        }
        else if (networkPlayerState.Value == PlayerState.isRunning)
        {
            m_Animator.SetBool("isRunning", true);
            m_Animator.SetBool("isWalking", false);
            m_Animator.SetBool("isFalling", false);
            m_Animator.SetBool("isGrounded", true);
            m_Animator.SetBool("isFallingOfTheWall", false);
            m_Animator.SetBool("isOnWall", false);
            m_Animator.SetBool("isWallClimbing", false);
        }
        else if (networkPlayerState.Value == PlayerState.isIdle)
        {
            m_Animator.SetBool("isRunning", false);
            m_Animator.SetBool("isWalking", false);
            m_Animator.SetBool("isGrounded", true);
            m_Animator.SetBool("isOnWall", false);
            m_Animator.SetBool("isWallClimbing", false);
            m_Animator.SetBool("isFallingOfTheWall", false);
        }
        else if (networkPlayerState.Value == PlayerState.isJumping)
        {
            m_Animator.SetBool("isJumping", true);
            m_Animator.SetBool("isRunning", false);
            m_Animator.SetBool("isWalking", false);
        }
        else if (networkPlayerState.Value == PlayerState.isFalling)
        {
            m_Animator.SetBool("isFalling", true);
            m_Animator.SetBool("isJumping", false);
            m_Animator.SetBool("isGrounded", false);
            m_Animator.SetBool("isOnWall", false);
            m_Animator.SetBool("isWallClimbing", false);
        }
        else if (networkPlayerState.Value == PlayerState.isLanding)
        {
            m_Animator.SetBool("isGrounded", true);
            m_Animator.SetBool("isFalling", false);
        }
        else if (networkPlayerState.Value == PlayerState.isOnWall)
        {
            m_Animator.SetBool("isOnWall", true);
            m_Animator.SetBool("isWallClimbing", false);
            m_Animator.SetBool("isGrounded", false);
            m_Animator.SetBool("isRunning", false);
            m_Animator.SetBool("isWalking", false);
            m_Animator.SetBool("isJumping", false);
            m_Animator.SetBool("isFalling", false);
        }
        else if (networkPlayerState.Value == PlayerState.isClimbingWall)
        {
            m_Animator.SetBool("isOnWall", true);
            m_Animator.SetBool("isWallClimbing", true);
            m_Animator.SetBool("isRunning", false);
            m_Animator.SetBool("isWalking", false);
            m_Animator.SetBool("isClimbingOnTopOfTheWall", false);
        }
        else if (networkPlayerState.Value == PlayerState.isFallingOfTheWall)
        {
            m_Animator.SetBool("isFallingOfTheWall", true);
        }
        else if (networkPlayerState.Value == PlayerState.isClimbingOnTopOfTheWall)
        {
            m_Animator.SetBool("isClimbingOnTopOfTheWall", true);
        }
    }

    public void Jump()
    {
        if (!isGrounded || !canJump)
            return;

        velocity.y = Mathf.Sqrt(-jumpHeight * gravity);
        jumping = true;
        isJumping = true;
    }

    void StartJumpCD()
    {
        StartCoroutine(StartCooldown(jumpCooldown));
    }

    public IEnumerator StartCooldown(float cooldown)
    {
        canJump = false;
        yield return new WaitForSeconds(cooldown);
        canJump = true;
    }

    void sprint()
    {
        if (Input.GetKeyDown("left shift") && isGrounded)
        {
            enable_sprint();
        }
        if (Input.GetKeyUp("left shift"))
        { 
            disable_sprint();
        }
    }

    public void enable_sprint()
    {
        if (!isGrounded)
            return;
        running = true;
    }

    public void disable_sprint()
    {
        running = false;
    }

    void currentSpeed()
    {
        if (running)
            speed = runningSpeed;
        else
            speed = walkingSpeed;
    }
}