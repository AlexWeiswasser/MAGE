using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuttingRockCode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		StartCoroutine(DestroyRock());
	}
	IEnumerator DestroyRock()
	{
		yield return new WaitForSeconds(.5f);
        //crumble particles
        Destroy(gameObject);
	}

}
