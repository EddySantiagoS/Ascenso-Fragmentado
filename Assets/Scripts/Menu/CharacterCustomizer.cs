using UnityEngine;

public class CharacterCustomizer : MonoBehaviour
{
    [Header("Slots del personaje")]
    public Transform headSlot; //  Gorro
    public Transform faceSlot; //  Gafas

    [Header("Accesorios disponibles")]
    public GameObject[] hats;      // Prefabs de gorros
    public GameObject[] glasses;   // Prefabs de gafas

    private GameObject currentHat;
    private GameObject currentGlasses;

    // 🧢 Cambiar o quitar gorro
    public void ChangeHat(int index)
    {
        // Si hay un gorro actual, lo quitamos primero
        if (currentHat != null)
            Destroy(currentHat);

        // Si el índice es válido, instanciamos el nuevo gorro
        if (index >= 0 && index < hats.Length)
        {
            currentHat = Instantiate(hats[index], headSlot);
            ResetTransform(currentHat);
        }
        else
        {
            currentHat = null; // Nada equipado
        }
    }

    // 😎 Cambiar o quitar gafas
    public void ChangeGlasses(int index)
    {
        if (currentGlasses != null)
            Destroy(currentGlasses);

        if (index >= 0 && index < glasses.Length)
        {
            currentGlasses = Instantiate(glasses[index], faceSlot);
            ResetTransform(currentGlasses);
        }
        else
        {
            currentGlasses = null;
        }
    }

    // 🚫 Quitar accesorios directamente
    public void RemoveHat()
    {
        if (currentHat != null)
        {
            Destroy(currentHat);
            currentHat = null;
        }
    }

    public void RemoveGlasses()
    {
        if (currentGlasses != null)
        {
            Destroy(currentGlasses);
            currentGlasses = null;
        }
    }

    // 🔧 Ajusta la posición y rotación del accesorio al slot
    private void ResetTransform(GameObject obj)
    {
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
    }
}
