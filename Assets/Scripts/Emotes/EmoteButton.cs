using UnityEngine;
using UnityEngine.UI;

public class EmoteButton : MonoBehaviour
{
    [HideInInspector] public EmoteMenuController controller;
    [HideInInspector] public int index;
    public Image image;

    public void Setup(EmoteMenuController controller, int index)
    {
        this.controller = controller;
        this.index = index;
        image = GetComponent<Image>();
    }
}
