using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EmoteMenuController : MonoBehaviour
{
    [Header("Configuración General")]
    [Tooltip("Panel principal que contiene los botones de emotes.")]
    public GameObject panelEmote;

    [Tooltip("Arreglo de botones que representan cada emote (asignar desde el inspector).")]
    public Button[] botonesEmote;

    private int selectedIndex = 0;
    private PlayerControls controls;
    private bool isHoldingKey = false;

    // Guardamos los sprites originales para restaurarlos cuando no estén seleccionados
    private Sprite[] originalSprites;

    void Awake()
    {
        controls = new PlayerControls();

        // Evento: mantener pulsado B
        controls.Player.OpenEmoteMenu.started += ctx => OnEmoteKeyPressed();
        controls.Player.OpenEmoteMenu.canceled += ctx => OnEmoteKeyReleased();

        // Evento: scroll del mouse (asegúrate de tener la acción Scroll configurada)
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

        // Guardar sprites originales
        originalSprites = new Sprite[botonesEmote.Length];
        for (int i = 0; i < botonesEmote.Length; i++)
        {
            Image img = botonesEmote[i].GetComponent<Image>();
            originalSprites[i] = img != null ? img.sprite : null;
        }

        UpdateButtonVisuals();

        Debug.Log("EmoteMenuController (modo scroll) inicializado correctamente.");
    }

    private void OnEmoteKeyPressed()
    {
        if (panelEmote == null) return;

        panelEmote.SetActive(true);
        isHoldingKey = true;

        // Mostrar cursor o no según tu preferencia; lo bloqueamos para evitar problemas en FP:
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        selectedIndex = Mathf.Clamp(selectedIndex, 0, botonesEmote.Length - 1);
        UpdateButtonVisuals();

        Debug.Log("Menú de emotes abierto (modo scroll).");
    }

    private void OnEmoteKeyReleased()
    {
        if (!isHoldingKey) return;
        isHoldingKey = false;

        if (panelEmote != null)
            panelEmote.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        ExecuteEmote(selectedIndex);
    }

    private void OnScroll(float scrollValue)
    {
        if (!isHoldingKey || botonesEmote.Length == 0) return;

        // Scroll positivo = arriba => avanzar
        if (scrollValue > 0.1f)
        {
            selectedIndex = (selectedIndex + 1) % botonesEmote.Length;
        }
        else if (scrollValue < -0.1f)
        {
            selectedIndex = (selectedIndex - 1 + botonesEmote.Length) % botonesEmote.Length;
        }

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

            // Obtener los sprites definidos en Sprite Swap (si usas Sprite Swap)
            Sprite highlighted = btn.spriteState.highlightedSprite;
            Sprite selectedSprite = btn.spriteState.selectedSprite;
            Sprite pressed = btn.spriteState.pressedSprite;

            if (i == selectedIndex)
            {
                // Si definiste Selected Sprite, úsalo; si no, usa Highlighted; si no, deja el original
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
                // Restaurar sprite original si no está seleccionado
                img.sprite = originalSprites[i];
            }
        }
    }

    public void ExecuteEmote(int index)
    {
        Debug.Log($"Ejecutando emote #{index + 1}");
        // TODO: disparar animación/sonido/evento del emote aquí.
    }
}
