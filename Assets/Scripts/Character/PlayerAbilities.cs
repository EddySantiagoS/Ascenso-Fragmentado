using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{

    [Header("Referencias")]
    public PlayerMovement movement;
    public AbilityHUD hud;

    [Header("Estado de habilidades")]
    public bool hasDoubleJump;
    public bool hasTimeSlow;

    [Header("Double Jump")]
    public float doubleJumpDuration = 10f;
    private Coroutine doubleJumpRoutine;

    [Header("Time Slow")]
    public float slowDuration = 8f;
    public float slowFactor = 0.3f;
    private Coroutine timeSlowRoutine;

    void Awake()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (hasDoubleJump && Keyboard.current.qKey.wasPressedThisFrame)
        {
            ActivateDoubleJump();
        }

        if (hasTimeSlow && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ActivateTimeSlow();
        }
    }

    public void GiveAbility(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.DoubleJump:
                GiveDoubleJump();
                break;

            case AbilityType.TimeSlow:
                GiveTimeSlow();
                break;
        }
    }

    // RECIBIR HABILIDADES

    public void GiveDoubleJump()
    {
        hasDoubleJump = true;
        hud?.ShowAbility(AbilityType.DoubleJump);
    }

    public void GiveTimeSlow()
    {
        hasTimeSlow = true;
        hud?.ShowAbility(AbilityType.TimeSlow);
    }

    // ACTIVACIÓN

    void ActivateDoubleJump()
    {
        if (doubleJumpRoutine != null) return;
        doubleJumpRoutine = StartCoroutine(DoubleJumpRoutine());
    }

    void ActivateTimeSlow()
    {
        if (timeSlowRoutine != null) return;
        timeSlowRoutine = StartCoroutine(TimeSlowRoutine());
    }

    // CORUTINAS

    IEnumerator DoubleJumpRoutine()
    {
        movement.canDoubleJump = true;
        movement.extraJumps = 1;

        yield return new WaitForSeconds(doubleJumpDuration);

        movement.canDoubleJump = false;
        movement.extraJumps = 0;
        hasDoubleJump = false;

        hud?.HideAbility(AbilityType.DoubleJump);

        doubleJumpRoutine = null;
    }

    IEnumerator TimeSlowRoutine()
    {
        Time.timeScale = slowFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        movement.ignoreTimeScale = true;

        yield return new WaitForSecondsRealtime(slowDuration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        movement.ignoreTimeScale = false;
        hasTimeSlow = false;

        hud?.HideAbility(AbilityType.TimeSlow);

        timeSlowRoutine = null;
    }
}