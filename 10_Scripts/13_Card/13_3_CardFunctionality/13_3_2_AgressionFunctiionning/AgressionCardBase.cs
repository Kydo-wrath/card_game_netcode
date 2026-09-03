using Unity.Netcode;
using UnityEngine;

public class AgressionCardBase : CardBaseFunctionning
{
    protected AgressionCards AgressionCardData;
    
    void Start()
    {
        if (CardData is AgressionCards)
        {
            AgressionCardData = (AgressionCards)CardData;
        }
    }

    public override void PlayCard()
    {
        base.PlayCard();

        ChooseTarget();
    }

    public override CardBase GetCardData()
    {
        return AgressionCardData;
    }

    public override void DoSpecialEffect()
    {
        foreach (GameObject player in GameMaster.Instance.GetPlayers())
        {
            if (player.GetComponent<CardPlayerNet>().GetPlayerState().IsOverWatching)
            {
                playerOwner.GetComponent<PlayerHand>().OverWatchCost(player);
            }
        }

        PingPlayerResponceRpc(playerTarget.GetComponent<NetworkObject>());
    }

    public override void IncreaseStat()
    {
        GardManager.Instance.IncrementAlertRpc(AgressionCardData.awarnessLevel);
    }

    public override void ActivateCard()
    {
        IncreaseStat();

        if (playerTarget.GetComponent<CardPlayerNet>().GetPlayerState().IsAggressionProtected)
        {
            SetNetTargetRpc(new NetworkObjectReference());

            removeStatusToPlayer(playerTarget, SideEffect.AvoidAggression);
        }

    }

    public override void ChooseTarget()
    {
        if(playerOwner.GetComponent<CardPlayerNet>().GetPlayerState().IsCharmed)
            HudManager.instance.ShowTargetPlayerChoice(playerOwner, this, NetworkManager.Singleton.ConnectedClients[playerOwner.GetComponent<CardPlayerNet>().GetPlayerState().PlayerCharmerId].PlayerObject.gameObject);
        else
            base.ChooseTarget();
    }

}
