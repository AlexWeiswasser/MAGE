using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CameraAssigner : MonoBehaviour
{
    private char lastCharacter;
    private int valueToMatch; 
    private bool isAttached = false; 

    private void Start()
    {
        lastCharacter = this.name[this.name.Length - 1];
        valueToMatch = int.Parse(lastCharacter.ToString());
    }
    void Update()
    {
        if (!isAttached)
        {
            PlayerCode[] allObjects = FindObjectsOfType<PlayerCode>();

            foreach (PlayerCode obj in allObjects)
            {
                if (obj.playerID == valueToMatch)
                {
                    this.transform.SetParent(obj.transform);

                    this.transform.localPosition = new Vector3(0f, 0f, -100f); 
                    this.transform.localRotation = Quaternion.identity;

                    isAttached = true;

                    break;
                }
            }
        }
    }
}
