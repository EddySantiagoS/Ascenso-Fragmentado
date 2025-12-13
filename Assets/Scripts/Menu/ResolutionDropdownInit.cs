using TMPro;
using UnityEngine;

public class ResolutionDropdownInit : MonoBehaviour
{
    void Start()
    {
        TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();

        
        dropdown.SetValueWithoutNotify(0);
    }
}