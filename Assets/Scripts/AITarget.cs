﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITarget : MonoBehaviour {

    public GameObject neighbour;
    public AIControler ai;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Node")
        {
            /*collision.GetComponent<Node>().t = gameObject.transform.parent.gameObject;
            if(neighbour != null && neighbour != collision.gameObject) neighbour.GetComponent<Node>().t = null;
            neighbour = collision.gameObject;*/
            collision.GetComponent<Node>().targets.Add(gameObject.transform.parent.gameObject);
            if(neighbour != null) neighbour.GetComponent<Node>().targets.Remove(gameObject.transform.parent.gameObject);
            neighbour = collision.gameObject;
        }
    }
}