using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DevilScript : MonoBehaviour
{

    public string playerTag = "Player"; // Tag used to identify the player in the scene
    public GameObject reticle; // Reference to the reticle prefab
    public float trackingSpeed = 2.0f; // Speed at which the reticle tracks the player
    private float attackDelay; // Delay before the enemy attacks
    public float maxAttackDelay = 2.0f;
    public float minAttackDelay = 1.5f;
    public float health = 1f;
    Rigidbody2D rb;
    Rigidbody2D playerRb;
    private Transform player; // Reference to the player's transform
    private bool isAttacking = false;
    private bool canShoot;
    private bool seesPlayer;
    public GameObject Spell;
    public float moveSpeed = 10f; // Speed at which the enemy moves away from the player
    public float maxMoveSpeed = 10f;
    public float minMoveSpeed = 5f;
    public float moveCooldown = .5f;


    public LayerMask obstructionMask;

    SpriteRenderer staffRenderer;
    Collider2D staffCollider; 
    Vector2 direction;
    bool canMove;
    private Vector2 devilPos;
    private Vector2 staffPos;
    Transform TransStaff;
    public int behaviorTimer;
    GameObject Staff;//Staff the gameobject the child of this guy
    GameObject StaffChild;//Staff's Visual Child
    //private bool canMoveLeft = true;
    //private bool canMoveRight = true;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        
        TransStaff = transform.GetChild(0);
        Staff = TransStaff.gameObject;
        Transform TransStaffChild = TransStaff.GetChild(0);
        StaffChild = TransStaffChild.gameObject;
        staffCollider = Staff.GetComponent<Collider2D>();
        staffRenderer = StaffChild.GetComponent<SpriteRenderer>();
        
        canShoot = true;
        canMove = true;
        behaviorTimer = 180;
        FindPlayer();

    }


    void Update()
    {
        FindPlayer();
        //Debug.Log("searching for player");
        devilPos = new Vector2(transform.position.x, transform.position.y);//Player's Position in 2D
        staffPos = new Vector2(TransStaff.position.x, TransStaff.position.y);
        direction = (devilPos - staffPos); 
        if (player != null && !isAttacking)
        {
            //Debug.Log("tracking player");
        }
        //WhatDoIDo();
        if (health <= 0)
        {
            Destroy(gameObject);
        }

    }
    void FixedUpdate()
    {
        
        behaviorTimer--;
        if (behaviorTimer <= 0)
        {
            Debug.Log("picking a behavior");
            DetermineBehavior();
        }
    }


    void FindPlayer()
    {
        GameObject playerObject = FindClosestPlayer();
        player = playerObject.transform;
        playerRb = playerObject.GetComponent<Rigidbody2D>();
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
        float fixedDistance = 1.5f; // Adjust this value as needed

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

    void ReticleFaceJumpDirection(Vector2 directionToGo)
    {
        // Set a fixed distance for the reticle from the object
        float fixedDistance = 1.5f;

        // Calculate the inverse of the direction the object is moving in
        Vector3 inverseForceDirection = -new Vector3(directionToGo.x, directionToGo.y, 0);

          // Update the reticle's position based on the inverse direction and fixed distance
        Vector3 newPosition = reticle.transform.parent.position + inverseForceDirection * fixedDistance;
        reticle.transform.position = newPosition;

        // Make the reticle face in the direction of the jump (or movement)
        float angle = Mathf.Atan2(directionToGo.y, directionToGo.x) * Mathf.Rad2Deg;
        reticle.transform.rotation = Quaternion.Euler(0, 0, angle);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Spell" || collision.gameObject.tag == "BigSpell")
        {
            Vector2 Knockback;
            Knockback = collision.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
            rb.velocity += Knockback * 5000 * Time.fixedDeltaTime;
            health--;
        }
    }

    public void Attack()
    {
        CheckLineOfSight();
        if (canShoot && seesPlayer)
        {
            //Debug.Log("Im shooting");
            GameObject projectile = Instantiate(Spell, reticle.transform.position + reticle.transform.right * 2f, Quaternion.identity, transform);
            projectile.transform.localScale = Spell.transform.localScale;


            staffCollider.enabled = false;
            canShoot = false;

            StartCoroutine(ShootCooldown());
        }
    }

    IEnumerator ShootCooldown()
    {
        attackDelay = Random.Range(minAttackDelay, maxAttackDelay);
        yield return new WaitForSeconds(attackDelay);

        canShoot = true;
    }

    void CheckLineOfSight()
    {
        // Calculate direction from enemy to player
        Vector2 directionToPlayer = (player.position - transform.position).normalized;


        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Debug.DrawRay(transform.position, directionToPlayer * distanceToPlayer, Color.red);
        // Cast a ray from enemy to player in 2D
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstructionMask);
       // Debug.Log("SPOPOKY");
        // Check if the ray hit anything
        if (hit.collider != null)
        {

            // The ray hit an obstacle before reaching the player
            seesPlayer = false;
        }
        else
        {
            seesPlayer = true;
        }
       // Debug.Log(canShoot);
    }




    public void MoveAndBlock()
    {
        if (canMove)
        {
            //Debug.Log("moving");
            // Get the current velocity
            Vector2 currentVelocity = rb.velocity;

            // Normalize the velocity components to the range 0-1 based on magnitude
            float xFactor = Mathf.Clamp01(Mathf.Abs(currentVelocity.x) / 50f);
            float yFactor = Mathf.Clamp01(Mathf.Abs(currentVelocity.y) / 50f);

            // Determine how much the force should be scaled based on the direction of velocity
            float xScale = (currentVelocity.x * direction.x > 0) ? (1f - xFactor) : 0.7f; // 0.9f weakens the force slightly even when not aligned
            float yScale = (currentVelocity.y * direction.y > 0) ? (1f - yFactor) : 0.7f; // Adjust this value as needed

            // Apply the scaled force in the direction
            Vector2 adjustedDirection = new Vector2(direction.x * xScale, direction.y * yScale);

            // Apply the jump force
            rb.velocity += adjustedDirection * 2500 * Time.fixedDeltaTime;

            staffCollider.enabled = true;
            staffRenderer.enabled = true;
            canMove = false;

            StartCoroutine(MovementCooldown());
        }
    }

    public void MoveAndBlock(Vector2 myDirection)
    {
        if (canMove)
        {
            //Debug.Log("moving");
            // Get the current velocity
            Vector2 currentVelocity = rb.velocity;

            // Normalize the velocity components to the range 0-1 based on magnitude
            float xFactor = Mathf.Clamp01(Mathf.Abs(currentVelocity.x) / 50f);
            float yFactor = Mathf.Clamp01(Mathf.Abs(currentVelocity.y) / 50f);

            // Determine how much the force should be scaled based on the direction of velocity
            float xScale = (currentVelocity.x * myDirection.x > 0) ? (1f - xFactor) : 0.7f; // 0.9f weakens the force slightly even when not aligned
            float yScale = (currentVelocity.y * myDirection.y > 0) ? (1f - yFactor) : 0.7f; // Adjust this value as needed

            // Apply the scaled force in the direction
            Vector2 adjustedDirection = new Vector2(myDirection.x * xScale, myDirection.y * yScale);

            // Apply the jump force
            rb.velocity += adjustedDirection * 2500 * Time.fixedDeltaTime;

            staffCollider.enabled = true;
            staffRenderer.enabled = true;
            canMove = false;
            //reticle.SetActive(true);


            StartCoroutine(MovementCooldown());
        }
    }
     
    IEnumerator MovementCooldown()
    {
        yield return new WaitForSeconds(moveCooldown);
        canMove = true;
        staffCollider.enabled = false;
        staffRenderer.enabled = false;
        //reticle.SetActive(false);
    }



    void DetermineBehavior()
    {
        float r = Random.Range(0f, 1f);



        if (transform.position.x < -48)
        {
            StartCoroutine(moveRight());
        }
        else if (transform.position.x > 48)
        {
            StartCoroutine(moveLeft());
        }
        else if (transform.position.y < -20)
        {
            Debug.Log("we are too low");
            StartCoroutine(TakeOff());
        }
        else if(player.position.y < -20)
        {
            StartCoroutine(BombingRun());
        }
       
        else if (r < .25f)
        {
            StartCoroutine(AllOutAttack());
        }
        else if (r < .5f)
        {
            StartCoroutine(Hover());
        }
        else if (r < .75f)
        {
            StartCoroutine(Dodge());
        }
        else
        {
            StartCoroutine(Retreat());
        }
    }


    IEnumerator AllOutAttack()
    {
        Debug.Log("all out attacking");
        float myDegrees;
        Vector2 directionToGo;
        behaviorTimer = 360;
        moveCooldown = 1f;
        maxAttackDelay = 1f;
        minAttackDelay = .75f;
        while (behaviorTimer > 0f)
        {

            myDegrees = Random.Range(80f, 100f);
            directionToGo = DegreesToVector2(myDegrees);
            if (canMove)
            {
                ReticleFaceJumpDirection(directionToGo);
                MoveAndBlock(directionToGo);
            }
            if (canShoot)
            {
                TrackPlayer();
                Attack();
            }
            yield return null;
        }
    }

    IEnumerator Hover()
    {
        Debug.Log("hovering");
        float myDegrees;
        Vector2 directionToGo;
        behaviorTimer = 360;
        moveCooldown = .7f;
        maxAttackDelay = 1.7f;
        minAttackDelay = 1.2f;
        while (behaviorTimer > 0f)
        {
            
            myDegrees = Random.Range(70f, 110f);
            directionToGo = DegreesToVector2(myDegrees);
            if (canMove)
            {
                ReticleFaceJumpDirection(directionToGo);
                MoveAndBlock(directionToGo);
            }
            if (canShoot)
            {
                TrackPlayer();
                Attack();
            }
            yield return null;
        }
    }

    IEnumerator Dodge()
    {
        Debug.Log("dodging");
        behaviorTimer = 240;
        moveCooldown = .7f;
        maxAttackDelay = 1.7f;
        minAttackDelay = 1.2f;
        float myDegrees;
        float upOrDown;
        Vector2 directionToGo;
        while (behaviorTimer > 0f)
        {
            upOrDown = Random.Range(0f, 1f);
            Vector3 directionToPlayer = player.position - reticle.transform.parent.position;
            float angleInRadians = Mathf.Atan2(directionToPlayer.x, directionToPlayer.y);
            float angleInDegrees = angleInRadians * Mathf.Rad2Deg;
            if (upOrDown < .5f)
            {
                myDegrees = angleInDegrees + Random.Range(-10f, 10f);
            }
            else
            {
                myDegrees = (angleInDegrees * -1) + Random.Range(-10f, 10f);
            }
            directionToGo = DegreesToVector2(myDegrees);
            if (canMove)
            {
                ReticleFaceJumpDirection(directionToGo);
                MoveAndBlock(directionToGo);
            }
            if (canShoot)
            {
                TrackPlayer();
                Attack();
            }
            yield return null;
        }
    }

    IEnumerator Retreat()
    {
        Debug.Log("retreating");
        behaviorTimer = 180;
        moveCooldown = .5f;
        maxAttackDelay = 2.0f;
        minAttackDelay = 1.5f;
        while (behaviorTimer > 0f)
        {
            if (canMove)
            {
                TrackPlayer();
                MoveAndBlock();
            }
            if (canShoot)
            {
                TrackPlayer();
                Attack();
            }
            yield return null;
        }
    }

    IEnumerator BombingRun()
    {
        Debug.Log("Bombing Run");
        behaviorTimer = 1800;
        moveCooldown = .5f;
        maxAttackDelay = .6f;
        minAttackDelay = .3f;
        int stage = 0;
        Vector2 corner1 = new Vector2(0,0);
        Vector2 corner2 = new Vector2(0, 0);
        Vector2 directionToGo;
        float angleInRadians;
        float angleInDegrees;
        while (behaviorTimer > 0)
        {
            
            if (stage == 0)
            {
                if (transform.position.x < 0)
                {
                    corner1.x = -50;
                    corner1.y = 26;
                    corner2.x = 50;
                    corner2.y = 26;
                }
                else
                {
                    corner1.x = 50;
                    corner1.y = 26;
                    corner2.x = -50;
                    corner2.y = 26;
                }
                Debug.Log("stage 0 active");
                angleInRadians = Mathf.Atan2(corner1.x, corner1.y);
                angleInDegrees = (angleInRadians * Mathf.Rad2Deg) - 180;
                angleInDegrees += Random.Range(-10f, 10f);
                directionToGo = DegreesToVector2(angleInDegrees);
                if (canMove)
                {
                    ReticleFaceJumpDirection(directionToGo);
                    MoveAndBlock(directionToGo);
                }
                if((transform.position.x >= 45 && transform.position.y >= 20) || (transform.position.x <= -45 && transform.position.y >= 20))
                {
                    stage = 1;
                }
            }

            else if(stage == 1)
            {
                Debug.Log("stage 1 active");
                angleInRadians = Mathf.Atan2(corner2.x, corner2.y);
                angleInDegrees = (angleInRadians * Mathf.Rad2Deg);
                angleInDegrees += Random.Range(-10f, 10f);
                directionToGo = DegreesToVector2(angleInDegrees);
                if (canMove)
                {
                    ReticleFaceJumpDirection(directionToGo);
                    MoveAndBlock(directionToGo);
                }
                if (canShoot)
                {
                    TrackPlayer();
                    Attack();
                }
                Debug.Log((corner2.x == 50));
                Debug.Log(corner2.x);
                if ((corner2.x == 50 && transform.position.x >= 45) || (corner2.x == -50 && transform.position.x <= -45))
                {
                    Debug.Log("stages complete");
                    stage = 3;
                    behaviorTimer = 0;
                    yield break;
                }
            }
            yield return null;
        }
        
    }

    IEnumerator TakeOff()
    {
        float myDegrees;
        Vector2 directionToGo;

        // Continue moving until the y-position is 0 or higher
        moveCooldown = .5f;
        while (transform.position.y < 0)
        {

            myDegrees = Random.Range(60f, 120f);
            directionToGo = DegreesToVector2(myDegrees);
            if (canMove)
            {
                ReticleFaceJumpDirection(directionToGo);
                MoveAndBlock(directionToGo);
            }
            yield return null;  // Wait until the next frame before continuing
        }
        behaviorTimer = 0;
    }

    IEnumerator moveRight()
    {
        float myDegrees;
        Vector2 directionToGo;

        // Continue moving until the y-position is 0 or higher
        moveCooldown = .5f;
        while (transform.position.x < -38)
        {

            myDegrees = Random.Range(-30f, 30f);
            directionToGo = DegreesToVector2(myDegrees);
            if (canMove)
            {
                ReticleFaceJumpDirection(directionToGo);
                MoveAndBlock(directionToGo);
            }
            yield return null;  // Wait until the next frame before continuing
        }
        behaviorTimer = 0;
    }

    IEnumerator moveLeft()
    {
        float myDegrees;
        Vector2 directionToGo;

        // Continue moving until the y-position is 0 or higher
        moveCooldown = .5f;
        while (transform.position.x < -38)
        {

            myDegrees = Random.Range(150f, 210f);
            directionToGo = DegreesToVector2(myDegrees);
            if (canMove)
            {
                ReticleFaceJumpDirection(directionToGo);
                MoveAndBlock(directionToGo);
            }
            yield return null;  // Wait until the next frame before continuing
        }
        behaviorTimer = 0;
    }

    public Vector2 DegreesToVector2(float degrees)
    {
        // Convert degrees to radians
        float radians = degrees * Mathf.Deg2Rad;

        // Calculate x and y components
        float x = Mathf.Cos(radians);
        float y = Mathf.Sin(radians);

        // Return the Vector2
        return new Vector2(x, y).normalized;
    }
}

