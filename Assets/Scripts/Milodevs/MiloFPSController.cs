using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MiloFPSController : MonoBehaviour
{
    [Header("Camera Settings")] 
    public Transform CameraPivot;
    public Vector2 CameraMinMax = new Vector2(-80f, 80f);
    public Vector2 MouseSensitivity = new Vector2(2f, 2f);
    
    [Header("Movement Settings")]
    public float WalkSpeed = 5f;
    public float RunSpeed = 10f;
    public float CrouchSpeed = 2.5f;
    public float JumpForce = 5f;
    public float Gravity = -9.81f;
    public float SlopeForce = 3f;
    public float SlopeForceRayLength = 1.5f;

    [Header("Head Bob Settings")]
    public float BobSpeed = 14f;
    public float BobAmount = 0.05f;
    public float BobSmoothing = 10f;

    [Header("FOV Settings")]
    public float DefaultFOV = 60f;
    public float RunFOV = 65f;
    public float FOVChangeSpeed = 4f;

    [Header("Jump and Land Settings")]
    public float JumpRecoil = 0.1f;
    public float LandRecoil = 0.2f;
    public float RecoilRecoverySpeed = 5f;

    private CharacterController controller;
    private Camera playerCamera;
    private float speed;
    private float horizontal;
    private float vertical;
    private Vector3 movement;
    private float velocityY;
    private float mouseX;
    private float mouseY;
    private float currentCameraRotationX = 0f;

    private float timer;
    private Vector3 bobOffset = Vector3.zero;
    private Vector3 originalCameraPosition;
    private float currentFOV;
    private float targetFOV;
    private bool wasGrounded;
    private Vector3 recoilOffset = Vector3.zero;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = CameraPivot.GetComponentInChildren<Camera>();
        originalCameraPosition = playerCamera.transform.localPosition;
        currentFOV = DefaultFOV;
        targetFOV = DefaultFOV;
        Cursor.lockState = CursorLockMode.Locked;
        wasGrounded = true;
    }

    void Update()
    {
        GetInputs();
        MovePlayer();
        ApplyRotations();
        ApplyHeadBob();
        UpdateFOV();
        ApplyJumpAndLandEffects();
    }

    void GetInputs()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        mouseX = Input.GetAxisRaw("Mouse X") * MouseSensitivity.x;
        mouseY = Input.GetAxisRaw("Mouse Y") * MouseSensitivity.y;
        
        speed = Input.GetKey(KeyCode.LeftShift) ? RunSpeed : WalkSpeed;
        speed = Input.GetKey(KeyCode.LeftControl) ? CrouchSpeed : speed;

        movement = transform.right * horizontal + transform.forward * vertical;

        targetFOV = Input.GetKey(KeyCode.LeftShift) ? RunFOV : DefaultFOV;
    }

    void MovePlayer()
    {
        if (controller.isGrounded && velocityY < 0)
        {
            velocityY = -2f;
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocityY = JumpForce;
        }

        velocityY += Gravity * Time.deltaTime;
        Vector3 moveVector = movement * speed * Time.deltaTime;
        moveVector.y = velocityY * Time.deltaTime;

        if (OnSlope())
        {
            moveVector += Vector3.down * controller.height / 2 * SlopeForce;
        }

        controller.Move(moveVector);
    }

    void ApplyRotations()
    {
        transform.Rotate(Vector3.up * mouseX);
        currentCameraRotationX -= mouseY;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, CameraMinMax.x, CameraMinMax.y);
        CameraPivot.localRotation = Quaternion.Euler(currentCameraRotationX, 0f, 0f);
    }

    void ApplyHeadBob()
    {
        if (movement.magnitude > 0.1f && controller.isGrounded)
        {
            timer += Time.deltaTime * BobSpeed;
            bobOffset = new Vector3(
                Mathf.Cos(timer) * BobAmount,
                Mathf.Sin(timer * 2) * BobAmount,
                0f
            );
        }
        else
        {
            timer = 0f;
            bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * BobSmoothing);
        }

        Vector3 targetPosition = originalCameraPosition + bobOffset + recoilOffset;
        playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetPosition, Time.deltaTime * BobSmoothing);
    }

    void UpdateFOV()
    {
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * FOVChangeSpeed);
        playerCamera.fieldOfView = currentFOV;
    }

    void ApplyJumpAndLandEffects()
    {
        if (controller.isGrounded != wasGrounded)
        {
            if (controller.isGrounded)
            {
                // Landed
                recoilOffset = new Vector3(0f, -LandRecoil, 0f);
            }
            else
            {
                // Jumped
                recoilOffset = new Vector3(0f, JumpRecoil, 0f);
            }
        }

        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * RecoilRecoverySpeed);
        wasGrounded = controller.isGrounded;
    }

    private bool OnSlope()
    {
        if (!controller.isGrounded) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, controller.height / 2 * SlopeForceRayLength))
        {
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }
}