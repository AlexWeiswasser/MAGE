using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellHitboxCode : MonoBehaviour
{
    public GameObject[] ExplosionParticles;
    public int prefabIndex;

	private void Start()
	{
        prefabIndex = this.GetComponentInParent<ProjectileCode>().GetPrefabIndex();
	}
	private void OnTriggerEnter2D(Collider2D collision)
    {
		
		if (collision.gameObject.tag == "Floor")
        {
			Instantiate(ExplosionParticles[prefabIndex], new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
            Destroy(this.transform.parent.gameObject);
        }
    }

}
