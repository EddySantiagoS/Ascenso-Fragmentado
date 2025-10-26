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

        // Aseguramos que ambos personajes est�n visibles
        maleCharacter.SetActive(true);
        femaleCharacter.SetActive(true);
    }
    void Update()
    {
        if (transitioning) return;

        // Usamos el nuevo Input System:
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Obtener posici�n actual del mouse
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.IsChildOf(maleCharacter.transform))
                    SelectCharacter("Male");
                else if (hit.transform.IsChildOf(femaleCharacter.transform))
                    SelectCharacter("Female");
            }
        }
    }

    void SelectCharacter(string character)
    {
        selectedCharacter = character;
        Debug.Log("Personaje seleccionado: " + character);

        // Resaltar personaje seleccionado
        maleCharacter.transform.localScale = (character == "Male") ? Vector3.one * 1.1f : Vector3.one;
        femaleCharacter.transform.localScale = (character == "Female") ? Vector3.one * 1.1f : Vector3.one;

        // Desactivar el otro personaje
        if (character == "Male")
            femaleCharacter.SetActive(false);
        else
            maleCharacter.SetActive(false);

        // ✅ Desactivar este script para evitar más clics
        this.enabled = false;

        // Iniciar movimiento de cámara
        StartCoroutine(MoveCameraBehindCharacter());
    }
    IEnumerator MoveCameraBehindCharacter()
    {
        transitioning = true;
        GameObject targetChar = selectedCharacter == "Male" ? maleCharacter : femaleCharacter;

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

        var follow = mainCam.gameObject.AddComponent<CameraFollow>();
        follow.target = targetChar.transform;
    }
}