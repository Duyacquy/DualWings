using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class CreateRoomUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI ipText;

    [Header("Lobby Interaction")]
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI startButtonText;
    [SerializeField] private GameObject fullMoon;
    [SerializeField] private GameObject CrescentMoon;

    private NetworkVariable<bool> isRoomFull = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        ipText.text = "IP: " + GetLocalIPAddress();

        isRoomFull.OnValueChanged += OnRoomFullStateChanged;

        SetVisualState(isRoomFull.Value);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        isRoomFull.OnValueChanged -= OnRoomFullStateChanged;

        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public void Back()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenuScene");
    }

    private void OnClientConnected(ulong clientId)
    {
        isRoomFull.Value = NetworkManager.Singleton.ConnectedClients.Count >= 2;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        isRoomFull.Value = NetworkManager.Singleton.ConnectedClients.Count < 2;
    }

    private void OnRoomFullStateChanged(bool previousValue, bool newValue)
    {
        SetVisualState(newValue);
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    private void SetVisualState(bool isReady)
    {
        fullMoon.SetActive(isReady);
        CrescentMoon.SetActive(!isReady);

        if (IsServer)
        {
            startButton.interactable = isReady;
            startButtonText.text = isReady ? "Start" : "Waiting...";
        }
        else
        {
            startButton.interactable = false;
            startButtonText.text = "Waiting...";
        }
    }

    public void StartGame()
    {
        if (IsServer && isRoomFull.Value)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameplayScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}