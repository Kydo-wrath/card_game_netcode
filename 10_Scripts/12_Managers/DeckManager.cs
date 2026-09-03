using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeckManager : NetworkBehaviour
{
    [SerializeReference] private  List<GameObject> PrincipalDeck = new List<GameObject>();
    private GameObject CurrentCardToSpawn;

    public static DeckManager deckInstance;

    [SerializeField] GameObject Deck;

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            ShufleCards();
            for (int i = 0; i < PrincipalDeck.Count; i++)
            {
                CurrentCardToSpawn = Instantiate(PrincipalDeck[i]);
                SpawnCardRpc();
            }
        }
        
    }

    private void Awake()
    {
        if (deckInstance == null)
        {
            deckInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShufleCards()
    {
        GameObject CurrentCardToShuffle = null;

        for (int i = 0; i < PrincipalDeck.Count; i++)
        {
            CurrentCardToShuffle = PrincipalDeck[i];
            int Randomizer = Random.Range(0, PrincipalDeck.Count - 1);
            PrincipalDeck[i] = PrincipalDeck[Randomizer];
            PrincipalDeck[Randomizer] = CurrentCardToShuffle;
        }
    }

    [Rpc(SendTo.Server)]
    public void SpawnCardRpc()
    {
        CurrentCardToSpawn.GetComponent<NetworkObject>().Spawn(true);
        CurrentCardToSpawn.transform.SetParent(Deck.transform);
        CurrentCardToSpawn.transform.localPosition = Vector3.zero;
        CurrentCardToSpawn.transform.localRotation = new Quaternion(0, 0, 0, 0);
    }

    [Rpc(SendTo.Server)]
    public void insertCardRpc(NetworkObjectReference Card)
    {
        if (Card.TryGet(out NetworkObject CardNetwork))
        {
            CardNetwork.transform.SetParent(Deck.transform);
            CardNetwork.transform.localPosition = Vector3.zero;
            CardNetwork.transform.localRotation = new Quaternion(0, 0, 0, 0);
        }
    }

    public Transform PickUpCard()
    {
        Transform cardGiven = null;
        cardGiven = Deck.transform.GetChild(Deck.transform.childCount - 1);
        DecrementCardRpc(cardGiven.GetComponent<NetworkObject>());

        if (Deck.transform.childCount == 1)
        {
            GetDeckoutCardsRpc();
        }

        return cardGiven;
    }
    public Transform PickUpNextCard(int next)
    {
        Transform cardGiven = null;
        cardGiven = Deck.transform.GetChild(Deck.transform.childCount - next);
        DecrementCardRpc(cardGiven.GetComponent<NetworkObject>());

        if (Deck.transform.childCount == 1)
        {
            GetDeckoutCardsRpc();
        }

        return cardGiven;
    }

    [Rpc(SendTo.Server)]
    public void DecrementCardRpc(NetworkObjectReference Card, RpcParams netParams = default)
    {
        if (Card.TryGet(out NetworkObject CardNetwork))
        { 
            CardNetwork.transform.SetParent(null);
        }

    }

    [Rpc(SendTo.Server)]
    public void GetDeckoutCardsRpc()
    {
        GameObject DeckOut =  GameMaster.Instance.GetDeckOut();

        for(int i = 0; i < DeckOut.transform.childCount; i++)
        {
            Transform CurrentCardObject = DeckOut.transform.GetChild(0);
            CurrentCardObject.SetParent(gameObject.transform);
            CurrentCardObject.localPosition = Vector3.zero;
            CurrentCardObject.localRotation = new Quaternion(0, 0, 0, 0);
        }
        ShufleDeckRpc();
    }

    [Rpc(SendTo.Server)]
    public void ShufleDeckRpc()
    {
        for (int i = 0; i < transform.transform.childCount; i++)
        {
            Transform CurrentCardObject = Deck.transform.GetChild(Random.Range(0, transform.childCount - 1));


            CurrentCardObject.SetParent(null);
            CurrentCardObject.SetParent(Deck.transform);
            CurrentCardObject.localPosition = Vector3.zero;
            CurrentCardObject.localRotation = new Quaternion(0, 0, 0, 0);

        }
    }

}
