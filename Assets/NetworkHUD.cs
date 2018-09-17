using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkHUD : MonoBehaviour {

    private NetworkManagerHUD HUD;

    void Start () {
        HUD = GetComponent<NetworkManagerHUD>();
	}

    void ShowHUD(bool show)
    {
        HUD.showGUI = show;
    }

    private void OnGUI()
    {
        if (HUD.enabled)
        {
            HUD.offsetX = Screen.width / 3;
            HUD.offsetY = Screen.height / 3;
        }
    }
}
