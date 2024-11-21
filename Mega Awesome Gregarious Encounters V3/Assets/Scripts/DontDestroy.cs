using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
	private void Awake()
	{
		if (GameObject.Find("Manager") != null)
		{
			Destroy(this);
		}
	}
	// Start is called before the first frame update
	void Start()
    {
		GameObject.DontDestroyOnLoad(this);
    } 
}
