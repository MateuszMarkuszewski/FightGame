using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager {

    public GameManager gameManager;
    
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        //nie słucha do momentu wygenerowania areny
        NetworkServer.dontListen = true;


        if (singleton.numPlayers == 1)
        {
            GameData.secondClientConnected = true;
        }
    }
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);
        if (singleton.numPlayers == 2)//nadaje władze nad obiketem GameManager drugiemu klientowi aby ten mógł wysyłać żadania do serwera
        {
            gameManager.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
        }
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
