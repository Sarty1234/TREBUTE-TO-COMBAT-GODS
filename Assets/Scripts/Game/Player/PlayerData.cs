using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<int> kills = new NetworkVariable<int>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> death = new NetworkVariable<int>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<float> deathTime = new NetworkVariable<float>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> team = new NetworkVariable<int>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);


    public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(value: true, readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        ResetData();
        SetDeathTimeServerRPC();
    }


    [ServerRpc(RequireOwnership = true)]
    private void SetDeathTimeServerRPC()
    {
        deathTime.Value = Time.time;
    }


    public void ResetData()
    {
        kills.Value = 0;
        death.Value = 0;
    }


    public void AddKill()
    {
        if (!IsOwner) return;
        AddKillServerRPC();
    }


    [ServerRpc(RequireOwnership = false)]
    private void AddKillServerRPC()
    {
        kills.Value += 1;
    }


    public void Dead()
    {
        if (!IsOwner) return;
        DeadServerRPC();
    }


    [ServerRpc(RequireOwnership = false)]
    private void DeadServerRPC()
    {
        death.Value += 1;
        IsDead.Value = true;
    }


    [ClientRpc()]
    public void SetPositionClientRPC(ulong playerID, Vector3 newPosition)
    {
        if (!IsOwner) return;
        if (GetComponent<NetworkTransform>().OwnerClientId != playerID) return;


        transform.position = newPosition;
    }
}
