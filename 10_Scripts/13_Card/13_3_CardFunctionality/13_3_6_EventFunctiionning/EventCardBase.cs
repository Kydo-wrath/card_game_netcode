using Unity.Netcode;
using UnityEngine;

public class EventCardBase : CardBaseFunctionning
{
    protected CardEvent EventCardData;

    private int NumberOfResponce;

    void Start()
    {
        if (CardData is CardEvent)
        {
            EventCardData = (CardEvent)CardData;
        }
    }

    public override void PlayCard()
    {
        base.PlayCard();
    }

    public override CardBase GetCardData()
    {
        return EventCardData;
    }

    public override void DrawCard()
    {
        DoSpecialEffect();
    }

    public override void ActivateCard()
    {
        playerAnswerRpc();
    }
    protected virtual void EventEffect()
    {
        OutCard();
    }

    public override void DoSpecialEffect()
    {
        foreach (GameObject player in GameMaster.Instance.GetPlayers())
        {
            PingPlayerResponceRpc(player.GetComponent<NetworkObject>());
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void playerAnswerRpc()
    {
        if (NumberOfResponce < GameMaster.Instance.GetPlayers().Length - 1)
            NumberOfResponce++;
        else
            EventEffect();
    }
}
