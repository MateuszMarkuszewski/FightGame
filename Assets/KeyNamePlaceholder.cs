using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//klasa odpowiedzialna za wyświetlanie odpowiednich nazw klawiszy w menu opcji
public class KeyNamePlaceholder : MonoBehaviour {

    OptionsMenu optionsMenu;
    public string keyName; 

    private void Start()
    {
        optionsMenu = transform.GetComponentInParent<OptionsMenu>();
        KeyCode? key = optionsMenu.buttons[keyName].key;
        if(key != null)
            GetComponent<TMP_InputField>().text = key.ToString();
    }
}
