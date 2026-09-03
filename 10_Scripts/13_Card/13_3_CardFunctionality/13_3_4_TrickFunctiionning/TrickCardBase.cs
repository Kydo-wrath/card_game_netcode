using Unity.Netcode;
using UnityEngine;

public class TrickCardBase : CardBaseFunctionning
{

    [SerializeField] protected bool haveToAim;
    protected TrickCard TrickCardData;

    void Start()
    {
        if (CardData is TrickCard)
        {
            TrickCardData = (TrickCard)CardData;
        }
    }

    public override void PlayCard()
    {
        base.PlayCard();

        if (haveToAim)
        {
            ChooseTarget();
        }
        else
        {
            DoSpecialEffect();
        }
    }

    public override void DoSpecialEffect()
    {
        if (haveToAim)
            PingPlayerResponceRpc(playerTarget.GetComponent<NetworkObject>());
        else
        {
            GiveStatusToPlayer(playerOwner, CardSideEffect);

            OutCard();
        }
    }

    public override void ActivateCard()
    {
        if (playerTarget.GetComponent<CardPlayerNet>().GetPlayerState().IsTrickProtected)
        {
            SetNetTargetRpc(new NetworkObjectReference());

            removeStatusToPlayer(playerTarget, SideEffect.AvoidTrick);
        }
    }

    public override CardBase GetCardData()
    {
        return TrickCardData;
    }
    

}
