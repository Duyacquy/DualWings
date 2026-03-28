using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class FindRoomUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TextMeshProUGUI statusText; 
    [SerializeField] private float connectionTimeout = 5f; 

    private Coroutine timeoutCoroutine;

    private readonly string IP_PATTERN = @"^(\d{1,3}\.){3}\d{1,3}$";

    void Start()
    {
        if (statusText != null) statusText.text = "Please enter the host's IP";
        
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnect;
        NetworkManager.Singleton.OnClientConnectedCallback += OnSuccess;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnect;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnSuccess;
        }
    }

    public void JoinRoom()
    {
        string ip = ipInputField.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            ShowError("Please enter an IP address");
            return;
        }

        if (!Regex.IsMatch(ip, IP_PATTERN))
        {
            ShowError("Invalid IP format");
            return;
        }

        Connect(ip);
    }

    public void Back()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene("MainMenuScene");
    }

    private void Connect(string ip)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, 7777);

        statusText.text = "Connecting...";
        statusText.color = Color.white;

        if (NetworkManager.Singleton.StartClient())
        {
            if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = StartCoroutine(ConnectionTimeoutRoutine());
        }
        else
        {
            ShowError("System error");
        }
    }

    private void ShowError(string message)
    {
        statusText.text = message;
        statusText.color = new Color(1f, 0.4f, 0.4f);
    }

    private IEnumerator ConnectionTimeoutRoutine()
    {
        yield return new WaitForSeconds(connectionTimeout);

        if (NetworkManager.Singleton.IsConnectedClient == false)
        {
            NetworkManager.Singleton.Shutdown();
            ShowError("Room not found");
        }
    }

    private void OnSuccess(ulong clientId)
    {
        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        statusText.text = "Connection successful";
        statusText.color = Color.green;
    }

    private void OnDisconnect(ulong clientId)
    {
        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        ShowError("Connection failed");
    }
}