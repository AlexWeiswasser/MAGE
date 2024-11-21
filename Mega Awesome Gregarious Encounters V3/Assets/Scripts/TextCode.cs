using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextCode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(CounterSpell());
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator CounterSpell()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
