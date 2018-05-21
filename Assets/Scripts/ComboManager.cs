using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour {

    private bool ClickedOnce = false;
    private float startTime;
    private float stopTime;
    private float minComboTime = 0.05f;
    private float maxComboTime = 0.2f;

    public bool DoubleClick(KeyCode key)
    {
        if (!ClickedOnce && Input.GetKeyDown(key))
        {
            startTime = Time.time;
            ClickedOnce = true;
        }
        if (ClickedOnce && Input.GetKeyDown(key))
        {
            if ((Time.time - startTime) > minComboTime && (Time.time-startTime) < maxComboTime)
            {
                ClickedOnce = false;
                Debug.Log("doubleclick");
                return true;
            }
            else if ((Time.time - startTime) > maxComboTime)
            {
                ClickedOnce = false;
            }
        }
        
        return false;
    }

}
