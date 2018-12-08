using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour {

    private bool clickedOnce = false;
    private float startTime;
    private float maxComboTime = 0.5f;
    private bool startCombo = false;
    private float startComboTime;
    private KeyCode checkKey;

    //sprawdza czy klawisz został wciśnięty 2 razy w małym odstępie czasu
    public bool DoubleClick(KeyCode key)
    {
        if (clickedOnce && (key == checkKey))
        {
            float passedTime = Time.time - startTime;
            if ((passedTime) < maxComboTime)
            {
                clickedOnce = false;
                return true;
            }
            else if ((passedTime) > maxComboTime)
            {
                clickedOnce = false;
            }
        }
        if (!clickedOnce || key != checkKey)
        {
            checkKey = key;
            startTime = Time.time;
            clickedOnce = true;
        }          
        return false;
    }

    //zwraca numer ataku w sekwencji który ma być teraz wykonany
    public int Step(int currentStep, int maxStep)
    {
        if (((Time.time - startComboTime) < maxComboTime) && currentStep + 1 <= maxStep)
        {
            startComboTime = Time.time;
            return currentStep + 1;
        }
        startComboTime = Time.time;
        return 1;
    }
}
