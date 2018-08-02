using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

    public static bool target = false;
    public AIControler ai;

    public bool enemy = false;
    public int nodeNum;
    public List<GameObject> neighbours;
    public List<float> distances;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "AI")
        {
            if (target == true) collision.gameObject.GetComponent<AIControler>().Djikstra(this);

            //ai.Djikstra(this);
            //Debug.Log("AI in block " + nodeNum);
            // collision.gameObject.GetComponent<AIControler>().FindPath(this);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Target")
        {
            enemy = true;
            if(target==false)target = true;
        }
        if (collision.gameObject.tag == "AI")
        {
            //Debug.Log("AI in block " + nodeNum);
            //if(target==true)collision.gameObject.GetComponent<AIControler>().Djikstra(this);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Target")
        {
            enemy = false;
        }
    }
}
