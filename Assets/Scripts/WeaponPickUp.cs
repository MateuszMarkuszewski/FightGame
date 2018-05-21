using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickUp : MonoBehaviour {
    //skrypt dla triggera odpowiedzialnego za podnoszenie broni
    //gdy gracz sie zbliży to hitboxy zostają uaktywnione a trigger podnoszenia dezaktywowany
    //podniesiona broń staje sie dzieckiem gameobjectu gracza
    //uruchamiana jest również funkcja w PlayerControler przypisująca zmiennej weapon podniesioną broń
    public GameObject weaponOnGround;
    void Start () {
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        //Debug.Log("pickup collision name = " + col.gameObject.tag);
       // Debug.Log("----------");

        if (col.gameObject.tag == "Player")
        {
            weaponOnGround.SetActive(false);
            weaponOnGround.transform.Find("AttackCollider").gameObject.SetActive(true);
            weaponOnGround.transform.Find("Trigger").gameObject.SetActive(false);

            weaponOnGround.transform.SetParent(col.transform);
            col.gameObject.SendMessage("TakeWeapon", weaponOnGround);
        }
    }
}
