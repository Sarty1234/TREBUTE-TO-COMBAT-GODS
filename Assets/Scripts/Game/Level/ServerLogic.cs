using Assets.Scripts.Game.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        detectGameMode();
        StopCoroutine(everyoneForHimselfRespawnCoroutine());


        switch(gameMode)
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


    private Vector3 spawnPosition;
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
                    spawnPosition = findSpawnPosition();
                    if (spawnPosition != new Vector3(0, 0, 0))
                    {
                        player.SetPositionClientRPC(player.GetComponent<NetworkTransform>().OwnerClientId, spawnPosition);
                        player.GetComponent<PlayerHP>().ResetHPClientRPC(GetComponent<NetworkTransform>().OwnerClientId);
                        player.IsDead.Value = false;
                        if ((player.transform.position - spawnPosition).magnitude <= 1)
                        {
                            Debug.Log("DeadPlayer");
                        }
                    }
                }
            }
            yield return delay;
        }
    }


    public Vector3 findSpawnPosition()
    {
        availableSpawns = FindObjectsByType<PlayerSpawn>(FindObjectsSortMode.None).Where(obj => obj.CanSpawn).ToArray();

        if (availableSpawns.Length == 0) return new Vector3(0, 0, 0);
        return availableSpawns.First().transform.position;
    }
}
