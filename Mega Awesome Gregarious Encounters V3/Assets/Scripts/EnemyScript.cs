using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{

    public string playerTag = "Player"; // Tag used to identify the player in the scene
    public GameObject reticle; // Reference to the reticle prefab
    public float trackingSpeed = 2.0f; // Speed at which the reticle tracks the player
    public float attackDelay = 1.5f; // Delay before the enemy attacks
    Rigidbody2D rb;
    private Transform player; // Reference to the player's transform
    private bool isAttacking = false;
    private bool canShoot;
    public GameObject Spell;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        canShoot = true;
        // Attempt to find the player in the scene
        FindPlayer();

    }

    void Update()
    {
            FindPlayer();
            //Debug.Log("searching for player");
        

        if (player != null && !isAttacking)
        {
            TrackPlayer();
            //Debug.Log("tracking player");
        }
        Attack();

    }

    void FindPlayer()
    {
        GameObject playerObject = FindClosestPlayer();
            player = playerObject.transform;
    }
    public GameObject FindClosestPlayer()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    void TrackPlayer()
    {
        // Ensure the reticle is active
        if (!reticle.activeInHierarchy)
        {
            reticle.SetActive(true);
        }

        // Define the fixed distance from the parent (enemy)
        float fixedDistance = 2.0f; // Adjust this value as needed

        // Calculate the direction from the enemy to the player
        Vector3 directionToPlayer = player.position - reticle.transform.parent.position;
        directionToPlayer.Normalize();

        // Ensure the reticle is within a reasonable range
        Vector3 newPosition = reticle.transform.parent.position + directionToPlayer * fixedDistance;


        // Set the reticle's position to be a fixed distance from the enemy along the direction to the player
        reticle.transform.position = newPosition;

        // Make the reticle face the player (for 2D, adjust LookAt to only rotate around Z-axis)
        Vector3 lookDirection = player.position - reticle.transform.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        reticle.transform.rotation = Quaternion.Euler(0, 0, angle);

        //Debug.Log("Reticle tracking the player at a fixed distance from the enemy.");
    }

    public void StartAttack()
    {
        if (player == null) 
            return;

        isAttacking = true;
        Invoke(nameof(AttackPlayer), attackDelay);
    }

    void AttackPlayer()
    {
        //add attack here
        //Debug.Log("Enemy attacks!");

        // Reset for the next attack
        isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Spell")
        {
            Vector2 Knockback;
            Knockback = collision.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
            rb.velocity += Knockback * 5000 * Time.fixedDeltaTime;
        }
    }

    public void Attack()
    {
        if (canShoot)
        {
            //Debug.Log("Im shooting");
            GameObject projectile = Instantiate(Spell, reticle.transform.position + reticle.transform.right * 2f, Quaternion.identity, transform);
            projectile.transform.localScale = Spell.transform.localScale;
            


            canShoot = false;

            StartCoroutine(ShootCooldown());
        }
    }

    IEnumerator ShootCooldown()
    {
        yield return new WaitForSeconds(attackDelay);
        canShoot = true;
    }
}
