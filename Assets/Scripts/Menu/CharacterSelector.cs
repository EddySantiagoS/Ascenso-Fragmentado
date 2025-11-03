using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CharacterSelector : MonoBehaviour
{
    [Header("Characters")]
    public GameObject maleCharacter;
    public GameObject femaleCharacter;


    [Header("Camera")]
    public float cameraDistance = 3f;
    public float cameraHeight = 1.7f;
    public float transitionSpeed = 2f;

    private Camera mainCam;
    private string selectedCharacter = "None";
    private bool transitioning = false;


    void Start()
    {
        mainCam = Camera.main;

        // Ambos personajes visibles
        if (maleCharacter != null) maleCharacter.SetActive(true);
        if (femaleCharacter != null) femaleCharacter.SetActive(true);

        // Quitar control de movimiento al inicio
        DisablePlayerMovement(maleCharacter);
        DisablePlayerMovement(femaleCharacter);

        // Quitar tag de "Player"
        if (maleCharacter != null) maleCharacter.tag = "Untagged";
        if (femaleCharacter != null) femaleCharacter.tag = "Untagged";
    }

    void Update()
    {
        if (transitioning) return;

        // Detección de clic con el nuevo Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (maleCharacter != null && hit.transform.IsChildOf(maleCharacter.transform))
                    SelectCharacter("Male");
                else if (femaleCharacter != null && hit.transform.IsChildOf(femaleCharacter.transform))
                    SelectCharacter("Female");
            }
        }
    }

    void SelectCharacter(string character)
    {
        selectedCharacter = character;
        Debug.Log("Personaje seleccionado: " + character);

        // Resaltar el personaje elegido
        if (maleCharacter != null) maleCharacter.transform.localScale = (character == "Male") ? Vector3.one * 1.1f : Vector3.one;
        if (femaleCharacter != null) femaleCharacter.transform.localScale = (character == "Female") ? Vector3.one * 1.1f : Vector3.one;

        // Desactivar el personaje no elegido
        if (character == "Male")
        {
            if (femaleCharacter != null) femaleCharacter.SetActive(false);
        }
        else
        {
            if (maleCharacter != null) maleCharacter.SetActive(false);
        }

        // Activar el personaje seleccionado
        GameObject activePlayer = (character == "Male") ? maleCharacter : femaleCharacter;
        if (activePlayer == null)
        {
            Debug.LogWarning("SelectCharacter: activePlayer es null.");
            return;
        }

        // Asignar tag "Player" al personaje elegido
        activePlayer.tag = "Player";

        // Desactivar tag del otro
        if (character == "Male")
        {
            if (femaleCharacter != null) femaleCharacter.tag = "Untagged";
        }
        else
        {
            if (maleCharacter != null) maleCharacter.tag = "Untagged";
        }

        // 🔥 Habilitar movimiento SOLO del personaje elegido
        EnablePlayerMovement(activePlayer);

        // Asignar el jugador al sistema externo (BookInteraction)
        BookInteraction book = FindFirstObjectByType<BookInteraction>();
        if (book != null)
            book.SetPlayer(activePlayer.transform);

        // Desactivar este script para evitar más clics
        this.enabled = false;

        // Iniciar la transición de cámara
        StartCoroutine(MoveCameraBehindCharacter());
    }

    IEnumerator MoveCameraBehindCharacter()
    {
        transitioning = true;
        GameObject targetChar = selectedCharacter == "Male" ? maleCharacter : femaleCharacter;
        if (targetChar == null)
        {
            transitioning = false;
            yield break;
        }

        while (true)
        {
            Vector3 targetPos = targetChar.transform.position
                - targetChar.transform.forward * cameraDistance
                + Vector3.up * cameraHeight;

            Quaternion targetRot = Quaternion.LookRotation(
                targetChar.transform.position + Vector3.up * 1.5f - mainCam.transform.position
            );

            mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, targetPos, Time.deltaTime * transitionSpeed);
            mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, targetRot, Time.deltaTime * transitionSpeed);

            if (Vector3.Distance(mainCam.transform.position, targetPos) < 0.05f)
                break;

            yield return null;
        }

        transitioning = false;

        // Evitar añadir múltiples CameraFollow si ya existe uno
        var existing = mainCam.gameObject.GetComponent<CameraFollow>();
        if (existing != null) Destroy(existing);

        var follow = mainCam.gameObject.AddComponent<CameraFollow>();
        follow.SetTarget(targetChar.transform);
    }

    // ------------------------------
    // 🔒 CONTROL DE MOVIMIENTO
    // ------------------------------

    void DisablePlayerMovement(GameObject character)
    {
        if (character == null) return;

        var move = character.GetComponent<PlayerMovement>();
        if (move != null)
        {
            move.enabled = true; // El script sigue activo para mantener animaciones
            move.AllowMovement = false; // Pero bloqueamos el input
        }
    }

    void EnablePlayerMovement(GameObject character)
    {
        if (character == null) return;

        var move = character.GetComponent<PlayerMovement>();
        if (move != null)
        {
            move.AllowMovement = true;

            if (move.staminaUI != null)
            move.staminaUI.SetActive(true);
        }
    }
}
