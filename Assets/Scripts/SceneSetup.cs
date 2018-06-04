using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class Room
{
    public static int x = 4;
    public static int y = 4;
    public static int[] probability = { 0, 1, 2, 3, 4, 5, 6 };

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
    //prawdopodobieństwo jaki będzie nastepny pokój dla pokoju o danym indeksie
    public static int[][] roomProbabilities = new int[][]
    {
        new int[] { 1, 2, 3, 4, 5, 6},
        new int[] { 0, 2, 3, 5, 6 },
        new int[] { 0, 1, 3, 4, 5 },
        new int[] { 4, 2, 4, 5, 6, 4, 4},
        new int[] { 0, 1, 2, 3, 5, 6 },
        new int[] { 1, 6, 3, 4, 6 , 6, 6},
        new int[] { 0, 1, 2, 3, 4, 5 },

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
        //inicjuje pokój od danym numerze i zmienia prawdopodobieństwo dla następnego losowania
        this.value = probability[value];
        probability = roomProbabilities[this.value];
    }

    public void SetPlatformCoordinates(int x, int y)
    {
        
        //Gizmos.DrawCube(new Vector3(Room.x * x - 2, Room.y * y - 2, 0), new Vector3(2, 2, 0));
        Debug.DrawLine(new Vector3(Room.x * x, Room.y * y,0), new Vector3(Room.x * x, Room.y * y - 4,0), Color.red,200,false);
        Debug.DrawLine(new Vector3(Room.x * x -4, Room.y * y, 0), new Vector3(Room.x * x, Room.y * y, 0), Color.red, 200, false);

        Debug.DrawLine(new Vector3(Room.x * x - 4, Room.y * y -1, 0), new Vector3(Room.x * x, Room.y * y-1, 0), Color.green, 200, false);
        Debug.DrawLine(new Vector3(Room.x * x - 4, Room.y * y -2, 0), new Vector3(Room.x * x, Room.y * y-2, 0), Color.green, 200, false);
        Debug.DrawLine(new Vector3(Room.x * x - 4, Room.y * y -3, 0), new Vector3(Room.x * x, Room.y * y-3, 0), Color.green, 200, false);

        Debug.DrawLine(new Vector3(Room.x * x-3, Room.y * y, 0), new Vector3(Room.x * x -3, Room.y * y - 4, 0), Color.green, 200, false);
        Debug.DrawLine(new Vector3(Room.x * x-2, Room.y * y, 0), new Vector3(Room.x * x -2, Room.y * y - 4, 0), Color.green, 200, false);
        Debug.DrawLine(new Vector3(Room.x * x-1, Room.y * y, 0), new Vector3(Room.x * x -1, Room.y * y - 4, 0), Color.green, 200, false);



        if (this.value != 0)
        {
            //Debug.Log(roomTypes[this.value].Length);
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
        GeneratePlatforms();
        SetupMainGround();
        SetupPlayers();
    }

    public void GeneratePlatforms()
    {

        Room[,] rooms = new Room[(int)width / Room.x, (int)height / Room.y];
        //Debug.Log((int)height);
        //Debug.Log((int)width);
        // przelatuje po siatce pokoi i losuje typ dla każdego
        for (int x = 0; x < rooms.GetLength(0); x++)
        {
            for (int y = 0; y < rooms.GetLength(1); y++)
            {
                rooms[x, y] = new Room((int)Random.Range(0, Room.probability.Length - 1));
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
        Instantiate(ground, new Vector3(platform.x[0], platform.y[0]), Quaternion.identity);
        Instantiate(ground, new Vector3(platform.x[1], platform.y[1]), Quaternion.identity);
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

