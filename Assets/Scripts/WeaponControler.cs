using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class WeaponControler : NetworkBehaviour {

    //ilość ciosów w sekwencji
    public int maxcombo;
    public GameObject attackCollider;
    public GameObject pickUpTrigger;
    //ścieżka do części ciała avatara której ma być przpisana broń np. Skeleton/Pelvis/Torso/R_arm_1/R_arm_2
    public string holdingRig;
    //wytrzymałość broni
    public float maxDurability;
    [SyncVar(hook = "OnDurabilityChange")] public float durability;
    public Image durabilityImage;
    //pozycja broni po podniesieniu, względem obiektu-rodzica a nie świata
    public Vector3 PickUpPos;
    public Vector3 PickUpRot;
    //avatar który trzyma broń
    public GameObject handler;
    //referencja do skryptu przechowującego informacje o arenie
    public SceneSetup sceneMenager;

    
    public override void OnStartClient()
    {
        gameObject.name = gameObject.name.Replace("(Clone)", "");
        durability = maxDurability;
    }

    //funkcja wykonywana podczas sychronizacji wytzymałości, aktualizuje UI
    void OnDurabilityChange(float durability)
    {
        if (durabilityImage != null)
        {
            durabilityImage.color = new Color(255f, 255f, 255f, durability / maxDurability);
        }
    }

    //wywoływana po upuszczeniu
    public void Clear()
    {
        durabilityImage = null;
        handler = null;
    }

    //aktualizacja informacji o dostępnych broniach 
    public void AddToArena()
    {
        if(isServer) sceneMenager.weaponsOnArena.Add(gameObject);
    }

    //gdy rzucona/upuszczona broń dotknie coś innego niż gracz to można ją podnieść i nie zadaje obrażeń
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Untagged" || col.gameObject.tag == "Platform")
        {
            gameObject.layer = 10;
            attackCollider.SetActive(false);
            pickUpTrigger.SetActive(true);
        }
    }

    //funkcja wywoływana z obiektu avatara który podnosi broń
    //dostosowuje oręż do walki
    public void HandleWeapon(Transform player)
    {
        //wyłączenie niepotrzebnych interakcji
        pickUpTrigger.SetActive(false);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        //umieszczenie broni np. w rekach
        transform.SetParent(player.Find(holdingRig).transform);
        transform.localPosition = PickUpPos;
        transform.localEulerAngles = PickUpRot;

        handler = player.gameObject;
        //przypisanie broni warstwy trzymającego avatara, tak aby była identyfikowana jako jego część
        attackCollider.layer = transform.parent.gameObject.layer;

        if(isServer)sceneMenager.weaponsOnArena.Remove(gameObject);
    }

    private void Update()
    {
        if(durability <= 0)
        {
            if (isServer) sceneMenager.DecreaseWeaponNum(gameObject);         
            NetworkServer.Destroy(gameObject);
        }
    }

    //w przypadku gdy obiekt broni jest niszczony oraz jeżeli jest w danech chwili trzymany to zostaje najpierw odłączony od avatara
    //wykonywana na obu instancjach
    public override void OnNetworkDestroy()
    {
        if (handler)
        {
            PlayerControler PC = handler.GetComponent<PlayerControler>();

            PC.DropWeapon();
            PC.DetachWeapon();
        }
    }
    
    //polecenie do serwera zmniejszające wytrzymałość
    [Command]
    public void CmdDecreaseDurability(int value)
    {
        durability -= value;
    }
}
