using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkAvatarSetUp : NetworkBehaviour {

    public PlayerControler PC;
    [SyncVar]public int playerNum;
    public GameObject[] hitboxes;

    //wykonywane na wszytki kopiach tego obiektu
    //ustala którym graczem jest client i dostosowuje elementy
    public override void OnStartClient()
    {
        ActiveHitboxes(true);
        if (isServer)playerNum = NetworkServer.connections.Count;
        Debug.Log("client"+playerNum);
        SetPlayerUI();
        SetPlayerLayer();
        SetLayerToJumpTest();
        ActiveHitboxes(false);        
    }

    //przypisuje odpowiedni interfejs gracza
    void SetPlayerUI()
    {
        Transform playerInterface = GameObject.Find("Canvas").transform.Find("Player" + playerNum);
        SetPlayerHealthBar(playerInterface.Find("Healthbar").Find("RedBar").Find("GreenBar").GetComponent<Image>());
        SetPlayerWeaponUI(playerInterface.Find("WeaponDurability").GetComponent<Image>());
    }
    
    void SetPlayerHealthBar(Image img)
    {
        PC.healthBar = img;
    }

    void SetPlayerWeaponUI(Image img)
    {
        PC.weaponDurability = img;
    }

    //ustala odpowiednie warstwy elementów avatara
    void SetPlayerLayer()
    {
        int mainLayer = playerNum == 1 ? 12 : 17;
        int rigLayer = playerNum == 1 ? 13 : 16;
        Transform[] objects = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform obj in objects)
        {
            obj.gameObject.layer = mainLayer;
        }
        Transform[] skeleton = transform.Find("Skeleton").gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform rig in skeleton)
        {
            rig.gameObject.layer = rigLayer;
        }
    }

    //Używane do aktywowania a nastepnie dezktywowania hitboxów aby ustalić ich warstwę
    void ActiveHitboxes(bool set)
    {
        foreach(GameObject go in hitboxes)
        {
            go.SetActive(set);
        }
    }

    //dodaje warstwe przeciwnika do powierzchni od których mozna sie odbić
    void SetLayerToJumpTest()
    {
        LayerMask layerToJump = LayerMask.GetMask(LayerMask.LayerToName(playerNum == 1 ? 17 : 12));
        PC.layersToTest = PC.layersToTest | layerToJump;
    }
}
