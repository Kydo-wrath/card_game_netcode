using System;
using System.Collections;
using System.Diagnostics;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<bool> SlotTaken = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<ulong> Player = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private Transform coinSpawnPoint;
    [SerializeField] private Transform workSlot;
    [SerializeField] private Transform objectSlot;
    [SerializeField] private Transform crimeSlot;

    [SerializeField] private Button ButtonSlotTarget;

    public event EventHandler<OnPlayerTakeSeatArgs> OnPlayerTakeSeat;

    public class OnPlayerTakeSeatArgs : EventArgs
    {
        public GameObject playerSeat;
    }


    private void Awake()
    {
        Vector3 direction = FindAnyObjectByType<LookAtPointer>().GetLookAtPoint() - transform.position;

        direction.y = 0f;   

        transform.rotation = Quaternion.LookRotation(direction);
    }
    public bool IsSeatTaken()
    {
        return SlotTaken.Value;
    }

    public Transform ReturnCoinSpawnLocation()
    {
        return coinSpawnPoint; 
    }  

    [Rpc(SendTo.Server)]
    public void PlayerTakeSeatRpc(NetworkObjectReference player, RpcParams sender = default)
    {
        NetworkObject playerNetworkObject = null;

        if(player.TryGet(out NetworkObject playerObject))
            playerNetworkObject = playerObject;

        if (NetworkManager.Singleton.ConnectedClients[sender.Receive.SenderClientId].PlayerObject.GetComponent<CardPlayerNet>().IsAlreadySeat())
            return;
        
        if (SlotTaken.Value)
        {
            return;
        }
        Player.Value = sender.Receive.SenderClientId;
        SlotTaken.Value = true;

        playerNetworkObject.transform.position = transform.position;

        SetPlayerFieldsRpc(player);
        NotifyGameMasterPlayerTakeSeatRpc(player);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void NotifyGameMasterPlayerTakeSeatRpc(NetworkObjectReference player, RpcParams sender = default) 
    {
        if (player.TryGet(out NetworkObject playerObject))
        {
            OnPlayerTakeSeat?.Invoke(this, new OnPlayerTakeSeatArgs
            {
                playerSeat = playerObject.gameObject
            });
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetPlayerFieldsRpc(NetworkObjectReference player, RpcParams sender = default)
    {
        NetworkObject playerNetworkObject = null;

        if (player.TryGet(out NetworkObject playerObject))
            playerNetworkObject = playerObject;

        playerNetworkObject.GetComponent<CardPlayerNet>().SetCoinSpawnPoint(coinSpawnPoint);
        playerNetworkObject.GetComponent<PlayerHand>().SetWorkSlot(workSlot);
        playerNetworkObject.GetComponent<PlayerHand>().SetObjectSlot(objectSlot);
        playerNetworkObject.GetComponent<PlayerHand>().SetCrimeSlot(crimeSlot);

    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ActualisationPlayerFieldsRpc(RpcParams sender = default)
    {
        if (sender.Receive.SenderClientId == Player.Value)
            return;

        Getplayer().GetComponent<CardPlayerNet>().SetCoinSpawnPoint(coinSpawnPoint);
        Getplayer().GetComponent<PlayerHand>().SetWorkSlot(workSlot);
        Getplayer().GetComponent<PlayerHand>().SetObjectSlot(objectSlot);
        Getplayer().GetComponent<PlayerHand>().SetCrimeSlot(crimeSlot);
    }

    public GameObject Getplayer()
    {
        return NetworkManager.Singleton.ConnectedClients[Player.Value].PlayerObject.gameObject;
    }

    public Button GetButtonSlotTarget()
    {
        return ButtonSlotTarget;
    }
}
