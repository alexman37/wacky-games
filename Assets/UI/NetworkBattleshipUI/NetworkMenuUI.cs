using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Games.Battleship
{
    public class NetworkMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        public Button hostButton;
        public Button clientButton;
        public TMP_InputField ipAddressInput;
        public TMP_InputField portInput;
        public GameObject connectionPanel;
        public TMP_Text statusText;

        private UnityTransport transport;

        void Start()
        {
            transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            // Setup button listeners
            hostButton.onClick.AddListener(StartHost);
            clientButton.onClick.AddListener(StartClient);

            // Set default values
            ipAddressInput.text = "127.0.0.1";
            portInput.text = "7777";

            // Setup network callbacks
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public void StartHost()
        {
            UpdateTransportSettings();
            UpdateStatus("Starting Host...");

            bool success = NetworkManager.Singleton.StartHost();
            if (!success)
            {
                UpdateStatus("Failed to start Host");
            }
        }

        public void StartClient()
        {
            UpdateTransportSettings();
            UpdateStatus("Connecting to Host...");

            bool success = NetworkManager.Singleton.StartClient();
            if (!success)
            {
                UpdateStatus("Failed to connect to Host");
            }
        }

        void UpdateTransportSettings()
        {
            if (transport != null)
            {
                transport.ConnectionData.Address = ipAddressInput.text;
                if (ushort.TryParse(portInput.text, out ushort port))
                {
                    transport.ConnectionData.Port = port;
                }
            }
        }

        void OnClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                UpdateStatus($"Client {clientId} connected");
            }
            else
            {
                UpdateStatus("Connected to Host!");
                HideConnectionUI();
            }
        }

        void OnClientDisconnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                UpdateStatus($"Client {clientId} disconnected");
            }
            else
            {
                UpdateStatus("Disconnected from Host");
                ShowConnectionUI();
            }
        }

        void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
                Debug.Log($"Network Status: {message}");
            }
        }

        public void HideConnectionUI()
        {
            connectionPanel.SetActive(false);
        }

        void ShowConnectionUI()
        {
            connectionPanel.SetActive(true);
        }

        void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    }
}