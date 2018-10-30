using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class NetworkHUD : NetworkBehaviour {

    public NetworkManager NM;

    public void Host()
    {
        NM.StartHost();
    }

    public void Connect()
    {
        NM.StartClient();
    }

    public void ChangeServerAdress(TMP_InputField _input)
    {       
        NM.networkAddress = _input.text;
    }
}
