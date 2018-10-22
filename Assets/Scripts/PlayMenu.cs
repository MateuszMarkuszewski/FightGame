using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayMenu : MonoBehaviour {

    public Button playbutton;

    private void Update()
    {
        if (GameData.sizeMap != null && GameData.ai != null && !playbutton.interactable)
        {
            playbutton.interactable = true;
        }
    }

    public void Play()
    {
        SceneManager.LoadScene(1);
    }

    public void SetSmallArena()
    {
        GameData.sizeMap = 6;
    }
    public void SetMediumArena()
    {
        GameData.sizeMap = 8;
    }
    public void SetLargeArena()
    {
        GameData.sizeMap = 10;
    }
    public void EnableAI()
    {
        GameData.ai = true;
    }
    public void DisableAI()
    {
        GameData.ai = false;
    }
}
