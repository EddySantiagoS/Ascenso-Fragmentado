using UnityEngine;

public class AbilityHUD : MonoBehaviour
{
    public GameObject doubleJumpIcon;
    public GameObject timeSlowIcon;

    void Start()
    {
        if (doubleJumpIcon) doubleJumpIcon.SetActive(false);
        if (timeSlowIcon) timeSlowIcon.SetActive(false);
    }

    public void ShowAbility(AbilityType ability)
    {
        switch (ability)
        {
            case AbilityType.DoubleJump:
                if (doubleJumpIcon) doubleJumpIcon.SetActive(true);
                break;

            case AbilityType.TimeSlow:
                if (timeSlowIcon) timeSlowIcon.SetActive(true);
                break;
        }
    }

    public void HideAbility(AbilityType ability)
    {
        switch (ability)
        {
            case AbilityType.DoubleJump:
                if (doubleJumpIcon) doubleJumpIcon.SetActive(false);
                break;

            case AbilityType.TimeSlow:
                if (timeSlowIcon) timeSlowIcon.SetActive(false);
                break;
        }
    }
}