using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//przypisane do obiektu reprezentującego pokój (wierzchołek grafu)
public class Node : MonoBehaviour {

    public int nodeNum;
    //lista sąsiadów w grafie
    public List<Node> neighbours;
    //lista odległości do owych sąsiadów (odpowiadające sobie wartości rozpoznawane za pomocą indeksów)
    public List<float> distances;
    //cele do któych może zmierzać bot i znajdują się w pokoju reprezentowanym przez ową instację Node
    public List<GameObject> targets;

}
