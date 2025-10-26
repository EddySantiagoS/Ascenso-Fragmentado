using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Referencias de personajes")]
    public GameObject maleCharacter;
    public GameObject femaleCharacter;

    [Header("Render Texture")]
    public Camera characterPreviewCamera; // Cámara que renderiza al personaje
    public RawImage previewImage; // Donde se muestra el RenderTexture

    private bool isMaleActive = true;

    void Start()
    {
        // Aseguramos que solo uno esté activo
        maleCharacter.SetActive(true);
        femaleCharacter.SetActive(false);

        // Configuramos la cámara para ver el personaje actual
        FocusOnCharacter(maleCharacter.transform);
    }

    public void OnSwitchCharacter()
    {
        // Alternar personajes
        isMaleActive = !isMaleActive;

        maleCharacter.SetActive(isMaleActive);
        femaleCharacter.SetActive(!isMaleActive);

        // Actualizar la cámara de vista previa
        if (isMaleActive)
            FocusOnCharacter(maleCharacter.transform);
        else
            FocusOnCharacter(femaleCharacter.transform);

        Debug.Log("Personaje cambiado a: " + (isMaleActive ? "Hombre" : "Mujer"));
    }

    void FocusOnCharacter(Transform character)
    {
        if (characterPreviewCamera == null) return;

        // Ajusta la cámara para que mire al personaje desde una distancia adecuada
        characterPreviewCamera.transform.position = character.position + character.forward * -1.5f + Vector3.up * 1.5f;
        characterPreviewCamera.transform.LookAt(character.position + Vector3.up * 1.5f);
    }
}