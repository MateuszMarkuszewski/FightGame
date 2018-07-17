using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class Room
{
    public static float x = 3;
    public static float y = 3;
    public int value;

    public Room( int value)
    {
        this.value = value;
    }

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
    private Room[,] rooms;

    public GameObject[] weapons;
    public int maxWeaponsNum;
    private int currentWeaponNum = 0;
    private float[] weaponProbability;

    void Start()
    {
        SetupCamera();
        width = 2 * size * camera.aspect;
        height = 2 * size;
        SetBackgroundSize();
        SetupMainGround();
        GeneratePlatforms();
        //SetupPlayers();
        CalculateProbability();


        DropWeapon(weapons[0]);
        DropWeapon(weapons[1]);

        StartCoroutine(WeaponDropMenager());
    }

    private void Update()
    {
        
    }
    private void DecreaseWeaponNum()
    {
        currentWeaponNum--;
    }

    IEnumerator WeaponDropMenager()
    {
        
        while (true)
        {
            Debug.Log("loop");
            while (currentWeaponNum == maxWeaponsNum)
            {
                yield return null;
            }
            yield return new WaitForSeconds(1f);

            RandomWeapon(Random.value);          
        }
    }

    private void RandomWeapon(float number)
    {
        float probability = 0;
        for (int i = 0; i < weapons.Length; i++)
        {
            probability += weaponProbability[i];
            if (probability >= number)
            {
                Debug.Log(weapons[i]);
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
            Debug.Log(weapons[i] + " " + weaponProbability[i]);
        }
    }

    public void DropWeapon(GameObject weapon)
    {
        int[] room = new int[] { (int)Random.Range(0, rooms.GetLength(0)), (int)Random.Range(0, rooms.GetLength(1))};
        Instantiate(weapon, new Vector3(room[0]* Room.x + (Room.x / 2f), room[1] * Room.y + 2), Quaternion.identity).name = weapon.name;
        currentWeaponNum++;
    }

    public void SetBackgroundSize()
    {
        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
        background.transform.localScale = new Vector3((height / Screen.height * Screen.width) / sr.sprite.bounds.size.x, height / sr.sprite.bounds.size.y, 10f);
        background.transform.position = new Vector3(size * camera.aspect, size, 20f);
    }

    public void GeneratePlatforms()
    {
        //ustalanie ilosci pokoi
        Room.x = width / size;
        Room.y = height / (2*size/3);
        rooms = new Room[(int)size,(int)(2*size/3)-1];
        //przeskalowanie objektu platformy aby pokrywał pokój
        platform.transform.localScale += new Vector3(platform.transform.localScale.x * (Room.x - 1), 0, 0);
        /*
        for( int a =0; a < (int)width / Room.x; a++)
        {
            for (int b = 0; b < (int)height / Room.y; b++)
            {
                Room.SetRoomCoordinates(a, b);
            }
        }
        */
        int x = (int)Random.Range(0, rooms.GetLength(0));
        int y = 0;
        int side = 1;
        while(true)
        {
            Room.SetRoomCoordinates(x, y);
            //tworzona jest platforma
            Instantiate(platform, new Vector3(x * Room.x + (Room.x/2f), y * Room.y+Room.y), Quaternion.identity);
            //losowane jest gdzie bedzie następny pokoj
            rooms[x, y] = new Room((int)Random.Range(0, 5));
            if (rooms[x, y].value == 3)
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
            Instantiate(wallLeft, new Vector3(0f, y), Quaternion.identity);
        }
        x = 2 * size * camera.aspect;
        for (y = 0f; y < (2 * size) + 1; y++)
        {
            Instantiate(wallRight, new Vector3(x, y), Quaternion.identity);
        }
        for (x = 0f; x < 2 * size * camera.aspect; x++)
        {
            Instantiate(ground, new Vector3(x, 0f), Quaternion.identity);
        }
        y = (2 * size) + 1;
        for ( x=0 ; x < 2 * size * camera.aspect; x++)
        {
            Instantiate(ceiling, new Vector3(x, y), Quaternion.identity);
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
        player1.transform.position = new Vector3(1f, 1f);
    }

    public void Perlin()
    {
        //generuje szum Perlina
        // Debug.Log(height);
        for (int x = 0; x < 2 * size * camera.aspect; x++)
        {

            Debug.Log(Mathf.PerlinNoise(((float)x) / 35, 0f));
            for (int lvl = 10; lvl < height; lvl = lvl + 10)
            {
                Instantiate(ground, new Vector3(x, (Mathf.PerlinNoise(((float)x) / 35, 0f) * 10) + lvl), Quaternion.identity);

            }
        }
    }
}

