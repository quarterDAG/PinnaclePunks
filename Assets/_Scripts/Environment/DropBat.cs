using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropBat : MonoBehaviour, ICharacter
{
    [SerializeField] private GameObject flyingMonsterGO;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float speed;
    [SerializeField] private List<GameObject> dropPrefabs;
    [SerializeField] private float minDropInterval = 1f;
    [SerializeField] private float maxDropInterval = 5f;

    [SerializeField] private SpriteRenderer iceCube;

    private ParticleSystem ps;
    private float dropTimer;
    private int health = 50;
    private int maxHealth = 50;
    private bool isDead;
    private bool isFrozen;

    private Coroutine moveCoroutine;

    void Start ()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        moveCoroutine = StartCoroutine(MoveBetweenPoints());
        dropTimer = Random.Range(minDropInterval, maxDropInterval);
    }

    void Update ()
    {
        if (isDead || isFrozen) return;

        HandleDropTimer();
    }

    private void HandleDropTimer ()
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
        Vector3 target = pointA.position;

        while (!isDead)
        {
            if (transform.position == target)
            {
                target = target == pointA.position ? pointB.position : pointA.position;
            }

            FlipSprite(target);

            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }
    private void FlipSprite ( Vector3 target )
    {
        bool movingTowardsPointA = target == pointA.position;

        if (movingTowardsPointA)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }


    private void DropRandomPrefab ()
    {
        if (dropPrefabs.Count == 0) return;

        int randomIndex = Random.Range(0, dropPrefabs.Count);
        Instantiate(dropPrefabs[randomIndex], transform.position, Quaternion.identity);
    }

    public void TakeDamage ( int damage, int shooterIndex )
    {
        if (isDead) return;

        health -= damage;
        ps.Play();

        if (health <= 0)
        {
            Die(-1);
        }
    }

    public void Die ( int _shooterIndex )
    {
        flyingMonsterGO.SetActive(false);
        iceCube.enabled = false;
        isDead = true;
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine ()
    {
        yield return new WaitForSeconds(5f);

        transform.position = respawnPoint.position;
        isDead = false;
        flyingMonsterGO.SetActive(true);
        health = maxHealth;
        moveCoroutine = StartCoroutine(MoveBetweenPoints());
    }

    public void Freeze ( float duration )
    {
        if (!isFrozen && !isDead)
        {
            isFrozen = true;
            iceCube.enabled = true;
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            StartCoroutine(UnfreezeAfterDelay(duration));
        }
    }

    private IEnumerator UnfreezeAfterDelay ( float duration )
    {
        yield return new WaitForSeconds(duration);

        iceCube.enabled = false;
        isFrozen = false;
        if (!isDead) moveCoroutine = StartCoroutine(MoveBetweenPoints());
    }
}
