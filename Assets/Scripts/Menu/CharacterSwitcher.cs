using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Referencias de personajes")]
    public GameObject maleCharacter;
    public GameObject femaleCharacter;

    [Header("Render Texture (vista del libro)")]
    public Camera characterPreviewCamera; // Cámara que renderiza al personaje
    public RawImage previewImage;         // Donde se muestra el RenderTexture

    [Header("Cámara del juego (tercera persona)")]
    public float cameraDistance = 3f;
    public float cameraHeight = 1.7f;
    public float cameraFollowSpeed = 10f;
    public float cameraRotateSpeed = 10f;

    private bool isMaleActive = true;

    void Start()
    {
        // Detectar qué personaje está activo al inicio
        isMaleActive = maleCharacter.activeSelf && !femaleCharacter.activeSelf;

        // Solo nos aseguramos de que la cámara exista (no la tocamos)
        if (characterPreviewCamera != null)
            characterPreviewCamera.enabled = true;
    }

    public void OnSwitchCharacter()
    {
        // 1) Determinar quién es el jugador actual de forma fiable (por tag)
        GameObject activeCharacter = GameObject.FindWithTag("Player");
        GameObject newCharacter = null;

        if (activeCharacter == null)
        {
            // fallback: usar isMaleActive por compatibilidad
            activeCharacter = isMaleActive ? maleCharacter : femaleCharacter;
            newCharacter = isMaleActive ? femaleCharacter : maleCharacter;
        }
        else
        {
            // elegir el otro
            newCharacter = (activeCharacter == maleCharacter) ? femaleCharacter : maleCharacter;
        }

        if (activeCharacter == null || newCharacter == null)
        {
            Debug.LogWarning("[CharacterSwitcher] No se pudo determinar active/new character.");
            return;
        }

        Debug.Log($"[CharacterSwitcher] Switch requested. Active: {activeCharacter.name} -> New: {newCharacter.name}");

        // 2) Guardar posición y rotación del actual
        Vector3 currentPos = activeCharacter.transform.position;
        Quaternion currentRot = activeCharacter.transform.rotation;

        // 3) Desactivar CharacterControllers ANTES de desactivar GameObject
        CharacterController oldCC = activeCharacter.GetComponent<CharacterController>();
        CharacterController newCC = newCharacter.GetComponent<CharacterController>();

        if (oldCC != null) oldCC.enabled = false;
        if (newCC != null) newCC.enabled = false;

        // 4) Desactivar el actual (ya sin CC)
        activeCharacter.SetActive(false);

        // 5) Posicionar y rotar el nuevo ANTES de activarlo
        newCharacter.transform.SetPositionAndRotation(currentPos, currentRot);

        // 6) Activar el nuevo
        newCharacter.SetActive(true);

        // 7) Esperar un frame y luego habilitar su CharacterController (corrutina)
        StartCoroutine(EnableCharacterControllerNextFrame(newCC));

        // 8) Actualizar tags y movimiento
        activeCharacter.tag = "Untagged";
        newCharacter.tag = "Player";

        var oldMove = activeCharacter.GetComponent<PlayerMovement>();
        if (oldMove != null) oldMove.AllowMovement = false;

        var newMove = newCharacter.GetComponent<PlayerMovement>();
        if (newMove != null) newMove.AllowMovement = true;

        // 9) Actualizar el flag isMaleActive según el nuevo personaje
        isMaleActive = (newCharacter == maleCharacter);

        // 10) Notificar BookInteraction para que use el nuevo jugador
        foreach (BookInteraction book in FindObjectsByType<BookInteraction>(FindObjectsSortMode.None))
            book.SetPlayer(newCharacter.transform);

        // 11) Reasignar cámara (lo haces en corrutina para esperar frame)
        StartCoroutine(ReassignGameCamera(newCharacter.transform));
    }

    IEnumerator ReassignGameCamera(Transform newTarget)
    {
        yield return null;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CameraFollow follow = mainCam.GetComponent<CameraFollow>();
            if (follow == null)
            {
                follow = mainCam.gameObject.AddComponent<CameraFollow>();
                follow.offset = new Vector3(0f, cameraHeight, -cameraDistance);
                follow.smoothSpeed = cameraFollowSpeed;
                follow.rotationSpeed = cameraRotateSpeed;
            }

            follow.SetTarget(newTarget);
        }
    }

    IEnumerator EnableCharacterControllerNextFrame(CharacterController cc)
    {
        yield return null;
        if (cc != null)
            cc.enabled = true;
    }

    IEnumerator ActivateNextCharacter(GameObject newCharacter, Vector3 pos, Quaternion rot)
    {
        yield return null; // Espera un frame para asegurar que el desactivado se procese

        newCharacter.SetActive(true);
        newCharacter.transform.position = pos;
        newCharacter.transform.rotation = rot;
    }
}