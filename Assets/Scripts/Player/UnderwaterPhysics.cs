using Mirror;
using UnityEngine;

public class UnderwaterPhysics : NetworkBehaviour
{
    [Header("Threshold")]
    public float underwaterY = -1000f;

    [Header("Underwater Modifiers")]
    [Range(0f, 1f)]
    public float speedMultiplier = 0.5f;
    public float underwaterDrag = 4f;
    public float underwaterAngularDrag = 2f;

    [Header("Transition")]
    public float dragTransitionSpeed = 3f;

    private FPSController fpsController;
    private Rigidbody rb;

    // Su altına girerken anlık değerleri kayıt eder
    private float savedWalkSpeed;
    private float savedRunSpeed;
    private float savedJumpForce;
    private float originalDrag;
    private float originalAngularDrag;

    private bool  isUnderwater;
    private float targetDrag;
    private float targetAngularDrag;

    private void Start()
    {
        if (!isLocalPlayer) return;

        fpsController = GetComponent<FPSController>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            originalDrag        = rb.linearDamping;
            originalAngularDrag = rb.angularDamping;
        }

        targetDrag        = originalDrag;
        targetAngularDrag = originalAngularDrag;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        bool shouldBeUnderwater = transform.position.y < underwaterY;

        if (shouldBeUnderwater != isUnderwater)
        {
            isUnderwater = shouldBeUnderwater;

            if (fpsController != null)
            {
                if (isUnderwater)
                {
                    // Mevcut hızları kayıt et, sonra %50 düşür
                    savedWalkSpeed = fpsController.walkSpeed;
                    savedRunSpeed  = fpsController.runSpeed;
                    savedJumpForce = fpsController.jumpForce;

                    fpsController.walkSpeed  = savedWalkSpeed * speedMultiplier;
                    fpsController.runSpeed   = savedRunSpeed  * speedMultiplier;
                    fpsController.jumpForce  = savedJumpForce * speedMultiplier;
                    fpsController.isSwimming = true;
                }
                else
                {
                    fpsController.walkSpeed  = savedWalkSpeed;
                    fpsController.runSpeed   = savedRunSpeed;
                    fpsController.jumpForce  = savedJumpForce;
                    fpsController.isSwimming = false;
                }
            }

            if (rb != null)
                rb.useGravity = !isUnderwater;

            targetDrag        = isUnderwater ? underwaterDrag        : originalDrag;
            targetAngularDrag = isUnderwater ? underwaterAngularDrag : originalAngularDrag;
        }

        // Smooth drag geçişi
        if (rb != null)
        {
            rb.linearDamping  = Mathf.Lerp(rb.linearDamping,  targetDrag,        Time.deltaTime * dragTransitionSpeed);
            rb.angularDamping = Mathf.Lerp(rb.angularDamping, targetAngularDrag, Time.deltaTime * dragTransitionSpeed);
        }
    }

    private void OnDestroy()
    {
        if (!isLocalPlayer) return;

        if (fpsController != null && isUnderwater)
        {
            fpsController.walkSpeed  = savedWalkSpeed;
            fpsController.runSpeed   = savedRunSpeed;
            fpsController.jumpForce  = savedJumpForce;
            fpsController.isSwimming = false;
        }

        if (rb != null)
        {
            rb.useGravity     = true;
            rb.linearDamping  = originalDrag;
            rb.angularDamping = originalAngularDrag;
        }
    }
}
