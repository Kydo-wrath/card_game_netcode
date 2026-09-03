
using UnityEngine;
using UnityEngine.Events;
public class AgressionDwarfLauncher : AgressionCardBase
{
    [SerializeField] string ChoiceCoinLoss;
    [SerializeField] string ChoiceDamage;

    public override void ActivateCard()
    {
        if (!playerTarget.GetComponent<CardPlayerNet>().GetPlayerState().IsAggressionProtected)
        {
            playerTarget.GetComponent<PlayerHand>().TutoMakeChoice(ChoiceCoinLoss, ChoiceDamage, (() =>
            {
                coinLossChoice(base.ActivateCard);
            }), (() =>
            {
                DamageChoice(base.ActivateCard);
            }));
        }
        else
        {
            base.ActivateCard();
            OutCard();
        }
    }

    public void coinLossChoice(UnityAction BaseCardActivation)
    {

        if(playerTarget.GetComponent<CardPlayerNet>().GetCurrentCoins() >= GameMaster.Instance.GetPlayers().Length)
        {
            playerTarget.GetComponent<CardPlayerNet>().decremenGoldRpc(GameMaster.Instance.GetPlayers().Length);
            StartCoroutine(playerTarget.GetComponent<CardPlayerNet>().DeleteCoins(GameMaster.Instance.GetPlayers().Length));
        }
        else
        {
            playerTarget.GetComponent<CardPlayerNet>().decremenGoldRpc(playerTarget.GetComponent<CardPlayerNet>().GetCurrentCoins());
            StartCoroutine(playerTarget.GetComponent<CardPlayerNet>().DeleteCoins(playerTarget.GetComponent<CardPlayerNet>().GetCurrentCoins()));
        }

        BaseCardActivation.Invoke();
    }

    public void DamageChoice(UnityAction BaseCardActivation)
    {
        GiveStatusToPlayer(playerTarget, StatAffliction);
        BaseCardActivation.Invoke();
        OutCard();
    }
}
