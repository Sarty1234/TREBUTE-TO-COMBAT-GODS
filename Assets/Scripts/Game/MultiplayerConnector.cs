using System;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;

namespace Assets.Scripts
{
    public class MultiplayerConnector : MonoBehaviour
    {
        public void Awake()
        {
            if (LobbyManager.Instance.IsHost)
            {
                NetworkManager.Singleton.StartHost();
            } else
            {
                NetworkManager.Singleton.StartClient();
            }
        }


        
    }
}
