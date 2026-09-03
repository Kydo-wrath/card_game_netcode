using UnityEngine;

public class EventAffliction : EventCardBase
{
    protected override void EventEffect()
    {
        if (CardData.id == 55 || CardData.id == 51)
            GameMaster.Instance.SetPlayerWhoActivateEvent(playerOwner);

        foreach (GameObject player in GameMaster.Instance.GetPlayers())
        {
            if (!player.GetComponent<CardPlayerNet>().GetPlayerState().IsEventProtected)
            {
                if (StatAffliction != TargetStat.none)
                    GiveStatusToPlayer(player, StatAffliction);

                if (CardSideEffect != SideEffect.none)
                    GiveStatusToPlayer(player, CardSideEffect);
            }
            else
                removeStatusToPlayer(player, SideEffect.AvoidEvent);
        }
        
        base.EventEffect();
    }
}
