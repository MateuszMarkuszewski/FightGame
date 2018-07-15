using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickUp : MonoBehaviour {
    //skrypt dla triggera odpowiedzialnego za podnoszenie broni
    //gdy gracz sie zbliży to hitboxy zostają uaktywnione a trigger podnoszenia dezaktywowany
    //podniesiona broń staje sie dzieckiem gameobjectu gracza
    //uruchamiana jest również funkcja w PlayerControler przypisująca zmiennej weapon podniesioną broń
    public GameObject weaponOnGround;
    public Vector3 PickUpPos;
    public Vector3 PickUpRot;

    private void OnTriggerEnter2D(Collider2D col)
    {
        
        //Debug.Log("pickup collision name = " + col.gameObject.tag);
       // Debug.Log("----------");
        if (col.gameObject.tag == "Player")
        {      
           // col.gameObject.SendMessage("TakeWeapon", weaponOnGround);  
        }
    }
}
