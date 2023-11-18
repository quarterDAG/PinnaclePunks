using Spine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DropBat : MonoBehaviour, ICharacter
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float speed;
    [SerializeField] private List<GameObject> dropPrefabs;
    [SerializeField] private float minDropInterval = 1f;
    [SerializeField] private float maxDropInterval = 5f;

    private ParticleSystem ps;
    private MeshRenderer meshRenderer;
    private float dropTimer;

    private int health = 50;
    private int maxHealth = 50;
    private bool IsDead;




    void Start ()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        ps = GetComponentInChildren<ParticleSystem>();
        StartCoroutine(MoveBetweenPoints());
        dropTimer = Random.Range(minDropInterval, maxDropInterval);
    }

    void Update ()
    {
        if (!IsDead)
        {
            dropTimer -= Time.deltaTime;
            if (dropTimer <= 0f)
            {
                DropRandomPrefab();
                dropTimer = Random.Range(minDropInterval, maxDropInterval);
            }
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


    public void TakeDamage ( int damage, int shooterIndex )
    {
        if (IsDead) return;

        health -= damage;
        ps.Play();

        if (health <= 0)
        {
            Die(shooterIndex);
        }
    }

    public void Die ( int _shooterIndex )
    {
        meshRenderer.enabled = false;
        IsDead = true;
        Respawn();
    }


    async void Respawn ()
    {
        await Task.Delay(5000);

        transform.position = respawnPoint.position;
        IsDead = false;
        meshRenderer.enabled = true;
        health = maxHealth;
    }
}
