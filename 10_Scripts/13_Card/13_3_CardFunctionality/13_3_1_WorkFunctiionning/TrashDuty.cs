using Unity.Netcode;
using UnityEngine;

public class TrashDuty : WorkCardBase
{
    public override void DoSpecialEffect()
    {
        Transform Deckout = playerOwner.GetComponent<PlayerHand>().GetDeckOutSlot();

        for (int i = 0; i < Deckout.childCount; i++)
        {
            if(Deckout.GetChild(i).GetComponent<CardBaseFunctionning>().GetCardData().CardFamily == CardType.Object)
            {
                NetworkObjectReference CardRef = Deckout.GetChild(i).GetComponent<NetworkObject>();
                playerOwner.GetComponent<PlayerHand>().CardInHandRpc(CardRef);
                break;
            }
        }
    }
    
}
