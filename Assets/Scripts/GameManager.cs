using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

    public static bool paused = false;
    public GameObject pauseMenu;
    public List<GameObject> aiGameObjects;
    public GameObject connectionWaitMenu;
    public GameObject secondAvatarPrefab;
    GameObject secondAvatar;


	void Start () {
        if (isClient) WaitingMenuActive(false);

        if (GameData.ai == null)
        {
            return;
        }
        else//gra jest lokalna
        {
            secondAvatar = Instantiate(secondAvatarPrefab);
            secondAvatar.transform.position = new Vector2(1f,1f);           
        }
        if((bool)GameData.ai)
        {
            secondAvatar.transform.Find("AI").gameObject.SetActive(true);
        }
        WaitingMenuActive(false);
        NetworkServer.Spawn(secondAvatar);
    }
    private void Update()
    {
        Debug.Log(NetworkServer.dontListen);
        if (isServer && GameData.ai == null)
        {//zeby ciagle nie zmienialo
            if (!GameData.secondClientConnected)
            {
                Time.timeScale = 0f;
                WaitingMenuActive(true);
            }
            else
            {
                Time.timeScale = 1f;
                WaitingMenuActive(false);
            }
        }
        
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

        if (isServer)
        {
            NetworkManager.singleton.StopHost();
            NetworkServer.Reset();
        }
        else
        {
            NetworkServer.Reset();
            NetworkManager.singleton.StopClient();
        }
    }

    void WaitingMenuActive(bool set)
    {
        if(connectionWaitMenu.activeSelf != set) connectionWaitMenu.SetActive(set);
    }




  
    
    
   [Command]
    public void CmdStartGame()
    {      
        Debug.Log(NetworkServer.connections.Count);
        if (isServer && NetworkServer.connections.Count == 2)
        {
            NetworkServer.dontListen = true;
            Time.timeScale = 1f;
            connectionWaitMenu.SetActive(false);
        }
        else if(isServer)
        {
            NetworkServer.dontListen = false;
            Time.timeScale = 0f;
            connectionWaitMenu.SetActive(true);
        }
    }
}
