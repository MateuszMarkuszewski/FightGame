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
    private Camera camera;
    public GameObject player1;

    private float width;
    private float height;


    void Start()
    {
        SetupCamera();
        width = 2 * size * camera.aspect;
        height = 2 * size;
        SetupMainGround();
        GeneratePlatforms();
        SetupPlayers();
    }

    public void GeneratePlatforms()
    {
        //TODO: zależniość miadzy iloscia pokoi a rozmiarem areny
        Room.x = width / 6;
        Room.y = height / 4;
        Room[,] rooms = new Room[6,4];
        //rozszerzenie sprita ziemi aby pokrywał pokój
        ground.transform.localScale += new Vector3(ground.transform.localScale.x * (Room.x - 1), 0, 0);
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
            Instantiate(ground, new Vector3(x * Room.x + (Room.x/2f), y * Room.y+2), Quaternion.identity);
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
        //TODO: sufit?
        //Tworzy podłogę i ściany   
        for (float y = 0f; y < (2 * size) +1  ; y++)
        {
            Instantiate(wallLeft, new Vector3(0f, y), Quaternion.identity);
        }
        float x = 2 * size * camera.aspect;
        for (float y = 0f; y < (2 * size) + 1; y++)
        {
            Instantiate(wallRight, new Vector3(x, y), Quaternion.identity);
        }
        for (x = 0f; x < 2 * size * camera.aspect + 1; x++)
        {
            Instantiate(ground, new Vector3(x, 0f), Quaternion.identity);
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

