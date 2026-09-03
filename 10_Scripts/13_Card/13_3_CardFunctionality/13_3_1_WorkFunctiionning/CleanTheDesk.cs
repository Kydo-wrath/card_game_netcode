using UnityEngine;


public class CleanTheDesk : WorkCardBase
{
    [SerializeField] protected int[] CardCountedInTheEffect;
    public override void PlayCard()
    {
        isPlayed = true;
        MoveToWorkSlotRpc();
        playerOwner.GetComponent<PlayerHand>().CardPlayedRpc(CardData.CardFamily);
        if (playerOwner.GetComponent<CardPlayerNet>().GetPlayerState().IsPoisonned || playerOwner.GetComponent<CardPlayerNet>().playerIsBeerPoisonned())
            playerOwner.GetComponent<PlayerHand>().PoisonChoiceActivation();
    }

    public override void OutCard()
    {
        if (CurrentNumberOfTurnPassed >= workCardData.numberOfTurn)
            DoSpecialEffect();

            base.OutCard();
    }
    public override void DoSpecialEffect()
    {
        GameObject[] players = GameMaster.Instance.GetPlayers();

        foreach (GameObject p in players)
        {
            for (int i = 0; i < playerOwner.GetComponent<PlayerHand>().GetWorkSlot().childCount; i++)
            {
                Transform card = playerOwner.GetComponent<PlayerHand>().GetWorkSlot().GetChild(i);

                foreach (var cardId in CardCountedInTheEffect)
                {
                    if (card.GetComponent<CardBaseFunctionning>().GetCardData().id == cardId)
                        GardManager.Instance.DecrementAlertRpc(1);
                }
            }
        }
    }
}
