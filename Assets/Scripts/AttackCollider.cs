using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AttackCollider : MonoBehaviour
{   
    //skrypt przypisywany hitboxom. zadaje obrażenia
    public int dmg;
    public NetworkIdentity networkIdentity;

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.tag == "Hitbox")
        {
            //TODO: odrzut
        }
        else if (collision.gameObject.tag == "Player" && !collision.isTrigger)
        {
            Debug.Log("attack");
            if (networkIdentity.isServer)
            {
                Vector2 dir = collision.transform.position - transform.position;
                PlayerControler enemy = collision.GetComponent<PlayerControler>();
                enemy.SaveForce(dir);
                // collision.GetComponent<Rigidbody2D>().AddForce(dir.normalized * 2000, ForceMode2D.Force);
                enemy.ReduceHealth(dmg);
                if (GetComponentInParent<WeaponControler>()) GetComponentInParent<WeaponControler>().CmdDecreaseDurability(10);
            }           
        }
    }
}
