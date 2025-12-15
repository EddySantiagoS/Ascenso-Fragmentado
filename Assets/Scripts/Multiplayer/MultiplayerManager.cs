using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    async void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async void InitializeMultiplayer()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
            return;

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("Servicios Multiplayer listos");
    }

    public async void CreateRoom()
    {
        Debug.Log("Creando sala...");

        // 1. Crear Relay
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(8);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // 2. Crear Lobby
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = false,
            Data = new System.Collections.Generic.Dictionary<string, DataObject>
        {
            {
                "joinCode",
                new DataObject(DataObject.VisibilityOptions.Public, joinCode)
            }
        }
        };

        await LobbyService.Instance.CreateLobbyAsync("Sala Parkour", 8, options);

        // 3. Configurar transporte
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,   // clientConnectionData
            allocation.ConnectionData,   // hostConnectionData (HOST usa la misma)
            true
        );

        // 4. Iniciar Host
        NetworkManager.Singleton.StartHost();

        Debug.Log("Host iniciado");
    }

    public async void JoinRoom()
    {
        Debug.Log("Buscando sala...");

        var lobbies = await LobbyService.Instance.QueryLobbiesAsync();

        if (lobbies.Results.Count == 0)
        {
            Debug.Log("No hay salas disponibles");
            return;
        }

        Lobby lobby = lobbies.Results[0];
        string joinCode = lobby.Data["joinCode"].Value;

        // 1. Unirse al Relay
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        // 2. Configurar transporte
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,
            allocation.HostConnectionData,
            true
        );

        // 3. Iniciar Cliente
        NetworkManager.Singleton.StartClient();

        Debug.Log("Cliente conectado");
    }
}