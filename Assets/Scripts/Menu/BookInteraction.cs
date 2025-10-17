using UnityEngine;

public class BookInteraction : MonoBehaviour
{
    public GameObject hologramUI;
    public Transform player;
    public float activationDistance = 2.5f;

    private bool isVisible = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hologramUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance < activationDistance && !isVisible)
        {
            ShowHologram();
        }
        else if (distance >= activationDistance && isVisible)
        {
            HideHologram();
        }
    }

    void ShowHologram()
    {
        isVisible = true;
        hologramUI.SetActive(true);
        Debug.Log("Libro activado");
    }

    void HideHologram()
    {
        isVisible = false;
        hologramUI.SetActive(false);
    }
}
