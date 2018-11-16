using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;



public class CustomLobbyManager : NetworkLobbyManager {

    public override void OnLobbyServerConnect(NetworkConnection conn)
    {
        base.OnLobbyServerConnect(conn);
        Debug.Log(NetworkServer.connections.Count);
        if (NetworkServer.connections.Count == 1)
        {
            Debug.Log("scene change serwer");
            ServerChangeScene(playScene);//SceneManager.LoadScene(playScene, LoadSceneMode.Single);
            //NetworkServer.SetClientNotReady(conn);
        }
        /* if (NetworkServer.connections.Count == 2)
        {   
             ServerChangeScene(playScene);
             //ClientScene.
        }*/

    }
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        //NetworkServer.SpawnObjects();
        Debug.Log("server loaded");
    }
    

    //bazowa powodowała błąd, "The default implementation of OnClientSceneChanged
    //in the NetworkManager is to add a player object for the connection if no player object exists."
    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
    }
    public override void OnLobbyClientConnect(NetworkConnection conn)
    {
        Debug.Log("client not connected ");

        base.OnLobbyClientConnect(conn);
        ClientScene.Ready(conn);
        
        
        //ClientScene.AddPlayer(0);
        Debug.Log("client connected ");       
    }
   /*
    public override void OnClientConnect(NetworkConnection connection)
    {
        ClientScene.Ready(connection);
        ClientScene.AddPlayer(0);
    }*/

    
}
