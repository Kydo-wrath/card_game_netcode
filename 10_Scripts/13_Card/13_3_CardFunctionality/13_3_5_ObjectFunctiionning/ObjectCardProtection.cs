using UnityEngine;

public class ObjectCardProtection : ObjectCardBase
{
    public override void ActivateCard()
    {

        CardBaseFunctionning CardAttacks = playerOwner.GetComponent<PlayerHand>().GetCardAttacker();
        GameObject PlayerAttacker = null;
        if (CardAttacks != null )
            PlayerAttacker = CardAttacks.GetPlayerOwner();

        Debug.Log("Personnal log : The card who is attacking is " + CardAttacks);
        Debug.Log("Personnal log : The player who own card " + PlayerAttacker);

        if (CardData.id == 42 && playerOwner.GetComponent<CardPlayerNet>().IsMyTurnToPlay())
        {

        }
        else if (CardData.id == 42 && !playerOwner.GetComponent<CardPlayerNet>().IsMyTurnToPlay())
            GiveStatusToPlayer(playerOwner, CardSideEffect);
        else if (CardData.id != 42)
            GiveStatusToPlayer(playerOwner, CardSideEffect);

        if (CardData.id == 42)
            GiveStatusToPlayer(playerOwner, SideEffect.AvoidTrick);
        else if (CardData.id == 45 && !playerOwner.GetComponent<CardPlayerNet>().IsMyTurnToPlay())
        {

            if (PlayerAttacker.GetComponent<CardPlayerNet>().GetCurrentCoins() >= 4)
            {
                playerOwner.GetComponent<CardPlayerNet>().IncrementGoldRpc(4);
                PlayerAttacker.GetComponent<CardPlayerNet>().decremenGoldRpc(4);
                StartCoroutine(PlayerAttacker.GetComponent<CardPlayerNet>().SendCoins(4, playerOwner.GetComponent<CardPlayerNet>().GetCoinsSpawnPoint()));
            }
            else
            {
                playerOwner.GetComponent<CardPlayerNet>().IncrementGoldRpc(PlayerAttacker.GetComponent<CardPlayerNet>().GetCurrentCoins());
                PlayerAttacker.GetComponent<CardPlayerNet>().decremenGoldRpc(PlayerAttacker.GetComponent<CardPlayerNet>().GetCurrentCoins());
                StartCoroutine(PlayerAttacker.GetComponent<CardPlayerNet>().SendCoins(PlayerAttacker.GetComponent<CardPlayerNet>().GetCurrentCoins(), playerOwner.GetComponent<CardPlayerNet>().GetCoinsSpawnPoint()));
            }
        }
        else if (CardData.id == 47)
            PlayerAttacker.GetComponent<PlayerHand>().FlushAggressionCards();

        base.ActivateCard();
    }
}
