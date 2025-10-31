using UnityEngine;
using System.Collections;

public class FloatingPetFollow : MonoBehaviour
{
    [Header("Configuración de seguimiento")]
    public Vector3 offset = new Vector3(0, 1.5f, -2f);
    public float followSpeed = 3f;

    [Header("Efecto de flotación")]
    public float floatAmplitude = 0.2f;
    public float floatFrequency = 2f;

    private Transform player;
    private Vector3 startOffset;

    void Start()
    {
        startOffset = offset;
        StartCoroutine(UpdateActivePlayer());
    }

    void Update()
    {
        if (player == null) return;

        // Movimiento de flotación
        float newY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        Vector3 floatOffset = new Vector3(0, newY, 0);

        // Movimiento suave hacia el jugador
        Vector3 targetPos = player.position + startOffset + floatOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        transform.LookAt(player);
    }

    IEnumerator UpdateActivePlayer()
    {
        while (true)
        {
            GameObject male = GameObject.Find("male");
            GameObject female = GameObject.Find("female");

            Transform newTarget = null;
            if (male != null && male.activeInHierarchy)
                newTarget = male.transform;
            else if (female != null && female.activeInHierarchy)
                newTarget = female.transform;

            // Si el objetivo cambia, actualiza el player
            if (newTarget != player && newTarget != null)
            {
                player = newTarget;
                Debug.Log("Mascota siguiendo a: " + player.name);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }
}
