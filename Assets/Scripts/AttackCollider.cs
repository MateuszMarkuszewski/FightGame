using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{   
    //skrypt przypisywany hitboxom. zadaje obrażenia
    public int dmg;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.tag == "Hitbox")
        {
            Debug.Log("weapon");
        }
        else if (collision.gameObject.tag == "Player" && !collision.isTrigger)
        {
            Vector2 dir = collision.transform.position - transform.position;
            PlayerControler enemy = collision.GetComponent<PlayerControler>();
            enemy.SaveForce(dir);
            collision.GetComponent<Rigidbody2D>().AddForce(dir.normalized * 2000, ForceMode2D.Force);
            enemy.DealDamage(dmg);
            GetComponentInParent<WeaponControler>().DecreaseDurability(10);
        }
    }
}
