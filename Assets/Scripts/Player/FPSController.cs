using UnityEngine;
using Mirror;

public class FPSController : NetworkBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 5f;

    [Header("Mouse")]
    public float mouseSensitivity = 100f;
    public Camera playerCamera;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [HideInInspector] public bool isSwimming = false;

    [Header("Weight Penalty")]
    [Tooltip("Her eşya için hız çarpanı. 0.15 = her eşya %~13 daha yavaş")]
    public float weightFactor = 0.15f;
    [Tooltip("Maksimum yavaşlama: hız asla bu çarpanın altına inmez (0.2 = %20 minimum)")]
    public float minSpeedMultiplier = 0.2f;

    private float baseWalkSpeed;
    private float baseRunSpeed;
    private float xRotation = 0f;
    private Rigidbody rb;
    private bool isGrounded;

    public bool IsGrounded => isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;
    }

    void Start()
    {
        if (!isLocalPlayer)
        {
            playerCamera.gameObject.SetActive(false);
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (MarketUI.IsOpen) return; // Market açıkken hareket ve kamera dondurulur

        CheckGround();
        HandleMovement();
        HandleMouseLook();
        HandleJump();
    }

    void CheckGround()
    {
        Vector3 checkPos = groundCheck != null
            ? groundCheck.position
            : transform.position + Vector3.down * 0.9f;

        // Oyuncunun kendi layer'ını maskeden çıkar — kendi collideri algılanmasın
        int mask = groundMask == 0
            ? ~(1 << gameObject.layer)
            : groundMask & ~(1 << gameObject.layer);

        isGrounded = Physics.CheckSphere(checkPos, groundDistance, mask);
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move;
        if (isSwimming)
            move = playerCamera.transform.right * x + playerCamera.transform.forward * z;
        else
            move = transform.right * x + transform.forward * z;

        Vector3 newPosition = rb.position + move * currentSpeed * Time.deltaTime;
        rb.MovePosition(newPosition);
    }

    void HandleJump()
    {
        if (isSwimming) return;
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, mouseX, 0f));
    }

    // PlayerInventory tarafından her eşya alımında çağrılır.
    // Formül: hız = baseHız / (1 + eşyaSayısı * weightFactor)
    // Örn: 0 eşya = %100, 3 eşya = %69, 6 eşya = %53, sonsuz → %0'a yaklaşır ama minSpeedMultiplier ile sabitlenir
    // Upgrade: base hızları kalıcı olarak artırır, mevcut ağırlık cezasını yeniden uygular
    public void ApplySpeedUpgrade(float bonus)
    {
        baseWalkSpeed += bonus;
        baseRunSpeed += bonus;
        PlayerInventory inv = GetComponent<PlayerInventory>();
        ApplyWeightPenalty(inv != null ? inv.toplananEsya : 0);
    }

    public void ApplyJumpUpgrade(float bonus)
    {
        jumpForce += bonus;
    }

    public void ApplyWeightPenalty(int itemCount)
    {
        float multiplier = 1f / (1f + itemCount * weightFactor);
        multiplier = Mathf.Max(multiplier, minSpeedMultiplier);

        walkSpeed = baseWalkSpeed * multiplier;
        runSpeed = baseRunSpeed * multiplier;
    }
}
