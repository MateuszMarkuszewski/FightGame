using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterManager : MonoBehaviour {

    public KeyCode attack;

    private void OnTriggerStay2D(Collider2D col)
    {
        //Debug.Log(col.gameObject.tag);
        if (col.gameObject.tag == "Hitbox" && Input.GetKeyDown(attack))
        {
            Debug.Log("counter");
          
        }

    }
}
