using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TreeEditor;
using UnityEngine;

public class ScreenShakeCode : MonoBehaviour
{
    Vector3 before;
    float spellTimer = 0f;
    float bigSpellTimer = 0f;
    float shakeStrength;

	// Start is called before the first frame update
	void Start()
    {
		before = transform.position;
	}
    // Update is called once per frame
    void Update()
    {
	    if (bigSpellTimer > 0)
		{
			transform.position = transform.position + Random.insideUnitSphere * shakeStrength;
			transform.position = new Vector3(transform.position.x, transform.position.y, -10);
			bigSpellTimer = bigSpellTimer - Time.deltaTime;
		}
		else if (spellTimer > 0)
        {
            transform.position = transform.position + Random.insideUnitSphere * shakeStrength;
			transform.position = new Vector3(transform.position.x, transform.position.y, -10);
            spellTimer = spellTimer - Time.deltaTime;
		}
        else
		{
			transform.position = before;
		}
	}
    public void spellShake(float power)
    {
        if (power > 1)
        {
            spellTimer = 0;
            shakeStrength = power;
            bigSpellTimer += .15f;

        }
        else if(bigSpellTimer <= 0)
        {
			shakeStrength = power;
			spellTimer += .15f;
        }
    }
}
