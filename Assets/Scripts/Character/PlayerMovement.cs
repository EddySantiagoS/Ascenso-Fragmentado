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

    [Header("Escalada")]
    public float climbCheckDistance = 1f;   // Distancia del raycast
    public float climbSpeed = 2f;           // Velocidad de subida
    public bool isClimbing = false;
    public LayerMask climbableLayer;        // Capa de muros escalables

    private float climbReleaseTimer = 0f;
    public float climbReleaseDelay = 0.3f; // medio segundo de tolerancia

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

    
        // Escalar
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

        if (isClimbing)
        {
            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 1f;

            // Si deja de tocar el muro, termina la escalada
            if (!Physics.Raycast(origin, transform.forward, out hit, climbCheckDistance, climbableLayer))
            {
                Debug.Log(" Ya no hay muro al frente. Deteniendo escalada.");

                // 💡 Si todavía no está en el suelo, lo subimos un poco
                if (!controller.isGrounded)
                {
                    controller.Move(Vector3.up * 1f); // lo sube para quedar sobre el borde
                    Debug.Log(" Ajustando posición para quedar sobre el borde");
                }

                StopClimb();
            }
            else
            {
                // Solo sube si sigue presionando las teclas
                bool pressingForward = moveInput.y > 0.5f;
                bool pressingCtrl = Keyboard.current.leftCtrlKey.isPressed;

                if (pressingForward && pressingCtrl)
                {
                    controller.Move(Vector3.up * climbSpeed * Time.deltaTime);
                }
                else
                {
                    Debug.Log(" Se soltó W o Ctrl. Deteniendo escalada.");
                    StopClimb();
                }
            }

            return; // evita que se ejecute el resto del movimiento
        }

        if (!isClimbing)
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        bool wasGrounded = isGrounded;
        // Si está en el aire y cayendo (velocidad negativa)
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

    void TryClimb()
{
    bool pressingForward = moveInput.y > 0.5f;
    bool pressingCtrl = Keyboard.current.leftCtrlKey.isPressed;

    // Permitir escalada incluso si está en el aire
    if (pressingForward && pressingCtrl)
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 1f;

        Debug.DrawRay(origin, transform.forward * climbCheckDistance, Color.red, 1f);

        if (Physics.Raycast(origin, transform.forward, out hit, climbCheckDistance, climbableLayer))
        {
            // Solo si toca una pared escalable
            if (!isClimbing)
            {
                Debug.Log($"🧗‍♂️ Iniciando escalada en {hit.collider.name}");

                // Reiniciamos estados previos
                isClimbing = true;
                velocity = Vector3.zero;
                animator.SetBool("IsFalling", false);
                animator.ResetTrigger("Jump1");
                animator.ResetTrigger("Jump2");
                animator.SetBool("IsClimbing", true);
                animator.Play("Climbing", 0, 0f);
            }
        }
        else
        {
            // En caso de no detectar muro, debug visual
            Debug.Log("🚫 No hay muro escalable al frente.");
        }
    }
}

    void StopClimb()
    {
        Debug.Log("Stop climb llamado");
        if (isClimbing)
        {
            isClimbing = false;
            animator.SetBool("IsClimbing", false);

            // 🔧 Restaurar la velocidad normal del animator
            animator.speed = 1f;

            // 👉 Activar caída si está en el aire
            if (!controller.isGrounded)
            {
                animator.SetBool("IsFalling", true);
            }
        }
    }
}
