using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControler : MonoBehaviour {

    public int maxcombo;
    public int dmg;
    private Rigidbody2D rgdBody;

    void Start () {

    }
    //gdy rzucona/upuszczona broń dotknie coś innego niż gracz to można ją podnieść i nie zadaje obrażeń
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Untagged")
        {
            gameObject.GetComponent<Rigidbody2D>().Sleep();
            transform.Find("AttackCollider").gameObject.SetActive(false);
            transform.Find("Trigger").gameObject.SetActive(true);
        }

    }
}
