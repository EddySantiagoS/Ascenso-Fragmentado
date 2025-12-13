using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
    public void SetGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("GraphicsQuality", index);

        Debug.Log("Calidad gráfica cambiada a: " + QualitySettings.names[index]);
    }

    void Start()
    {
        int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", 1);
        QualitySettings.SetQualityLevel(savedQuality, true);
    }
}