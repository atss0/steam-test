using UnityEngine;
using Mirror;

public class PlayerAnimator : NetworkBehaviour
{
    static readonly int SpeedHash         = Animator.StringToHash("Speed");
    static readonly int IsJumpingHash     = Animator.StringToHash("IsJumping");
    static readonly int PushHash          = Animator.StringToHash("Push");
    static readonly int MoveDirectionHash = Animator.StringToHash("MoveDirection");

    [SerializeField] private float animationSpeedMultiplier = 1.5f;

    private Animator animator;
    private FPSController fpsController;

    void Start()
    {
        animator      = GetComponentInChildren<Animator>();
        fpsController = GetComponent<FPSController>();

        if (isLocalPlayer)
            SetModelVisible(false);
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        UpdateMovementAnimation();
        UpdateJumpAnimation();
    }

    void UpdateMovementAnimation()
    {
        if (animator == null || fpsController == null) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float inputMag = Mathf.Clamp01(new Vector2(h, v).magnitude);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = inputMag * (isRunning ? fpsController.runSpeed : fpsController.walkSpeed);

        float direction = v < -0.1f ? -1f : 1f;

        animator.SetFloat(SpeedHash, targetSpeed, 0.05f, Time.deltaTime);
        animator.SetFloat(MoveDirectionHash, direction * animationSpeedMultiplier, 0.05f, Time.deltaTime);
    }

    void UpdateJumpAnimation()
    {
        if (animator == null || fpsController == null) return;
        animator.SetBool(IsJumpingHash, !fpsController.IsGrounded);
    }

    public void TriggerButtonPush()
    {
        if (animator != null)
            animator.SetTrigger(PushHash);
    }

    public void SetModelVisible(bool visible)
    {
        foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>())
            smr.enabled = visible;
    }
}
