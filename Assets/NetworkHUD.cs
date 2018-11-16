using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class NetworkHUD : NetworkBehaviour {

    public Button hostButton;

    private void Start()
    {
        //hostButton.interactable = false;
    }

    public void HostLocalGame()
    {
        NetworkManager NM = NetworkLobbyManager.singleton;
        NM.StartHost();
        NM.ServerChangeScene("test");
        NetworkServer.dontListen = true;
    }

    public void HostNetworkGame()
    {
        NetworkManager NM = NetworkLobbyManager.singleton;

        NM.StartHost();
    }

    public void Connect()
    {
        NetworkManager NM = NetworkLobbyManager.singleton;

        NM.StartClient();
    }

    public void ChangeServerAdress(TMP_InputField _input)
    {
        NetworkManager NM = NetworkLobbyManager.singleton;

        NM.networkAddress = _input.text;
    }

    public void ReadyToHost(bool ready)
    {
        hostButton.interactable = ready;
    }

}
