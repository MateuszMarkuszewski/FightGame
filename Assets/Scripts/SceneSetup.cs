using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Networking;


public class SceneSetup : NetworkBehaviour
{
    //rozmiar mapy
    [SyncVar] public float size;
    //obiekty składające się na arenę
    public GameObject ground;
    public GameObject wallLeft;
    public GameObject wallRight;
    public GameObject ceiling;
    public GameObject platform;
    public GameObject background;
    public Camera mainCamera;
    public GameObject node;
    public GameObject[] weapons;

    [SyncVar] public float cameraAspect;
    
    private float width;
    private float height;
    
    //tablica reprezentująca pokoje
    public Node[,] rooms;
    
    //wykorzystywane do zrzutu broni
    private int maxWeaponsNum;
    public int currentWeaponNum = 0;
    private float[] weaponProbability;
    public LayerMask antyDropCollisionMask;

    //wykorzystywane do ai
    public List<GameObject> weaponsOnArena;
    public List<Node> nodes;
    [SyncVar]public bool gridDone = false;

    //rozmiary pokoju
    public static float roomSizeX;
    public static float roomSizeY;


    //wywolywane tylko na serwerze
    public override void OnStartServer()
    {
        cameraAspect = mainCamera.aspect;
        //ustawienie sceny
        size = (float)GameData.sizeMap;
        SetupCamera(size);
        width = 2 * size * cameraAspect;
        height = 2 * size;
        SetBackgroundSize();
        SetupMainGround();
        GeneratePlatforms();
        maxWeaponsNum = (int)size/2;
        MakeGraph();
        gridDone = true;
        CalculateProbability();
        StartCoroutine(WeaponDropMenager());
        //oznacza to ze gra nie jest lokalna i czeka na polaczenie
        if(GameData.ai == null)NetworkServer.dontListen = false;
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

    //tworzony jest obiekt Node i przypisywany do odpowiedniego miejsca w tablicy pokoi
    void MakeNode(int i, int j, float posX, float posY, int num)
    {
        GameObject n = Instantiate(node, new Vector3(posX, posY, 0), Quaternion.identity);
        n.GetComponent<Node>().nodeNum = num;
        n.GetComponent<BoxCollider2D>().size = new Vector2(roomSizeX, roomSizeY);
        rooms[i, j] = n.GetComponent<Node>();
        nodes.Add(n.GetComponent<Node>());
    }

    //tworzenie krawędzi pomiędzy wierzchołami
    void MakeGraph()
    {
        for (int j = 0; j < rooms.GetLength(1) ; j++)
        {
            for (int i = 0; i < rooms.GetLength(0); i++)
            {
                if (rooms[i, j] != null)
                {
                    MakeEdge(i, j, i - 1, j);
                    MakeEdge(i, j, i + 1, j);
                    MakeEdge(i, j, i, j + 1);
                    MakeEdge(i, j, i - 1, j - 1);
                    MakeEdge(i, j, i + 1, j - 1);
                    MakeEdge(i, j, i - 2, j + 1);
                    MakeEdge(i, j, i + 2, j + 1);
                    //dodanie każdej platformy poniżej obecnej
                    for (int z = 1; z < rooms.GetLength(1) + 1; z++)
                    {
                        MakeEdge(i, j, i, j - z);
                    }
                }
            }
        }

    }

    //sprawdza czy istnieją pokoje o tych indeksach i jeśli tak to tworzy wierzchołek
    void MakeEdge(int room1x, int room1y, int room2x, int room2y)
    {
        if(room1x >= 0 && room1x < rooms.GetLength(0) && room1y >= 0 && room1y < rooms.GetLength(1) &&
           room2x >= 0 && room2x < rooms.GetLength(0) && room2y >= 0 && room2y < rooms.GetLength(1) &&
           rooms[room2x,room2y] != null)
        {
            Node node1 = rooms[room1x, room1y];
            Node node2 = rooms[room2x, room2y];
            node1.neighbours.Add(node2.GetComponent<Node>());
            node1.distances.Add(Distance(node1.transform.position.x, node1.transform.position.y, node2.transform.position.x, node2.transform.position.y));
            //Debug.DrawLine(new Vector3(node1.transform.position.x, node1.transform.position.y), new Vector3(node2.transform.position.x, node2.transform.position.y), Color.blue, 200, false);
        }
    }

    float Distance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow((x2 - x1), 2) + Mathf.Pow((y2 - y1), 2));
    }

    //usunięcie broni z danych areny
    public void DecreaseWeaponNum(GameObject w)
    {
        //dostepne do podniesienia
        weaponsOnArena.Remove(w);    
        //całkowita ilosć broni na arenie
        currentWeaponNum--;
    }

    //uruchomiona przez całą rozgrywkę, zarządza pojawianiem się broni
    IEnumerator WeaponDropMenager()
    {
        while (true)
        {
            //nie losuje liczby dopoki maxymalna liczba broni na arenie
            while (currentWeaponNum == maxWeaponsNum)
            {
                yield return null;
            }
            //losuje co określony czas
            yield return new WaitForSeconds(1f);

            RandomWeapon();
        }
    }

    //losowanie która broń ma się pojawić na arenie
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
            weaponProbability[i] = size / weapons[i].GetComponentInChildren<AttackCollider>().dmg / weapons.Length;
            weapons[i].GetComponentInChildren<AttackCollider>().enabled = false;
        }
    }

    //losowany jest numer wierzchołka(Node) w którym ma pojawić się broń a następnie ją tworzy
    //jeżeli w wylosowanym wierzchołku znajduje się gracz lub inna broń to losowany jest inny do skutku
    public void DropWeapon(GameObject weapon)
    {
        while (true)
        {
            int v = (int)Random.Range(0, nodes.Count - 1);
            if (!nodes[v].GetComponent<BoxCollider2D>().IsTouchingLayers(antyDropCollisionMask))
            {
                GameObject w = Instantiate(weapon, nodes[v].transform.position, Quaternion.Euler(0f, 0f, 90f));
                w.GetComponent<WeaponControler>().sceneMenager = this;
                currentWeaponNum++;
                weaponsOnArena.Add(w);
                CmdSpawnGameObjectOnClient(w);
                break;
            }
        }
    }

    //tworzony jest obiekt tła i nadawane są mu rozmiary
    public void SetBackgroundSize()
    {
        GameObject BG = Instantiate(background);
        SpriteRenderer sr = BG.GetComponent<SpriteRenderer>();
        Vector3 backgroundSizes = sr.sprite.bounds.size;
        BG.GetComponent<NetworkBackgroundScaleSync>().SetScale(width / backgroundSizes.x, height / backgroundSizes.y);
        BG.transform.position = new Vector2(size * cameraAspect, size);
        CmdSpawnGameObjectOnClient(BG);
    }

    //generowanie areny
    public void GeneratePlatforms()
    {
        //rozmiar pojedynczego pokoju 
        roomSizeX = 2 * cameraAspect;
        roomSizeY = 3;
        //ustalanie ilosci pokoi
        rooms = new Node[(int)size, (int)(2 * size / 3)];
        
        //tworzone są obiekty Node w każdym pokoju przy podłodze
        int num = 0;
        for (int i = 0; i < rooms.GetLength(0); i++)
        {
            MakeNode(i, 0, i * roomSizeX + (roomSizeX / 2f), 0 * roomSizeY + (roomSizeY / 2f), num);
            num++;
        }
        //losowanie pokoju z pierwszego rzędu 
        int x = Random.Range(0, rooms.GetLength(0));
        int y = 1;
        //przekazanie obiektowi platformy informacji aby ten mógł podczas tworzenia się przeskalować
        platform.GetComponent<NetworkPlatformScaleSync>().roomSizeX = roomSizeX;
        //losowanie kierunku
        int[] way = { 1, -1 };
        int side = way[Random.Range(0, 1)];
        //pętla która wykonuje się do momentu wyjscia poza górną granicę siatki
        while (true)
        {
            //DrawRoomBounds(x, y);
            //tworzona jest platforma
            GameObject go = Instantiate(platform, new Vector2(x * roomSizeX + (roomSizeX / 2f), y * roomSizeY), Quaternion.Euler(0f, 0f, 0f));            
            CmdSpawnGameObjectOnClient(go);
            //w wylosowanym pokoju tworzony jest wierzchołek (obiekt Node)
            MakeNode(x, y, x * roomSizeX + (roomSizeX / 2f), y * roomSizeY + (roomSizeY / 2f), num);
            num++;
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

    //Tworzona jest podłoga, sufit i ściany   
    public void SetupMainGround()
    {
        float y;
        float x;
        for (y = 0f; y < (2 * size) + 1; y++)
        {
            CmdSpawnGameObjectOnPosition(wallLeft, 0f, y);
        }
        x = 2 * size * cameraAspect;
        for (y = 0f; y < (2 * size) + 1; y++)
        {
            CmdSpawnGameObjectOnPosition(wallRight, x, y);
        }
        for (x = 0f; x < 2 * size * cameraAspect; x++)
        {
            CmdSpawnGameObjectOnPosition(ground, x, 0f);
        }
        for (x = 0; x < 2 * size * cameraAspect; x++)
        {
            CmdSpawnGameObjectOnPosition(ceiling, x, y);
        }
    }

    //zmiana rozmiaru kamery w zależności od rozmiaru areny tak aby ich krawędzie się pokrywały
    public void SetupCamera(float size)
    {
        mainCamera.orthographicSize = size;
        mainCamera.transform.position = new Vector3(size * cameraAspect, size, -10f);
    }

    //przyjmuje jako argument prefab, tworzy go na serwerzse a nastepnie na klientach
    [Command]
    void CmdSpawnGameObjectOnPosition(GameObject go, float x, float y)
    {
        GameObject instance = Instantiate(go);
        instance.transform.position = new Vector2(x, y);
        NetworkServer.Spawn(instance);
    }

    //tworzy na klientach stworzony wcześniej GameObject 
    [Command]
    void CmdSpawnGameObjectOnClient(GameObject instance)
    {
        NetworkServer.Spawn(instance);
    }
    
    public override void OnStartClient()
    {
        //modyfikuje kamere klienta aby dopasowac do areny
        if (isClient && !isServer)
        {
            SetupClientCamera();
        }
    }
    
    public void SetupClientCamera()
    {
        mainCamera.aspect = cameraAspect;
        SetupCamera(size);
    }
}
