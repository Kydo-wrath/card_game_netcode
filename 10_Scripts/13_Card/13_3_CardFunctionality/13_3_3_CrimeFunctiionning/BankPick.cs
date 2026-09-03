using UnityEngine;

public class BankPick : CrimeCardBase
{
    [SerializeField] bool StillTheWholeBank;
    public override void DoSpecialEffect()
    {

        BankManager BankFunction = GameMaster.Instance.Getbank().GetComponent<BankManager>(); 

        if (StillTheWholeBank)
        {
            playerOwner.GetComponent<CardPlayerNet>().IncrementGoldRpc(BankFunction.GetCurrentbankCoins());
            StartCoroutine(BankFunction.SendCoins(BankFunction.GetCurrentbankCoins(), playerOwner.GetComponent<CardPlayerNet>().GetCoinsSpawnPoint()));
            BankFunction.DecrementCoinsRpc(BankFunction.GetCurrentbankCoins());
        }
        else
        {
            if (BankFunction.GetCurrentbankCoins() >= 3)
            {
                playerOwner.GetComponent<CardPlayerNet>().IncrementGoldRpc(3);
                StartCoroutine(BankFunction.SendCoins(3, playerOwner.GetComponent<CardPlayerNet>().GetCoinsSpawnPoint()));
                BankFunction.DecrementCoinsRpc(3);
            }
            else
            {
                playerOwner.GetComponent<CardPlayerNet>().IncrementGoldRpc(BankFunction.GetCurrentbankCoins());
                StartCoroutine(BankFunction.SendCoins(BankFunction.GetCurrentbankCoins(), playerOwner.GetComponent<CardPlayerNet>().GetCoinsSpawnPoint()));
                BankFunction.DecrementCoinsRpc(BankFunction.GetCurrentbankCoins());
            }
        }

        foreach (var player in GameMaster.Instance.GetPlayers())
        {
            if (player.GetComponent<CardPlayerNet>().GetPlayerState().IsWatchingBank)
            {
                playerOwner.GetComponent<CardPlayerNet>().decremenGoldRpc(1);
                playerOwner.GetComponent<CardPlayerNet>().SendCoins(1, player.GetComponent<CardPlayerNet>().GetCoinsSpawnPoint());
            }
        }

        base.DoSpecialEffect();
    }
}
