using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static bool paused = false;
    public GameObject pauseMenu;
    public List<GameObject> aiGameObjects;

	void Start () {
      /* if ((bool)GameData.ai)
       {
            foreach(GameObject go in aiGameObjects)
            {
                go.SetActive(true);
            }
       }*/

	}
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
            
        }
    }

    void PauseGame()
    {
        paused = !paused;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
    }
    public void ResumeGame()
    {
        paused = !paused;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    public void LoadMenu()
    {
        GameData.ai = null;
        GameData.sizeMap = null;
        Time.timeScale = 1f;
        GameData.NM.StopHost();
        //SceneManager.LoadScene(0);
    }
}
