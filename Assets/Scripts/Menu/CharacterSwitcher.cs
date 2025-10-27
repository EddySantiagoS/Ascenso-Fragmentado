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
        // Activamos solo uno al inicio
        maleCharacter.SetActive(true);
        femaleCharacter.SetActive(false);

        // Solo nos aseguramos de que la cámara exista (no la tocamos)
        if (characterPreviewCamera != null)
            characterPreviewCamera.enabled = true;
    }

    public void OnSwitchCharacter()
    {
        // Determinar personajes activo/nuevo
        GameObject activeCharacter = isMaleActive ? maleCharacter : femaleCharacter;
        GameObject newCharacter = isMaleActive ? femaleCharacter : maleCharacter;

        // Guardar posición y rotación actuales
        Vector3 currentPos = activeCharacter.transform.position;
        Quaternion currentRot = activeCharacter.transform.rotation;

        // Obtener controladores
        CharacterController oldCC = activeCharacter.GetComponent<CharacterController>();
        CharacterController newCC = newCharacter.GetComponent<CharacterController>();

        if (oldCC != null) oldCC.enabled = false;
        if (newCC != null) newCC.enabled = false;

        // Cambiar visibilidad
        activeCharacter.SetActive(false);
        newCharacter.SetActive(true);

        // Aplicar la misma posición y rotación
        newCharacter.transform.position = currentPos;
        newCharacter.transform.rotation = currentRot;

        if (newCC != null) newCC.enabled = true;

        isMaleActive = !isMaleActive;

        // Actualizar cámara del juego (tercera persona)
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
}