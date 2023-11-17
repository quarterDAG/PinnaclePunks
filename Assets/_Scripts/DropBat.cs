using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropBat : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed;
    [SerializeField] private List<GameObject> dropPrefabs;
    [SerializeField] private float minDropInterval = 1f;
    [SerializeField] private float maxDropInterval = 5f;

    private float dropTimer;


    void Start ()
    {
        StartCoroutine(MoveBetweenPoints());
        dropTimer = Random.Range(minDropInterval, maxDropInterval);
    }

    void Update ()
    {
        dropTimer -= Time.deltaTime;
        if (dropTimer <= 0f)
        {
            DropRandomPrefab();
            dropTimer = Random.Range(minDropInterval, maxDropInterval);
        }
    }

    private IEnumerator MoveBetweenPoints ()
    {
        while (true)
        {
            yield return StartCoroutine(MoveToPoint(pointB.position));
            yield return StartCoroutine(MoveToPoint(pointA.position));
        }
    }

    private IEnumerator MoveToPoint ( Vector3 target )
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            // Determine the direction of movement
            Vector3 direction = target - transform.position;
            // Flip the sprite based on the direction
            if (direction.x > 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (direction.x < 0)
                transform.localScale = new Vector3(1, 1, 1);

            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
            yield return null;
        }
    }
    private void DropRandomPrefab ()
    {
        if (dropPrefabs.Count == 0) return;

        int randomIndex = Random.Range(0, dropPrefabs.Count);
        GameObject drop = Instantiate(dropPrefabs[randomIndex], transform.position, Quaternion.identity);
    }
}
