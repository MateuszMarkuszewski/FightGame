using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControler : MonoBehaviour {

    public int maxcombo;
    private Rigidbody2D rgdBody;

    public Vector3 PickUpPos;
    public Vector3 PickUpRot;

    void Start () {
    }
    //gdy rzucona/upuszczona broń dotknie coś innego niż gracz to można ją podnieść i nie zadaje obrażeń
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Untagged" || col.gameObject.tag == "Platform")
        {
            gameObject.layer = 10;
            gameObject.GetComponent<Rigidbody2D>().Sleep();
            transform.Find("AttackCollider").gameObject.SetActive(false);
            transform.Find("PickUpTrigger").gameObject.SetActive(true);
        }
    }
    void HandleWeapon(Transform player, Transform holdingRig)
    {
        //this.transform.Find("AttackCollider").gameObject.SetActive(true);
        this.transform.Find("PickUpTrigger").gameObject.SetActive(false);
        this.GetComponent<Collider2D>().enabled = false;
        this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        //umieszczenie broni np. w rekach
        this.transform.SetParent(player.Find("Skeleton/Pelvis/Torso/R_arm_1/R_arm_2").transform);
        this.transform.localPosition = PickUpPos;
        this.transform.localEulerAngles = PickUpRot;

        this.transform.Find("AttackCollider").gameObject.layer = this.transform.parent.gameObject.layer;
    }
}
