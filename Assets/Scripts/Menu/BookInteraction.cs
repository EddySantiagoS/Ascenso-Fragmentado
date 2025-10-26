using UnityEngine;
using System.Collections;

public class BookInteraction : MonoBehaviour
{
    [Header("UI Holográfica")]
    public GameObject hologramUI;

    [Header("Configuración")]
    public float activationDistance = 2.5f;
    public string playerTag = "Player";

    private Transform player;
    private bool isVisible = false;
    private Vector3 originalScale; // guardamos la escala real

    void Start()
    {
        if (hologramUI != null)
        {
            originalScale = hologramUI.transform.localScale; // guardamos la escala original
            hologramUI.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || hologramUI == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance < activationDistance && !isVisible)
            ShowHologram();
        else if (distance >= activationDistance && isVisible)
            HideHologram();
    }

    void ShowHologram()
    {
        isVisible = true;
        hologramUI.SetActive(true);
        hologramUI.transform.localScale = Vector3.zero;
        StartCoroutine(ScaleIn());
        Debug.Log("Libro activado");
    }

    void HideHologram()
    {
        if (isVisible)
            StartCoroutine(ScaleOut());
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        Debug.Log($"Jugador asignado manualmente: {newPlayer.name}");
    }

    IEnumerator ScaleIn()
    {
        float t = 0f;
        float duration = 0.4f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.SmoothStep(0f, 1f, t / duration);
            hologramUI.transform.localScale = originalScale * scale;
            yield return null;
        }

        hologramUI.transform.localScale = originalScale;
    }

    IEnumerator ScaleOut()
    {
        isVisible = false;
        float t = 0f;
        float duration = 0.3f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.SmoothStep(1f, 0f, t / duration);
            hologramUI.transform.localScale = originalScale * scale;
            yield return null;
        }

        hologramUI.SetActive(false);
    }
}