using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Room
{
    public static int x = 4;
    public static int y = 4;
    public static int[] values = { 0, 1, 2, 3, 4, 5, 6, 7 };
    //pierwsza współrzdna to typ pokoju, druga to składowe platformy które zawierają współżędne x i y
    public static int[][][] roomTypes = new int[][][]
    {
        new int[][] {},
        new int[][] { new int[] { -2, -3 }, new int[] { -3, -3 } },
        new int[][] { new int[] { -2, -2 }, new int[] { -3, -2 } },
        new int[][] { new int[] { -1, -3 }, new int[] { -2, -3 } },
        new int[][] { new int[] { -3, -3 }, new int[] { -4, -3 } },
        new int[][] { new int[] { -1, -2 }, new int[] { -2, -2 } },
        new int[][] { new int[] { -3, -2 }, new int[] { -4, -2 } },
    };
    public class Platform
    {
        public int[] x;
        public int[] y;

        public Platform( int length)
        {
            this.x = new int[length];
            this.y = new int[length];

            //Debug.Log(Room.roomTypes[1][0][1]);
        }

    }
    public Platform platform;
    public int value;

    public Room( int value)
    {
        this.value = values[value];
    }

    public void SetPlatformCoordinates(int x, int y)
    {
        if(this.value != 0)
        {
            Debug.Log(roomTypes[this.value].Length);
            this.platform = new Platform(roomTypes[this.value].Length);
            this.platform.x[0] = Room.x * x + roomTypes[this.value][0][0];
            this.platform.y[0] = Room.y * y + roomTypes[this.value][0][1];
            this.platform.x[1] = Room.x * x + roomTypes[this.value][1][0];
            this.platform.y[1] = Room.y * y + roomTypes[this.value][0][1];
        }

    }

}

public class SceneSetup : MonoBehaviour
{

    public float size;
    public GameObject groundObject;
    private Camera camera;
    public GameObject player1;

    private float width;
    private float height;


    void Start()
    {
        //Debug.Log((int)Random.Range(1, 10));
        SetupCamera();
        width = 2 * size * camera.aspect;
        height = 2 * size;
        GeneratePlatforms();
        SetupMainGround();
        SetupPlayers();
        //Perlin();
    }

    public void Perlin()
    {
       // Debug.Log(height);
        //int div = Random.value;
        for (int x = 0; x < 2 * size * camera.aspect; x++)
        {

            Debug.Log(Mathf.PerlinNoise(((float)x)/35 , 0f));
            for (int lvl = 10; lvl < height; lvl=lvl+10)
            {
                Instantiate(groundObject, new Vector3(x, (Mathf.PerlinNoise(((float)x) / 35, 0f) * 10) + lvl), Quaternion.identity);

            }
        }
    }

    public void GeneratePlatforms()
    {

        Room[,] rooms = new Room[(int)width / Room.x, (int)height / Room.y];
        //Debug.Log((int)height);
        //Debug.Log((int)width);

        for (int x = 0; x < rooms.GetLength(0); x++)
        {
            for (int y = 0; y < rooms.GetLength(1); y++)
            {
                rooms[x, y] = new Room((int)Random.Range(0, 4));
                rooms[x, y].SetPlatformCoordinates(x+1, y+1);
                try
                {
                    DrawPlatform(rooms[x, y].platform);

                }
                catch (System.NullReferenceException e)
                {

                }

            }
        }
    }
    public void DrawPlatform(Room.Platform platform)
    {
        //Debug.Log(platform.x[0]);
        Instantiate(groundObject, new Vector3(platform.x[0], platform.y[0]), Quaternion.identity);
        Instantiate(groundObject, new Vector3(platform.x[1], platform.y[1]), Quaternion.identity);
    }


    public void SetupMainGround()
    {
       
        for (float y = 0f; y < 2 * size  ; y++)
        {
            Instantiate(groundObject, new Vector3(0f, y), Quaternion.identity);
        }
        float x = 2 * size * camera.aspect;
        for (float y = 0f; y < 2 * size; y++)
        {
            Instantiate(groundObject, new Vector3(x, y), Quaternion.identity);
        }
        for (x = 0f; x < 2 * size * camera.aspect; x++)
        {
            Instantiate(groundObject, new Vector3(x, 0f), Quaternion.identity);
        }
    }

    public void SetupCamera()
    {
        camera = Camera.main;
        camera.orthographicSize = size;
        camera.transform.position = new Vector3(size * camera.aspect, size, -10f);
    }

    public void SetupPlayers()
    {
        player1.transform.position = new Vector3(1f, 1f);
    }
}

