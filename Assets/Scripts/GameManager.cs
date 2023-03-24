using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public bool gameHasStarted = false;

    public int playerID = -1;

    // Singleton
    public static GameManager _instance;
    public static GameManager instance => _instance;

    private string _lobbyId;

    private RelayHostData _hostData;
    private RelayJoinData _joinData;

    // Notify state Update
    public UnityAction<string, ulong> UpdateState;
    // Notify match found
    public UnityAction MatchFound;

    private bool HeartBeatActive = false;

    [SerializeField] private Menu menuContoller;

    private void Awake()
    {
        // Just a basic Singleton
        if (_instance is null)
        {
            Debug.Log("Is Null");
            _instance = this;
            return;
        }
        Debug.Log("Destroying");

        Destroy(this);
    }

    // Start is called before the first frame update
    async void Start()
    {
        // Initialize unity services
        await UnityServices.InitializeAsync();

        SetupEvents();

        // Unity Login
        await SignInAnonymousAsync();

        // Subscribe to NetworkManager events
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
    }

    private void ClientConnected(ulong id)
    {
        Debug.Log("Connect player with ID " + id);
        playerID = (int)id;
        MatchFound?.Invoke();
        if (id == 1)
            UpdateState?.Invoke("Player found!", id);
    }

    private async void ClientDisconnected(ulong id)
    {
        Debug.Log("Player diconnected: " + id);
        NetworkManager.Singleton.Shutdown();
        HeartBeatActive = false;
        Cursor.lockState = CursorLockMode.None;

        UpdateState?.Invoke("", 0);
        menuContoller.Disconnect();
        GetComponent<Rounds>().Disconnect();

        await SignOut();
        NetworkManager networkManager = GameObject.FindObjectOfType<NetworkManager>();
        Destroy(networkManager.gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    #region Login


    async Task SignInAnonymousAsync()
    {
        try {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }

        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion

    #region Lobby

    public async void FindMatch()
    {
        Debug.Log("Looking for a lobby...");

        UpdateState?.Invoke("Looking for a match... ", 0);


        try
        {
            // Looking for a lobby

            // Add options (mode, rank, etc..)
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            // Quick join a random lobby
            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync(options);

            Debug.Log("Joined lobby: " + lobby.Id);
            Debug.Log("Joined Players: " + lobby.Players.Count);

            // Retrieve the Relay code previousely set in the create match
            string joinCode = lobby.Data["joinCode"].Value;
            _lobbyId = lobby.Id;

            Debug.Log("Received code: " + joinCode);

            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

            // Create object
            _joinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            // Set transport data
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _joinData.IPv4Address,
                _joinData.Port,
                _joinData.AllocationIDBytes,
                _joinData.Key,
                _joinData.ConnectionData,
                _joinData.HostConnectionData);

            NetworkManager.Singleton.StartClient();
            GameObject menu = GameObject.Find("Canvas");
            StartCoroutine(menu.GetComponent<Menu>().StartCooldown());

            // Trigger events
            UpdateState?.Invoke("Match found!", 0);
            MatchFound?.Invoke();
        }
        catch (LobbyServiceException ex)
        {
            // If we dont find any lobby, let's create a new one
            Debug.Log("Cannot find a lobby: " + ex);
            CreateMatch();
        }
    }

    private async void CreateMatch()
    {
        Debug.Log("Creating a new lobby...");

        UpdateState?.Invoke("Creating a new match...", 0);

        // External connections
        int maxConnections = 1;

        try
        {
            // Create RELAY object
            Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxConnections);
            _hostData = new RelayHostData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            // Retrieve JoinCode
            _hostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

            string lobbyName = "game_lobby";
            int maxPlayers = 2;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;

            options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "joinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: _hostData.JoinCode)
                },
            };

            var lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            _lobbyId = lobby.Id;

            Debug.Log("Created lobby: " + lobby.Id);

            // Heartbeat hte lobby every 15 sec
            HeartBeatActive = true;
            StartCoroutine(HeartBeatLobbyCoroutine(lobby.Id, 15));

            // Now that RELAY and LOBBY are set..
            // Set transport data
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _hostData.IPv4Address,
                _hostData.Port,
                _hostData.AllocationIDBytes,
                _hostData.Key,
                _hostData.ConnectionData);
            // Start Host
            try
            {
                NetworkManager.Singleton.StartHost();
            }
            catch
            {
                Debug.Log("Failed to StartHost()");
            }
            GameObject menu = GameObject.Find("Canvas");
            StartCoroutine(menu.GetComponent<Menu>().StartCooldown());

            // Trigger events
            UpdateState?.Invoke("Waiting for players...", 0);
        }
        catch (LobbyServiceException ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
        GetComponent<Rounds>().Disconnect();
        Cursor.lockState = CursorLockMode.None;
        HeartBeatActive = false;
        deletePlayer();
        await SignOut();
        NetworkManager networkManager = GameObject.FindObjectOfType<NetworkManager>();
        Destroy(networkManager.gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    async Task SignOut()
    {
       AuthenticationService.Instance.SignOut();
    }

    private void deletePlayer()
    {
        GameObject[] tPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        foreach (var player in tPlayer)
        {
                myPlayer = player;
        }
        Destroy(myPlayer);
    }


    IEnumerator HeartBeatLobbyCoroutine(string loobyID, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (HeartBeatActive)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(loobyID);
            Debug.Log("Lobby HeartBeat");
            yield return delay;
        }
    }

    private void OnDestroy()
    {
        // We need to delete the lobby when we're not using it
        Lobbies.Instance.DeleteLobbyAsync(_lobbyId);
    }

    #endregion

    // Setup authentication event handlers if desired
    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () => {
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };

        AuthenticationService.Instance.SignInFailed += (err) => {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () => {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
        };
    }

    // RelayHostData represents the necessary information for a Host to host a game on a Relay
    public struct RelayHostData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] Key;
    }

    public struct RelayJoinData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }

}
