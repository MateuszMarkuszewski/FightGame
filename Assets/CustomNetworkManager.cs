using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager {

    
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        NetworkServer.dontListen = true;//tu nie dziala moze serwer nie aktywny

        Debug.Log("onserverconn");
        if (NetworkServer.connections.Count == 2) GameData.secondClientConnected = true;
    }
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        //GameData.secondClientConnected = true;
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {

        NetworkServer.dontListen = true;//byc moze nie potrzebne 
        GameData.secondClientConnected = false;
        base.OnServerDisconnect(conn);
        Debug.Log(NetworkManager.singleton.numPlayers);
        NetworkManager.singleton.ServerChangeScene(onlineScene);

    }



}
