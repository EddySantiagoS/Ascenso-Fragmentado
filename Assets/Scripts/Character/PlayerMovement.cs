using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

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
    public bool AllowMovement = true; // bloquea el movimiento hasta selección

    [Header("Escalada")]
    public float climbCheckDistance = 1f;
    public float climbSpeed = 2f;
    public bool isClimbing = false;
    public LayerMask climbableLayer;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina;
    public float staminaDrainRun = 10f;   // por segundo
    public float staminaDrainJump = 20f;  // por salto
    public float staminaDrainClimb = 15f; // por segundo
    public float staminaRegenRate = 8f;   // por segundo
    public bool canUseStamina = true;
    public Slider staminaBar;
    public GameObject staminaUI;

    [Header("Caída")]
    public float fallHeightThreshold = 2f; // Altura mínima para mostrar animación de caída
    private float fallStartY;
    private bool isFallingAnimActive = false;

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
        stamina = maxStamina;

        if (staminaUI != null)
        staminaUI.SetActive(false);
    }

    void Update()
    {
        if (controller == null || !controller.enabled) return;
        // --- actualizaciones básicas ---
        isGrounded = controller.isGrounded;

        // gravedad y reset rápido al tocar suelo
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // --- DETECCIÓN DE CAÍDA PROLONGADA (solo una vez, no duplicada) ---
        // Cuando empieza a bajar (velocidad negativa) y no estamos escalando, marcamos inicio de posible caída
        if (!isGrounded && !isClimbing && velocity.y < -1f)
        {
            if (!isFallingAnimActive)
            {
                fallStartY = transform.position.y;   // altura inicial de la caída
                isFallingAnimActive = true;
            }

            // ahora calculamos distancia de caída
            float fallDistance = fallStartY - transform.position.y;

            if (fallDistance > fallHeightThreshold)
            {
                // sólo activamos la animación si superó el umbral de altura
                animator.SetBool("IsFalling", true);
            }
            else
            {
                animator.SetBool("IsFalling", false);
            }
        }
        else
        {
            // si tocamos el suelo o estamos escalando, desactivamos la animación y reseteamos
            if (isGrounded && animator.GetBool("IsFalling"))
            {
                animator.SetBool("IsFalling", false);
                // opcional: animator.SetTrigger("Land");
            }

            isFallingAnimActive = false;
            // NOTA: no rompemos otras animaciones aquí; sólo reseteamos la bandera.
        }

        // --- Si movimiento deshabilitado, salir (mantener idle) ---
        if (!AllowMovement)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsRunning", false);
            return;
        }

        // --- Input & movimiento (idéntico a tu implementación) ---
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        Vector3 move = camRight * moveInput.x + camForward * moveInput.y;

        if (move.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        float currentSpeed = (isRunning && canUseStamina) ? runSpeed : walkSpeed;
        controller.Move(move.normalized * currentSpeed * Time.deltaTime);

        animator.SetFloat("Speed", move.magnitude * (isRunning ? 1f : 0.5f));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsRunning", isRunning);

        // --- Escalada existente (sin cambios en lógica) ---
        if (isClimbing)
        {
            HandleClimb();
            HandleStamina();
            return;
        }

        // Aplicar gravedad vertical
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        HandleStamina();
    }

    void HandleClimb()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 1f;

        // Comprobamos si todavía hay superficie escalable enfrente
        if (!Physics.Raycast(origin, transform.forward, out hit, climbCheckDistance, climbableLayer))
        {
            // 🔹 Si no hay pared, verificamos si está en el borde superior
            if (CheckForLedge())
            {
                StartCoroutine(ClimbLedge()); // hace la subida final
            }
            else
            {
                StopClimb();
            }
            return;
        }

        bool pressingForward = moveInput.y > 0.5f;
        bool pressingCtrl = Keyboard.current.leftCtrlKey.isPressed;

        // Si se quedó sin stamina → se suelta
        if (!canUseStamina)
        {
            StopClimb();
            return;
        }

        // Movimiento de escalada
        if (pressingForward && pressingCtrl)
        {
            controller.Move(Vector3.up * climbSpeed * Time.deltaTime);
        }
        else
        {
            // Quieto en la pared (sin moverse)
            controller.Move(Vector3.zero);
        }
    }

    bool CheckForLedge()
    {
        // Raycast hacia arriba para ver si hay suelo encima del jugador
        RaycastHit topHit;
        Vector3 checkOrigin = transform.position + Vector3.up * 1.5f;

        // Si hay suelo cerca arriba, significa que estamos tocando la cima
        return Physics.Raycast(checkOrigin, Vector3.up, out topHit, 1f, climbableLayer) == false;
    }

    IEnumerator ClimbLedge()
    {
        // Evitamos repetir
        if (!isClimbing) yield break;

        isClimbing = false;
        animator.SetBool("IsClimbing", false);

        // Movimiento suave hacia arriba (simula subir al borde)
        float climbUpTime = 0.5f;
        float elapsed = 0f;

        while (elapsed < climbUpTime)
        {
            controller.Move(Vector3.up * (climbSpeed * 1.5f) * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Posicionar al jugador justo sobre el borde
        controller.Move(Vector3.forward * 0.3f); // un empujoncito hacia adelante
        velocity = Vector3.zero;

        // Termina en idle
        animator.SetBool("IsFalling", false);
        animator.SetBool("IsGrounded", true);
    }

    void Jump()
    {
        if (!AllowMovement || !canUseStamina) return; // Bloqueamos salto si no se ha seleccionado personaje
        

        if (isGrounded)
        {
            stamina -= staminaDrainJump;
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
        if (!AllowMovement || !canUseStamina) return; // Bloquea escalada sin selección 

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

    void HandleStamina()
    {
        // Si está corriendo
        if (isRunning && moveInput.magnitude > 0.1f)
            stamina -= staminaDrainRun * Time.deltaTime;

        // Si está escalando
        if (isClimbing)
            stamina -= staminaDrainClimb * Time.deltaTime;

        // Si no está haciendo nada exigente, regenera
        if (!isRunning && !isClimbing && controller.isGrounded && moveInput.magnitude < 0.1f)
            stamina += staminaRegenRate * Time.deltaTime;

        // Limitar valores
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        // Control de uso
        canUseStamina = stamina > 0;

        if (staminaBar != null)
            staminaBar.value = stamina / maxStamina;

    }
}
