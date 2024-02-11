﻿using System;
﻿using Assets.Scripts.Game.Other;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ServerLogic : NetworkBehaviour
{
    public ServerLogic Instance;


    public ServerLogic()
    {
        Instance = this;
    }



    public int respawnTime;
    public int nextRespawnTime;
    public bool canRespawn = true;
    public string gameMode;


    private PlayerData[] deadPlayers;


    private PlayerSpawn[] availableSpawns;


    public override void OnNetworkSpawn()
    {
        Debug.Log($"Is Server - {IsServer}");
        if (!IsServer) return;

        gameStart();
    }


    public void gameStart()
    {
        if (!IsServer) return;
        Debug.Log("Server");


        detectGameMode();
        StopCoroutine(everyoneForHimselfRespawnCoroutine());


        switch (gameMode)
        {
            case "each for himself":
                StartCoroutine(everyoneForHimselfRespawnCoroutine());
                break;
            case "team death match":
                break;
        }
    }


    public void detectGameMode()
    {
        gameMode = LobbyManager.Instance.lobbie.Data["GameMode"].Value;
    }


    
    IEnumerator<WaitForSecondsRealtime> everyoneForHimselfRespawnCoroutine()
    {
        var delay = new WaitForSecondsRealtime(1f);
        respawnTime = 0;
        nextRespawnTime = 0;
        canRespawn = true;

        while (true)
        {
            deadPlayers = FindObjectsByType<PlayerData>(FindObjectsSortMode.None).Where(obj => obj.IsDead.Value).ToArray();
            foreach (PlayerData player in deadPlayers)
            {
                if (player.deathTime.Value + respawnTime < Time.time && nextRespawnTime < Time.time && canRespawn)
                {
                    RevivePlayer(player);
                }
            }
            yield return delay;
        }
    }


    private Vector3 spawnPosition;
    private void RevivePlayer(PlayerData player)
    {
        Debug.Log($"Reviving player {player.OwnerClientId}");
        spawnPosition = findSpawnPosition();
        player.IsDead.Value = false;


        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { player.OwnerClientId }
            }
        };


        if (new Vector3(0, 0, 0) == spawnPosition)
        {
            Debug.Log($"Unable to respawn player {player.OwnerClientId}");
        } else
        {
            player.GetComponent<PlayerHP>().ResetHPClientRPC(player.OwnerClientId, clientRpcParams);
            player.SetPositionClientRPC(spawnPosition, clientRpcParams);
        }
    }


    public Vector3 findSpawnPosition()
    {
        availableSpawns = FindObjectsByType<PlayerSpawn>(FindObjectsSortMode.None).Where(obj => obj.CanSpawn).ToArray();

        if (availableSpawns.Length == 0) return new Vector3(0, 0, 0);
        return availableSpawns.First().transform.position;
    }
}