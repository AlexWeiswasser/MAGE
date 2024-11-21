using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePlayer : MonoBehaviour
{
    public GameObject PlayerPrefab;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(PlayerPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
