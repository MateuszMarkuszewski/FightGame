using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{   
    //skrypt przypisywany hitboxom. zadaje obrażenia
    public int dmg;

    public struct C
    {
        public int dmg;
        public int way;
        public C(int i, int j)
        {
            dmg = i;
            way = j;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.tag == "Untagget")
        {

        }

        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("attack collision name = " + collision.gameObject.tag);
            collision.GetComponent<Rigidbody2D>().AddForce(new Vector2((transform.position.x > collision.transform.position.x ? -dmg : dmg) *100, 0f), ForceMode2D.Force);
            //collision.GetComponent<Rigidbody2D>().velocity = new Vector2((transform.position.x > collision.transform.position.x ? -dmg : dmg) * 2, 0f);
            collision.gameObject.SendMessage("DealDamage", dmg);
            SendMessageUpwards("DecreaseDurability", 10);
        }
    }
}
