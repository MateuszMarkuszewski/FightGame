using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class Room
{
    public static float x = 3;
    public static float y = 3;


    public static void SetRoomCoordinates(int x, int y)
    {
        Debug.DrawLine(new Vector3(Room.x * x, Room.y * y,0), new Vector3(Room.x * x, Room.y * y + Room.y), Color.red,200,false);
        Debug.DrawLine(new Vector3(Room.x * x +Room.x, Room.y * y, 0), new Vector3(Room.x * x, Room.y * y, 0), Color.red, 200, false);
    }

}

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
    public Room[,] rooms;

    public GameObject[] weapons;
    public int maxWeaponsNum;
    private int currentWeaponNum = 0;
    private float[] weaponProbability;
    public LayerMask antyDropCollisionMask;

    //info dla AI
    public bool AI = false;
    public GameObject[,] grid;
    public GameObject node;
    public List<GameObject> weaponsOnArena;
    public List<Node> nodes;
    public bool gridDone = false;



    void Awake()
    {   
        size = (float)GameData.sizeMap;
        //ustawienie sceny
        SetupCamera();
        width = 2 * size * camera.aspect;
        height = 2 * size;
        SetBackgroundSize();
        SetupMainGround();
        GeneratePlatforms();
        //AI
        if (AI == true)
        {
            
        }
        MakeGrid();
        MakeGraph();
        SetupPlayers();
        gridDone = true;
        //bronie
        CalculateProbability();
        //do wyrzucenia
        DropWeapon(weapons[0]);
        DropWeapon(weapons[1]);

        StartCoroutine(WeaponDropMenager());
    }

    /// <summary>
    /// Tworzenie grafu dla AI
    /// </summary>
    void MakeNode(int i, int j, float posX, float posY, int num)
    {
        GameObject n = Instantiate(node, new Vector3(posX, posY, 0), Quaternion.identity);
        n.GetComponent<Node>().nodeNum = num;
        n.GetComponent<BoxCollider2D>().size = new Vector2(Room.x, Room.y);
        grid[i, j] = n;
        nodes.Add(n.GetComponent<Node>());
    }

    void MakeGrid()
    {
        grid = new GameObject[(int)size, (int)(2 * size / 3)];
        int num = 0;
        for (int i = 0; i < rooms.GetLength(0); i++)
        {
            MakeNode(i, 0, i * Room.x + (Room.x / 2f), 0 * Room.y + (Room.y / 2f), num);
            num++;
        }
        for (int j = 0; j < rooms.GetLength(1); j++)
        {
            for (int i = 0; i < rooms.GetLength(0); i++)
            {
                if (rooms[i, j] != null)
                {
                    MakeNode(i, j+1, i * Room.x + (Room.x / 2f), (j + 1) * Room.y + (Room.y / 2f) , num);
                    num++;
                }
                else
                {
                    grid[i, j + 1] = null;
                }              
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
                for(int z = 1; z < rooms.GetLength(1) + 1; z++)
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
        if(AI == true)
        {
            weaponsOnArena.Remove(w);
        }
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
            yield return new WaitForSeconds(10f);
           
            RandomWeapon(Random.value);          
        }
    }

    private void RandomWeapon(float number)
    {
        //porownuje wylosowana liczbe z prawdopodobienstwem wylosowania broni
        float probability = 0;
        for (int i = 0; i < weapons.Length; i++)
        {
            probability += weaponProbability[i];
            if (probability >= number)
            {
                DropWeapon(weapons[i]);
                break;
            }
        }
    }
    
    //
    private void CalculateProbability()
    {
        weaponProbability = new float[weapons.Length];
        for( int i = 0; i < weapons.Length; i++)
        {
            weaponProbability[i] = size / weapons[i].GetComponentInChildren<AttackCollider>().dmg  / weapons.Length;
            weapons[i].GetComponentInChildren<AttackCollider>().enabled = false;
        }
    }

    public void DropWeapon(GameObject weapon)
    {
        while (true)
        {
            int v = (int)Random.Range(0, nodes.Count - 1);
            if (!nodes[v].GetComponent<BoxCollider2D>().IsTouchingLayers(antyDropCollisionMask))
            {
                //Debug.Log(grid[room[0], room[1]].transform.position);
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
        background.transform.localScale = new Vector2(width / sr.sprite.bounds.size.x, height / sr.sprite.bounds.size.y);
        background.transform.position = new Vector2(size * camera.aspect, size);
    }
    
    public void GeneratePlatforms()
    {
        //ustalanie ilosci pokoi
        Room.x = 2 * camera.aspect;
        Room.y = 3;
        rooms = new Room[(int)size,(int)(2*size/3)-1];

        //przeskalowanie objektu platformy aby pokrywał pokój
        platform.transform.localScale += new Vector3(platform.transform.localScale.x * (Room.x - 1), 0, 0);
        int value;
        int x = (int)Random.Range(0, rooms.GetLength(0));
        int y = 0;
        int side = 1;
        while(true)
        {
            //Room.SetRoomCoordinates(x, y);
            //tworzona jest platforma
            Instantiate(platform, new Vector3(x * Room.x + (Room.x/2f), y * Room.y+Room.y), Quaternion.identity);
            //losowane jest gdzie bedzie następny pokoj
            rooms[x, y] = new Room();
            value = (int)Random.Range(0, 5);
            if (value == 3)
            {
                y = y + 1;
                if( x != 0 && x != rooms.GetLength(0))
                {
                    if ((int)Random.Range(1, 2) == 1)
                    {
                        side = -side;
                    }
                }
            }
            else
            {
                x = x + side;
            }
            //sprwdzanie czy nie wyszlo poza siatkę pokoi
            if (x == rooms.GetLength(0) || x < 0)
            {
                y = y + 1;
                side = -side;
                x = x + side;

            }
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
        for (y = 0f; y < (2 * size) +1  ; y++)
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
        for ( x=0 ; x < 2 * size * camera.aspect; x++)
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
        player1.transform.position = grid[0,0].transform.position;
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

