using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

namespace Assets.Scripts
{
    public class InLobbyScript : MonoBehaviour
    {
        public TMP_Text OutputText;
        public TMP_Text PlayersCountText;
        public float UpdateCoroutineTime = 2f;


        public int numberOfTryesForLobbyRefresh;




        private void OnEnable()
        {
            OutputText.text = "Connecting";
            PlayersCountText.text = "";
            StopAllCoroutines();
            Invoke("StartCoroutines", 0.3f);
        }


        private void OnDisable()
        {
            //StopCoroutine(lobbyUpdateCoroutine());
            StopAllCoroutines();
        }


        public void StartCoroutines()
        {
            OutputText.text = "Connected";
            //StopCoroutine(lobbyUpdateCoroutine());
            


            StartCoroutine(lobbyUpdateCoroutine());
            numberOfTryesForLobbyRefresh = 1;
        }



        IEnumerator<WaitForSecondsRealtime> lobbyUpdateCoroutine()
        {
            var delay = new WaitForSecondsRealtime(UpdateCoroutineTime);

            while (true)
            {
                refreshPlayersCount();
                numberOfTryesForLobbyRefresh++;
                yield return delay;
            }
        }


        public async void refreshPlayersCount()
        {
            OutputText.text = "Trying to update lobby";
            LobbyManager.Instance.UpdateLobby(false);


            OutputText.text = $"Checking lobby if it is valid {numberOfTryesForLobbyRefresh}";
            if (LobbyManager.Instance.lobbie == null) {
                OutputText.text = $"Lobby isn't valid {numberOfTryesForLobbyRefresh}";
                return;
            }


            Debug.Log($"Lobby data updated");
            OutputText.text = "Lobby data updated";
            PlayersCountText.text = $"{LobbyManager.Instance.lobbie.MaxPlayers - LobbyManager.Instance.lobbie.AvailableSlots}/{LobbyManager.Instance.lobbie.MaxPlayers}";


            if (LobbyManager.Instance.lobbie.Players.Count != LobbyManager.Instance.lobbie.MaxPlayers)
            {
                OutputText.text = "Waiting for players...";
            } else
            {
                OutputText.text = "Prepare to fight";
                //Invoke("StartGame", 2f);
                await StartGame();
            }
        }


        public async Task<bool> StartGame()
        {
            if (LobbyManager.Instance.IsHost)
            {
                LobbyManager.Instance.relayJoinCode = await StartHostWithRelay(LobbyManager.Instance.lobbie.MaxPlayers);
                LobbyManager.Instance.UpdateLobbyJoinCode(LobbyManager.Instance.relayJoinCode);
                SceneManager.LoadScene("GameScene");


                Debug.Log(LobbyManager.Instance.relayJoinCode);
            }
            else
            {
                Debug.Log("Waiting for join code...");
                float lastUpdateTime = Time.time;


                StopAllCoroutines();
                StartCoroutine(refreshLobbyListCoroutineUntillCode());
            }
            return true;
        }


        IEnumerator<WaitForSecondsRealtime> refreshLobbyListCoroutineUntillCode()
        {
            var delay = new WaitForSecondsRealtime(2f);
            int maxRepeats = 10;


            while (true)
            {
                try
                {
                    if (maxRepeats <= 0)
                    {
                        Debug.Log("Returning to lobby");
                        StopAllCoroutines();
                        ChangeCanvasScreen.Instance.LoadObject("MainMenu");
                        break;
                    }


                    maxRepeats--;
                    LobbyManager.Instance.UpdateLobby(false);
                    Debug.Log(LobbyManager.Instance.relayJoinCode == string.Empty);
                    Debug.Log(LobbyManager.Instance.relayJoinCode == "");



                    if (LobbyManager.Instance.relayJoinCode != string.Empty)
                    {
                        Debug.Log($"Join code - {LobbyManager.Instance.relayJoinCode}");
                        StartClientWithRelay(LobbyManager.Instance.relayJoinCode);
                        break;
                    }
                    else
                    {
                        Debug.Log("Waiting for code");
                    }
                } 
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                yield return delay;
            }
        }



        public async Task<string> StartHostWithRelay(int maxConnections)
        {
            await UnityServices.InitializeAsync();


            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }


            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return joinCode;
            //return NetworkManager.Singleton.StartHost() ? joinCode : null;
        }


        public async Task<bool> StartClientWithRelay(string joinCode)
        {
            /*await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }*/


            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));



            SceneManager.LoadScene("GameScene");
            return true;
            //return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
        }
    }
}
