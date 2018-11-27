using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager {

    
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        //nie słucha do momentu wygenerowania areny
        NetworkServer.dontListen = true;
        Debug.Log(singleton.numPlayers);
        if (singleton.numPlayers == 1) GameData.secondClientConnected = true;
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //w chwili wyjscia klienta restartowana jest rozgrywka
        //alternatywa: zapisywanie transform i currentHealth w GameData// problem z trzymaną bronią
        NetworkServer.dontListen = true;
        GameData.secondClientConnected = false;
        base.OnServerDisconnect(conn);
        NetworkManager.singleton.ServerChangeScene(onlineScene);
    }

}
