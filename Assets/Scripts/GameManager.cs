using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Events;
using TMPro;


[System.Serializable]
public class IntEvent : UnityEvent<int>
{
}

    public class GameManager : NetworkBehaviour {

    public static IntEvent deadEvent;
    public static bool paused = false;
    public GameObject pauseMenu;
    public List<GameObject> aiGameObjects;
    public GameObject connectionWaitMenu;
    public GameObject secondAvatarPrefab;
    GameObject secondAvatar;
    public GameObject endGameMenu;
    public GameObject winnerTextSpot;

    public override void OnStartServer()
    {
        NetworkManager.singleton.GetComponent<CustomNetworkManager>().gameManager = this;
    }

    public override void OnStartClient()
    {
        if (isClient) WaitingMenuActive(false);

        if (deadEvent == null)
            deadEvent = new IntEvent();

        deadEvent.AddListener(GameOver);

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
        //Debug.Log(NetworkServer.dontListen);
        if (isServer && GameData.ai == null)
        {//zeby ciagle nie zmienialo
            if (!GameData.secondClientConnected)
            {
                if (!connectionWaitMenu.activeSelf)
                {
                    Time.timeScale = 0f;
                    WaitingMenuActive(true);
                }            
            }
            else if(connectionWaitMenu.activeSelf)
            {                              
                Time.timeScale = 1f;
                WaitingMenuActive(false);              
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
            {
                //PauseGame();
                CmdPauseGame();
            }
            else
            {
                // ResumeGame();
                CmdResumeGame();
            }
            
        }
    }

    [Command]
    void CmdPauseGame()
    {
        RpcPauseGame();
    }
    [ClientRpc]
    void RpcPauseGame()
    {
        paused = !paused;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
    }

    [Command]
    void CmdResumeGame()
    {
        RpcResumeGame();
    }
    [ClientRpc]
    void RpcResumeGame()
    {
        paused = !paused;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
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
            //NetworkServer.Reset();
            NetworkManager.singleton.StopClient();
        }
    }

    void WaitingMenuActive(bool set)
    {
        if(connectionWaitMenu.activeSelf != set) connectionWaitMenu.SetActive(set);
    }

    void GameOver(int i)
    {
        Debug.Log("GameOver" + i);
        endGameMenu.SetActive(true);
        //Time.timeScale = 0f;
        winnerTextSpot.GetComponent<TextMeshProUGUI>().SetText("Game Over \nPlayer" + i + " won");
    }

    public void RestartGame()
    {
        /* NetworkServer.dontListen = true;
         GameData.secondClientConnected = false;*/

        //NetworkManager.singleton.ServerChangeScene(NetworkManager.singleton.onlineScene);
        CmdRestartGame();
        
    }

    [Command]
    public void CmdRestartGame()
    {
        RpcRespawnPlayer();
       // NetworkManager.singleton.ServerChangeScene(NetworkManager.singleton.onlineScene);
    }

    //usunięcie avatara i stworzenie nowego
    [ClientRpc]
    public void RpcRespawnPlayer()
    {
        //przypisanie części avatara która była "odczepiona" w chwili śmierci, tak aby ona również była usunięta
        NetworkManager.singleton.client.connection.playerControllers[0].gameObject.GetComponent<PlayerControler>().AssingSkeleton();
        short playerId = NetworkManager.singleton.client.connection.playerControllers[0].playerControllerId;
        ClientScene.RemovePlayer(playerId);
        ClientScene.AddPlayer(NetworkManager.singleton.client.connection, playerId);

        endGameMenu.SetActive(false);
    }
}
