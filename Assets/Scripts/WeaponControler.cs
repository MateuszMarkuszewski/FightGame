using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class WeaponControler : NetworkBehaviour {

    // Child GameObject PickUpTrigger musi być pierwszy (AI)

    public int maxcombo;
    private Rigidbody2D rgdBody;
    public GameObject attackCollider;
    public GameObject pickUpTrigger;
    public string holdingRig = "Skeleton/Pelvis/Torso/R_arm_1/R_arm_2";

    public float restTimeAfterCombo;
    public float maxDurability;
    public float durability;
    public Image durabilityImage;
    public Sprite weaponImage;

    public Vector3 PickUpPos;
    public Vector3 PickUpRot;

    public GameObject handler;
    public SceneSetup sceneMenager;

    
    public override void OnStartClient()
    {
        gameObject.name.Replace("(Clone)", "");
    }

    void Start ()
    {
        durability = maxDurability;
    }

    void AlphaUI()
    {
        durabilityImage.color = new Color(255f,255f,255f, durability/maxDurability);
    }

    public void Clear()
    {
        durabilityImage = null;
        handler = null;
    }

    public void AddToArena()
    {
        sceneMenager.weaponsOnArena.Add(gameObject);
    }

    //gdy rzucona/upuszczona broń dotknie coś innego niż gracz to można ją podnieść i nie zadaje obrażeń
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Untagged" || col.gameObject.tag == "Platform")
        {
            //DecreaseDurability(5);
            gameObject.layer = 10;//to juz było w rzucie, czy trzeba?
            //gameObject.GetComponent<Rigidbody2D>().Sleep();
            attackCollider.SetActive(false);
            pickUpTrigger.SetActive(true);
        }
    }

    public void HandleWeapon(Transform player)
    {
        //this.transform.Find("AttackCollider").gameObject.SetActive(true);
        pickUpTrigger.SetActive(false);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        //umieszczenie broni np. w rekach
        transform.SetParent(player.Find(holdingRig).transform);
        transform.localPosition = PickUpPos;
        transform.localEulerAngles = PickUpRot;

        handler = player.gameObject;
        attackCollider.layer = transform.parent.gameObject.layer;
        //dla AI
        sceneMenager.weaponsOnArena.Remove(gameObject);
        
    }

    private void Update()
    {
        if(durability < 0)
        {

            if (handler)
            {
                PlayerControler PC = handler.GetComponent<PlayerControler>();
            
                PC.DropWeapon();
                PC.DetachWeapon();
            }         
            sceneMenager.DecreaseWeaponNum(gameObject);
            Destroy(gameObject);
        }
        if(durabilityImage!=null)AlphaUI();
    }

    public void DecreaseDurability(int value)
    {
        durability -= value;
    }
}
