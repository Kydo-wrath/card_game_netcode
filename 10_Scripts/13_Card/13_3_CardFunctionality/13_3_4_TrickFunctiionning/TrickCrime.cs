
using Unity.Netcode;
using UnityEngine;

public class  TrickCrime: TrickCardBase
{
    public override void ActivateCard()
    {
        base.ActivateCard();

        if(playerTarget == null)
        {
            OutCard();
            return;
        }

        if(CardData.id == 35)
        {
            CardBase card = GetLastCrimeCardPlayed().GetComponent<CardBaseFunctionning>().GetCardData();
            CrimeCards Crime = (CrimeCards)card;
            int billSplit = Crime.goldGain / 2;


            playerTarget.GetComponent<CardPlayerNet>().decremenGoldRpc(billSplit);
            StartCoroutine(playerTarget.GetComponent<CardPlayerNet>().SendCoins(billSplit, playerOwner.GetComponent<CardPlayerNet>().GetCoinsSpawnPoint()));
        }

        if (CardData.id == 36)
        {

            CrimeCardBase currentCrimeCard = GetLastCrimeCardPlayed().GetComponent<CrimeCardBase>();

            currentCrimeCard.SetPlayerOwner(playerTarget);
            currentCrimeCard.MoveToCrimeSlotRpc();
        }

        OutCard();
    }

    public override void DoSpecialEffect()
    {
        if (CardData.id == 35)
        {
            foreach (var player in GameMaster.Instance.GetPlayers())
            {
                if (player.GetComponent<CardPlayerNet>().IsMyTurnToPlay())
                {
                    if (player.GetComponent<PlayerHand>().IsCrimeCardPlayed())
                    {
                        SetPlayerTarget(player);
                        PingPlayerResponceRpc(player.GetComponent<NetworkObject>());
                    }
                }
            }
        }
        else
            base.DoSpecialEffect();
    }
    protected Transform GetLastCrimeCardPlayed()
    {
        foreach (var player in GameMaster.Instance.GetPlayers())
        {
            if (player.GetComponent<CardPlayerNet>().IsMyTurnToPlay())
            {
                if (player.GetComponent<PlayerHand>().IsCrimeCardPlayed())
                {
                    return player.GetComponent<PlayerHand>().GetCrimeSlot().GetChild(player.GetComponent<PlayerHand>().GetCrimeSlot().childCount - 1);
                }
            }
        }

        return null;
    }

}
