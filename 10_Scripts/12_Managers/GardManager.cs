using System;
using Unity.Netcode;
using UnityEngine;

public class GardManager : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<int> AlertLevel = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> NumberOfResponse = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public static GardManager Instance;


    public event EventHandler OnAlertActivate;


    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        AlertLevel.OnValueChanged += AlertLevelChangedRpc;
    }

    public int getCurrentAlertLevel()
    { 
        return AlertLevel.Value; 
    }

    [Rpc(SendTo.Server)]
    public void IncrementAlertRpc(int alertGain)
    {
        AlertLevel.Value += alertGain;

        if (AlertLevel.Value >= 12)
        { 
            AlertLevel.Value = 12;
            ActivateAlertRpc();
        }
    }
    [Rpc(SendTo.Server)]
    public void DecrementAlertRpc(int alertLoss)
    {
        AlertLevel.Value -= alertLoss;

        if (AlertLevel.Value < 0)
            AlertLevel.Value = 0;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void AlertLevelChangedRpc(int previousValue, int newValue)
    {
        HudManager.instance.SetAlertLevelDisplay(newValue);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ActivateAlertRpc()
    {
        OnAlertActivate?.Invoke(this,new EventArgs());
    }

    [Rpc(SendTo.Server)]
    public void ResponceReceivedRpc(NetworkObjectReference player)
    {
        NumberOfResponse.Value += 1;
        playAlertRpc(player);

        if (NumberOfResponse.Value >= GameMaster.Instance.GetPlayers().Length)
            AlertLevel.Value = 0;

    }
    [Rpc(SendTo.ClientsAndHost)]
    public void playAlertRpc(NetworkObjectReference player)
    {
        if (player.TryGet(out NetworkObject TransformNet))
        {
            TransformNet.gameObject.GetComponent<CardPlayerNet>().GardAlertEffect();
        }

    }

}
