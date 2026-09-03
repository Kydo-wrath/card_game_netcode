using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerHand : NetworkBehaviour
{
    [SerializeField] private GameObject handPrefab;
    [SerializeField] private GameObject Player;
    [SerializeField] private Transform FirstHandLocation;
    [SerializeField] private Transform SecondHandLocation;
    [SerializeField] private Transform FirstHandTableView;
    [SerializeField] private Transform SecondHandTableView;
    [SerializeField] private DeckManager deck;
    [SerializeField] int NumberOfCardToBegin;

    [SerializeField] private InputActionReference Swipe;
    [SerializeField] private InputActionReference Touch;

    private GameObject handFirstLine;
    private GameObject handSecondLine;

    private Transform WorkSlot;
    private Transform ObjectSlot;
    private Transform CrimeSlot;
    private Transform deckOut;
    private Transform CurrentCardSelect;

    private List<Transform>  EventBeginPick = new List<Transform>();

    private Vector2 screenPointerPosition;
    private Vector2 screenPointerPositionNormalized;
    private Vector2 screenPointStartNormalized;
    private Vector2 screenPointEndNormalized;

    private bool OnTableView = false;
    private bool CanvasHovered = false;

    private bool CardWorkPlayed = false;
    private bool CardTrickPlayed = false;
    private bool CardCrimePlayed = false;
    private bool CardAggressionPlayed = false;
    private bool CardObjectPlayed = false;

    private bool PoisonChoice = false;
    private bool isResponding = false;
    private bool isGardAlert = false;
    private bool isWaiting = false;

    private HudManager hud;

    private CardBaseFunctionning cardRespondingTo;

    private void OnEnable()
    {
        Swipe.action.performed += screenSwipe;
        Swipe.action.performed += screentouchPosition;
        Touch.action.started += TouchScreen;
        Touch.action.canceled += TouchScreen;
        Touch.action.started += TouchCard;
        Touch.action.canceled += TouchCard;
    }

    private void OnDisable()
    {
        Swipe.action.performed -= screenSwipe;
        Swipe.action.performed -= screentouchPosition;
        Touch.action.started -= TouchScreen;
        Touch.action.canceled -= TouchScreen;
        Touch.action.started -= TouchCard;
        Touch.action.canceled -= TouchCard;
    }

    private void TouchScreen(InputAction.CallbackContext context)
    {

        if (!IsOwner)
            return;

        if (context.action.IsPressed())
        { 
            screenPointStartNormalized = screenPointerPositionNormalized;
        }
        else
        {
            screenPointEndNormalized = screenPointerPositionNormalized;

            if (!OnTableView && screenPointStartNormalized.y - screenPointEndNormalized.y >= 0.2)
            {
                MoveHandTableSelectedRpc();
                OnTableView = true;
            }
            else if (OnTableView && screenPointStartNormalized.y - screenPointEndNormalized.y <= -0.2)
            {
                MoveHandTableUnselectedRpc();
                OnTableView = false;
            }
            screenPointStartNormalized = Vector2.zero;
            screenPointEndNormalized = Vector2.zero;

        }

    }

    private void screenSwipe(InputAction.CallbackContext obj)
    {
        screenPointerPositionNormalized = obj.action.ReadValue<Vector2>()/ new Vector2(Screen.width, Screen.height);
    }

    private void TouchCard(InputAction.CallbackContext context)
    {

        if ((!IsOwner || !gameObject.GetComponent<CardPlayerNet>().IsMyTurnToPlay()|| isWaiting) && !isResponding)
            return;

        if (CanvasHovered)
            return;

        Ray cast = gameObject.GetComponent<CardPlayerNet>().GetPlayerCamera().ScreenPointToRay(screenPointerPosition);
        RaycastHit hit;

        if (!context.action.IsPressed())
        {
            if (CurrentCardSelect)
            {
                if (Physics.Raycast(cast, out hit))
                {
                    if (hit.collider == null)
                    {
                        if (!CurrentCardSelect.IsChildOf(handFirstLine.transform) && !CurrentCardSelect.IsChildOf(handSecondLine.transform))
                            CurrentCardSelect.GetComponent<CardBaseFunctionning>().UnselectCard();
                        CurrentCardSelect = null;
                        return;
                    }

                    if (CurrentCardSelect != null && CurrentCardSelect != hit.collider.transform)
                    {
                        if (!CurrentCardSelect.IsChildOf(handFirstLine.transform) && !CurrentCardSelect.IsChildOf(handSecondLine.transform))
                            CurrentCardSelect.GetComponent<CardBaseFunctionning>().UnselectCard();
                        hud.CardUnselection();
                        CurrentCardSelect = null;
                        return; 
                    }

                }

                if (PoisonChoice)
                {
                    if (CurrentCardSelect.IsChildOf(handFirstLine.transform) || CurrentCardSelect.IsChildOf(handSecondLine.transform))
                    {
                        CurrentCardSelect.GetComponent<CardBaseFunctionning>().OutCard();
                        PoisonChoice = false;
                        gameObject.GetComponent<CardPlayerNet>().CanPassTurn(true);
                    }
                }
                else
                {
                    if (CurrentCardSelect.IsChildOf(handFirstLine.transform) || CurrentCardSelect.IsChildOf(handSecondLine.transform))
                    {
                        CurrentCardSelect.GetComponent<CardBaseFunctionning>().SelectCard();
                        tryPlayCard(CurrentCardSelect);
                    }
                    else if (CurrentCardSelect.IsChildOf(ObjectSlot))
                    {
                        hud.CardSelection(HudManager.SelectionTiming.ObjectToActivate, CurrentCardSelect, cardRespondingTo, isResponding, gameObject.GetComponent<CardPlayerNet>().GetPlayerCamera());
                    }

                }
            }
            else
            {
                if (Physics.Raycast(cast, out hit))
                {
                    if (OnTableView )
                        return;

                    if (hit.collider == null)
                        return;

                    if (!hit.collider.gameObject.GetComponent<CardBaseFunctionning>())
                        return;


                    if (hit.collider.gameObject.GetComponent<CardBaseFunctionning>().GetPlayerOwner() != gameObject)
                        return;

                    CurrentCardSelect = hit.collider.gameObject.transform;

                    if (PoisonChoice)
                    {
                        if (CurrentCardSelect.IsChildOf(handFirstLine.transform) || CurrentCardSelect.IsChildOf(handSecondLine.transform))
                        {
                            CurrentCardSelect.GetComponent<CardBaseFunctionning>().OutCard();
                            PoisonChoice = false;
                            gameObject.GetComponent<CardPlayerNet>().CanPassTurn(true);
                            CurrentCardSelect = null;

                        }
                    }
                    else
                    {
                        if (CurrentCardSelect.IsChildOf(handFirstLine.transform) || CurrentCardSelect.IsChildOf(handSecondLine.transform))
                        {
                            CurrentCardSelect.GetComponent<CardBaseFunctionning>().SelectCard();
                            tryPlayCard(CurrentCardSelect);
                        }
                        else if (CurrentCardSelect.IsChildOf(ObjectSlot))
                        {
                            hud.CardSelection(HudManager.SelectionTiming.ObjectToActivate, CurrentCardSelect, cardRespondingTo, isResponding, gameObject.GetComponent<CardPlayerNet>().GetPlayerCamera());
                        }

                    }

                }
            }
            
        }
        else
        {
            if(Physics.Raycast(cast, out hit))
            {
                if (hit.collider == null)
                    return;
              
                if (!hit.collider.gameObject.GetComponent<CardBaseFunctionning>())
                    return;


                if (hit.collider.gameObject.GetComponent<CardBaseFunctionning>().GetPlayerOwner() != gameObject)
                    return;


                if (CurrentCardSelect != null && CurrentCardSelect != hit.collider.gameObject)
                    return;


                CurrentCardSelect = hit.collider.gameObject.transform;

            }

        }
    }

    private void screentouchPosition(InputAction.CallbackContext context)
    {
        screenPointerPosition = context.action.ReadValue<Vector2>();
    }

    private void tryPlayCard(Transform CardToPlay)
    {
        CardBaseFunctionning currentCard = CardToPlay.GetComponent<CardBaseFunctionning>();
        CardPlayerNet currentPlayer = gameObject.GetComponent<CardPlayerNet>();
        GameObject[] players = GameMaster.Instance.GetPlayers();

        if(gameObject.GetComponent<CardPlayerNet>().GetPlayerState().IsPoisonned && handFirstLine.transform.childCount < 2)
            return;


        if (currentCard.GetCardData().CardFamily == CardType.Work && !CardWorkPlayed && !gameObject.GetComponent<CardPlayerNet>().GetPlayerState().IsAngry)
        {
            if (currentPlayer.GetPlayerState().WorkIsfreezed || isResponding)
                return;

            if (WorkSlot.childCount > 0)
            {
                for (int i = 0; i < WorkSlot.childCount; i++)
                {
                    WorkCardBase currentWorkPlayed = WorkSlot.GetChild(i).GetComponent<WorkCardBase>();

                    if(currentWorkPlayed.GetCardData().id == currentCard.GetCardData().id)
                    {
                        hud.CardUnselection();
                        break;
                    }

                    for (int j = 0; j < currentWorkPlayed.GetWorkCardPlayable().Length; j++)
                    {
                        if (currentCard.GetCardData().id == currentWorkPlayed.GetWorkCardPlayable()[j])
                        {
                            hud.CardSelection(HudManager.SelectionTiming.CardPlayable, CardToPlay, gameObject.GetComponent<CardPlayerNet>().GetPlayerCamera(), currentCard.PlayCard);

                        }

                    }
                }
            }
            else
            {
                hud.CardSelection(HudManager.SelectionTiming.CardPlayable, CardToPlay, gameObject.GetComponent<CardPlayerNet>().GetPlayerCamera(), currentCard.PlayCard);
            }
        }
        else if (currentCard.GetCardData().CardFamily == CardType.Crime && !CardCrimePlayed && !gameObject.GetComponent<CardPlayerNet>().GetPlayerState().IsAngry)
        {
            if (isResponding)
                return;

            foreach (GameObject player in players)
            {
                if (player.GetComponent<CardPlayerNet>().GetPlayerState().IsOverWatching)
                {
                    Debug.Log("Can make an wearning if someone overwatch");
                }
            }

            if (currentCard.GetCardData().id == 23)
            {
                foreach (GameObject player in players)
                {
                    if (player.GetComponent<CardPlayerNet>().GetPlayerState().IsCounting)
                    {
                        return;
                    }
                }
            }

            hud.CardSelection(HudManager.SelectionTiming.CardPlayable, CardToPlay, gameObject.GetComponent<CardPlayerNet>().GetPlayerCamera(), currentCard.PlayCard);
        }
        else if (currentCard.GetCardData().CardFamily == CardType.Agression && !CardAggressionPlayed)
        {
            if (isResponding)
                return;

            foreach (GameObject player in players)
            {
                if (player.GetComponent<CardPlayerNet>().GetPlayerState().IsOverWatching)
                {
                    Debug.Log("Can make an wearning if someone overwatch");
                }
            }
            hud.CardSelection(HudManager.SelectionTiming.CardPlayable, CardToPlay, gameObject.GetComponent<CardPlayerNet>().GetPlayerCamera(), currentCard.PlayCard);

        }
        else if (currentCard.GetCardData().CardFamily == CardType.Object && !CardObjectPlayed && !gameObject.GetComponent<CardPlayerNet>().GetPlayerState().IsAngry)
        {
            if (isResponding)
                return;

            hud.CardSelection(HudManager.SelectionTiming.CardPlayable, CardToPlay, gameObject.GetComponent<CardPlayerNet>().GetPlayerCamera(), currentCard.PlayCard);

        }
        else if (currentCard.GetCardData().CardFamily == CardType.Trick && !CardTrickPlayed && !gameObject.GetComponent<CardPlayerNet>().GetPlayerState().IsAngry || currentCard.GetCardData().CardFamily == CardType.Event && !CardTrickPlayed && !gameObject.GetComponent<CardPlayerNet>().GetPlayerState().IsAngry)
        {
            if (!isResponding && currentCard.IsProtection())
            {
                return;
            }
            else if (isResponding && (isGardAlert || cardRespondingTo))
            {
                if (!currentCard.IsProtection())
                    return;

                if (!cardRespondingTo)
                {
                    if (currentCard.GetCardData().id != 34)
                        return;
                }
                else
                {
                    if (cardRespondingTo.GetCardData().CardFamily == CardType.Agression && currentCard.GetCardData().id != 33)
                        return;
                    else if (cardRespondingTo.GetCardData().CardFamily == CardType.Event && currentCard.GetCardData().id != 38)
                        return;
                }
            }
            else if (isResponding && !isGardAlert && !cardRespondingTo)
            {
                if (currentCard.IsProtection())
                    return;
            }

            if (currentCard.GetCardData().id == 32 && gameObject.GetComponent<CardPlayerNet>().GetCurrentCoins() < 6)
                return;

            if (currentCard.GetCardData().id == 35 || currentCard.GetCardData().id == 36)
            {
                foreach (var player in GameMaster.Instance.GetPlayers())
                {
                    if(player.GetComponent<CardPlayerNet>().IsMyTurnToPlay())
                    {
                        Debug.Log("personnal log : Character " + player.GetComponent<CharacterFunctionnality>().GetPlayerCharacter() + " has played crime card " + player.GetComponent<PlayerHand>().IsCrimeCardPlayed());
                        if (!player.GetComponent<PlayerHand>().IsCrimeCardPlayed())
                            return;
                    }
                }
            }

            hud.CardSelection(HudManager.SelectionTiming.CardPlayable, CardToPlay, gameObject.GetComponent<CardPlayerNet>().GetPlayerCamera(), currentCard.PlayCard);
        }
    }

    public  void ShowTutoResponseButton()
    {
        if (!IsOwner)
            return;

        hud.ShowHideInterruptionButton(true);
        hud.GetInteruptionButton().onClick.AddListener(() =>
        {
            TutoResponce(true);
        });
    }

    public void TutoResponce(bool responding)
    {
        if (!IsOwner)
            return;

        if (responding)
        {
            hud.ShowHideInterruptionButton(!responding);

            hud.GetResponseButton().onClick.AddListener(() =>
            {
                TutoResponce(false);
            });
        }
        else
        { 
            hud.GetResponseButton().onClick.RemoveAllListeners();
            if(cardRespondingTo != null)
            {
                cardRespondingTo.ActivateCard();
                cardRespondingTo = null;
            }

            if (gameObject.GetComponent<CardPlayerNet>().IsMyTurnToPlay())
                hud.showHud();
            else
                hud.ShowHideInterruptionButton(!responding);

            if (isGardAlert)
            {
                isGardAlert = false;
                GardManager.Instance.ResponceReceivedRpc(gameObject.GetComponent<NetworkObject>());
            }
        }

        hud.ShowHideResponceButton(responding);

        isResponding = responding;
        gameObject.GetComponent<CardPlayerNet>().EnableCharacterCapacity(responding);

        CardTrickPlayed = false;

        foreach (var player in GameMaster.Instance.GetPlayers())
        {
            player.GetComponent<PlayerHand>().SomeoneRespondingRpc(gameObject.GetComponent<NetworkObject>(), responding);
        }
    }
    public void TutoResponce( CardBaseFunctionning CardAiming)
    {
        if (!IsOwner)
            return;

        hud.GetResponseButton().onClick.AddListener(() =>
        {
            TutoResponce(false);
        });

        cardRespondingTo = CardAiming;

        hud.ShowHideResponceButton(true);

        isResponding = true;
        gameObject.GetComponent<CardPlayerNet>().EnableCharacterCapacity(true);

        CardTrickPlayed = false;

        foreach (var player in GameMaster.Instance.GetPlayers())
        {
            player.GetComponent<PlayerHand>().SomeoneRespondingRpc(gameObject.GetComponent<NetworkObject>(), true);
        }
    }

    public void TutoResponce()
    {
        if (!IsOwner)
            return;

        hud.GetResponseButton().onClick.AddListener(() =>
        {
            TutoResponce(false);
        });

        isGardAlert = true;

        hud.ShowHideResponceButton(true);

        isResponding = true;
        gameObject.GetComponent<CardPlayerNet>().EnableCharacterCapacity(true);

        CardTrickPlayed = false;

        foreach (var player in GameMaster.Instance.GetPlayers())
        {
            player.GetComponent<PlayerHand>().SomeoneRespondingRpc(gameObject.GetComponent<NetworkObject>(), true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SomeoneRespondingRpc(NetworkObjectReference playerResponding, bool HaveToWait)
    {
        if (!IsOwner)
            return;
        GameObject CurrentResponse = null;

        if (playerResponding.TryGet(out NetworkObject playerObject))
            CurrentResponse = playerObject.gameObject;

        if (CurrentResponse == gameObject)
            return;

        if (isResponding)
            return;

        if (HaveToWait)
            hud.hideHud();
        else
        {
            if (gameObject.GetComponent<CardPlayerNet>().IsMyTurnToPlay())
                hud.showHud();
            else
                hud.ShowHideInterruptionButton(!HaveToWait);
        }

            isWaiting = HaveToWait; 
    }

    public void TutoMakeChoice(string choiceOne, string choiceTwo, UnityAction ActionOne, UnityAction ActionTwo)
    {
        if (!IsOwner)
            return;

        foreach (var player in GameMaster.Instance.GetPlayers())
        {
            player.GetComponent<PlayerHand>().SomeoneRespondingRpc(gameObject.GetComponent<NetworkObject>(), true);
        }

        hud.hideHud();

        hud.binaryChoice(choiceOne, choiceTwo, ActionOne, ActionTwo, ChoiceMade);
    }

    public void ChoiceMade()
    {
        if (!IsOwner)
            return;

        foreach (var player in GameMaster.Instance.GetPlayers())
        {
            player.GetComponent<PlayerHand>().SomeoneRespondingRpc(gameObject.GetComponent<NetworkObject>(), false);
        }

        if (gameObject.GetComponent<CardPlayerNet>().IsMyTurnToPlay())
            hud.showHud();
        else
            hud.ShowHideInterruptionButton(true);
    }


    public void OverWatchCost(GameObject player)
    {
        CardPlayerNet currentPlayer = gameObject.GetComponent<CardPlayerNet>();
        GameObject[] players = GameMaster.Instance.GetPlayers();

        currentPlayer.decremenGoldRpc(1);
        StartCoroutine(currentPlayer.SendCoins(1, player.GetComponent<CardPlayerNet>().GetCoinsSpawnPoint()));
        player.GetComponent<CardPlayerNet>().IncrementGoldRpc(1);
    }

    public void PoisonChoiceActivation()
    {
        PoisonChoice = true;
        gameObject.GetComponent<CardPlayerNet>().CanPassTurn(false);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void CardPlayedRpc(CardType type)
    {
        CurrentCardSelect = null;

        if (IsOwner)
            gameObject.GetComponent<CardPlayerNet>().CanPassTurn(true);

        if (type == CardType.Work)
            CardWorkPlayed = true;
        else if (type == CardType.Agression)
        {
            if (!gameObject.GetComponent<CardPlayerNet>().GetPlayerState().IsAngry)
                CardAggressionPlayed = true;
        }
        else if (type == CardType.Crime)
            CardCrimePlayed = true;
        else if (type == CardType.Trick || type == CardType.Event)
            CardTrickPlayed = true;
        else if (type == CardType.Object)
            CardObjectPlayed = true;
    }

    public void Humiliation()
    {
        if (!WorkSlot)
            return;

        for (int i = 0; i < WorkSlot.childCount; i++)
        {
            WorkSlot.GetChild(0).GetComponent<WorkCardBase>().OutCard();
        }
    }

    public void intimidation()
    {
        if (!WorkSlot)
            return;

        for (int i = 0; i < WorkSlot.childCount; i++)
        {
            WorkSlot.GetChild(0).GetComponent<WorkCardBase>().Intimidate();
        }
    }

    public void FlushCrimeCards()
    {
        if(!IsOwner)
            return;

        if (!CrimeSlot)
            return;

        for (int i = 0;i < CrimeSlot.childCount;i++)
        {
            if (CrimeSlot.GetChild(i).GetComponent<CrimeCardBase>().GetCardData().id == 25)
                gameObject.GetComponent<CardPlayerNet>().IsAKiller();

            CrimeSlot.GetChild(i).GetComponent<CrimeCardBase>().OutCard();
        }
    }
    public void FlushObjectCards()
    {
        if (!ObjectSlot)
            return;

        for (int i = 0;i < ObjectSlot.childCount;i++)
        {
            ObjectSlot.GetChild(i).GetComponent<CrimeCardBase>().OutCard();
        }
    }

    public void FlushAggressionCards()
    {
        if (!handFirstLine || !handSecondLine)
            return;

        for (int i = 0;i < handFirstLine.transform.childCount;i++)
        {
            if (handFirstLine.transform.GetChild(i).GetComponent<CardBaseFunctionning>().GetCardData().CardFamily == CardType.Agression)
                handFirstLine.transform.GetChild(i).GetComponent<CardBaseFunctionning>().OutCard();
        }

        for (int i = 0;i < handSecondLine.transform.childCount;i++)
        {
            if (handSecondLine.transform.GetChild(i).GetComponent<CardBaseFunctionning>().GetCardData().CardFamily == CardType.Agression)
                handSecondLine.transform.GetChild(i).GetComponent<CardBaseFunctionning>().OutCard();
        }
    }


    public void NewTurnBegin()
    {
        if (!IsOwner)
        {
            return;
        }

        CardWorkPlayed = false;
        CardTrickPlayed = false;
        CardCrimePlayed = false;
        CardAggressionPlayed = false;
        CardObjectPlayed = false;
        
        isWaiting = false;
        isResponding = false;   

        pickCard();

        if (ObjectSlot.childCount > 0)
        {
            for(int i = 0; i < ObjectSlot.childCount; i++)
                ObjectSlot.GetChild(i).GetComponent<CardBaseFunctionning>().ATurnPassed();
        }

        foreach(var player in GameMaster.Instance.GetPlayers())
        {
            if (player.GetComponent<CardPlayerNet>().GetPlayerState().TakeBath)
                return;
        }

        if(WorkSlot.childCount>0)
        {
            for (int i = 0; i < WorkSlot.childCount; i++)
                WorkSlot.GetChild(i).GetComponent<CardBaseFunctionning>().ATurnPassed();
        }

        if (CrimeSlot.childCount>0)
        {
            for (int i = 0;i < CrimeSlot.childCount;i++)
                CrimeSlot.GetChild(i).GetComponent<CardBaseFunctionning>().ATurnPassed();
        }

    }

    private void Start()
    {
        deck = DeckManager.deckInstance;
        hud = HudManager.instance;
    }
    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            CanvasHovered = true;
        }
        else 
            CanvasHovered = false;
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }

        SpawnPlayerHandlinesRpc(gameObject.GetComponent<NetworkObject>());

        StartCoroutine(PickBeginCards());
    }

    public IEnumerator PickBeginCards()
    {

        for (int i = 0; i < NumberOfCardToBegin; i++)
        {
            yield return new WaitForSeconds(1);

            Transform currentPick = EventBeginSecurity(deck.PickUpCard(), 0);

            CardBaseFunctionning currentCardPicked = currentPick.GetComponent<CardBaseFunctionning>();

            currentCardPicked.SetPlayerOwner(gameObject);
            currentCardPicked.DrawCard();

            NetworkObjectReference CardRef = currentCardPicked.transform.GetComponent<NetworkObject>();
            CardInHandRpc(CardRef);
        }

        FlushBeginSecurity();
    }

    private void FlushBeginSecurity()
    {

        if (EventBeginPick.Count > 0)
        {
            for (int i = 0; i < EventBeginPick.Count; i++)
            {
                deck.insertCardRpc(EventBeginPick[0].GetComponent<NetworkObject>());
                EventBeginPick.Remove(EventBeginPick[0]);
            }

            deck.ShufleDeckRpc();
        }
    }

    private Transform EventBeginSecurity(Transform CardPick, int Reroll)
    {
        CardBaseFunctionning currentCardPicked = CardPick.GetComponent<CardBaseFunctionning>();

        if(currentCardPicked.GetCardData().CardFamily == CardType.Event)
        {
            EventBeginPick.Add(CardPick);
            return EventBeginSecurity(deck.PickUpNextCard(Reroll += 1), Reroll += 1);
        }

        return CardPick;
    }

    public void pickCard()
    {
        Transform currentPick = deck.PickUpCard();

        CardBaseFunctionning currentCardPicked = currentPick.GetComponent<CardBaseFunctionning>();

        currentCardPicked.SetPlayerOwner(gameObject);
        currentCardPicked.DrawCard();

        NetworkObjectReference CardRef = currentPick.GetComponent<NetworkObject>();
        CardInHandRpc(CardRef);
    }

   
    public void SetWorkSlot(Transform slot)
    {
        WorkSlot = slot;
    }

    public Transform GetWorkSlot()
    {
        return WorkSlot;
    }
    public void SetObjectSlot(Transform slot)
    {
        ObjectSlot = slot;
    }
    public Transform GetObjectSlot()
    {
        return ObjectSlot;
    }
    public void SetCrimeSlot(Transform slot)
    {
        CrimeSlot = slot;   
    }
    public Transform GetCrimeSlot()
    {
        return CrimeSlot;
    }
    public void SetDeckOutSlot(Transform slot)
    {
        deckOut = slot;
    }
    public Transform GetDeckOutSlot()
    {
        return deckOut;
    }

    public CardBaseFunctionning GetCardAttacker()
    {
        return cardRespondingTo;
    }

    public bool IsCrimeCardPlayed()
    {
        return CardCrimePlayed;
    }

    [Rpc(SendTo.Server)]
    public void CardInHandRpc(NetworkObjectReference Pick, RpcParams netParams = default)
    {
        if(Pick.TryGet(out NetworkObject CardNetwork))
        {
            CardNetwork.gameObject.GetComponent<CardBaseFunctionning>().SetPlayerOwner(Player);

            if (handFirstLine.transform.childCount < 8)
            {
                CardNetwork.transform.SetParent(handFirstLine.transform);
                CardNetwork.transform.localPosition = Vector3.zero;
                CardNetwork.transform.localRotation = new Quaternion(0, 0, 0, 0);
            }
            else
            {
                CardNetwork.transform.SetParent(handSecondLine.transform);
                CardNetwork.transform.localPosition = Vector3.zero;
                CardNetwork.transform.localRotation = new Quaternion(0, 0, 0, 0);
            }
        }
       
    }
    [Rpc(SendTo.Server)]
    public void SpawnPlayerHandlinesRpc(NetworkObjectReference Sender)
    {
        handFirstLine = Instantiate(handPrefab);
        handSecondLine = Instantiate(handPrefab);

        handFirstLine.GetComponent<NetworkObject>().Spawn(true);
        handFirstLine.transform.SetParent(Player.transform);
        handFirstLine.transform.localPosition = FirstHandLocation.localPosition;
        handFirstLine.transform.localRotation = new Quaternion(0, 0, 0, 0);

        handSecondLine.GetComponent<NetworkObject>().Spawn(true);
        handSecondLine.transform.SetParent(Player.transform);
        handSecondLine.transform.localPosition = SecondHandLocation.localPosition;
        handSecondLine.transform.localRotation = new Quaternion(0, 0, 0, 0);

        if (Sender.TryGet(out NetworkObject PlayerNet))
            PlayerNet.GetComponent<PlayerHand>().SetPlayerHandlinesClientRpc(Sender, handFirstLine, handSecondLine);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetPlayerHandlinesClientRpc(NetworkObjectReference Sender, NetworkObjectReference HandFL, NetworkObjectReference HandSL)
    {

        if (Sender.TryGet(out NetworkObject PlayerNet))
            if (PlayerNet.gameObject != gameObject)
                return;


        if (HandFL.TryGet(out NetworkObject HandFLNet))
            handFirstLine = HandFLNet.gameObject;


        if (HandSL.TryGet(out NetworkObject HandSLNet))
            handSecondLine = HandSLNet.gameObject;


    }

    [Rpc(SendTo.Server)]
    public void MoveHandTableSelectedRpc()
    {
        handFirstLine.transform.localPosition = FirstHandTableView.localPosition;
        handSecondLine.transform.localPosition = SecondHandTableView.localPosition;
    }

    [Rpc(SendTo.Server)]
    public void MoveHandTableUnselectedRpc()
    {
        handFirstLine.transform.localPosition = FirstHandLocation.localPosition;
        handSecondLine.transform.localPosition = SecondHandLocation.localPosition;
    }
}
