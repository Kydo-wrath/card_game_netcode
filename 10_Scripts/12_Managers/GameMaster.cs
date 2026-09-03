using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [SerializeField] private int GardAlertLevel;
    [SerializeField] private GameObject Bank;
    [SerializeField] private GameObject Deck;
    [SerializeField] private GameObject DeckOut;
    [SerializeField] private GameObject playerActivatingEvent;
    [SerializeField] private GameObject[] Seats;
    [SerializeField] private List<GameObject> Players = new List<GameObject>();

    private List<Character> CharactersLeft = new List<Character>(); 

    public static GameMaster Instance;

    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public PlayerSlot ReturnOpenSeat()
    {
        PlayerSlot seat = null;

        for (int i = 0; i < Seats.Length; i++)
        {
            seat = Seats[i].GetComponent<PlayerSlot>();
            if (!seat.IsSeatTaken())
            { 
                return seat;
            }
        }

        return null;
    }

    private void Start()
    {
        CharactersLeft.AddRange(new Character[]
        {
            Character.paladin,
            Character.Dwarf,
            Character.sorcerer,
            Character.thief,
            Character.bard,
            Character.mercenary,
        });

        foreach(var slot in Seats)
        {
            slot.GetComponent<PlayerSlot>().OnPlayerTakeSeat += PlaySlot_OnPLayerTakeSeat;
        }
    }

    private void PlaySlot_OnPLayerTakeSeat (object sender, PlayerSlot.OnPlayerTakeSeatArgs e)
    {
        PlayerSlot CurrenterSlotSender = null;

        if (sender is PlayerSlot)
            CurrenterSlotSender = sender as PlayerSlot;

        Players.Add(e.playerSeat);

        foreach (var slot in Seats)
        {
            if(slot.GetComponent<PlayerSlot>().IsSeatTaken())
            {
                bool PlayerRegistered = false;  

                foreach (var play in Players)
                {
                    if (slot.GetComponent<PlayerSlot>().Getplayer() == play)
                    {
                        PlayerRegistered = true;
                        break;
                    }
                }

                if (!PlayerRegistered)
                {
                    GameObject playerToSeat = slot.GetComponent<PlayerSlot>().Getplayer();

                    slot.GetComponent<PlayerSlot>().ActualisationPlayerFieldsRpc();

                    Players.Add(playerToSeat);
                    playerToSeat.GetComponent<PlayerHand>().SetDeckOutSlot(DeckOut.transform);
                    playerToSeat.GetComponent<CardPlayerNet>().OnPlayerEndTurn += GameMaster_OnPlayerEndTurn;
                    CharactersLeft.Remove(playerToSeat.GetComponent<CharacterFunctionnality>().GetPlayerCharacter());

                    playerToSeat.GetComponent<CardPlayerNet>().AddTargetablePlayer(slot.GetComponent<PlayerSlot>().GetButtonSlotTarget());
                }

            }
            else
                break;
        }

        Character RandomCharacter = CharactersLeft[Random.Range(0, CharactersLeft.Count - 1)];

        e.playerSeat.GetComponent<PlayerHand>().SetDeckOutSlot(DeckOut.transform);
        e.playerSeat.GetComponent<CardPlayerNet>().OnPlayerEndTurn += GameMaster_OnPlayerEndTurn;
        e.playerSeat.GetComponent<CharacterFunctionnality>().SetPlayerCharacter(RandomCharacter);
        CharactersLeft.Remove(RandomCharacter);

        e.playerSeat.GetComponent<CardPlayerNet>().AddTargetablePlayer(CurrenterSlotSender.GetButtonSlotTarget());

        if (Players.Count >= 3)
        {
            GameObject NextSit = SearchNextPlayerToPlay(0);

            GameObject NextPlayer = NextSit.GetComponent<PlayerSlot>().Getplayer();

            NextPlayer.GetComponent<CardPlayerNet>().BeginTurn();
        }
    }

    private void GameMaster_OnPlayerEndTurn(object sender, CardPlayerNet.OnPlayerEndTurnsArgs e)
    {
        for (int i = 0;i < Seats.Length; i ++)
        {
            if(Seats[i].GetComponent<PlayerSlot>().Getplayer() == e.player)
            {
                GameObject NextSit = SearchNextPlayerToPlay(i += 1);

                GameObject NextPlayer = NextSit.GetComponent<PlayerSlot>().Getplayer();

                NextPlayer.GetComponent<CardPlayerNet>().BeginTurn();
                break;
            }
        }
    }

    public void SetPlayerWhoActivateEvent(GameObject PlayerActivator)
    {
        playerActivatingEvent = PlayerActivator;
    }

    private void newTurnBegin()
    {
        if (GardManager.Instance.getCurrentAlertLevel() > 0)
            GardManager.Instance.IncrementAlertRpc(1);

        foreach(var player in Players)
        {
            if (playerActivatingEvent == player && player.GetComponent<CardPlayerNet>().IsMyTurnToPlay())
            {
                foreach (var infectedPlayer in Players)
                {
                    if (infectedPlayer.GetComponent<CardPlayerNet>().GetPlayerState().WorkIsfreezed)
                        infectedPlayer.GetComponent<CardPlayerNet>().DisablePlayerStateRpc(SideEffect.FreezeWork);
                    else if (infectedPlayer.GetComponent<CardPlayerNet>().GetPlayerState().IsHealed)
                        infectedPlayer.GetComponent<CardPlayerNet>().DisablePlayerStateRpc(SideEffect.heal);
                }
            }
        }
    }

    private GameObject SearchNextPlayerToPlay(int slotNumber)
    {
        if (slotNumber >= Seats.Length)
        { 
            slotNumber = 0;
            newTurnBegin();
        }

        if (!Seats[slotNumber].GetComponent<PlayerSlot>().IsSeatTaken())
        {
            return SearchNextPlayerToPlay(slotNumber += 1);
        }

        return Seats[slotNumber];
    }
    private GameObject SearchPlayerSeatLeft(int slotNumber)
    {
        if (slotNumber >= Seats.Length)
        { 
            slotNumber = 0;
        }

        if (!Seats[slotNumber].GetComponent<PlayerSlot>().IsSeatTaken())
        {
            return SearchNextPlayerToPlay(slotNumber += 1);
        }

        return Seats[slotNumber];
    }
    private GameObject SearchPlayerSeatRight(int slotNumber)
    {
        if (slotNumber < 0)
        { 
            slotNumber = Seats.Length - 1;
        }

        if (!Seats[slotNumber].GetComponent<PlayerSlot>().IsSeatTaken())
        {
            return SearchNextPlayerToPlay(slotNumber -= 1);
        }

        return Seats[slotNumber];
    }

    public GameObject SearchNextPlayer(GameObject playerAsker)
    {
        int slotNumberAsker = 0;

        for (int i = 0; i < Seats.Length; i ++)
        {
            if (Seats[i].GetComponent<PlayerSlot>().Getplayer() == playerAsker)
            {
                slotNumberAsker = i; break;
            }
        }

        return SearchPlayerSeatLeft(slotNumberAsker += 1).GetComponent<PlayerSlot>().Getplayer();
    }

    public GameObject SearchPreviousPlayer(GameObject playerAsker)
    {
        int slotNumberAsker = 0;

        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i].GetComponent<PlayerSlot>().Getplayer() == playerAsker)
            {
                slotNumberAsker = i; break;
            }
        }

        return SearchPlayerSeatRight(slotNumberAsker -= 1).GetComponent<PlayerSlot>().Getplayer();
    }

    public GameObject Getbank()
    {
        return Bank;
    }

    public GameObject GetDeckOut()
    {
        return DeckOut;
    }
    public GameObject[] GetPlayers()
    {
        return Players.ToArray();
    }
}
