using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkAvatarSetUp : NetworkBehaviour {

    public bool secondLocalAvatar;
    public PlayerControler PC;
    [SyncVar]public int playerNum = 0;
    public GameObject[] hitboxes;
    [SyncVar]public bool authorityAssigned = false;


    [Command]
    public void CmdSetPlayerNum(int num)
    {
        playerNum = num;
    }

    [Command]
    public void CmdAuthorityConfirmation()
    {
        authorityAssigned = true;
    }

    public override void OnStartAuthority()
    {
        CmdAuthorityConfirmation();
    }

    IEnumerator WaitForAuthority()
    {
        yield return new WaitWhile(() => !authorityAssigned);
        AvatarNumberSetUp();
    }
    //
    IEnumerator WaitForSynchronization()
    {
        yield return new WaitWhile(() => playerNum == 0);
        AvatarSetUp();
    }

    //numer avatara ustala klient który ma nad nim władzę
    void AvatarNumberSetUp()
    {
        if (hasAuthority)
        {
            CmdSetPlayerNum(NetworkManager.singleton.client.connection.connectionId + 1);
        }
        StartCoroutine(WaitForSynchronization());
    }

    void AvatarSetUp()
    {
        //if (isServer) playerNum = NetworkManager.singleton.numPlayers;//.connections.Count;
        Debug.Log("playernum " + playerNum);
        PC.playerNum = playerNum;
        ControlsSetUp(GameData.p1);
        SetPlayerUI();
        ActiveHitboxes(true);
        SetPlayerLayer();
        SetLayerToJumpTest();
        ActiveHitboxes(false);
        PC.networkSetUpDone = true;
    }

    //wykonywane na wszytki kopiach tego obiektu
    //ustala numer avatara i dostosowuje elementy
    public override void OnStartClient()
    {
        if (!secondLocalAvatar)//aby ustalic numer avatara potrzebna jest wartosc hasAuthority która tu jest jeszcze nie ustalona
        {
            StartCoroutine(WaitForAuthority());
        }
        else
        {
            ControlsSetUp(GameData.p2);
            playerNum = 2;
            SetPlayerUI();
            if ((bool)GameData.ai) transform.Find("AITarget").gameObject.SetActive(true);
            PC.networkSetUpDone = true;
        }
        
    }

    void ControlsSetUp(GameData.Controls player)
    {
        if(player.attack.key != null)
        {
            PC.attack = (KeyCode)player.attack.key;
        }
        if (player.pickUp.key != null)
        {
            PC.takeWeapon = (KeyCode)player.pickUp.key;
        }
        if (player.up.key != null)
        {
            PC.jumpKey = (KeyCode)player.up.key;
        }
        if (player.down.key != null)
        {
            PC.down = (KeyCode)player.down.key;
        }
        if (player.right.key != null)
        {
            PC.right = (KeyCode)player.right.key;
        }
        if (player.left.key != null)
        {
            PC.left = (KeyCode)player.left.key;
        }
    }


    //przypisuje odpowiedni interfejs gracza
    void SetPlayerUI()
    {
        Transform playerInterface = GameObject.Find("Canvas").transform.Find("Player" + playerNum);
        Image healthBar = playerInterface.Find("Healthbar").Find("RedBar").Find("GreenBar").GetComponent<Image>();
        Image weaponDurability = playerInterface.Find("WeaponDurability").GetComponent<Image>();
        SetPlayerHealthBar(healthBar);
        SetPlayerWeaponUI(weaponDurability);

        healthBar.fillAmount = 1;
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
