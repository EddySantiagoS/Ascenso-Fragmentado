using TMPro;
using UnityEngine;

public class ResolutionDropdownInit : MonoBehaviour
{
    void Start()
    {
        TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();

        // Por ejemplo, iniciar en 1920x1080 (index 2)
        dropdown.SetValueWithoutNotify(0);
    }
}