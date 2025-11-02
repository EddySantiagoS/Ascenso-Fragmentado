using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isRunning;

    private PlayerControls controls;
    private Vector2 moveInput;

    [Header("Control de Movimiento")]
    public bool AllowMovement = true; // 🔒 Nueva variable: bloquea el movimiento hasta selección

    [Header("Escalada")]
    public float climbCheckDistance = 1f;
    public float climbSpeed = 2f;
    public bool isClimbing = false;
    public LayerMask climbableLayer;

    private float climbReleaseTimer = 0f;
    public float climbReleaseDelay = 0.3f;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Run.performed += ctx => isRunning = true;
        controls.Player.Run.canceled += ctx => isRunning = false;

        controls.Player.Jump.performed += ctx => Jump();

        controls.Player.Climb.performed += ctx => TryClimb();
        controls.Player.Climb.canceled += ctx => StopClimb();
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // ⚠️ Si el movimiento está bloqueado, mantener solo animaciones idle
        if (!AllowMovement)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsRunning", false);
            return;
        }

        // Movimiento relativo a cámara
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camRight * moveInput.x + camForward * moveInput.y;

        // Rotación
        if (move.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Movimiento físico
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        controller.Move(move.normalized * currentSpeed * Time.deltaTime);

        // Animaciones
        float speedPercent = move.magnitude * (isRunning ? 1f : 0.5f);
        animator.SetFloat("Speed", speedPercent);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsRunning", isRunning);

        if (isClimbing)
        {
            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 1f;

            if (!Physics.Raycast(origin, transform.forward, out hit, climbCheckDistance, climbableLayer))
            {
                if (!controller.isGrounded)
                {
                    controller.Move(Vector3.up * 1f);
                }
                StopClimb();
            }
            else
            {
                bool pressingForward = moveInput.y > 0.5f;
                bool pressingCtrl = Keyboard.current.leftCtrlKey.isPressed;

                if (pressingForward && pressingCtrl)
                {
                    controller.Move(Vector3.up * climbSpeed * Time.deltaTime);
                }
                else
                {
                    StopClimb();
                }
            }

            return;
        }

        if (!isClimbing)
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        if (!isGrounded && velocity.y < -1f && !isClimbing)
        {
            animator.SetBool("IsFalling", true);
        }
        else
        {
            animator.SetBool("IsFalling", false);
        }
    }

    void Jump()
    {
        if (!AllowMovement) return; // ❌ Bloqueamos salto si no se ha seleccionado personaje

        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            int jumpType = Random.Range(0, 2);
            if (jumpType == 0)
                animator.SetTrigger("Jump1");
            else
                animator.SetTrigger("Jump2");
        }
    }

    void TryClimb()
    {
        if (!AllowMovement) return; // ❌ Bloquea escalada sin selección

        bool pressingForward = moveInput.y > 0.5f;
        bool pressingCtrl = Keyboard.current.leftCtrlKey.isPressed;

        if (pressingForward && pressingCtrl)
        {
            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 1f;

            if (Physics.Raycast(origin, transform.forward, out hit, climbCheckDistance, climbableLayer))
            {
                if (!isClimbing)
                {
                    isClimbing = true;
                    velocity = Vector3.zero;
                    animator.SetBool("IsFalling", false);
                    animator.ResetTrigger("Jump1");
                    animator.ResetTrigger("Jump2");
                    animator.SetBool("IsClimbing", true);
                    animator.Play("Climbing", 0, 0f);
                }
            }
        }
    }

    void StopClimb()
    {
        if (isClimbing)
        {
            isClimbing = false;
            animator.SetBool("IsClimbing", false);
            animator.speed = 1f;

            if (!controller.isGrounded)
            {
                animator.SetBool("IsFalling", true);
            }
        }
    }
}
