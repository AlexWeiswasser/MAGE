using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class ProjectileCode : MonoBehaviour
{

    Vector2 PlayerPos;
    Vector2 ProjectilePos; 
    Vector2 Direction;

    GameObject Player;

    public GameObject ExplosionParticle;
    public GameObject TrailParticle;
	public GameObject DamagedParticle;
	public GameObject FullHealthRock;
	public GameObject SemiHurtRock;
	public GameObject VeryHurtRock;

    Transform Transparent;

    Rigidbody2D rb;

	public int prefabIndex;

    float bounceAmount;

    bool iframes = false;

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
        if (prefabIndex + 1 == 1)//It's A Fireball!
        {
            Instantiate(TrailParticle, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
            Vector3 Direction = Player.transform.position - transform.position;
            float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            rb.velocity = transform.right * -6000 * Time.fixedDeltaTime;
            StartCoroutine(CreateParticle());
        }
		if (prefabIndex + 1 == 2)//It's A Thunderball!
		{
			Vector3 Direction = Player.transform.position - transform.position;
			float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
			rb.velocity = transform.right * -6000 * Time.fixedDeltaTime;
			Instantiate(TrailParticle, new Vector3(transform.position.x, transform.position.y), this.transform.rotation);
			StartCoroutine(CreateParticle());
		}
		if (prefabIndex + 1 == 3)//It's A Harry!
		{
			Instantiate(TrailParticle, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
			Vector3 Direction = Player.transform.position - transform.position;
			float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
			rb.velocity = transform.right * -6000 * Time.fixedDeltaTime;
			StartCoroutine(CreateParticle());
		}
		if (prefabIndex + 1 == 4)//It's A Unknown!
		{
			Instantiate(TrailParticle, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
			Vector3 Direction = Player.transform.position - transform.position;
			float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
			rb.velocity = transform.right * -6000 * Time.fixedDeltaTime;
			StartCoroutine(CreateParticle());
		}
		if (prefabIndex + 1 == 5)//It's A Rock!
		{
			Vector3 Direction = Player.transform.position - transform.position;
			float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
			rb.velocity = transform.right * -4000 * Time.fixedDeltaTime;
            bounceAmount = 2;
			iframes = true;
			StartCoroutine(IframeCooldown());
		}

		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

		foreach (GameObject player in players)
		{
			
			Collider2D[] colliders = player.GetComponents<Collider2D>();

			foreach (Collider2D col in colliders)
			{
				// Check if the collider is not a trigger
				if (!col.isTrigger)
				{
					// Ignore collision for the non-trigger collider
					Physics2D.IgnoreCollision(col, GetComponent<Collider2D>());
				}
			}
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Evap")
        {
            Destroy(gameObject);
		}
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Enemy")
        {
            if (collision.gameObject != Player) 
            {
				Destroy(gameObject);
            }  
        }
        if(collision.gameObject.tag == "Spell" || collision.gameObject.tag == "Kick")
		{
            if (Player.transform == collision.transform.parent || iframes)
            {
                return;
            }
            else if(prefabIndex + 1 == 5 && bounceAmount > 0)
            {		
				Bounced();
			}
			else
            {
                Instantiate(ExplosionParticle, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
				Destroy(gameObject);
            }
        }
    }
	private void OnCollisionEnter2D(Collision2D collision)
	{
        if (!iframes)
        {
            if (bounceAmount > 0)
            {
				Bounced();
            }
            else if (prefabIndex + 1 == 5)
            {
				Instantiate(ExplosionParticle, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
				Destroy(gameObject);
			}
        }
	}
	IEnumerator CreateParticle()
    {
        yield return new WaitForSeconds(Random.Range(0.02f, 0.04f));
        Instantiate(TrailParticle, new Vector3(transform.position.x, transform.position.y), this.transform.rotation);
        StartCoroutine(CreateParticle());
    }
	public void SetPrefabIndex(int index)
	{
		prefabIndex = index;
	}
    public int GetPrefabIndex()
    {
        return prefabIndex;
    }
	IEnumerator IframeCooldown()
	{
        yield return new WaitForSeconds(.1f);
		iframes = false;
	}
	void Bounced()
	{
		if (bounceAmount == 2)
		{
			FullHealthRock.GetComponent<SpriteRenderer>().enabled = false; 
			SemiHurtRock.GetComponent<SpriteRenderer>().enabled = true;
			Instantiate(DamagedParticle, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
		}
		else if (bounceAmount == 1)
		{
			SemiHurtRock.GetComponent<SpriteRenderer>().enabled = false;
			VeryHurtRock.GetComponent <SpriteRenderer>().enabled = true;
			Instantiate(DamagedParticle, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
		}
		bounceAmount--;
		iframes = true;
		StartCoroutine(IframeCooldown());
	}
}
