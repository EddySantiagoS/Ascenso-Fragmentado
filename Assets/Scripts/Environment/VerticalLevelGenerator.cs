using UnityEngine;
using System.Collections.Generic;

public class VerticalLevelGenerator : MonoBehaviour
{
    [Header("Bloques del mapa (prefabs)")]
    public GameObject[] platformPrefabs;
    public GameObject[] climbPrefabs;
    public GameObject[] obstaclePrefabs;

    [Header("Parámetros de generación")]
    public int initialSegments = 10;
    public float minVerticalGap = 2f;
    public float maxVerticalGap = 5f;
    public float spiralRadius = 4f;
    public float angleStep = 45f;
    public Transform startPoint;

    private float currentAngle = 0f;
    private float currentHeight = 0f;
    private List<GameObject> spawnedSegments = new List<GameObject>();
    private Dictionary<GameObject, Vector3> prefabSizes = new Dictionary<GameObject, Vector3>();


    void Start()
    {
        if (startPoint != null)
            currentHeight = startPoint.position.y;

        GenerateInitialMap();
    }

    void GenerateInitialMap()
    {
        for (int i = 0; i < initialSegments; i++)
            SpawnNextSegment();
    }

    void SpawnNextSegment()
    {
        // Elegimos tipo de bloque
        GameObject[] pool;
        int type = Random.Range(0, 3);
        if (type == 0) pool = platformPrefabs;
        else if (type == 1) pool = climbPrefabs;
        else pool = obstaclePrefabs;

        if (pool.Length == 0) return;

        GameObject prefab = pool[Random.Range(0, pool.Length)];

        // Tamaño del bloque (cacheado)
        Vector3 size = GetCachedPrefabSize(prefab);
        float blockHeight = size.y;

        // Movimiento en espiral
        float verticalOffset = Random.Range(minVerticalGap, maxVerticalGap) + blockHeight;
        currentHeight += verticalOffset;

        currentAngle += angleStep;
        float rad = currentAngle * Mathf.Deg2Rad;

        float x = Mathf.Cos(rad) * spiralRadius;
        float z = Mathf.Sin(rad) * spiralRadius;
        Vector3 spawnPos = new Vector3(x, currentHeight, z);

        // Verificar superposición con bloques anteriores
        if (IsOverlapping(spawnPos, size))
            spawnPos.y += blockHeight * 1.5f; // empuja hacia arriba si se solapan

        // Instanciamos el bloque
        GameObject newSegment = Instantiate(prefab, spawnPos, prefab.transform.rotation, transform);

        spawnedSegments.Add(newSegment);
    }

    bool IsOverlapping(Vector3 position, Vector3 size)
    {
        foreach (GameObject seg in spawnedSegments)
        {
            if (!seg) continue;

            Vector3 otherPos = seg.transform.position;
            Vector3 otherSize = GetObjectSize(seg);

            // Chequeo de AABB simple
            if (Mathf.Abs(position.x - otherPos.x) < (size.x + otherSize.x) * 0.5f &&
                Mathf.Abs(position.y - otherPos.y) < (size.y + otherSize.y) * 0.5f &&
                Mathf.Abs(position.z - otherPos.z) < (size.z + otherSize.z) * 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    // Tamaño exacto en base a renderers
    Vector3 GetObjectSize(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return Vector3.one;

        Bounds total = renderers[0].bounds;
        foreach (var r in renderers) total.Encapsulate(r.bounds);
        return total.size;
    }

    // Tamaño cacheado de prefabs
    Vector3 GetCachedPrefabSize(GameObject prefab)
    {
        if (!prefabSizes.ContainsKey(prefab))
        {
            // Instancia temporal sólo una vez
            GameObject temp = Instantiate(prefab);
            Vector3 size = GetObjectSize(temp);
            prefabSizes[prefab] = size;
            DestroyImmediate(temp);
        }
        return prefabSizes[prefab];
    }

    // Para generar más bloques cuando el jugador sube
    public void GenerateMore(int count = 5)
    {
        for (int i = 0; i < count; i++)
            SpawnNextSegment();
    }
}