

using UnityEngine;

public class HelpToServe : WorkCardBase
{
    public override void DoSpecialEffect()
    {
        GameObject nextPlayer = GameMaster.Instance.SearchNextPlayer(playerOwner);
        GameObject previousPlayer = GameMaster.Instance.SearchPreviousPlayer(playerOwner);

        if (nextPlayer.GetComponent<CardPlayerNet>().GetCurrentCoins() > 0)
        {
            playerOwner.GetComponent<CardPlayerNet>().IncrementGoldRpc(1);
        }

        if (previousPlayer.GetComponent<CardPlayerNet>().GetCurrentCoins() > 0)
        {
            playerOwner.GetComponent<CardPlayerNet>().IncrementGoldRpc(1);
        }

        nextPlayer.GetComponent<CardPlayerNet>().decremenGoldRpc(1);
        StartCoroutine(nextPlayer.GetComponent<CardPlayerNet>().SendCoins(1, playerOwner.GetComponent<CardPlayerNet>().GetCoinsSpawnPoint()));

        previousPlayer.GetComponent<CardPlayerNet>().decremenGoldRpc(1);
        StartCoroutine(previousPlayer.GetComponent<CardPlayerNet>().SendCoins(1, playerOwner.GetComponent<CardPlayerNet>().GetCoinsSpawnPoint()));
    }
}
