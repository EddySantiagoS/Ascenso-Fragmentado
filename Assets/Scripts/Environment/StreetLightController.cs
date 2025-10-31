using UnityEngine;

public class StreetLightController : MonoBehaviour
{
    [Header("Configuraci�n de la farola")]
    public Light lampLight;        // Luz de la farola
    public Light sunLight;         // Direcci�n del sol
    public GameObject emissionObject;

    void Update()
    {
        if (lampLight == null || sunLight == null) return;

        float rotX = sunLight.transform.rotation.eulerAngles.x;

        // Si el sol est� debajo del horizonte (180��360�)
        bool isNight = rotX > 180f && rotX < 360f;

        lampLight.enabled = isNight;

        if (emissionObject != null)
        {
            var mat = emissionObject.GetComponent<Renderer>().material;
            if (isNight)
                mat.EnableKeyword("_EMISSION");
            else
                mat.DisableKeyword("_EMISSION");
        }
    }
}