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
            //Debug.Log(gameObject.transform.parent.name);
            //collision.GetComponent<Rigidbody2D>().AddForce(new Vector2((transform.position.x > collision.transform.position.x ? -dmg : dmg) *100, 0f), ForceMode2D.Force);
            //collision.GetComponent<Rigidbody2D>().velocity = new Vector2((transform.position.x > collision.transform.position.x ? -dmg : dmg) * 2, 0f);
            collision.gameObject.GetComponent<PlayerControler>().DealDamage(dmg);
            try
            {
                SendMessageUpwards("DecreaseDurability", 10);
            }
            catch
            {
                //atakowanie bez broni
            }
        }
    }
}
