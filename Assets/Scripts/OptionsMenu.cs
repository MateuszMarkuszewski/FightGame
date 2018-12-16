using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class OptionsMenu : MonoBehaviour {

    public Toggle fullscreenButton;
    GameObject namePlaceHolder;
    //czy zmieniany jest jakiś klawisz
    bool setUpKey = false;
    //zmieniany klawisz, wartości tylko ze słownika buttons
    string setKey;
    public Dictionary<string, GameData.Key> buttons = new Dictionary<string, GameData.Key>()
    { { "attackP1", GameData.p1.attack },
      { "pickUpP1", GameData.p1.pickUp },
      { "upP1", GameData.p1.up },
      { "downP1", GameData.p1.down },
      { "rightP1", GameData.p1.right },
      { "leftP1", GameData.p1.left },
      { "attackP2", GameData.p2.attack },
      { "pickUpP2", GameData.p2.pickUp },
      { "upP2", GameData.p2.up },
      { "downP2", GameData.p2.down },
      { "rightP2", GameData.p2.right },
      { "leftP2", GameData.p2.left },
    };

    private void Awake()
    {
        fullscreenButton.isOn = Screen.fullScreen;
    }

    public void FullScreen()
    {
        Screen.fullScreen = fullscreenButton.isOn;
    }

    private void OnGUI()
    {
        //gdy został naciśnięty któryś z przycisków zmiany klawiszy
        //oczekuje na wciśnięcie klawisza który zostanie przypisany do akcji odpowiadającej wcześniej wciśniętemu przuciskowi w menu
        if (setUpKey)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                buttons[setKey].key = e.keyCode;
                setUpKey = false;
                namePlaceHolder.GetComponent<TMP_InputField>().text = buttons[setKey].key.ToString();
            }
        }
    }

    //przypisywane do przycisków zmiany klawisza w menu
    public void SetUpKey(string key)
    {
        setKey = key;
        setUpKey = true;
    }

    //przypisaywe zostaje miejsce na text z menu opcji gdy odpowiadający jej przycisk zostanie wciśnięty
    public void ButtonNamePlaceHolder(GameObject placeHolder)
    {
        namePlaceHolder = placeHolder;
    }
}
