using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigCode : MonoBehaviour
{
    Vector2 PlayerPos;
    Vector2 ProjectilePos;
    Vector2 Direction;

    GameObject Player;

    public GameObject FireballSpawn;
    public GameObject FireballParticles;

    Transform Transparent;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Transparent = transform.parent;
        Player = Transparent.gameObject;
        transform.parent = null;
        transform.rotation = Quaternion.identity;
    }
    // Start is called before the first frame update
    void Start()
    {
        Vector3 Direction = Player.transform.position - transform.position;
        float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        rb.velocity = transform.right * -10000 * Time.fixedDeltaTime;
        StartCoroutine(CreateParticle());
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Enemy")
        {
            if (collision.gameObject != Player)
            {
                Instantiate(FireballParticles, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
    IEnumerator CreateParticle()
    {
        //0.025f
        yield return new WaitForSeconds(Random.Range(0.02f, 0.04f));
        Instantiate(FireballSpawn, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
        StartCoroutine(CreateParticle());
    }
}
