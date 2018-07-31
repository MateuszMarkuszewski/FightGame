using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

    public int nodeNum;
    public List<GameObject> neighbours;
    public List<float> distances;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "AI")
        {
            //Debug.Log("AI in block " + nodeNum);
            //collision.gameObject.GetComponent<AIControler>().FindPath(neighbours, distances, gameObject);
        }
    }

}
