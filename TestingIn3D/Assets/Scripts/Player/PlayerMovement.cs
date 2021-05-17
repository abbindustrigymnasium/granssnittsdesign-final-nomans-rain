using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [TextArea]
    public string Notes = "";

    public Rigidbody rb;

    [Header("movement")]
    [SerializeField]
    private float walkSpeed = 3f;
    [SerializeField]
    private float sprintSpeed = 6f;
    [SerializeField]
    private float moveSmoothDampening = 0.1f;

    public Vector3 moveDir;
    private Vector3 targetMoveAmount;
    private Vector3 moveAmount;
    private Vector3 smoothMoveVelocity;

    [Header("jumping")]
    [SerializeField]
    private float jumpForce = 200f;
    [SerializeField]
    private float minJumpTime = 0.075f;
    [SerializeField]
    private float jumpBeforeGroundDelay = 0.16f;
    [SerializeField]
    private float jumpAfterFallingDelay = 0.07f;

    private float timerForJumpBeforeGround = 0.0f;
    private float timerForJumpAfterFalling = 0.0f;
    private bool wantsToJump = false;
    private float airTime;

    [Header("grounded")]
    [SerializeField]
    private float rayDist = 0.11f;
    [SerializeField]
    private LayerMask groundedMask;
    public bool isGrounded;
    public bool isJumping;
    public bool canMove;
    public GameObject playerModel;
    Animator playerAnimator;
    private DashBar dashBarScript;

    private PlayerStats playerStats;

    [Header("camera")]
    CinemachineFreeLook vcam;
    [SerializeField]
    private float minFov = 42.5f;
    private float currentFov = 42.5f;
    [SerializeField]
    private float maxFov = 55f;
    [SerializeField]
    private float fovSleerpDuration = 1f;
    private float fovSleerp;

    private float currentXSpeed;
    private float currentYSpeed;
    [SerializeField]
    private float maxXSpeed;
    [SerializeField]
    private float maxYSpeed;
    [SerializeField]
    private float minXSpeed;
    [SerializeField]
    private float minYSpeed;

    [Header("dashes")]
    [SerializeField]
    private DashState dashState;
    private enum DashState
    {
        Ready,
        Dashing,
        Cooldown
    }
    [SerializeField]
    private float dashSpeed = 12.00f;

    [SerializeField]
    private float dashTimeLength = 0.285f;
    [SerializeField]
    private float dashTimeCooldown = 0.185f;
    
    private float dashTimer = 0.0f;
    private PlayerGravity playerGravity;
    public float increasedGravityWhileDashing = 2f;
    private bool wantsToDash = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        dashBarScript = GameObject.Find("Dash Bar").GetComponent<DashBar>();
        canMove = true;
        playerAnimator = playerModel.GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        playerStats = GetComponent<PlayerStats>();

        vcam = GameObject.Find("PlayerCam").GetComponent<CinemachineFreeLook>();
        vcam.m_CommonLens = true;

        dashTimer = dashTimeCooldown;
        dashState = DashState.Cooldown;

        playerGravity = GetComponent<PlayerGravity>();
    }

    void Update()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        targetMoveAmount = moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed) * playerStats.Movementspeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, moveSmoothDampening);

        wantsToDash = Input.GetKeyDown(KeyCode.LeftControl);

        isGrounded = Grounded();
        Jump();
        playerAnimator.SetBool("IsRunning", canMove && moveAmount != Vector3.zero);
        CameraFov();
    }

    private bool Grounded()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        Debug.DrawRay(transform.position, -transform.up * rayDist, Color.red);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDist, groundedMask))
        {
            airTime = 0.0f;
            timerForJumpAfterFalling = 0.0f;
            return true;
        }
        else
        {
            airTime += Time.deltaTime;

            if (!isJumping)
            {
                timerForJumpAfterFalling += Time.deltaTime;
            }
            return false;
        }
    }

    private void Jump()
    {
        timerForJumpBeforeGround += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            timerForJumpBeforeGround = 0.0f;
            wantsToJump = true;
        }
        if (timerForJumpBeforeGround >= jumpBeforeGroundDelay)
        {
            wantsToJump = false;
        }

        if ((isGrounded && wantsToJump) || ((timerForJumpAfterFalling < jumpAfterFallingDelay) && Input.GetKeyDown(KeyCode.Space)) || (Input.GetKeyDown(KeyCode.Space) && isGrounded))
        {
            timerForJumpAfterFalling = jumpAfterFallingDelay;
            airTime = 0.0f;
            rb.AddForce(transform.up * jumpForce * playerStats.JumpHeight);
            isJumping = true;
        }

        if ((isJumping && Vector3.Dot(rb.velocity.normalized, transform.up.normalized) < 0.0f) || (!Input.GetKey(KeyCode.Space) && isJumping && (airTime >= minJumpTime)))
        {
            Debug.Log((isJumping && Vector3.Dot(rb.velocity.normalized, transform.up.normalized) < 0.0f));
            Debug.Log(!Input.GetKey(KeyCode.Space) && isJumping && (airTime >= minJumpTime));
            rb.velocity = rb.velocity - Vector3.Project(rb.velocity, transform.up);
            isJumping = false;
        }
    }

    private void Dash()
    {
        /*
        when dashing:
            apply consistant force forward for 2 seconds
            check for ground collision:
                if ground: be affected hard by gravity
                if no ground: be affected by gravity slightly
            camera movement should affect where your going
            movement should affect where your going slightly, except for when you go backwards then you should turn 180deg
        */

        /*
        dashCooldown -= Time.deltaTime;
        dashBarScript.SetMaxDash(5f * playerStats.DashCooldown);
        if (dashBarScript.slider.value <= dashBarScript.slider.maxValue)
        {
            dashBarScript.SetDash(dashBarScript.slider.maxValue - dashCooldown);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && dashCooldown <= 0)
        {
            if (moveAmount == Vector3.zero)
            {
                rb.velocity = transform.forward * 2.5f;
            }
            else
            {
                rb.velocity = (transform.forward * Input.GetAxisRaw("Vertical") * 2.5f + transform.right * Input.GetAxisRaw("Horizontal") * 2.5f) / (1 + 0.5f * Mathf.Abs(Input.GetAxisRaw("Vertical") + 0.5f * Mathf.Abs(Input.GetAxisRaw("Vertical"))));
            }
            dashCooldown = 5f * playerStats.DashCooldown;
        }
        */
        switch (dashState)
        {
            case DashState.Ready:
                if (wantsToDash)
                {
                    canMove = false;
                    wantsToDash = false;
                    isJumping = false;
                    dashTimer = 0.0f;
                    playerGravity.gravity = playerGravity.gravityConstForPlayer * increasedGravityWhileDashing;
                    dashBarScript.SetMaxDash(dashTimeLength);
                    dashState = DashState.Dashing;
                }
                break;

            case DashState.Dashing:
                dashTimer += Time.fixedDeltaTime;
                if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0){ 
                    rb.MovePosition(rb.position + (transform.forward * Input.GetAxisRaw("Vertical") * 2.5f + transform.right * Input.GetAxisRaw("Horizontal") * 2.5f) / (1 + 0.5f * Mathf.Abs(Input.GetAxisRaw("Vertical") + 0.5f * Mathf.Abs(Input.GetAxisRaw("Vertical")))) * dashSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    rb.MovePosition(rb.position + transform.forward * dashSpeed * Time.fixedDeltaTime);
                }
                dashBarScript.SetDash(dashTimeLength - dashTimer);

                if (dashTimer >= dashTimeLength)
                {
                    canMove = true;
                    rb.velocity = Vector3.zero;
                    playerGravity.gravity = playerGravity.gravityConstForPlayer;
                    dashTimer = 0.0f;
                    dashState = DashState.Cooldown;
                }
                break;

            case DashState.Cooldown:
                dashTimer += Time.fixedDeltaTime;

                dashBarScript.SetMaxDash(dashTimeCooldown * playerStats.DashCooldown);
                dashBarScript.SetDash(dashTimer);

                if (dashTimer >= dashTimeCooldown * playerStats.DashCooldown)
                {
                    dashTimer = 0f;
                    dashState = DashState.Ready;
                }
                break;
        }
    }

    private void CameraFov()
    {
        if (Input.GetKey(KeyCode.LeftShift) && moveDir.sqrMagnitude > 0f || dashState == DashState.Dashing)
        {
            fovSleerp = Mathf.Clamp01(fovSleerp + Time.deltaTime / fovSleerpDuration);
        }
        else if (currentFov > minFov)
        {
            fovSleerp = Mathf.Clamp01(fovSleerp - Time.deltaTime / fovSleerpDuration);
        }
        currentFov = Mathf.Lerp(minFov, maxFov, fovSleerp);
        currentXSpeed = Mathf.Lerp(maxXSpeed, minXSpeed, fovSleerp);
        currentYSpeed = Mathf.Lerp(maxYSpeed, minYSpeed, fovSleerp);
        vcam.m_XAxis.m_MaxSpeed = currentXSpeed;
        vcam.m_YAxis.m_MaxSpeed = currentYSpeed;
        vcam.m_Lens.FieldOfView = currentFov;
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
        }
        Dash();
    }
}
