using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityPickup : MonoBehaviour
{
    public AbilityType abilityType;

    [Header("Interacción")]
    public float pickupDistance = 2f;

    private Transform player;
    private PlayerAbilities playerAbilities;

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > pickupDistance) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Pickup();
        }
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) return;

        player = p.transform;
        playerAbilities = p.GetComponent<PlayerAbilities>();
    }

    void Pickup()
    {
        if (playerAbilities == null) return;

        playerAbilities.GiveAbility(abilityType);
        Destroy(gameObject);
    }
}