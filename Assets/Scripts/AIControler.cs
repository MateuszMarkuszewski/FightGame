using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;


public class AIControler : NetworkBehaviour {

    //postać gracza
    public GameObject enemy;
    //poprzez tą referencję avatar się porusza
    public PlayerControler playerControler;
    //dane o arenie np. bronie, wierzchołki grafu
    public SceneSetup arenaData;
    //warstwy
    public LayerMask platformMask;
    public LayerMask enemyMask;
    public LayerMask pickUpMask;
    public LayerMask wallMask;

    //czy szuka broni
    private bool findWeapon = false;
    private ContactFilter2D throwFilter;
    private GameObject previousNode;
    //cel do którego obecnie zmierza
    public GameObject target;
    //droga którą znalazł
    public List<GameObject> way;


    void Start ()
    {
        playerControler.AI = true;
        //zmniejszenie prędkości ruchu bota
        playerControler.heroSpeed = playerControler.heroSpeed * 3 / 4;
        //podczas testowania rzutu są sprawdzane 2 warstwy
        throwFilter.SetLayerMask(enemyMask | platformMask);
        target = enemy;
        StartCoroutine(WaitForArenaMake());
    }

    //oczekiwanie aż graf zostanie stworzony po czym aktywuję cillider który włącza szukanie ścieżki
    IEnumerator WaitForArenaMake()
    {
        yield return new WaitWhile(() => !arenaData.gridDone);
        GetComponent<BoxCollider2D>().enabled = true;
    }

    void Update () {
        DecisionTree();
        //podążanie wyznaczoną drogą, jeżeli jest wystarczająco blisko celu to zmierz bezpośrednio do niego
        if (playerControler.attackEnable || findWeapon)
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
        //atakuje gdy tylko ma okazję, sprawdzane na początku aby w przypadku 
        //gdy biegnie obok przeciwnika gdy nie jest jego celem, też go zaatakowało
        if (Physics2D.Raycast(transform.position, new Vector2(playerControler.horizontalMove, 0), 0.5f, enemyMask) && playerControler.attackEnable)
        {
            playerControler.BasicAttack();
        }
        if (playerControler.weapon == null &&
           arenaData.weaponsOnArena.Count != 0)
        {
            findWeapon = true;
            //szukanie najblizszej broni
            float minDistance = Mathf.Infinity;
            float tmp;
            //wybierana jest najbliższa obecnie broń
            for (int i = 0; i < arenaData.weaponsOnArena.Count; i++)
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
        else if (playerControler.attackEnable)
        {
            if(findWeapon) findWeapon = false;
            if (target!=enemy) target = enemy;

            //sprawdza czy bedąc w powietrzu znajduje sie nad przeciwnikiem, wtedy atakuje
            else if (Physics2D.Raycast(transform.position, Vector2.down, 5f, enemyMask))
            {
                //sprawdza czy na przeszkodzie nie stoi platforma
                RaycastHit2D[] hit = new RaycastHit2D[1]; ;
                Physics2D.Raycast(transform.position, Vector2.down, throwFilter, hit, 10f);
                if (hit[0].collider.gameObject.tag == "Player")
                {
                    playerControler.DropAttack();
                }
            }
            else if (playerControler.weapon != null)//jeżeli wytrzymałość broni jest niska to atakuje nią rzucając
            {      
                if(playerControler.weapon.GetComponent<WeaponControler>().durability <= 20)
                {
                    //sprawdza będąc w powietrzu przeciwnik jest w zasiegu rzutu bronią
                    if (playerControler.anim.GetBool("InAir") &&
                        Physics2D.Raycast(transform.position, new Vector2(playerControler.horizontalMove, -0.5f), 10f, enemyMask))
                    {
                        Throw(new Vector2(playerControler.horizontalMove, -0.5f));
                    }
                    // sprawdza czy może rzucić bronią
                    else if (!playerControler.anim.GetBool("InAir") &&
                             Physics2D.Raycast(transform.position, new Vector2(playerControler.horizontalMove, 0), 10f, enemyMask))
                    {
                        Throw(new Vector2(playerControler.horizontalMove, 0));
                    }
                }               
            }
        }
        else//ucieka od przeciwnika gdy nie moze atakowac
        {
            findWeapon = false;
            RunFromEnemy();
        }
    }

    //szuka scieżki do celu w grafie na który składają się obiekty Node
    public void Djikstra(Node current)
    {
        //lista wierzchołków, dla których najkrótsze ścieżki nie zostały jeszcze policzone
        List<Node> Q = new List<Node>(arenaData.nodes);
        //lista wierzchołków o policzonych już najkrótszych ścieżkach
        List<Node> S = new List<Node>();
        //tablica odległości między wierzchołkami
        float[] wayCost = new float[arenaData.nodes.Count];
        //tablica poprzedników na ścieżkach
        int[] prevNode = new int[arenaData.nodes.Count];
        //wypelnienie tablic wartosciami
        for(int i = 0; i < wayCost.Length; i++)
        {
            wayCost.SetValue(Mathf.Infinity, i);
            prevNode.SetValue(-1, i);
        }       
        //droga do samego siebie jest równa 0
        wayCost[current.nodeNum] = 0;
        //operacje wykonywane do momentu znaleźienia wierzchołka w którym znajduje się cel
        while (!current.targets.Contains(target))
        {
            S.Add(current);
            Q.Remove(current);
            //liczenie watrości dla sąsiadów wierzchołka znajdujących się w liście Q
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
            //szukanie najktótszej drogi do następnego wierzchołka w zbiorze Q
            //będzie on rozpatrywany przy następnym obiegu pętli
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
        //usunięcie wcześniej znalezionej drogi
        way.Clear();
        //tworzenie listy kolejnych wierzchołków drogi do przebycia za pomocą tablicy poprzedników
        //ostatni na liście to wierzchołek w którym znajduje się bot, a w pierwszym znajduje się cel
        while(prevNode[current.nodeNum] != -1)
        {
            way.Add(current.gameObject);
            current = arenaData.nodes[prevNode[current.nodeNum]];
        }
    }

   //porusza postacią tak aby doprowadzić ją do celu
    void AdjustCoordinates(Collider2D nextNode)
    {
        if (transform.position.x > nextNode.bounds.max.x)
        {
            playerControler.horizontalMove = -1f;
        }
        else if (transform.position.x < nextNode.bounds.min.x)
        {
            playerControler.horizontalMove = 1f;
        }

        if ((transform.position.y < nextNode.bounds.min.y) || 
            (target != enemy && transform.parent.GetComponent<CapsuleCollider2D>().IsTouchingLayers(enemyMask)))//zmierzając do broni może na drodze napotkać gracza
        {
            Jump();
        }
        else if (transform.position.y > nextNode.bounds.max.y)
        {
            //sprawdza czy dotyka platformy oraz czy jest jest pod avatarem
            if (transform.parent.GetComponent<CapsuleCollider2D>().IsTouchingLayers(platformMask) && Physics2D.Raycast(transform.position, Vector2.down, 1.5f, platformMask))
                playerControler.ComeDown();
        }
    }
    
    float Distance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow((x2 - x1),2) + Mathf.Pow((y2 - y1), 2));
    }
    
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

    //sprawdza czy miedzy celem a botem jest przeszkoda, jesli nie to rzuca
    void Throw(Vector2 direction)
    {
        RaycastHit2D[] hit = new RaycastHit2D[1]; ;
        Physics2D.Raycast(transform.position, direction, throwFilter, hit, 10f);
        if (hit[0].collider.gameObject.tag == "Player")
        {
            playerControler.Throw();
        }   
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //wywoływanie szukania ścieżki 
        if (collision.gameObject.tag == "Node")
        {
            Djikstra(collision.GetComponent<Node>());
        }
        //podnoszenie broni której szuka
        //"interaction" bo z triggerpickup musi byc kolizja
        if (collision.gameObject.tag == "Interaction")
        {
            if (collision.transform.parent.gameObject == target && findWeapon == true)
            {
                playerControler.TakeWeapon(collision.transform.parent.gameObject);
                findWeapon = false;
            }
        }       
    }

    //działa w analogiczny sposób jak AdjustCoordinates tylko że stara się zwiekszyć odległość miedzy botem a graczem
    void RunFromEnemy()
    {
        if (transform.position.x >= enemy.transform.position.x)
        {
            playerControler.horizontalMove = 1f;
        }
        else if (transform.position.x < enemy.transform.position.x)
        {
            playerControler.horizontalMove = -1f;
        }

        if ((transform.position.y >= enemy.transform.position.y) ||
            transform.parent.GetComponent<CapsuleCollider2D>().IsTouchingLayers(enemyMask))
        {
            Jump();
        }
        else if (transform.position.y < enemy.transform.position.y)
        {
            if(transform.parent.GetComponent<CapsuleCollider2D>().IsTouchingLayers(platformMask) && Physics2D.Raycast(transform.position, Vector2.down, 1.5f, platformMask))
                playerControler.ComeDown();
            
        }
    }
}
