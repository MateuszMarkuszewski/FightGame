using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour {

    private bool clickedOnce = false;
    private float startTime;
    private float minComboTime = 0.05f;
    private float maxComboTime = 0.2f;

    private bool startCombo = false;
    private float startComboTime;

    public bool DoubleClick(KeyCode key)
    {
        if (!clickedOnce && Input.GetKeyDown(key))
        {
            startTime = Time.time;
            clickedOnce = true;
        }
        if (clickedOnce && Input.GetKeyDown(key))
        {
            if ((Time.time - startTime) > minComboTime && (Time.time-startTime) < maxComboTime)
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
