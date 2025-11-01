using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class EmoteMenuController : MonoBehaviour
{
    [Header("Configuración General")]
    [Tooltip("Panel principal que contiene los botones de emotes.")]
    public GameObject panelEmote;

    [Tooltip("Arreglo de botones que representan cada emote (asignar desde el inspector).")]
    public Button[] botonesEmote;

    [Tooltip("Tiempo mínimo entre scrolls (para evitar saltos rápidos).")]
    public float scrollCooldown = 0.08f;

    [Tooltip("Umbral de movimiento para cancelar el emote (valor de magnitud del input).")]
    public float movementCancelThreshold = 0.15f;

    private int selectedIndex = 0;
    private PlayerControls controls;
    private bool isHoldingKey = false;
    private Sprite[] originalSprites;

    private bool emotePlaying = false;

    private float lastScrollTime = 0f;

    private Animator playerAnimator;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.OpenEmoteMenu.started += ctx => OnEmoteKeyPressed();
        controls.Player.OpenEmoteMenu.canceled += ctx => OnEmoteKeyReleased();

        controls.Player.Scroll.performed += ctx => OnScroll(ctx.ReadValue<Vector2>().y);
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        if (panelEmote != null)
            panelEmote.SetActive(false);

        if (botonesEmote == null || botonesEmote.Length == 0)
        {
            Debug.LogWarning("No hay botones asignados en el EmoteMenuController.");
            return;
        }

        originalSprites = new Sprite[botonesEmote.Length];

        for (int i = 0; i < botonesEmote.Length; i++)
        {
            Image img = botonesEmote[i].GetComponent<Image>();
            originalSprites[i] = img != null ? img.sprite : null;

            int idx = i;
            botonesEmote[i].onClick.RemoveAllListeners();
            botonesEmote[i].onClick.AddListener(() => ExecuteEmote(idx));
        }

        UpdateButtonVisuals();
    }

    private void OnEmoteKeyPressed()
    {
        // NO abrir el menú si NO hay un personaje seleccionado (no hay Player en escena activo)
        GameObject playerCheck = GameObject.FindWithTag("Player");
        if (playerCheck == null)
        {
            Debug.Log("Menú de emotes bloqueado: no hay personaje seleccionado.");
            return;
        }

        // adicional: asegurarnos que el objeto con tag Player esté activo en jerarquía
        if (!playerCheck.activeInHierarchy)
        {
            Debug.Log("Menú de emotes bloqueado: el objeto con tag 'Player' no está activo.");
            return;
        }

        if (panelEmote == null) return;

        panelEmote.SetActive(true);
        isHoldingKey = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        selectedIndex = Mathf.Clamp(selectedIndex, 0, botonesEmote.Length - 1);
        UpdateButtonVisuals();

        Debug.Log("Menú de emotes abierto.");
    }

    private void OnEmoteKeyReleased()
    {
        if (!isHoldingKey) return;
        isHoldingKey = false;

        if (panelEmote != null)
            panelEmote.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // no ejecutar emote si todavía no hay personaje
        GameObject playerCheck = GameObject.FindWithTag("Player");
        if (playerCheck == null || !playerCheck.activeInHierarchy)
        {
            Debug.Log("No hay jugador activo. Menú cerrado, pero no se ejecuta emote.");
            return;
        }

        ExecuteEmote(selectedIndex);
    }

    private void OnScroll(float scrollValue)
    {
        if (!isHoldingKey || botonesEmote == null || botonesEmote.Length == 0)
            return;

        if (Time.unscaledTime - lastScrollTime < scrollCooldown)
            return;

        lastScrollTime = Time.unscaledTime;

        if (scrollValue > 0.1f)
            selectedIndex = (selectedIndex + 1) % botonesEmote.Length;
        else if (scrollValue < -0.1f)
            selectedIndex = (selectedIndex - 1 + botonesEmote.Length) % botonesEmote.Length;

        UpdateButtonVisuals();
    }

    private void UpdateButtonVisuals()
    {
        for (int i = 0; i < botonesEmote.Length; i++)
        {
            Button btn = botonesEmote[i];
            if (btn == null) continue;

            Image img = btn.GetComponent<Image>();
            if (img == null) continue;

            Sprite highlighted = btn.spriteState.highlightedSprite;
            Sprite selectedSprite = btn.spriteState.selectedSprite;
            Sprite pressed = btn.spriteState.pressedSprite;

            if (i == selectedIndex)
            {
                if (selectedSprite != null)
                    img.sprite = selectedSprite;
                else if (highlighted != null)
                    img.sprite = highlighted;
                else if (pressed != null)
                    img.sprite = pressed;
                else
                    img.sprite = originalSprites[i];
            }
            else
            {
                img.sprite = originalSprites[i];
            }
        }
    }

    // ---------- EJECUCIÓN DE EMOTE ----------
    public void ExecuteEmote(int index)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("No se encontró un jugador activo.");
            return;
        }

        playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null)
        {
            Debug.LogWarning("El jugador activo no tiene Animator.");
            return;
        }

        if (index < 0 || index >= botonesEmote.Length)
        {
            Debug.LogWarning("Índice de emote inválido.");
            return;
        }

        Debug.Log($"Ejecutando emote #{index + 1}");

        // cancelar emote anterior (la transición Emote_X -> EmoteIdle depende del bool IsEmoting)
        playerAnimator.SetBool("IsEmoting", false);

        // iniciar nuevo emote
        emotePlaying = true;
        playerAnimator.SetBool("IsEmoting", true);

        string triggerName = $"Emote{index + 1}";
        playerAnimator.ResetTrigger(triggerName);
        playerAnimator.SetTrigger(triggerName);

        StopAllCoroutines();
        StartCoroutine(WaitForMovementToEndEmote());
    }

    IEnumerator WaitForMovementToEndEmote()
    {
        yield return new WaitForSecondsRealtime(0.15f);

        while (true)
        {
            Vector2 move = Vector2.zero;
            if (controls != null)
                move = controls.Player.Move.ReadValue<Vector2>();

            float magnitude = move.magnitude;

            if (magnitude > movementCancelThreshold)
            {
                if (playerAnimator != null)
                    playerAnimator.SetBool("IsEmoting", false);

                emotePlaying = false;
                UpdateButtonVisuals();
                yield break;
            }

            if (playerAnimator == null)
            {
                emotePlaying = false;
                yield break;
            }

            yield return null;
        }
    }
}