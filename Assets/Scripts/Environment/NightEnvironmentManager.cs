using System.Collections.Generic;
using UnityEngine;

public class NightEnvironmentManager : MonoBehaviour
{
    [Header("Referencia al sol")]
    public Light sunLight; // El sol direccional principal

    [Header("Configuración")]
    [Tooltip("Si es true, el sistema buscará automáticamente luces y partículas en escena")]
    public bool autoDetectObjects = true;

    [Tooltip("Cada cuántos segundos actualizar el estado (optimización)")]
    public float updateInterval = 1f;

    [Tooltip("Ángulo del sol en el que se considera que empieza la noche")]
    public float nightStartAngle = 180f;

    [Tooltip("Ángulo del sol en el que termina la noche")]
    public float nightEndAngle = 360f;

    private bool isNight = false;
    private float nextUpdateTime;

    // Listas para mantener referencias
    private List<Light> nightLights = new List<Light>();
    private List<ParticleSystem> nightParticles = new List<ParticleSystem>();
    private List<Renderer> emissiveObjects = new List<Renderer>();

    void Start()
    {
        if (autoDetectObjects)
            FindNightObjects();

        if (sunLight != null)
        {
            float rotX = sunLight.transform.rotation.eulerAngles.x;
            isNight = rotX > nightStartAngle && rotX < nightEndAngle;

            // Aplicar ese estado inicial a todos los objetos
            UpdateNightObjects();
        }
    }

    void Update()
    {
        if (Time.time < nextUpdateTime) return;
        nextUpdateTime = Time.time + updateInterval;

        if (sunLight == null) return;

        float rotX = sunLight.transform.rotation.eulerAngles.x;
        bool newIsNight = rotX > nightStartAngle && rotX < nightEndAngle;

        if (newIsNight != isNight)
        {
            isNight = newIsNight;
            UpdateNightObjects();
        }
    }

    // Busca todos los objetos relevantes en la escena
    public void FindNightObjects()
    {
        nightLights.Clear();
        nightParticles.Clear();
        emissiveObjects.Clear();

        // Buscar luces (excepto el sol)
        foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light == sunLight) continue;
            nightLights.Add(light);
        }

        // Buscar partículas
        foreach (var ps in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
        {
            nightParticles.Add(ps);
        }

        // Buscar objetos con emisión
        foreach (var rend in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (rend.material != null && rend.material.IsKeywordEnabled("_EMISSION"))
                emissiveObjects.Add(rend);
        }

        Debug.Log($"[NightManager] Detectados {nightLights.Count} luces, {nightParticles.Count} partículas y {emissiveObjects.Count} emisores.");
    }

    // Activa o desactiva los efectos nocturnos
    void UpdateNightObjects()
    {
        Debug.Log($"[NightManager] Cambiando a modo {(isNight ? "NOCHE" : "DÍA")}");

        foreach (var light in nightLights)
        {
            if (light != null)
                light.enabled = isNight;
        }

        foreach (var ps in nightParticles)
        {
            if (ps != null)
            {
                if (isNight && !ps.isPlaying)
                    ps.Play();
                else if (!isNight && ps.isPlaying)
                    ps.Stop();
            }
        }

        foreach (var rend in emissiveObjects)
        {
            if (rend == null) continue;

            var mat = rend.material;
            if (isNight)
                mat.EnableKeyword("_EMISSION");
            else
                mat.DisableKeyword("_EMISSION");
        }
    }

    // Permite registrar objetos dinámicamente (por ejemplo, prefabs instanciados)
    public void RegisterDynamicObject(GameObject obj)
    {
        foreach (var light in obj.GetComponentsInChildren<Light>())
        {
            if (light != sunLight)
            {
                nightLights.Add(light);
                light.enabled = isNight;
            }
        }

        foreach (var ps in obj.GetComponentsInChildren<ParticleSystem>())
        {
            nightParticles.Add(ps);
            if (isNight) ps.Play(); else ps.Stop();
        }

        foreach (var rend in obj.GetComponentsInChildren<Renderer>())
        {
            if (rend.material != null && rend.material.IsKeywordEnabled("_EMISSION"))
            {
                emissiveObjects.Add(rend);
                if (isNight)
                    rend.material.EnableKeyword("_EMISSION");
                else
                    rend.material.DisableKeyword("_EMISSION");
            }
        }
    }
}
