using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] lanes; // asignar 4 en inspector

    [Header("Enemy")]
    [SerializeField] private GameObject npcPrefab;

    [Header("Pool")]
    [SerializeField] private int poolSize = 20;
    private List<GameObject> pool = new List<GameObject>();

    [Header("Wave Settings")]
    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] private float timeBetweenSpawns = 0.5f;
    [SerializeField] private int carsPerWave = 5;

    private int currentWave = 0;

    void Start()
    {
        CreatePool();
        StartCoroutine(SpawnWaves());
    }

   
    void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(npcPrefab);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    
    GameObject GetFromPool()
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
                return obj;
        }

        // expandir pool si se queda corto
        GameObject newObj = Instantiate(npcPrefab);
        newObj.SetActive(false);
        pool.Add(newObj);
        return newObj;
    }

    //  Loop de oleadas
    IEnumerator SpawnWaves()
    {
        while (true)
        {
            currentWave++;
            int enemiesToSpawn = carsPerWave + currentWave; // escala dificultad

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(timeBetweenSpawns);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    // 🔹 Spawn individual
    void SpawnEnemy()
    {
        GameObject enemy = GetFromPool();

        int randomIndex = Random.Range(0, lanes.Length);
        Transform lane = lanes[randomIndex];

        enemy.transform.SetParent(lane); // 🔥 clave

        RectTransform rect = enemy.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero; // centrado en el carril

        enemy.SetActive(true);
    }
}
