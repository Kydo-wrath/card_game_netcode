using Unity.Netcode;
using UnityEngine;

public class WorkCardBase : CardBaseFunctionning
{
    [SerializeField] protected int[] cardPlayableId;

    protected bool intimidateBuffer;
    protected CardWorks workCardData;
   
    private void Start()
    {
        if (CardData is CardWorks)
        {
            workCardData = (CardWorks)CardData;
        }
    }

    public override void PlayCard()
    {
        base.PlayCard();
        MoveToWorkSlotRpc();
        DoSpecialEffect();
    }

    public override void OutCard()
    {
        if (CurrentNumberOfTurnPassed >= workCardData.numberOfTurn)
            IncreaseStat();
        else
        {
            if(workCardData.id == 18)
            {
                CardPlayerNet Player = playerOwner.GetComponent<CardPlayerNet>();

                Player.IncrementGoldRpc(CurrentNumberOfTurnPassed);
                StartCoroutine(Player.CreateCoins(CurrentNumberOfTurnPassed));
            }
        }

        if (CardSideEffect != SideEffect.none)
            removeStatusToPlayer(playerOwner, CardSideEffect);

        CurrentNumberOfTurnPassed = 0;
        intimidateBuffer = false;
            base.OutCard();
    }
    public override CardBase GetCardData()
    {
        return workCardData;
    }

    [Rpc(SendTo.Server)]
    public void MoveToWorkSlotRpc()
    {
        gameObject.transform.SetParent(playerOwner.GetComponent<PlayerHand>().GetWorkSlot());
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
    }

    public override void DoSpecialEffect()
    {
        if (CardSideEffect != SideEffect.none)
            GiveStatusToPlayer(playerOwner, CardSideEffect);
    }
    public override void IncreaseStat()
    {
        CardPlayerNet Player = playerOwner.GetComponent<CardPlayerNet>();

        if(intimidateBuffer)
            Player.IncrementGoldRpc(workCardData.goldGain + 2);
        else
            Player.IncrementGoldRpc(workCardData.goldGain);

        StartCoroutine(Player.CreateCoins(workCardData.goldGain));
    }

    public override void ATurnPassed()
    {
        
        CurrentNumberOfTurnPassed++;

        if(CurrentNumberOfTurnPassed >= workCardData.numberOfTurn)
        {
            if(playerOwner.GetComponent<CardPlayerNet>().GetPlayerState().IsInjured)
            {
                if (CurrentNumberOfTurnPassed >= workCardData.numberOfTurn + 2)
                    OutCard();

            }
            else
                OutCard();
        }
    }

    public int[] GetWorkCardPlayable()
    {
        return cardPlayableId;
    }

    public void Intimidate()
    {
        intimidateBuffer = true;
    }
}
