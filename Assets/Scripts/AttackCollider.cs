using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//przypisywany hitboxom, zadaje obrażenia
public class AttackCollider : MonoBehaviour
{   
    //ilość odejmowanego życia podczas ataku
    public int dmg;
    //identyfikator rodzica
    public NetworkIdentity networkIdentity;
    WeaponControler weaponControler;

    private void Awake()
    {
        weaponControler = GetComponentInParent<WeaponControler>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {       
        if (collision.gameObject.tag == "Hitbox")
        {
            //TODO: odrzut
        }
        else if (collision.gameObject.tag == "Player" && !collision.isTrigger)
        {
            if (networkIdentity.isServer)
            {
                Vector2 dir = collision.transform.position - transform.position;
                PlayerControler enemy = collision.GetComponent<PlayerControler>();
                //zapisywany kierunek w którym avatar poleci w przypadku śmierci
                enemy.SaveForce(dir);
                //odjęcie zdrowia avatarowi z którym koliduje
                enemy.ReduceHealth(dmg);
                //kończyny nie posiadają wytrzymałości
                if (weaponControler) weaponControler.CmdDecreaseDurability(10);
            }           
        }
    }
}
