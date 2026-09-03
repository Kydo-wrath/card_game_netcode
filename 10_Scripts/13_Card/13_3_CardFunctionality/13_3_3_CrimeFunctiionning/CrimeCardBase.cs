using Unity.Netcode;
using UnityEngine;

public class CrimeCardBase : CardBaseFunctionning
{
    protected CrimeCards crimeCardData;

    void Start()
    {
        if(CardData is CrimeCards)
        {
            crimeCardData = (CrimeCards)CardData;
        }
    }

    public override void PlayCard()
    {
        GameObject[] players = GameMaster.Instance.GetPlayers();

        foreach (GameObject player in players)
        {
            if (player.GetComponent<CardPlayerNet>().GetPlayerState().IsOverWatching)
            {
                playerOwner.GetComponent<PlayerHand>().OverWatchCost(player);
            }
        }

        base.PlayCard();
        MoveToCrimeSlotRpc();
        isPlayed = true;

        if (playerOwner.GetComponent<CardPlayerNet>().GetPlayerState().IsInjured == true)
            return;

        IncreaseStat();
        DoSpecialEffect();

        IsActivated = true;
        
    }
    public override void DoSpecialEffect()
    {
        if (CardSideEffect != SideEffect.none)
            GiveStatusToPlayer(playerOwner, CardSideEffect);

        if (CardSideEffect == SideEffect.intimidate)
            OutCard();  
    }

    public override void IncreaseStat()
    {
        CardPlayerNet Player = playerOwner.GetComponent<CardPlayerNet>();

        Player.IncrementGoldRpc(crimeCardData.goldGain);
        StartCoroutine(Player.CreateCoins(crimeCardData.goldGain));

        if (!playerOwner.GetComponent<CardPlayerNet>().GetPlayerState().IsCrimeProtected)
            GardManager.Instance.IncrementAlertRpc(crimeCardData.awarnessLevel);
    }

    public override CardBase GetCardData()
    {
        return crimeCardData;
    }

    [Rpc(SendTo.Server)]
    public void MoveToCrimeSlotRpc()
    {
        gameObject.transform.SetParent(playerOwner.GetComponent<PlayerHand>().GetCrimeSlot());
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
    }

    public override void ATurnPassed()
    {
        if (IsActivated)
            return;
        CurrentNumberOfTurnPassed++;

        if (!playerOwner.GetComponent<CardPlayerNet>().GetPlayerState().IsInjured || CurrentNumberOfTurnPassed >= 2 )
        {
            IncreaseStat();
            DoSpecialEffect();

            IsActivated = true;
        }
    }

    public override void OutCard()
    {
        CurrentNumberOfTurnPassed = 0;
        IsActivated = false;
        if (CardSideEffect != SideEffect.none)
            removeStatusToPlayer(playerOwner, CardSideEffect);
        base.OutCard();
    }
}
