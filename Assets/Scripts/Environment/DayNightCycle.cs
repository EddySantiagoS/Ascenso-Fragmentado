using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DayNightCycle : MonoBehaviour
{
    public Light sunLight;
    public float rotationspeed;

    void Update()
    {
        sunLight.transform.Rotate(Vector3.right, rotationspeed * Time.deltaTime);
    }
}