using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfinitePlatformGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> platformPrefabs; 
    [SerializeField] private float levelWidth = 3f;
    [SerializeField] private float minY = .2f;
    [SerializeField] private float maxY = 1.5f;
    [SerializeField] private GameObject safeZone;
    [SerializeField] private float safeZoneSpeed = 0.5f;
    [SerializeField] private float firstSpawnYIncrement;
    private float nextSpawnY;
    private List<GameObject> platforms = new List<GameObject>();
    private float spawnTimer = 0f;
    [SerializeField] private float fadeDuration = 2f; // Duration for the platform to completely fade out and the interval for spawning new platforms

    [SerializeField] private PlayerSpawner spawner;

    void Start ()
    {
        nextSpawnY = safeZone.transform.position.y + firstSpawnYIncrement;
        SpawnPlatform(); // Spawn the first platform immediately
    }

    void Update ()
    {
        MoveSafeZone();
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= fadeDuration)
        {
            spawnTimer = 0f;
            SpawnPlatform();
            spawner.UpdateAllPlayerRespawnPoints(); // Update respawn points after fading in
        }
    }

    void MoveSafeZone ()
    {
        safeZone.transform.position += new Vector3(0, safeZoneSpeed * Time.deltaTime, 0);
    }

    void SpawnPlatform ()
    {

        // Randomly select a platform prefab
        GameObject selectedPrefab = platformPrefabs[Random.Range(0, platformPrefabs.Count)];

        float platformXPosition = Random.Range(-levelWidth, levelWidth);
        Vector3 spawnPosition = new Vector3(platformXPosition, nextSpawnY, 0f);
        GameObject platform = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity, transform);
        platforms.Add(platform);
        platform.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f); // Start invisible
        nextSpawnY += Random.Range(minY, maxY);
        StartCoroutine(FadeIn(platforms[platforms.Count - 1])); // Fade in the last spawned platform
    }

    IEnumerator FadeIn ( GameObject platform )
    {
        yield return Fade(platform, 1f); // Fade in to opaque
    }

    IEnumerator Fade ( GameObject platform, float targetAlpha )
    {
        SpriteRenderer spriteRenderer = platform.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;

        Color startColor = spriteRenderer.color;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(startColor.a, targetAlpha, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public Vector3 GetLastPlatformPosition ()
    {
        if (platforms.Count > 0)
        {
            return platforms[platforms.Count - 1].transform.position;
        }
        return Vector3.zero; // Return a default value if no platforms are available
    }

}
