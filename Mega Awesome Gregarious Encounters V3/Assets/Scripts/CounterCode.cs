using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;

public class CounterCode : MonoBehaviour
{

    GameObject Player;
    public GameObject BigSpell;

    Rigidbody2D rb;

    float StandStillTimer;

    bool Parry;

    public TextMeshProUGUI CounterText;

    Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        Transform TransPlayer = transform.parent;
        Player = TransPlayer.gameObject;
        canvas = FindObjectOfType<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Spell" || collision.gameObject.tag == "BigSpell")
        {
            transform.parent.GetComponent<PlayerCode>().CollisionDetected(this);
        }
    }

}
