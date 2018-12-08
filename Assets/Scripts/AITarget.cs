using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//"dynamiczny" wierzchołek grafu, modyfikujący swoją krawędź
//przypisany do celów bota
public class AITarget : MonoBehaviour {

    //obiekt z komponentem Node zawierający rodzica AITargets w liście targets
    //inaczej: jedyny wierzchołek grafu z którym łączy AiTargets krawędź
    public GameObject neighbour;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //zmienia strukture grafu poprzez łączenie najbliżyszego wierzchołka z obiektem
        if(collision.gameObject.tag == "Node")
        {
            collision.GetComponent<Node>().targets.Add(gameObject.transform.parent.gameObject);
            if(neighbour != null) neighbour.GetComponent<Node>().targets.Remove(gameObject.transform.parent.gameObject);
            neighbour = collision.gameObject;
        }
    }
}
