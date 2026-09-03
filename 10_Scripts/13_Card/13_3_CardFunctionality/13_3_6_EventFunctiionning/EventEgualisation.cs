using UnityEngine;

public class EventEgualisation : EventCardBase
{
    protected override void EventEffect()
    {
        int GoldToShare = 0;
        int PlayersToShareBetween = 0;
        bool avoidThisEvent = false;

        foreach (var Player in GameMaster.Instance.GetPlayers())
        {
            if(Player.GetComponent<CardPlayerNet>().GetPlayerState().IsEventProtected)
            {
                Debug.Log("personnal log : character who don't have to choice " + Player.GetComponent<CharacterFunctionnality>().GetPlayerCharacter());

                removeStatusToPlayer(Player, SideEffect.AvoidEvent);
                avoidThisEvent = true;
            }
            if(!avoidThisEvent)
            {
                GoldToShare += Player.GetComponent<CardPlayerNet>().GetCurrentCoins();
                PlayersToShareBetween++;
            }
        }

        if (GoldToShare == 0 || PlayersToShareBetween == 0)
        {
            Debug.Log("Personnal log : total of gold of all players " + GoldToShare);
            Debug.Log("Personnal log : total of players who have to share " + PlayersToShareBetween);
            base.EventEffect();
            return;
        }

        int goldPerPlayer = GoldToShare / PlayersToShareBetween;
        int excessGold = GoldToShare % PlayersToShareBetween;

        foreach (var Player in GameMaster.Instance.GetPlayers())
        {
            if (!avoidThisEvent)
            {
                if(Player.GetComponent<CardPlayerNet>().GetCurrentCoins() > goldPerPlayer)
                {
                    Player.GetComponent<CardPlayerNet>().decremenGoldRpc(Player.GetComponent<CardPlayerNet>().GetCurrentCoins() - goldPerPlayer);
                    Player.GetComponent<CardPlayerNet>().DeleteCoins(Player.GetComponent<CardPlayerNet>().GetCurrentCoins() - goldPerPlayer);
                }
                else if (Player.GetComponent<CardPlayerNet>().GetCurrentCoins() < goldPerPlayer)
                {
                    Player.GetComponent<CardPlayerNet>().IncrementGoldRpc(goldPerPlayer - Player.GetComponent<CardPlayerNet>().GetCurrentCoins());
                    Player.GetComponent<CardPlayerNet>().CreateCoins(goldPerPlayer - Player.GetComponent<CardPlayerNet>().GetCurrentCoins());
                }
            }
        }

        if (GoldToShare == 0 || PlayersToShareBetween == 0)
        {
            Debug.Log("Personnal log : total of excess gold " + excessGold);
            base.EventEffect();
            return;
        }

        StartCoroutine(GameMaster.Instance.Getbank().GetComponent<BankManager>().CreateCoins(excessGold));

        base.EventEffect();
    }
}
