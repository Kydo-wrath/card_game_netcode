using UnityEngine;

public class EventWitchHunt : EventCardBase
{
    protected override void EventEffect()
    {

        foreach (var Player in GameMaster.Instance.GetPlayers())
        {
            bool avoidThisEvent = false;

            if (Player.GetComponent<CardPlayerNet>().GetPlayerState().IsEventProtected)
            {
                Debug.Log("personnal log : character who don't have to choice " + Player.GetComponent<CharacterFunctionnality>().GetPlayerCharacter());

                removeStatusToPlayer(Player, SideEffect.AvoidEvent);
                avoidThisEvent = true;
            }

            if (!avoidThisEvent)
            {
                if(Player.GetComponent<PlayerHand>().GetObjectSlot().childCount > 0)
                {
                    Player.GetComponent<PlayerHand>().FlushObjectCards();
                    Player.GetComponent<PlayerHand>().pickCard();
                }
            }

            base.EventEffect();
        }
    }
}
