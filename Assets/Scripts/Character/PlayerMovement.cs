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

    void Awake()
    {
        controls = new PlayerControls();

        // Movimiento
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // Correr
        controls.Player.Run.performed += ctx => isRunning = true;
        controls.Player.Run.canceled += ctx => isRunning = false;

        // Saltar
        controls.Player.Jump.performed += ctx => Jump();
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

        // --- Movimiento relativo a la cámara ---
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        // Eliminamos inclinación vertical de la cámara
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camRight * moveInput.x + camForward * moveInput.y;

        // --- Rotación del personaje ---
        if (move.magnitude >= 0.1f)
        {
            // Calculamos el ángulo de rotación
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            // Suavizamos la rotación
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // --- Movimiento ---
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        controller.Move(move.normalized * currentSpeed * Time.deltaTime);

        // --- Animaciones ---
        float speedPercent = move.magnitude * (isRunning ? 1f : 0.5f);
        animator.SetFloat("Speed", speedPercent);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsRunning", isRunning);

        // --- Gravedad ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Escoge una animación de salto aleatoria
            int jumpType = Random.Range(0, 2); // 0 o 1
            if (jumpType == 0)
                animator.SetTrigger("Jump1");
            else
                animator.SetTrigger("Jump2");
        }
    }
}
