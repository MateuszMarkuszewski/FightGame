using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkHUD : NetworkBehaviour {

    public Button hostButton;
    public Button connectButton;


    private void Update()
    {
        //kontrola dostępności przycisku rozpoczynającego rozgrywkę
        if (GameData.sizeMap != null)
        {
            hostButton.interactable = true;
        }
        else
        {
            hostButton.interactable = false;
        }
    }

    //aktywuje przycisk gdy host jest wpisany
    public void ConnectButtonEnableCheck(string addr)
    {
        if (addr.Length == 0)
            connectButton.interactable = false;
        else connectButton.interactable = true;
    }

    public void HostLocalGame()
    {
        NetworkManager NM = NetworkLobbyManager.singleton;
        NM.StartHost();
        NM.ServerChangeScene(NM.onlineScene);
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

    //zmiana nazwy serwera do którego będzie nawiązywane połączenie
    public void ChangeServerAdress(TMP_InputField _input)
    {
        NetworkManager NM = NetworkLobbyManager.singleton;

        NM.networkAddress = _input.text;
    }
}
