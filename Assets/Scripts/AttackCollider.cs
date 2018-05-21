using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{   
    //skrypt przypisywany hitboxom. zadaje obrażenia
    public int dmg;

    void Start()
    {

    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Untagget")
        {

        }

        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.SendMessage("DealDamage", dmg);
        }
    }   

}
