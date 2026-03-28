using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.UI;

public class ButtonHandler : NetworkBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button homeButton;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            if (playAgainButton != null) playAgainButton.interactable = false;
            if (homeButton != null) homeButton.interactable = false;
        }
    }

    public void PlayAgain()
    {
        if (!IsServer) return;

        Time.timeScale = 1f;

        GameManager.Instance.CleanupGameBeforeRestart();

        NetworkManager.Singleton.SceneManager.LoadScene("GameplayScene", LoadSceneMode.Single);
    }

    public void GoHome()
    {
        if (!IsServer) return;
        
        Time.timeScale = 1f; 

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene("MainMenuScene");
    }
}
