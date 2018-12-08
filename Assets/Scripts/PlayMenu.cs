using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayMenu : MonoBehaviour {

    public Button playbutton;


    private void Update()
    {
        //kontrola dostępności przycisku rozpoczynającego rozgrywkę
        if (GameData.sizeMap != null && GameData.ai != null)
        {
            playbutton.interactable = true;
        }
        else
        {
            playbutton.interactable = false;
        }
    }

    //przypisane do checkboxów ustawiających rozmiar areny
    //bool active - zaznaczenie checkboxa
    //obiekty te są w grupach gdzie tylko jeden checkbox może być aktywny
    //jeśli podczas zaznaczania jakiś jest już atywny to najpierw jest dezaktywowany
    public void SetSmallArena(bool active)
    {
        if(active)
            GameData.sizeMap = 6;
        else GameData.sizeMap = null;

    }
    public void SetMediumArena(bool active)
    {
        if (active)
            GameData.sizeMap = 8;
        else GameData.sizeMap = null;

    }
    public void SetLargeArena(bool active)
    {
        if (active)
            GameData.sizeMap = 10;
        else GameData.sizeMap = null;

    }

    //funkcje przypisane do checkboxów pvp i pvai
    //tylko jeden może być zaznaczony, działa jak wyżej
    public void EnableAI(bool active)
    {
        if (active)
            GameData.ai = true;
        else GameData.ai = null;
    }
    public void DisableAI(bool active)
    {
        if (active)
            GameData.ai = false;
        else GameData.ai = null;
    }
    

    public void ResetSetup()
    {
        GameData.ai = null;
        GameData.sizeMap = null;
    }

}
