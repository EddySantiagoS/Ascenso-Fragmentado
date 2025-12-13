using UnityEngine;
using System.Collections.Generic;

public class ResolutionManager : MonoBehaviour
{
    public List<Vector2Int> resolutions = new()
    {
        new(1280, 720),
        new(1600, 900),
        new(1920, 1080),
        new(2560, 1440)
    };

    private FullScreenMode currentMode = FullScreenMode.ExclusiveFullScreen;

    public void SetResolution(int index)
    {
        Debug.Log($"🔥 SetResolution llamado con index: {index}");

        if (index < 0 || index >= resolutions.Count)
        {
            Debug.LogError("❌ Index fuera de rango");
            return;
        }

        var res = resolutions[index];
        Screen.SetResolution(res.x, res.y, currentMode);

        Debug.Log($"✅ Resolución solicitada: {res.x}x{res.y}");
    }
}