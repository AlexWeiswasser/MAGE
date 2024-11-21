using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDetector : MonoBehaviour
{
    public Vector2 pointA = new Vector2(-51, -35); 
    public Vector2 pointB = new Vector2(51, 35); 
    public string targetTag = "Player"; 

    void Update()
    {
        Collider2D[] collidersInArea = Physics2D.OverlapAreaAll(pointA, pointB);

        int count = 0;

        // Iterate through the colliders and count the ones with the specified tag
        foreach (Collider2D collider in collidersInArea)
        {
            if (collider.CompareTag(targetTag))
            {
                count++;
            }
        }

        if (count <= 2)
        {
            StartCoroutine(BackToGame());
        }
    }

    IEnumerator BackToGame()
    {
        yield return new WaitForSeconds(2f);
		SceneManager.LoadScene(1);
	}
}
