using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [TextArea]
    public string Notes = "";

    // Bry dig om allt i denna kod tbh
    public Rigidbody rb;

    [Header("Forces walk")]
    [SerializeField]
    private float walkSpeed = 3f;
    [SerializeField]
    private float sprintSpeed = 6f;
    [SerializeField]
    private float moveSmoothDampening = 0.1f;

    private Vector3 moveDir;
    private Vector3 targetMoveAmount;
    private Vector3 moveAmount;
    private Vector3 smoothMoveVelocity;

    [Header("Forces jump")]
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

    [SerializeField]
    private float rayDist = 0.11f;
    [SerializeField]
    private LayerMask groundedMask;
    private bool isGrounded;
    private bool isJumping;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        targetMoveAmount = moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed);
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, moveSmoothDampening);

        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDist, groundedMask))
        {
            isGrounded = true;
            airTime = 0.0f;
            timerForJumpAfterFalling = 0.0f;
        }
        else
        {
            isGrounded = false;
            airTime += Time.deltaTime;

            if (!isJumping)
            {
                timerForJumpAfterFalling += Time.deltaTime;
            }
        }

        timerForJumpBeforeGround += Time.deltaTime;
        if (Input.GetButtonDown("Jump"))
        {
            timerForJumpBeforeGround = 0.0f;
            wantsToJump = true;
        }
        if (timerForJumpBeforeGround >= jumpBeforeGroundDelay)
        {
            wantsToJump = false;
        }

        if (!isJumping && ((isGrounded && wantsToJump) || ((timerForJumpAfterFalling < jumpAfterFallingDelay) && Input.GetButtonDown("Jump")) || (Input.GetButtonDown("Jump") && isGrounded)))
        {
            timerForJumpAfterFalling = jumpAfterFallingDelay;
            isJumping = true;
            airTime = 0.0f;
            rb.AddForce(transform.up * jumpForce);

            Debug.Log("jumped");
        }

        if (isJumping && (Vector3.Dot(rb.velocity.normalized, transform.up.normalized) <= 0.0f || !Input.GetButton("Jump")) && airTime >= minJumpTime)
        {
            rb.velocity = rb.velocity - Vector3.Project(rb.velocity, transform.up);
            isJumping = false;
            Debug.Log("stopped jumping");
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }
}
