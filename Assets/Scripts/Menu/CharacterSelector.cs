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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = Camera.main;

        // Aseguramos que ambos personajes estén visibles
        if (maleCharacter != null) maleCharacter.SetActive(true);
        if (femaleCharacter != null) femaleCharacter.SetActive(true);

        // IMPORTANT: Quitamos cualquier tag "Player" inicial para asegurarnos
        // de que el sistema de emotes NO se pueda abrir hasta elegir personaje.
        if (maleCharacter != null) maleCharacter.tag = "Untagged";
        if (femaleCharacter != null) femaleCharacter.tag = "Untagged";
    }

    void Update()
    {
        if (transitioning) return;

        // Usamos el nuevo Input System:
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Obtener posición actual del mouse
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

        // Resaltar personaje seleccionado
        if (maleCharacter != null) maleCharacter.transform.localScale = (character == "Male") ? Vector3.one * 1.1f : Vector3.one;
        if (femaleCharacter != null) femaleCharacter.transform.localScale = (character == "Female") ? Vector3.one * 1.1f : Vector3.one;

        // Desactivar el otro personaje
        if (character == "Male")
        {
            if (femaleCharacter != null) femaleCharacter.SetActive(false);
        }
        else
        {
            if (maleCharacter != null) maleCharacter.SetActive(false);
        }

        // Obtener el personaje activo
        GameObject activePlayer = (character == "Male") ? maleCharacter : femaleCharacter;
        if (activePlayer == null)
        {
            Debug.LogWarning("SelectCharacter: activePlayer es null.");
            return;
        }

        // ASIGNAR TAG "Player" AL PERSONAJE ELEGIDO
        activePlayer.tag = "Player";

        // QUITAR TAG AL OTRO POR SEGURIDAD
        if (character == "Male")
        {
            if (femaleCharacter != null) femaleCharacter.tag = "Untagged";
        }
        else
        {
            if (maleCharacter != null) maleCharacter.tag = "Untagged";
        }

        // Asignarlo al BookInteraction (si existe)
        BookInteraction book = FindFirstObjectByType<BookInteraction>();
        if (book != null)
            book.SetPlayer(activePlayer.transform);

        // Desactivar este script para evitar más clics
        this.enabled = false;

        // Iniciar movimiento de cámara
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
}