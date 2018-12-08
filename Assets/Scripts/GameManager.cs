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

    //event wywoływany w przypadku śmierci jednego z avatarów, 
    public static IntEvent deadEvent;
    public static bool paused = false;
    public SceneSetup arenaData;
    //obiekty na arenie których aktywnością skrypt zarządza
    public GameObject pauseMenu;
    public GameObject connectionWaitMenu;
    public GameObject secondAvatarPrefab;
    private GameObject secondAvatar;
    public GameObject endGameMenu;
    public GameObject winnerTextSpot;
    
    //zapisywana referencja do tego skryptu aby prawa do niego mogły zostać przekazane
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

        if (GameData.ai == null)//gra przez sieć
        {
            return;
        }
        else//gra jest lokalna
        {
            //SpawnSecondLocalAvatar();
            StartCoroutine(WaitForArena());
        }
    }

    //oczekiwanie aż siatka pokoo zostanie stworzona
    IEnumerator WaitForArena()
    {
        yield return new WaitWhile(() => !arenaData.gridDone);
        SpawnSecondLocalAvatar();
    }

    private void Update()
    {
        //kontrola oczekiwania na drugiego gracza
        if (isServer && GameData.ai == null)
        {
            if (!GameData.secondClientConnected)
            {
                if (!connectionWaitMenu.activeSelf)
                {
                    if(paused)CmdResumeGame();
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
        
        if (Input.GetKeyDown(KeyCode.Escape) && !connectionWaitMenu.activeSelf)
        {
            if (!paused)
            {
                CmdPauseGame();
            }
            else
            {
                CmdResumeGame();
            }            
        }
    }

    [Command]
    void CmdPauseGame()
    {
        RpcPauseGame();
    }

    //pauzowanie gry
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

    //wznowienie gry po pauzie
    [ClientRpc]
    void RpcResumeGame()
    {
        paused = !paused;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    //wyjście z gry, przypisane do klawisza "menu" w menu pauzy
    public void LoadMenu()
    {
        //resetowanie ustawień
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
            NetworkManager.singleton.StopClient();
        }
    }

    //ustawienie aktywności oczekiwania na drugiego gracza
    void WaitingMenuActive(bool set)
    {
        if(connectionWaitMenu.activeSelf != set) connectionWaitMenu.SetActive(set);
    }

    //wywoływane w przypadku wystąpienia zdarzenia deadEvent
    void GameOver(int i)
    {
        endGameMenu.SetActive(true);
        winnerTextSpot.GetComponent<TextMeshProUGUI>().SetText("Game Over \nPlayer" + i + " won");
    }

    public void RestartGame()
    {
        CmdRestartGame();       
    }

    [Command]
    public void CmdRestartGame()
    {
        RpcRespawnPlayer();
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
        //gra jest lokalna więc reset drugiego avatara
        if (GameData.ai != null)
        {
            NetworkServer.Destroy(secondAvatar);
            SpawnSecondLocalAvatar();
        }
    }

    //tworzenie drugiego avatara w grze lokalnej
    public void SpawnSecondLocalAvatar()
    {
        secondAvatar = Instantiate(secondAvatarPrefab);
        secondAvatar.transform.position = NetworkManager.singleton.startPositions[1].position;
        if ((bool)GameData.ai)//włączenie bota
        {
            GameObject AI = secondAvatar.transform.Find("AI").gameObject;
            AI.GetComponent<AIControler>().arenaData = arenaData;
            AI.GetComponent<AIControler>().enemy = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
            AI.SetActive(true);
        }
        NetworkServer.Spawn(secondAvatar);
    }
}
