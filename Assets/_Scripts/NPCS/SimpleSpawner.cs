using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleSpawner : MonoBehaviour
{
    [SerializeField] LevelManager levelManager;
    [Header("Spawn Points")]
    [SerializeField] private Transform[] lanes; // asignar 4 en inspector

    [Header("Enemy")]
    [SerializeField] private GameObject npcPrefab;

    [Header("Pool")]
    [SerializeField] private int poolSize = 20;
    private List<GameObject> pool = new List<GameObject>();


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
            SpawnPattern pattern = levelManager.CurrentLevel.levelPatterns[Random.Range(0, levelManager.CurrentLevel.levelPatterns.Length)];
            Debug.LogWarning("Spawn Pattern: " + pattern.name);
            yield return StartCoroutine(SpawnPatternRoutine(pattern));

            yield return new WaitForSeconds(levelManager.CurrentLevel.timeBetweenWaves);
        }
    }

    IEnumerator SpawnPatternRoutine(SpawnPattern pattern)
    {
        foreach (var spawn in pattern.spawns)
        {
            yield return new WaitForSeconds(spawn.delay);
            SpawnEnemyInLane(spawn.laneIndex);
        }
    }

    void SpawnEnemyInLane(int laneIndex)
    {
        GameObject enemy = GetFromPool();

        Transform lane = lanes[laneIndex];

        enemy.transform.SetParent(lane);

        RectTransform rect = enemy.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        enemy.SetActive(true);
        StartCoroutine(ReturnToPoolAfterTime(enemy, 8f));
    }

    IEnumerator ReturnToPoolAfterTime(GameObject enemy, float time)
    {
        yield return new WaitForSeconds(time);

        if (enemy.activeInHierarchy) // evita errores si ya se desactivó
        {
            enemy.SetActive(false);
        }
    }
}
