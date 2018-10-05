using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class SceneSetup : MonoBehaviour
{

    public float size;
    public GameObject ground;
    public GameObject wallLeft;
    public GameObject wallRight;
    public GameObject ceiling;
    public GameObject platform;
    private Camera camera;
    public GameObject background;
    public GameObject player1;
    public GameObject player2;

    private float width;
    private float height;
    public Node[,] rooms;

    public GameObject[] weapons;
    private int maxWeaponsNum;
    //wykorzystywane do dropu
    private int currentWeaponNum = 0;
    private float[] weaponProbability;
    public LayerMask antyDropCollisionMask;

    //info dla AI
    public bool AI = false;
    public GameObject[,] grid;
    public GameObject node;
    //wykorzystywane do ai
    public List<GameObject> weaponsOnArena;
    public List<Node> nodes;
    public bool gridDone = false;

    public static float roomSizeX;
    public static float roomSizeY;


    void Awake()
    {
        //size = (float)GameData.sizeMap;
        //ustawienie sceny
        SetupCamera();
        width = 2 * size * camera.aspect;
        height = 2 * size;
        SetBackgroundSize();
        SetupMainGround();
        GeneratePlatforms();
        maxWeaponsNum = 4;
        //AI
        if (AI == true)
        {
            
        }
        //MakeGrid();
        //MakeGraph();
        //SetupPlayers();
        gridDone = true;
        //bronie
        CalculateProbability();
        //do wyrzucenia
        DropWeapon(weapons[0]);
        DropWeapon(weapons[1]);

        StartCoroutine(WeaponDropMenager());
    }

    public static void DrawRoomBounds(int x, int y)
    {
        //lewy
        Debug.DrawLine(new Vector3(roomSizeX * x, roomSizeY * y, 0), new Vector3(roomSizeX * x, roomSizeY * y + roomSizeY), Color.red, 200, false);
        //dol
        Debug.DrawLine(new Vector3(roomSizeX * x + roomSizeX, roomSizeY * y, 0), new Vector3(roomSizeX * x, roomSizeY * y, 0), Color.red, 200, false);
        //gora
        Debug.DrawLine(new Vector3(roomSizeX * x + roomSizeX, roomSizeY * y + roomSizeY, 0), new Vector3(roomSizeX * x, roomSizeY * y + roomSizeY, 0), Color.red, 200, false);
        //prawy
        Debug.DrawLine(new Vector3(roomSizeX * x + roomSizeX, roomSizeY * y, 0), new Vector3(roomSizeX * x + roomSizeX, roomSizeY * y + roomSizeY, 0), Color.red, 200, false);
    }

    /// <summary>
    /// Tworzenie grafu dla AI
    /// </summary>
    void MakeNode(int i, int j, float posX, float posY, int num)
    {
        GameObject n = Instantiate(node, new Vector3(posX, posY, 0), Quaternion.identity);
        n.GetComponent<Node>().nodeNum = num;
        n.GetComponent<BoxCollider2D>().size = new Vector2(roomSizeX, roomSizeY);
        rooms[i, j] = n.GetComponent<Node>();
        nodes.Add(n.GetComponent<Node>());
    }

    void MakeGrid()
    {
        grid = new GameObject[(int)size, (int)(2 * size / 3)];
        int num = 0;
        for (int i = 0; i < rooms.GetLength(0); i++)
        {
            Debug.Log(rooms[i,0]);
            /*MakeNode(i, 0, i * roomSizeX + (roomSizeX / 2f), 0 * roomSizeY + (roomSizeY / 2f), num);
            num++;*/
        }
        for (int j = 0; j < rooms.GetLength(1); j++)
        {
            for (int i = 0; i < rooms.GetLength(0); i++)
            {
                Debug.Log(rooms[i, j]);

                /* if (rooms[i, j] != null)
                 {
                     MakeNode(i, j + 1, i * roomSizeX + (roomSizeX / 2f), (j + 1) * roomSizeY + (roomSizeY / 2f), num);
                     num++;
                 }
                 else
                 {
                     grid[i, j + 1] = null;
                 }*/
            }
        }
    }

    void MakeGraph()
    {
        //tablica
        //x w graph
        for (int j = 0; j < rooms.GetLength(1) + 1; j++)
        {
            for (int i = 0; i < rooms.GetLength(0); i++)
            {
                try
                {
                    if (grid[i - 1, j] != null)
                    {
                        MakeEdge(grid[i, j], grid[i - 1, j]);
                    }
                }
                catch
                {
                }
                try
                {
                    if (grid[i + 1, j] != null)
                    {
                        MakeEdge(grid[i, j], grid[i + 1, j]);
                    }
                }
                catch
                {
                }
                try
                {
                    if (grid[i, j + 1] != null)
                    {
                        MakeEdge(grid[i, j], grid[i, j + 1]);
                    }
                }
                catch
                {
                }
                /*try
                {
                    if (grid[i, j - 1] != null)
                    {
                        MakeEdge(grid[i, j], grid[i, j - 1]);
                    }
                }
                catch
                {

                }*/
                try
                {
                    if (grid[i - 1, j - 1] != null)
                    {
                        MakeEdge(grid[i, j], grid[i - 1, j - 1]);
                    }
                }
                catch
                {
                }
                try
                {
                    if (grid[i + 1, j - 1] != null)
                    {
                        MakeEdge(grid[i, j], grid[i + 1, j - 1]);
                    }
                }
                catch
                {
                }
                try
                {
                    if (grid[i - 2, j + 1] != null)
                    {
                        MakeEdge(grid[i, j], grid[i - 2, j + 1]);
                    }
                }
                catch
                {
                }
                try
                {
                    if (grid[i + 2, j + 1] != null)
                    {
                        MakeEdge(grid[i, j], grid[i + 2, j + 1]);
                    }
                }
                catch
                {
                }
                //dodanie każdej platformy poniżej obecnej
                for (int z = 1; z < rooms.GetLength(1) + 1; z++)
                {
                    try
                    {
                        if (grid[i, j - z] != null)
                        {
                            MakeEdge(grid[i, j], grid[i, j - z]);
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

    }

    void MakeEdge(GameObject node1, GameObject node2)
    {
        node1.GetComponent<Node>().neighbours.Add(node2.GetComponent<Node>());
        node1.GetComponent<Node>().distances.Add(Distance(node1.transform.position.x, node1.transform.position.y, node2.transform.position.x, node2.transform.position.y));
        //Debug.DrawLine(new Vector3(node1.transform.position.x, node1.transform.position.y), new Vector3(node2.transform.position.x, node2.transform.position.y), Color.blue, 200, false);
    }

    float Distance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow((x2 - x1), 2) + Mathf.Pow((y2 - y1), 2));
    }

    /// <summary>
    /// Zrzuty broni
    /// </summary>
    public void DecreaseWeaponNum(GameObject w)
    {
        weaponsOnArena.Remove(w);    
        currentWeaponNum--;
    }

    IEnumerator WeaponDropMenager()
    {
        while (true)
        {
            //nie losuje liczby dopoki maxymalna liczba broni na arenie
            while (currentWeaponNum == maxWeaponsNum)
            {
                yield return null;
            }
            yield return new WaitForSeconds(1f);

            RandomWeapon();
        }
    }

    private void RandomWeapon()
    {
        //porownuje wylosowana liczbe z prawdopodobienstwem wylosowania broni
        //losowana jest liczba z przedziału 0-1
        float number = Random.value;
        float probability = 0;
        for (int i = 0; i < weapons.Length; i++)
        {
            //porównywanie wylosowanej liczby z prawdopodobieństwem każdej kolejnej broni
            probability += weaponProbability[i];
            if (probability >= number)
            {
                //tworzona jest instancja wylosowanej broni
                DropWeapon(weapons[i]);
                break;
            }
        }
    }

    //wyliczane jest prawdopodobienstwo pojawienia sie kazdej broni
    private void CalculateProbability()
    {
        weaponProbability = new float[weapons.Length];
        for (int i = 0; i < weapons.Length; i++)
        {
            weaponProbability[i] = nodes.Count / size / weapons[i].GetComponentInChildren<AttackCollider>().dmg / weapons.Length;
            weapons[i].GetComponentInChildren<AttackCollider>().enabled = false;
            Debug.Log(weaponProbability[i]);
        }
    }

    public void DropWeapon(GameObject weapon)
    {
        while (true)
        {
            int v = (int)Random.Range(0, nodes.Count - 1);
            if (!nodes[v].GetComponent<BoxCollider2D>().IsTouchingLayers(antyDropCollisionMask))
            {
                GameObject w = Instantiate(weapon, nodes[v].transform.position, Quaternion.Euler(0f, 0f, 90f));
                w.name = weapon.name;
                currentWeaponNum++;
                weaponsOnArena.Add(w);
                break;
            }

            /*
            int[] room = new int[] { (int)Random.Range(0, rooms.GetLength(0)), (int)Random.Range(0, rooms.GetLength(1) + 1) };
            try
            {
                if (!grid[room[0], room[1]].GetComponent<BoxCollider2D>().IsTouchingLayers(antyDropCollisionMask))
                {
                    //Debug.Log(grid[room[0], room[1]].transform.position);
                    GameObject w = Instantiate(weapon, grid[room[0], room[1]].transform.position, Quaternion.Euler(0f, 0f, 90f));
                    w.name = weapon.name;
                    currentWeaponNum++;
                    if (AI == true)
                    {
                        weaponsOnArena.Add(w);
                    }
                    break;
                }
            }
            catch
            {

            }*/

        }
    }

    /// <summary>
    /// Wygenerowanie areny
    /// </summary>
    public void SetBackgroundSize()
    {
        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
        Vector3 backgroundSizes = sr.sprite.bounds.size;
        background.transform.localScale = new Vector2(width / backgroundSizes.x, height / backgroundSizes.y);
        background.transform.position = new Vector2(size * camera.aspect, size);
    }

    public void GeneratePlatforms()
    {
        //rozmiar pojedynczego pokoju 
        roomSizeX = 2 * camera.aspect;
        roomSizeY = 3;
        //ustalanie ilosci pokoi
        rooms = new Node[(int)size, (int)(2 * size / 3)];
        //przeskalowanie objektu platformy aby pokrywał pokój
        platform.transform.localScale = new Vector2(roomSizeX / platform.GetComponent<SpriteRenderer>().sprite.bounds.size.x, platform.transform.localScale.y);
        int num = 0;
        for (int i = 0; i < rooms.GetLength(0); i++)
        {
            MakeNode(i, 0, i * roomSizeX + (roomSizeX / 2f), 0 * roomSizeY + (roomSizeY / 2f), num);
            num++;
        }
        //losowanie pokoju z pierwszego rzędu 
        int x = Random.Range(0, rooms.GetLength(0));
        int y = 1;
        //losowanie kierunku
        int[] way = { 1, -1 };
        int side = way[Random.Range(0, 1)];
        //pętla która wykonuje się do momentu wyjscia poza górną granicę siatki
        while (true)
        {
            DrawRoomBounds(x, y);
            //tworzona jest platforma
            Instantiate(platform).transform.position = new Vector3(x * roomSizeX + (roomSizeX / 2f), y * roomSizeY);
            MakeNode(x, y, x * roomSizeX + (roomSizeX / 2f), y * roomSizeY + (roomSizeY / 2f), num);
            num++;
            //rooms[x, y] = new Node();
            //losowane jest gdzie bedzie następny pokoj
            if (Random.Range(0, 10) == 3)
            {
                y = y + 1;
                side = way[Random.Range(0, 1)];
            }
            else
            {
                x = x + side;
            }
            //sprwdzanie czy nie wyszlo poza boczną granicę siatki
            if (x == rooms.GetLength(0) || x < 0)
            {
                y = y + 1;
                side = -side;
                x = x + side;
            }
            //sprawdzanie czy nie wyszlo poza górna granicę
            if (y == rooms.GetLength(1))
            {
                break;
            }
        }
    }

    public void SetupMainGround()
    {
        //Tworzy podłogę i ściany   
        float y;
        float x;
        for (y = 0f; y < (2 * size) + 1; y++)
        {
            Instantiate(wallLeft).transform.position = new Vector2(0f, y);
        }
        x = 2 * size * camera.aspect;
        for (y = 0f; y < (2 * size) + 1; y++)
        {
            Instantiate(wallRight).transform.position = new Vector2(x, y);
        }
        for (x = 0f; x < 2 * size * camera.aspect; x++)
        {
            Instantiate(ground).transform.position = new Vector2(x, 0f);
        }
        for (x = 0; x < 2 * size * camera.aspect; x++)
        {
            Instantiate(ceiling).transform.position = new Vector2(x, y);
        }
    }

    public void SetupCamera()
    {
        //zmiana rozmiaru kamery w zależności od rozmiaru 
        camera = Camera.main;
        camera.orthographicSize = size;
        camera.transform.position = new Vector3(size * camera.aspect, size, -10f);
    }

    public void SetupPlayers()
    {
        //ustawia graczy na pozycji startowej
        player1.transform.position = grid[0, 0].transform.position;
        if (AI) player1.GetComponentInChildren<AITarget>().neighbour = grid[0, 0];
    }

    public void Perlin()
    {
        //generuje szum Perlina
        // Debug.Log(height);
        for (int x = 0; x < 2 * size * camera.aspect; x++)
        {

            for (int lvl = 10; lvl < height; lvl = lvl + 10)
            {
                Instantiate(ground, new Vector3(x, (Mathf.PerlinNoise(((float)x) / 35, 0f) * 10) + lvl), Quaternion.identity);

            }
        }
    }
}
