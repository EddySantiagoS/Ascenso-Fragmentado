using UnityEngine;

public class BookMenu : MonoBehaviour
{
    public GameObject[] pages; 
    public Animator bookAnimator;

    private int currentPage = 0;
    private bool isTurning = false;

    void Start()
    {
        ShowPage(0);
    }

    public void GoToPage(int index)
    {
        if (isTurning || index == currentPage) return;

        StartCoroutine(TurnPage(index));
    }

    System.Collections.IEnumerator TurnPage(int newPage)
    {
        isTurning = true;
        // Reproduce la animación
        bookAnimator.SetTrigger("TurnPage");

        // Esperar mitad de la animación (puedes ajustar el tiempo según el clip)
        yield return new WaitForSeconds(0.6f);

        // Cambiar panel visible
        ShowPage(newPage);

        yield return new WaitForSeconds(0.5f);
        isTurning = false;
    }

    void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == index);

        currentPage = index;
    }
}
