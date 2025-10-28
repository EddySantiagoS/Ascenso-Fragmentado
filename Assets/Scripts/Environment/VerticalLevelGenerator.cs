using UnityEngine;
using System.Collections.Generic;

public class VerticalLevelGenerator : MonoBehaviour
{
    [Header("Bloques del mapa (prefabs)")]
    public GameObject[] platformPrefabs; // plataformas de salto
    public GameObject[] climbPrefabs;    // zonas escalables
    public GameObject[] obstaclePrefabs; // opcionales

    [Header("Parámetros de generación")]
    public int initialSegments = 10;
    public float minVerticalGap = 2f;  // distancia mínima entre bloques
    public float maxVerticalGap = 5f;  // distancia máxima
    public float horizontalRange = 3f; // desplazamiento lateral máximo
    public Transform startPoint;       // punto inicial (abajo)

    private Vector3 currentSpawnPoint;
    private List<GameObject> spawnedSegments = new List<GameObject>();


    void Start()
    {
        currentSpawnPoint = startPoint != null ? startPoint.position : Vector3.zero;
        GenerateInitialMap();
    }

    void GenerateInitialMap()
    {
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnNextSegment();
        }
    }

    void SpawnNextSegment()
    {
        // Elegimos aleatoriamente el tipo de bloque
        GameObject[] pool;
        int type = Random.Range(0, 3);
        if (type == 0) pool = platformPrefabs;
        else if (type == 1) pool = climbPrefabs;
        else pool = obstaclePrefabs;

        if (pool.Length == 0) return;

        GameObject prefab = pool[Random.Range(0, pool.Length)];

        // Calculamos una posición vertical y lateral aleatoria
        float verticalOffset = Random.Range(minVerticalGap, maxVerticalGap);
        float horizontalOffset = Random.Range(-horizontalRange, horizontalRange);
        Vector3 spawnPos = currentSpawnPoint + new Vector3(horizontalOffset, verticalOffset, 0f);

        // Instanciamos el bloque
        GameObject newSegment = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        spawnedSegments.Add(newSegment);

        // Actualizamos el punto de spawn
        currentSpawnPoint = spawnPos;
    }

    // Llamable cuando el jugador sube mucho (para generar más)
    public void GenerateMore(int count = 5)
    {
        for (int i = 0; i < count; i++)
            SpawnNextSegment();
    }
}