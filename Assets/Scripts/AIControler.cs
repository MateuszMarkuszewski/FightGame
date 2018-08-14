using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class AIControler : MonoBehaviour {

    public GameObject enemy;
    public PlayerControler playerControler;
    public delegate void Alghorthm();
    public SceneSetup arenaData;
    public LayerMask platformMask;
    public LayerMask enemyMask;
    public LayerMask pickUpMask;
    public LayerMask wallMask;

    private bool findWeapon = false;
    private bool findEnemy = true;
    //TODO: 1.5f na zmienną
    private ContactFilter2D throwFilter;
    private List<GameObject> wayToTarget;
    private GameObject previousNode;
    public GameObject target;


    public List<GameObject> way;

    public GameObject neighbour;


    void Start () {
        playerControler.AI = true;
        playerControler.heroSpeed = playerControler.heroSpeed * 3 / 4;
        arenaData.AI = true;
        throwFilter.SetLayerMask(enemyMask | platformMask);
        target = enemy;
        StartCoroutine(WaitForArenaMake());
    }

    IEnumerator WaitForArenaMake()
    {
        yield return new WaitWhile(() => !arenaData.gridDone);
        GetComponent<BoxCollider2D>().enabled = true;
    }

    void Update () {
        DecisionTree();
        if(findEnemy || findWeapon)
        {
            if (way.Count == 0 || way.Count == 1)
            {
                AdjustCoordinates(target.GetComponent<Collider2D>());
            }
            else AdjustCoordinates(way[way.Count - 1].GetComponent<BoxCollider2D>());
        }
    }

    //wywoływane co klatka drzewo decyzyjne które definiuje co AI ma robić
    void DecisionTree()
    {
        //sprawdza czy może uderzyć podstawowym atakiem
        if (Physics2D.Raycast(transform.position, new Vector2(playerControler.horizontalMove, 0), 0.5f, enemyMask) &&
            playerControler.attackEnable)
        {
            playerControler.BasicAttack();       
        }
        //sprawdza czy bedąc w powietrzu znajduje sie nad przeciwnikiem, wtedy atakuje
        else if(Physics2D.Raycast(transform.position, Vector2.down, 5f, enemyMask))
        {
            RaycastHit2D[] hit = new RaycastHit2D[1]; ;
            Physics2D.Raycast(transform.position, Vector2.down, throwFilter, hit, 10f);
            if (hit[0].collider.gameObject.tag == "Player")
            {
                playerControler.DropAttack();
            }
        }
        //sprawdza będąc w powietrzu przeciwnik jest w zasiegu rzutu bronią
        else if (playerControler.anim.GetBool("InAir") && 
                 playerControler.weapon != null && 
                 Physics2D.Raycast(transform.position, new Vector2(playerControler.horizontalMove, -0.5f), 10f, enemyMask))
        {
            Throw(new Vector2(playerControler.horizontalMove, -0.5f));
        }
        // sprawdza czy może rzucić bronią
        else if (!playerControler.anim.GetBool("InAir") && 
                 playerControler.weapon != null && 
                 Physics2D.Raycast(transform.position, new Vector2(playerControler.horizontalMove, 0), 10f, enemyMask) &&
                 playerControler.weapon.GetComponent<WeaponControler>().durability > playerControler.weapon.GetComponent<WeaponControler>().maxDurability / 5)
        {

            Throw(new Vector2(playerControler.horizontalMove, 0));
        }
        //czy ma szukać broni
        else if (playerControler.weapon == null && 
                //enemy.GetComponent<PlayerControler>().weapon != null && 
                arenaData.weaponsOnArena.Count != 0
                )
        {
            findWeapon = true;
            findEnemy = false;
            //szukanie najblizszej broni
            float minDistance = Mathf.Infinity;
            float tmp;
            for (int i = 0; i < arenaData.weaponsOnArena.Count ; i++)
            {
                tmp = Distance(transform.position.x, transform.position.y, arenaData.weaponsOnArena[i].transform.position.x, arenaData.weaponsOnArena[i].transform.position.y);
                if (tmp < minDistance)
                {
                    minDistance = tmp;
                    target = arenaData.weaponsOnArena[i];
                }
            }
        }
        //czy szukać przeciwnika
        else if(playerControler.attackEnable)
        {
            findEnemy = true;
            findWeapon = false;
            target = enemy;
        }
        else
        {
            findWeapon = false;
            findEnemy = false;
            AdjustCoordinates(arenaData.nodes[(int)Random.Range(0, arenaData.nodes.Count - 1)].GetComponent<BoxCollider2D>());
        }
    }

    //aktualizuje dane co klatka i wybiera sąsiada który ma bliżej celu.
    //w niektórych sytuacjach nie znajduje drogi
    public void FindPath(List<GameObject> neighbours, List<float> distances, GameObject current)
      {
          float tmp;
          float minDistance;
          GameObject target = enemy;
          //tu było find weapon

          int nextNode = -1;
          minDistance = Mathf.Infinity;

          //A*
          for(int i = 0; i < neighbours.ToArray().Length; i++)
          {
              
              //S.Add(neighbours[i]);
              tmp = distances[i] + Distance(neighbours[i].transform.position.x, neighbours[i].transform.position.y, target.transform.position.x, target.transform.position.y);
              if(tmp < minDistance && previousNode != neighbours[i])
              {
                  minDistance = tmp;
                  nextNode = i;
              }
          }
          if (minDistance > Distance(current.transform.position.x, current.transform.position.y, target.transform.position.x, target.transform.position.y))
          {
              //Debug.Log("current");
              AdjustCoordinates(target.GetComponent<CapsuleCollider2D>());
          }
          else
          {
              AdjustCoordinates(neighbours[nextNode].GetComponent<BoxCollider2D>());
          }
      }
   //z node ai wywolywane
    
    public void Djikstra(Node current)
    {
        List<Node> Q = new List<Node>(arenaData.nodes); ;
        List<Node> S = new List<Node>();
        float[] wayCost = new float[arenaData.nodes.Count];
        int[] prevNode = new int[arenaData.nodes.Count];

        //wypelnienie tablic wartosciami
        for(int i = 0; i < wayCost.Length; i++)
        {
            wayCost.SetValue(Mathf.Infinity, i);
            prevNode.SetValue(-1, i);
        }
        
        wayCost[current.nodeNum] = 0;

        //targets.contains
        while (!current.targets.Contains(target))//!currentNode.GetComponent<Node>().targets.Contains(target))//currentNode.GetComponent<Node>().enemy == false)
        {
            S.Add(current);
            Q.Remove(current);
            //liczenie watrosci dla sasiadów ze zbioru Q
            for (int i = 0; i < current.neighbours.Count; i++)
            {
                if (Q.Contains(current.neighbours[i]))
                {
                    if (wayCost[current.nodeNum] + current.distances[i] < wayCost[current.neighbours[i].GetComponent<Node>().nodeNum])
                    {
                        wayCost[current.neighbours[i].GetComponent<Node>().nodeNum] = current.distances[i];// + Distance(current.neighbours[i].transform.position.x, current.neighbours[i].transform.position.y, target.transform.position.x, target.transform.position.y);
                        prevNode[current.neighbours[i].GetComponent<Node>().nodeNum] = current.nodeNum;
                    }
                }
            }

            //szukanie najktótszej drogi w zbiorze Q
            float tmp = Mathf.Infinity;
            for(int j = 0; j < wayCost.Length; j++)
            {
                if (wayCost[j] < tmp && Q.Contains(arenaData.nodes[j]))
                {
                    tmp = wayCost[j];
                    current = arenaData.nodes[j];
                }
            }
        }
        
        while(prevNode[current.nodeNum] != -1)
        {
            Debug.Log(current.nodeNum);
            way.Add(current.gameObject);
            current = arenaData.nodes[prevNode[current.nodeNum]];
        }
        Debug.Log("----------------------------------");
    }

    //porusza postacią tak aby doprowadzić ją do celu
   void AdjustCoordinates(Collider2D nextNode)
    {
        //bo z triggerpick up musi byc 
        if (transform.parent.GetComponent<BoxCollider2D>().IsTouchingLayers(pickUpMask) && findWeapon == true)
        {
            Debug.Log(target.gameObject);
            playerControler.TakeWeapon(target.gameObject);
            findWeapon = false;
        }
        if (transform.position.x > nextNode.bounds.max.x)
        {
            playerControler.horizontalMove = -1f;
        }
        else if (transform.position.x < nextNode.bounds.min.x)
        {
            playerControler.horizontalMove = 1f;
        }

        if ((transform.position.y < nextNode.bounds.min.y) || 
            (target != enemy && transform.parent.GetComponent<BoxCollider2D>().IsTouchingLayers(enemyMask)))
        {
            Jump();
        }
        else if (transform.position.y > nextNode.bounds.max.y)
        {
            try
            {
                RaycastHit2D platform = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, platformMask);
                playerControler.ComeDown(platform.collider);
            }
            catch
            {
                //nie ma platformy pod sobą bo np. spada
            }
        }
        /*if(transform.position.x > target.bounds.min.x && transform.position.x < target.bounds.max.x && target.IsTouching(transform.parent.GetComponent<BoxCollider2D>()))
        {
            playerControler.Jump();
        }*/
        //way
        if (transform.GetComponent<BoxCollider2D>().IsTouching(nextNode))
        {
            way.RemoveAt(way.Count - 1);
        }
    }
    
    float Distance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow((x2 - x1),2) + Mathf.Pow((y2 - y1), 2));
    }

    void MeasureTime(Alghorthm alg)
    {
        float start = Time.realtimeSinceStartup;
        alg();
        Debug.Log("Time of alghorytm:" + (start - Time.realtimeSinceStartup));
    }

    /// <summary>
    /// ///Kontrola postaci:
    /// </summary>
    
    //obudowany skok aby dzialal bez oncollisionstay z rodzica
    void Jump()
    {
        if (!playerControler.anim.GetBool("InAir"))
        {
            playerControler.Jump();
        }
        else if (Physics2D.Raycast(transform.position, new Vector2(playerControler.horizontalMove, 0), 0.5f, wallMask))
        {
            playerControler.horizontalMove = -playerControler.horizontalMove;
            playerControler.Jump();
        }
    }
    //sprawdza czy miedzy celem a ai jest przeszkoda, jesli nie to rzuca
    void Throw(Vector2 direction)
    {
        RaycastHit2D[] hit = new RaycastHit2D[1]; ;
        Physics2D.Raycast(transform.position, direction, throwFilter, hit, 10f);
        if (hit[0].collider.gameObject.tag == "Player")
        {
            Debug.Log("throw");
            playerControler.Throw();
        }   
    }

    //enemy>this
   /* private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Node")
        {
            Djikstra(collision.GetComponent<Node>());
        }
    }*/

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Node")
        {
            Djikstra(collision.GetComponent<Node>());
        }
    }
}
