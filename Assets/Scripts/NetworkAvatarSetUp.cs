using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkAvatarSetUp : NetworkBehaviour {

    //czy avatar jest drugim lokalnym graczem lub botem
    public bool secondLocalAvatar;
    public PlayerControler PC;
    //0 oznacza że nie został mu jeszcze przypisany numer
    [SyncVar]public int playerNum = 0;
    //lista hitboxów przypisanych do konczyn avatara
    public GameObject[] hitboxes;
    [SyncVar]public bool authorityAssigned = false;

    //komenda do serwera nadająca avatarowi jego numer
    [Command]
    public void CmdSetPlayerNum(int num)
    {
        playerNum = num;
    }

    //komenda do serwera informująca że prawa do obiektu zostały nadane
    [Command]
    public void CmdAuthorityConfirmation()
    {
        authorityAssigned = true;
    }

    //uruchamia się na instancji która ma prawa do obiektu w chwili gdy te prawa otrzymuje
    public override void OnStartAuthority()
    {
        CmdAuthorityConfirmation();
    }

    //oczekuje na nadanie praw którejś z instancji
    IEnumerator WaitForAuthority()
    {
        yield return new WaitWhile(() => !authorityAssigned);
        AvatarNumberSetUp();
    }

    //oczekuje na nadanie avatarowi numeru
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

    //konfigurauje avatara nr1 w grze lokalnej lub każdego z gry sieciowej
    void AvatarSetUp()
    {
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
            //uaktywnia obiekt którego szuka bot
            if (GameData.ai == true) transform.Find("AITarget").gameObject.SetActive(true);
        }
        else
        {
            //konfiguracja drugiego avatara w grze lokalnej
            ControlsSetUp(GameData.p2);
            playerNum = 2;
            PC.playerNum = 2;
            SetPlayerUI();
            PC.networkSetUpDone = true;
        }
        
    }

    //przypisuje wcześniej ustwione klawisze, null oznacza że przypisany jest klawisz pojawiający się w menu opcji przy starcie gry
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
        //resetuje jego wartość (kolejna runda)
        healthBar.fillAmount = 1;
    }
    
    //przypisuje avatarowi odpowiedni obraz paska zdrowia z UI
    void SetPlayerHealthBar(Image img)
    {
        PC.healthBar = img;
    }

    //przypisuje avatarowi odpowiedni obraz noszonej broni z UI
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
