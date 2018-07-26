using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIControler : MonoBehaviour {

    public GameObject enemy;
    public PlayerControler playerControler;
    public delegate void Alghorthm();
    public SceneSetup arenaData;


    void Start () {
        playerControler.AI = true;
        arenaData.AI = true;
    }

    void Update () {

    }

    public void FindPath(List<GameObject> neighbours, List<float> distances, GameObject current)
    {
        int nextNode = 0;
        float minDistance = float.MaxValue;
        float tmp;
        //A*
        for(int i = 0; i < neighbours.ToArray().Length; i++)
        {
            tmp = distances[i] + Distance(neighbours[i].transform.position.x, neighbours[i].transform.position.y, enemy.transform.position.x, enemy.transform.position.y);
            if(tmp < minDistance)
            {
                minDistance = tmp;
                nextNode = i;
            }
        }
        /*if (minDistance > Distance(current.transform.position.x, current.transform.position.y, enemy.transform.position.x, enemy.transform.position.y))
        {
            AdjustCoordinates(enemy.transform.position.x, enemy.transform.position.y);
        }
        else
        {*/
            AdjustCoordinates(neighbours[nextNode].transform.position.x, neighbours[nextNode]);
        //}
    }

    void AdjustCoordinates(float x , GameObject target)
    {
        if (transform.position.x > x)
        {
            playerControler.Move(-1f);
        }
        else
        {
            playerControler.Move(1f);

        }
        if (transform.position.y < target.GetComponent<BoxCollider2D>().bounds.min.y)
        {
            //Debug.Log(transform.position.y);
            playerControler.Jump();
        }
        else
        {
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
}
