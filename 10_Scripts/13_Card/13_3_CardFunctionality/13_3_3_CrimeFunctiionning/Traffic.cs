using Unity.Netcode;
using UnityEngine;

public class Traffic : CrimeCardBase
{
    public override void DoSpecialEffect()
    {
        Transform Deckout = playerOwner.GetComponent<PlayerHand>().GetDeckOutSlot();
        Transform ObjectsPlace = playerOwner.GetComponent<PlayerHand>().GetObjectSlot();

        for (int i =0 ; i < ObjectsPlace.childCount; i++)
        {
            ObjectsPlace.GetChild(0).GetComponent<CardBaseFunctionning>().OutCard();
        }

        for (int i = 0; 0 < Deckout.childCount; i++)
        {
            if (Deckout.GetChild(i).GetComponent<CardBaseFunctionning>().GetCardData().CardFamily == CardType.Object)
            {
                NetworkObjectReference CardRef = Deckout.GetChild(i).GetComponent<NetworkObject>();
                SendCardToDeckRpc(CardRef);
                playerOwner.GetComponent<CardPlayerNet>().IncrementGoldRpc(1);
                StartCoroutine(playerOwner.GetComponent<CardPlayerNet>().CreateCoins(1));
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void SendCardToDeckRpc(NetworkObjectReference ObjectCard, RpcParams netParams = default)
    {
        Transform Deck = DeckManager.deckInstance.transform;

        if (ObjectCard.TryGet(out NetworkObject ObjectCardNetwork))
        {
            ObjectCardNetwork.transform.SetParent(Deck.transform);
            ObjectCardNetwork.transform.localPosition = Vector3.zero;
            ObjectCardNetwork.transform.localRotation = new Quaternion(0, 0, 0, 0);
        }

        DeckManager.deckInstance.ShufleDeckRpc();
    }
}
