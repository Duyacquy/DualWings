using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void CreateRoom()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", 7777);

        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene("CreateRoomScene", LoadSceneMode.Single);
        }
    }

    public void GoToFindRoomScene()
    {
        SceneManager.LoadScene("FindRoomScene");
    }

    public void QuitGame() => Application.Quit();
}
