using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour {

    private bool clickedOnce = false;
    private float startTime;
    private float minComboTime = 0.1f;
    private float maxComboTime = 0.5f;

    private bool startCombo = false;
    private float startComboTime;
    private KeyCode checkedKey;

    public bool DoubleClick(KeyCode key)
    {

        if (clickedOnce && Input.GetKeyDown(key) && (key == checkedKey))
        {
            if ((Time.time - startTime) < maxComboTime)
            {
                clickedOnce = false;
                Debug.Log("doubleclick");
                return true;
            }
            else if ((Time.time - startTime) > maxComboTime)
            {
                clickedOnce = false;
            }
        }
        if (!clickedOnce && Input.GetKeyDown(key))
        {
            
            checkedKey = key;
            startTime = Time.time;
            clickedOnce = true;
        }
            
        return false;
    }

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
