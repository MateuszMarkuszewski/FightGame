using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{   
    //skrypt przypisywany hitboxom. zadaje obrażenia
    public int dmg;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Untagget")
        {

        }

        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("attack collision name = " + collision.gameObject.tag);
            collision.gameObject.SendMessage("DealDamage", dmg);
            SendMessageUpwards("DecreaseDurability", 1);
        }
    }
}
