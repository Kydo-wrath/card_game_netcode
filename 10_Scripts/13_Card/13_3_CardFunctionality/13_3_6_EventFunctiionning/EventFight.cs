using UnityEngine;

public class EventFight : EventCardBase
{
    [SerializeField] string ChoiceGardAlert;
    [SerializeField] string ChoiceDamage;

    protected override void EventEffect()
    {
        
        foreach (var Player in GameMaster.Instance.GetPlayers())
        {
            bool avoidThisEvent = false;


            if(Player.GetComponent<CardPlayerNet>().GetPlayerState().IsEventProtected)
            {
                Debug.Log("personnal log : character who don't have to choice " + Player.GetComponent<CharacterFunctionnality>().GetPlayerCharacter());

                removeStatusToPlayer(Player, SideEffect.AvoidEvent);
                avoidThisEvent = true;
            }
            if(!avoidThisEvent)
                Player.GetComponent<PlayerHand>().TutoMakeChoice(ChoiceGardAlert, ChoiceDamage, IncreaseStat, (() =>
                {
                    GiveStatusToPlayer(Player, StatAffliction);
                }));
        }

        base.EventEffect();
    }

    public override void IncreaseStat()
    {
        GardManager.Instance.IncrementAlertRpc(1);
    }
}
