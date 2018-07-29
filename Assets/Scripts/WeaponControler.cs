using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponControler : MonoBehaviour {

    // Child GameObject PickUpTrigger musi być pierwszy (AI)

    public int maxcombo;
    private Rigidbody2D rgdBody;
    public GameObject attackCollider;
    public GameObject pickUpTrigger;

    public float restTimeAfterCombo;
    public float maxDurability;
    private float durability;
    public Image durabilityImage;
    public Sprite weaponImage;

    public Vector3 PickUpPos;
    public Vector3 PickUpRot;

    public GameObject player;
    public SceneSetup sceneMenager;

    void Start ()
    {
        durability = maxDurability;
    }

    void MakeUI()
    {
        durabilityImage.GetComponent<Image>().sprite = GetComponent<SpriteRenderer>().sprite;
        AlphaUI();
    }

    void AlphaUI()
    {
        durabilityImage.color = new Color(255f,255f,255f, durability/maxDurability);
    }

    public void Clear()
    {
        player = null;
        durabilityImage = null;
        //dla AI
        if (sceneMenager.AI == true)
        {
            sceneMenager.weaponsOnArena.Add(gameObject);
        }
    }

    //gdy rzucona/upuszczona broń dotknie coś innego niż gracz to można ją podnieść i nie zadaje obrażeń
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Untagged" || col.gameObject.tag == "Platform")
        {
            //DecreaseDurability(5);
            gameObject.layer = 10;
            //gameObject.GetComponent<Rigidbody2D>().Sleep();
            attackCollider.SetActive(false);
            pickUpTrigger.SetActive(true);
            attackCollider.GetComponent<Collider2D>().isTrigger = true;

        }
    }

    void HandleWeapon(Transform player)
    {
        //this.transform.Find("AttackCollider").gameObject.SetActive(true);
        pickUpTrigger.SetActive(false);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        //umieszczenie broni np. w rekach
        transform.SetParent(player.Find("Skeleton/Pelvis/Torso/R_arm_1/R_arm_2").transform);
        transform.localPosition = PickUpPos;
        transform.localEulerAngles = PickUpRot;

        attackCollider.layer = transform.parent.gameObject.layer;
        MakeUI();
        //dla AI
        if(sceneMenager.AI == true)
        {
            sceneMenager.weaponsOnArena.Remove(gameObject);
        }
    }

    private void Update()
    {
        if(durability <= 0)
        {
            sceneMenager.DecreaseWeaponNum(gameObject);
            player.GetComponent<PlayerControler>().DropWeapon();
            Destroy(gameObject);
        }
        if(durabilityImage!=null)AlphaUI();
    }

    private void DecreaseDurability(int value)
    {
        durability -= value;
        Debug.Log(durability);
    }
}
