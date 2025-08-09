using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    private const float PLAYER_DISTANCE_SPAWN_Platform_PART = 20f;

    [SerializeField] private Transform platform_Start;
    [SerializeField] private List<Transform> platform_list;
    [SerializeField] private Transform player;

    private Vector3 lastEndPosition;
    private List<Transform> spawnedPlatforms = new List<Transform>();

    private void Awake()
    {

        lastEndPosition = platform_Start.Find("End").position;

        int startingSpawnPlatforms = 5;
        spawnedPlatforms.Add(platform_Start);
        for (int i = 0; i < startingSpawnPlatforms; i++)
        {
            SpawnPlatform();
        }
    }

    private void Update()
    {
        if (Vector3.Distance(player.position, lastEndPosition) < PLAYER_DISTANCE_SPAWN_Platform_PART)
        {
            SpawnPlatform();
        }

        CleanupOldPlatformParts();
    }


    private void SpawnPlatform()
    {
        Transform chosenPlatformPart = platform_list[Random.Range(0, platform_list.Count)];


        float gap = Random.Range(2f, 3f);

        Vector3 prevEndPos = lastEndPosition;

        float newY = prevEndPos.y + Random.Range(-1.5f, 1.5f);
        Transform start = chosenPlatformPart.Find("Start");
        if (start == null)
        {
            Debug.LogError("Platform prefab missing 'Start' child: " + chosenPlatformPart.name);
            return;
        }


        Vector3 spawnPos = new Vector3(prevEndPos.x + gap, newY, prevEndPos.z) - start.localPosition;

        Transform lastPlatformPartTransform = SpawnPlatform(chosenPlatformPart, spawnPos);
        Transform end = lastPlatformPartTransform.Find("End");
        if (end == null)
        {
            Debug.LogError("Platform prefab missing 'End' child: " + lastPlatformPartTransform.name);
            return;
        }
        lastEndPosition = end.position;
        spawnedPlatforms.Add(lastPlatformPartTransform);
    }

    private Transform SpawnPlatform(Transform PlatformPart, Vector3 spawnPosition)
    {
        Transform PlatformPartTransform = Instantiate(PlatformPart, spawnPosition, Quaternion.identity);
        return PlatformPartTransform;
    }

    private void CleanupOldPlatformParts()
    {
        float leftEdge = player.position.x - 30f;
        for (int i = spawnedPlatforms.Count - 1; i >= 0; i--)
        {
            if (spawnedPlatforms[i] == null) continue;
            if (spawnedPlatforms[i].position.x < leftEdge)
            {
                Destroy(spawnedPlatforms[i].gameObject);
                spawnedPlatforms.RemoveAt(i);
            }
        }
    }
}