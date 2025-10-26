using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BookInteraction : MonoBehaviour
{
    [Header("UI Holográfica (Canvas en World Space)")]
    public GameObject hologramUI;

    [Header("Configuración")]
    public float activationDistance = 2.5f;
    public string playerTag = "Player";

    private Transform player;
    private PlayerMovement playerMovement;
    private bool isVisible = false;
    private bool inBookView = false;
    private Vector3 originalScale;

    // Guardamos la configuración original del Canvas
    private Canvas canvas;
    private RenderMode originalRenderMode;
    private Camera originalCamera;

    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private Vector3 savedScale;

    void Start()
    {
        if (hologramUI != null)
        {
            canvas = hologramUI.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No se encontró un Canvas en el padre del hologramUI. Asegúrate de que el UI esté dentro de un Canvas.");
                return;
            }

            originalScale = hologramUI.transform.localScale;
            originalRenderMode = canvas.renderMode;
            originalCamera = canvas.worldCamera;

            hologramUI.SetActive(false);
        }

        // Intentar encontrar al jugador
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null || hologramUI == null || canvas == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        // Mostrar/Ocultar holograma por proximidad
        if (!inBookView) // Solo controlar distancia cuando no estamos en modo libro
        {
            if (distance < activationDistance && !isVisible)
                ShowHologram();
            else if (distance >= activationDistance && isVisible)
                HideHologram();
        }

        // --- Entrada de teclado con el nuevo Input System ---
        if (Keyboard.current == null) return;

        // Abrir menú (E)
        if (isVisible && !inBookView && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ActivateBookView();
        }

        // Cerrar menú (Escape)
        if (inBookView && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            DeactivateBookView();
        }
    }

    // Permite asignar el jugador desde otro script (por ejemplo, selección de personaje)
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        Debug.Log($"Jugador asignado manualmente: {newPlayer.name}");
    }

    // --- Control de holograma ---

    void ShowHologram()
    {
        isVisible = true;
        hologramUI.SetActive(true);
        hologramUI.transform.localScale = Vector3.zero;
        StartCoroutine(ScaleIn());
    }

    void HideHologram()
    {
        if (isVisible)
            StartCoroutine(ScaleOut());
    }

    IEnumerator ScaleIn()
    {
        float t = 0f;
        float duration = 0.35f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.SmoothStep(0f, 1f, t / duration);
            hologramUI.transform.localScale = originalScale * scale;
            yield return null;
        }
        hologramUI.transform.localScale = originalScale;
    }

    IEnumerator ScaleOut()
    {
        isVisible = false;
        float t = 0f;
        float duration = 0.25f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.SmoothStep(1f, 0f, t / duration);
            hologramUI.transform.localScale = originalScale * scale;
            yield return null;
        }
        hologramUI.SetActive(false);
    }

    // --- Transición a modo de menú (Overlay) ---

    void ActivateBookView()
    {
        Canvas canvas = hologramUI.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // Guardamos la transformacion actual del canvas en el mundo
        savedPosition = canvas.transform.position;
        savedRotation = canvas.transform.rotation;
        savedScale = canvas.transform.localScale;

        inBookView = true;

        // Cambiar a Overlay
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;

        // Corregir orientación (invertir eje Z)
        Vector3 flip = canvas.transform.localScale;
        flip.z *= -1;
        canvas.transform.localScale = flip;

        // Activar cursor y bloquear control del jugador
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (playerMovement != null)
            playerMovement.enabled = false;

        Debug.Log("Vista del libro activada (modo Overlay).");
    }

    void DeactivateBookView()
    {
        Canvas canvas = hologramUI.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        inBookView = false;

        // Restaurar modo World Space
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = originalCamera;

        // Restaurar posición y orientación
        canvas.transform.position = savedPosition;
        canvas.transform.rotation = savedRotation;
        canvas.transform.localScale = savedScale;

        // Ocultar cursor y devolver control al jugador
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerMovement != null)
            playerMovement.enabled = true;

        Debug.Log("Vista del libro cerrada (volvió al mundo con su posición original).");
    }
}