using UnityEngine;
using System.Collections;

public class PetManager : MonoBehaviour
{
    [Header("Mascotas disponibles (Prefabs)")]
    public GameObject[] petPrefabs;

    private GameObject activePet;
    private Transform player;

    void Start()
    {
        // Busca el jugador activo automáticamente (Male o Female)
        StartCoroutine(FindActivePlayer());
    }

    IEnumerator FindActivePlayer()
    {
        while (player == null)
        {
            GameObject male = GameObject.Find("male");
            GameObject female = GameObject.Find("female");

            if (male != null && male.activeInHierarchy)
                player = male.transform;
            else if (female != null && female.activeInHierarchy)
                player = female.transform;

            yield return new WaitForSeconds(0.2f);
        }
    }

    // 🔹 Llamar este método desde un botón para activar una mascota específica
    public void ActivatePet(int petIndex)
    {
        if (player == null)
        {
            Debug.LogWarning("No hay jugador activo para asignar mascota.");
            return;
        }

        if (petIndex < 0 || petIndex >= petPrefabs.Length)
        {
            Debug.LogWarning("Índice de mascota inválido.");
            return;
        }

        // Si ya hay una mascota activa, eliminarla
        if (activePet != null)
            Destroy(activePet);

        // Instanciar la nueva mascota
        GameObject petPrefab = petPrefabs[petIndex];
        activePet = Instantiate(petPrefab);

        // Agregarle el script de seguimiento
        FloatingPetFollow follow = activePet.GetComponent<FloatingPetFollow>();
        if (follow == null)
            follow = activePet.AddComponent<FloatingPetFollow>();

        // Asignar manualmente el jugador si ya está disponible
        follow.StartCoroutine(AssignPlayer(follow));

        Debug.Log("Mascota activada: " + petPrefab.name);
    }

    IEnumerator AssignPlayer(FloatingPetFollow follow)
    {
        // Esperar hasta que el jugador exista
        while (player == null)
            yield return null;

        // Actualizar el objetivo manualmente
        follow.StopAllCoroutines(); // Evitar que el script busque por su cuenta
        follow.StartCoroutine(SetPlayerToFollow(follow));
    }

    IEnumerator SetPlayerToFollow(FloatingPetFollow follow)
    {
        // Simplemente asigna el Transform actual
        yield return null;
    }

    // 🔹 Llamar desde botón para quitar mascota
    public void RemovePet()
    {
        if (activePet != null)
        {
            Destroy(activePet);
            activePet = null;
            Debug.Log("Mascota eliminada.");
        }
    }
}
