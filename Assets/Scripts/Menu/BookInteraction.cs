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
        if (p != null)
        {
            player = p.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
        }
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

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        playerMovement = player.GetComponent<PlayerMovement>();
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
        if (canvas == null) return;

        // Guardamos la transformación actual del canvas en el mundo
        savedPosition = canvas.transform.position;
        savedRotation = canvas.transform.rotation;
        savedScale = canvas.transform.localScale;

        inBookView = true;

        // Aseguramos que la escala no esté invertida antes del Overlay
        Vector3 fixedScale = canvas.transform.localScale;
        fixedScale.x = Mathf.Abs(fixedScale.x);
        fixedScale.y = Mathf.Abs(fixedScale.y);
        fixedScale.z = Mathf.Abs(fixedScale.z);
        canvas.transform.localScale = fixedScale;

        // Reiniciamos la rotación para evitar reflejos en pantalla
        canvas.transform.rotation = Quaternion.identity;

        // Cambiar a Overlay
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;

        // Mostrar cursor y bloquear movimiento
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (playerMovement != null)
            playerMovement.enabled = false;

        Debug.Log("Vista del libro activada (modo Overlay, rotación previa aplicada).");
    }

    void DeactivateBookView()
    {
        if (canvas == null) return;

        inBookView = false;

        // Restaurar modo World Space
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = originalCamera;

        // Restaurar posición, rotación y escala exactas
        canvas.transform.position = savedPosition;
        canvas.transform.rotation = savedRotation;
        canvas.transform.localScale = savedScale;

        // Ocultar cursor y restaurar movimiento
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerMovement != null)
            playerMovement.enabled = true;

        Debug.Log("Vista del libro cerrada (volvió al mundo con su posición original).");
    }
}