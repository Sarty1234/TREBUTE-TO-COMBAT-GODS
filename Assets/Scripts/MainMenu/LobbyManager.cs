using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class LobbyManager : MonoBehaviour
{
    public class LobbyToShowElement
    {
        public string name;
        public string id;
    }



    public static LobbyManager Instance;
    public TMP_InputField LobbyNameInputField;
    public TMP_Dropdown GameModeDropDown;
    public TMP_Dropdown MaxPlayersDropDown;


    public LobbyManager()
    {
        Instance = this;
    }


    public string lobbyId;
    public Lobby lobbie;
    public Coroutine heartbeatCoroutine;
    public bool IsHost = false;


    public string relayJoinCode;



    async void Awake()
    {
        try
        {
            //await UnityServices.InitializeAsync();


            //await AuthenticationService.Instance.SignInAnonymouslyAsync();
            //Debug.Log("Signed in");
            await Authenticate();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    public async Task Authenticate()
    {
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile($"{UnityEngine.Random.Range(0, 100000)}");

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            //_playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync(new SignInOptions { CreateAccount = true });
    }


    private void OnEnable()
    {
        IsHost = false;
        lobbyId = string.Empty;
        lobbie = null;
        relayJoinCode = "";
    }


    public async void createLobby()
    {
        try
        {
            Debug.Log("Start creating lobby");


            //отримую данні для лобі
            string lobbyName = LobbyNameInputField.text;
            int maxPlayers = Int32.Parse(MaxPlayersDropDown.options[MaxPlayersDropDown.value].text);
            bool isPrivate = false;
            string gameMode = GameModeDropDown.options[GameModeDropDown.value].text;
            
            Dictionary<string, DataObject> lobbieData = new Dictionary<string, DataObject>()
            {
                {
                    "GameMode", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: gameMode, index: DataObject.IndexOptions.S1)
                },
                {
                    "RelayJoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Member, value: "", index: DataObject.IndexOptions.S2)
                }
            };


            //перевірямо данні
            if (lobbyName.Length == 0) return;



            //присвоюю данні до лобі
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.IsPrivate = isPrivate;
            createLobbyOptions.Data = lobbieData;


            //створюю лобі
            lobbie = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            if (lobbie == null) return;
            Debug.Log($"Lobby {lobbie.Name} created, mode - {lobbie.Data["GameMode"].Value}, max players - {lobbie.MaxPlayers}. It's id - {lobbie.Id}");


            if (heartbeatCoroutine != null)
            {
                StopAllCoroutines();
            }


            lobbyId = lobbie.Id;
            IsHost = true;
            heartbeatCoroutine = StartCoroutine(lobbyHearbeatCoroutine(lobbyId, 14f));
            
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    public async void UpdateLobby(bool returnToMenuOnError)
    {
        try
        {
            if (lobbyId == "") return;
            lobbie = await LobbyService.Instance.GetLobbyAsync(lobbyId);


            relayJoinCode = lobbie.Data["RelayJoinCode"].Value;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);


            if (returnToMenuOnError)
            {
                Debug.LogException(new Exception("Returning to lobby on error during UpdateLobby"));
                ReturnToMenuOnError();
            }
        }
    }


    public async void UpdateLobbyJoinCode(string joinCode)
    {
        try
        {
            string lobbyName = lobbie.Name;
            int maxPlayers = lobbie.MaxPlayers;
            bool isPrivate = lobbie.IsPrivate;
            string gameMode = lobbie.Data["GameMode"].Value;

            Dictionary<string, DataObject> newLobbieData = new Dictionary<string, DataObject>()
            {
                {
                    "GameMode", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: gameMode, index: DataObject.IndexOptions.S1)
                },
                {
                    "RelayJoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Member, value: joinCode, index: DataObject.IndexOptions.S2)
                }
            };



            //присвоюю данні до лобі
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions();
            updateLobbyOptions.IsPrivate = isPrivate;
            updateLobbyOptions.Data = newLobbieData;

            
            lobbie = await LobbyService.Instance.UpdateLobbyAsync(lobbie.Id, updateLobbyOptions);
            relayJoinCode = joinCode;
        }
        catch (Exception e)
        {
            Debug.LogException(new Exception("Returning to lobby on error during UpdateLobbyJoinCode"));
            //ReturnToMenuOnError();
            Debug.LogException(e);
        }
    }


    public void ReturnToMenuOnError()
    {
        StopAllCoroutines();
        lobbie = null;
        lobbyId = string.Empty;
        ChangeCanvasScreen.Instance.LoadObject("MainMenu");
    }


    public async Task<LobbyToShowElement[]> getLobbies(int maxElementsCount = 10, string gameMode = "", string roomName = "")
    {
        LobbyToShowElement[] responce = new LobbyToShowElement[0];
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = maxElementsCount;

            
            //записуємо фільтри пошуку лобі
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(field: QueryFilter.FieldOptions.AvailableSlots, op: QueryFilter.OpOptions.GT, value: "0"),
                new QueryFilter(field: QueryFilter.FieldOptions.S1, op: QueryFilter.OpOptions.EQ, value: gameMode),
                new QueryFilter(field: QueryFilter.FieldOptions.Name, op: QueryFilter.OpOptions.CONTAINS, value: roomName)
            };

            
            //записуємо порядок пошуку лобі
            options.Order = new List<QueryOrder>()
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };
            
            
            //робимо запит на данні
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);



            //перетворюємо отриманні данні у власний клас
            foreach (var item in queryResponse.Results)
            {
                LobbyToShowElement element = new LobbyToShowElement();
                element.name = item.Name;
                element.id = item.Id;
                responce = responce.Append(element).ToArray();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        


        return responce;
    }


    public void deleteLobby()
    {
        if (lobbie.Id == "") return;
        try
        {
            StopCoroutine(heartbeatCoroutine);
            LobbyService.Instance.DeleteLobbyAsync(lobbie.Id);


            Debug.Log($"Lobby deleted with id {lobbie.Id}");


            lobbyId = string.Empty;
            lobbie = null;
        } catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    public async void JoinLobby(string id)
    {
        try
        {
            lobbie = await LobbyService.Instance.JoinLobbyByIdAsync(id);
            lobbyId = lobbie.Id;
        } catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    public async void LeaveLobby()
    {
        StopAllCoroutines();
        try
        {
            if (IsHost)
            {
                deleteLobby();
            } else
            {
                string playerId = AuthenticationService.Instance.PlayerId;
                await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
            }



            lobbyId = string.Empty;
            lobbie = null;


            Debug.Log("Left lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    IEnumerator<WaitForSecondsRealtime> lobbyHearbeatCoroutine(string lobbyId, float heartbeatCycleTime)
    {
        var delay = new WaitForSecondsRealtime(heartbeatCycleTime);


        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            Debug.Log($"Sended heartbeat to lobbie with id {lobbyId}");
            yield return delay;
        }
    }
}
