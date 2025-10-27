using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class EmoteMenuController : MonoBehaviour
{
    [Header("Configuración General")]
    public GameObject panelEmote;           // Panel que contiene todos los botones
    public Sprite normalSprite;             // Sprite normal de ruleta
    public Sprite hoverSprite;              // Sprite de selección visual
    public Button[] botonesEmote;           // Los botones de emote (arrastrar desde el inspector)

    [Header("Debug Visual (opcional)")]
    public Text debugText;                  // Arrastra un Text del Canvas aquí para ver mensajes en pantalla

    private bool isHoldingKey = false;
    private int hoveredButtonIndex = -1;
    private int defaultEmoteIndex = 0;

    private PlayerMovement[] playerMovements;
    private CameraFollow cameraFollow;

    // Referencia al nuevo Input System
    private PlayerControls controls;

    void Awake()
    {
        // Inicializar los controles
        controls = new PlayerControls();

        // Vincular eventos a la acción de abrir/cerrar el menú
        controls.Player.OpenEmoteMenu.started += ctx => OnEmoteKeyPressed();
        controls.Player.OpenEmoteMenu.canceled += ctx => OnEmoteKeyReleased();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        if (panelEmote != null)
            panelEmote.SetActive(false);

        // Buscar referencias a scripts relacionados
        playerMovements = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        cameraFollow = FindFirstObjectByType<CameraFollow>();

        // Configurar eventos para los botones del menú
        for (int i = 0; i < botonesEmote.Length; i++)
        {
            int index = i;

            EventTrigger trigger = botonesEmote[i].gameObject.AddComponent<EventTrigger>();

            // Evento: Hover Enter
            EventTrigger.Entry entryEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entryEnter.callback.AddListener((data) => OnHoverEnter(index));
            trigger.triggers.Add(entryEnter);

            // Evento: Hover Exit
            EventTrigger.Entry entryExit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            entryExit.callback.AddListener((data) => OnHoverExit(index));
            trigger.triggers.Add(entryExit);

            // Click del botón
            botonesEmote[i].onClick.AddListener(() => OnClickEmote(index));
        }

        LogDebug("EmoteMenuController iniciado correctamente con el nuevo Input System");
    }

    // Se activa al presionar la tecla configurada (por ejemplo, "B")
    private void OnEmoteKeyPressed()
    {
        LogDebug("Tecla de emote presionada (Input System)");
        AbrirMenuEmote();
    }

    // Se activa al soltar la tecla
    private void OnEmoteKeyReleased()
    {
        LogDebug("Tecla de emote soltada (Input System)");
        CerrarMenuEmote();
    }

    private void AbrirMenuEmote()
    {
        if (panelEmote == null)
        {
            LogDebug("PanelEmote no asignado.");
            return;
        }

        panelEmote.SetActive(true);
        isHoldingKey = true;
        hoveredButtonIndex = -1;

        LogDebug("Menú de emotes abierto");

        // Desactivar movimiento de jugadores
        if (playerMovements == null || playerMovements.Length == 0)
            playerMovements = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        foreach (var pm in playerMovements)
        {
            if (pm != null)
                pm.enabled = false;
        }

        // Desactivar cámara
        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow>();

        if (cameraFollow != null)
            cameraFollow.enabled = false;

        // Mostrar cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void CerrarMenuEmote()
    {
        isHoldingKey = false;

        if (panelEmote != null)
        {
            panelEmote.SetActive(false);
            LogDebug("Menú de emotes cerrado");
        }

        // Reactivar movimiento del jugador
        if (playerMovements == null || playerMovements.Length == 0)
            playerMovements = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        foreach (var pm in playerMovements)
        {
            if (pm != null)
                pm.enabled = true;
        }

        // Reactivar cámara
        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow>();

        if (cameraFollow != null)
            cameraFollow.enabled = true;

        // Ocultar cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Ejecutar emote seleccionado o predeterminado
        if (hoveredButtonIndex != -1)
            ExecuteEmote(hoveredButtonIndex);
        else
            ExecuteEmote(defaultEmoteIndex);
    }

    private void OnHoverEnter(int index)
    {
        hoveredButtonIndex = index;
        Image img = botonesEmote[index].GetComponent<Image>();
        img.sprite = hoverSprite;
    }

    private void OnHoverExit(int index)
    {
        Image img = botonesEmote[index].GetComponent<Image>();
        img.sprite = normalSprite;

        if (hoveredButtonIndex == index)
            hoveredButtonIndex = -1;
    }

    private void OnClickEmote(int index)
    {
        panelEmote.SetActive(false);
        ExecuteEmote(index);

        if (playerMovements == null || playerMovements.Length == 0)
            playerMovements = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        foreach (var pm in playerMovements)
        {
            if (pm != null)
                pm.enabled = true;
        }

        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow>();

        if (cameraFollow != null)
            cameraFollow.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ExecuteEmote(int index)
    {
        LogDebug("Ejecutando emote #" + (index + 1));
        // Aquí puedes llamar a una animación, sonido o evento
    }

    private void LogDebug(string message)
    {
        Debug.Log(message);

        if (debugText != null)
            debugText.text = message;
    }
}
