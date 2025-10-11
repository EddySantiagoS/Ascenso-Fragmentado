using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DayNightCycle : MonoBehaviour
{
    public Light sunLight;
    public float rotationSpeed;

    void Update()
    {
        // Rotar el sol
        sunLight.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);

        // Obtener rotación actual en el eje X (0–360)
        float rotX = sunLight.transform.rotation.eulerAngles.x;

        // Si el sol está "debajo del horizonte" (entre 180° y 360°)
        if (rotX > 180f && rotX < 360f)
        {
            sunLight.shadows = LightShadows.None; // noche, sin sombras
        }
        else
        {
            sunLight.shadows = LightShadows.Soft; // día, con sombras
        }
    }
}