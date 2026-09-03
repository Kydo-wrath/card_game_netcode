using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardPlayerNet : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<bool> IsTurnToPlay = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private NetworkVariable<int> CurrentCoins = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private NetworkVariable<PlayerState> playerState = new NetworkVariable<PlayerState>(new PlayerState
    {
        IsInjured = false,
        IsAngry = false,
        IsPoisonned = false,
        IsCharmed = false,
        PlayerCharmerId  = 0,
        WorkIsfreezed = false,
        IsOverWatching = false,
        IsWatchingBank = false,
        IsCounting = false,
        HaveToPass = false,
        TakeBath = false,
        IsMasked = false,
        IsHealed = false,
        Inflationned = false,
        IsAggressionProtected = false,
        IsTrickProtected = false,
        IsEventProtected = false,
        IsGardProtected = false,
        IsCrimeProtected = false

    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private Camera PlayerCamrera;
    [SerializeField] private GameObject CoinPrefab;
    [SerializeField] private Transform CoinSpawnPoint;

    [SerializeField] private int AmountBeginCoins;
    [SerializeField] private int AmountOfDrawableCards;

    [SerializeField] private Button NextTurnButton;
    [SerializeField] private Button BuyCardButton;
    [SerializeField] private Button TradeCardButton;
    [SerializeField] private Button CharacterCapacityOne;
    [SerializeField] private Button CharacterCapacityTwo;

    private GameObject CurrentCoinToSpawn;
    private bool AlreadySeat;
    private bool isBeerPoisoned;

    private int currentAmountCardDraw;
    private int NumberOFTurnInPrison;
    private int NumberOFTurnCharmed;


    private void Awake()
    {

    }

    public event EventHandler<OnPlayerEndTurnsArgs> OnPlayerEndTurn;

    public class OnPlayerEndTurnsArgs : EventArgs
    {
        public GameObject player;

    }
    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
        {
            PlayerCamrera.gameObject.SetActive(false);
        }
        else
        {
            PlayerCamrera.gameObject.SetActive(true);
        }
        if (!IsOwner)
            return;

        NextTurnButton = HudManager.instance.GetPassButton();
        BuyCardButton = HudManager.instance.GetDrawButton();
        TradeCardButton = HudManager.instance.GetTradeButton();
        CharacterCapacityOne = HudManager.instance.GetCapacityOneButton();
        CharacterCapacityTwo = HudManager.instance.GetCapacityTwoButton();

        HudManager.instance.OnTradeOfferSend += HudManager_OnTradeOfferSend;
        GardManager.Instance.OnAlertActivate += GardManager_OnAlertActivate;

        NextTurnButton.onClick.AddListener(EndTurn);
        BuyCardButton.onClick.AddListener(BuyCard);
        TradeCardButton.onClick.AddListener(AskForATrade);
        CharacterCapacityOne.onClick.AddListener(() =>
        {
            gameObject.GetComponent<CharacterFunctionnality>().UseFirstCapacity();
            CharacterCapacityOne.interactable = false;
        });

        CharacterCapacityTwo.onClick.AddListener(() =>
        {
            gameObject.GetComponent<CharacterFunctionnality>().UseSecondCapacity();
            CharacterCapacityTwo.interactable = false;  
        });

        EnableCharacterCapacity(false);

        HudManager.instance.hideHud();

        IncrementGoldRpc(AmountBeginCoins);
        RequestSeat();
    }

    private void GardManager_OnAlertActivate(object sender, EventArgs e)
    {
        if (GardManager.Instance.getCurrentAlertLevel() < 12 && playerState.Value.IsEventProtected)
        {
            GardManager.Instance.ResponceReceivedRpc(gameObject.GetComponent<NetworkObject>());

            DisablePlayerStateRpc(SideEffect.AvoidEvent);
            return;
        }

        gameObject.GetComponent<PlayerHand>().TutoResponce();
    }

    public void GardAlertEffect()
    {
        if (playerState.Value.IsGardProtected)
            return;

       

        if (!gameObject.GetComponent<PlayerHand>().GetCrimeSlot())
            return;
        
        int CrimeGold = 0;

        if (!playerState.Value.Inflationned)
            CrimeGold = gameObject.GetComponent<PlayerHand>().GetCrimeSlot().childCount * 2;
        else
            CrimeGold = gameObject.GetComponent<PlayerHand>().GetCrimeSlot().childCount * 3;

        decremenGoldRpc(CrimeGold);

        StartCoroutine(DeleteCoins(CrimeGold));

        gameObject.GetComponent<PlayerHand>().FlushCrimeCards();

    }

    public void IsAKiller()
    {
        NumberOFTurnInPrison = 12;
    }

    public void AddTargetablePlayer(Button TargetButton)
    {
        if (gameObject.GetComponent<CharacterFunctionnality>().GetPlayerCharacter() == Character.none)
        {
            StartCoroutine(ActivateSlotButtonNextTick(TargetButton));
        }
        else
            HudManager.instance.ActivateSlotButton(TargetButton, gameObject.GetComponent<CharacterFunctionnality>().GetplayerCharacterIcone(), gameObject);
    }

    private IEnumerator ActivateSlotButtonNextTick(Button targetButton)
    {
        yield return null; 

        AddTargetablePlayer(targetButton);
    }
    public void CanPassTurn(bool CanPass)
    {
        NextTurnButton.interactable = CanPass;
    }

    private void HudManager_OnTradeOfferSend(object sender, HudManager.OnTradeOfferSendArgs e)
    {
        onTradeOfferSendRpc(e.TypeToTrade, e.priceToTrade);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void onTradeOfferSendRpc(CardType typeTrade , int priceTrade)
    {
        HudManager.instance.ReceiveTradeOffer(typeTrade, priceTrade);
    }
    private async void RequestSeat()
    {
        await Task.Yield();

        RequestPlayerseatRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RequestPlayerseatRpc()
    {

        GameMaster.Instance.ReturnOpenSeat().PlayerTakeSeatRpc(gameObject.GetComponent<NetworkObject>());
        AlreadySeat = true;

        Vector3 direction = FindAnyObjectByType<LookAtPointer>().GetLookAtPoint() - transform.position;
        transform.rotation = Quaternion.LookRotation(direction);

        if (!IsServer)
            return;
       
        StartCoroutine(CreateCoins(AmountBeginCoins));
    }

    public IEnumerator CreateCoins(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            yield return new WaitForSeconds(1);

            SpawnCoinsRpc();
        }
    }

    [Rpc(SendTo.Server)]
    public void SpawnCoinsRpc(RpcParams netParams = default)
    {

        CurrentCoinToSpawn = Instantiate(CoinPrefab, CoinSpawnPoint.position, UnityEngine.Random.rotation);
        CurrentCoinToSpawn.GetComponent<NetworkObject>().Spawn(true);
        CurrentCoinToSpawn.transform.SetParent(CoinSpawnPoint);
        CurrentCoinToSpawn = null;
    }

    private void EndTurn()
    {
        EndTurnRpc(gameObject.GetComponent<NetworkObject>());

        EnableCharacterCapacity(false);

        if(isBeerPoisoned)
            isBeerPoisoned = false;

        HudManager.instance.hideHud();
    }

    private void BuyCard()
    {
        if (!playerState.Value.Inflationned)
        {
            if (currentAmountCardDraw >= AmountOfDrawableCards || CurrentCoins.Value < 1)
                return;

            foreach (GameObject player in GameMaster.Instance.GetPlayers())
            {
                if (player.GetComponent<CardPlayerNet>().GetPlayerState().TakeBath)
                {
                    isBeerPoisoned = true;
                }
            }

            CanPassTurn(true);

            decremenGoldRpc(1);

            currentAmountCardDraw++;

            GameObject bank = GameMaster.Instance.Getbank();

            bank.GetComponent<BankManager>().IncrementCoinsRpc(1);

            gameObject.GetComponent<PlayerHand>().pickCard();

            StartCoroutine(SendCoins(1, bank.transform));

            if (currentAmountCardDraw >= AmountOfDrawableCards || CurrentCoins.Value < 1)
                BuyCardButton.interactable = false;
        }
        else
        {
            if (currentAmountCardDraw >= AmountOfDrawableCards || CurrentCoins.Value < 2)
                return;

            foreach (GameObject player in GameMaster.Instance.GetPlayers())
            {
                if (player.GetComponent<CardPlayerNet>().GetPlayerState().TakeBath)
                {
                    isBeerPoisoned = true;
                }
            }

            CanPassTurn(true);

            decremenGoldRpc(2);

            currentAmountCardDraw++;

            GameObject bank = GameMaster.Instance.Getbank();

            bank.GetComponent<BankManager>().IncrementCoinsRpc(2);

            gameObject.GetComponent<PlayerHand>().pickCard();

            StartCoroutine(SendCoins(2, bank.transform));

            if (currentAmountCardDraw >= AmountOfDrawableCards || CurrentCoins.Value < 2)
                BuyCardButton.interactable = false;
        }
        
    }

    public IEnumerator SendCoins(int numberOfCoinsToSent, Transform PositionToSend)
    {
        for(int i = 0; i < numberOfCoinsToSent; i++)
        {
            yield return new WaitForSeconds(1);

            NetworkObjectReference transformRef = PositionToSend.GetComponent<NetworkObject>();
            CoinSenderRpc(transformRef);
        }

    }
    [Rpc(SendTo.Server)]
    public void CoinSenderRpc(NetworkObjectReference Ref, RpcParams netParams = default)
    {
        if (Ref.TryGet(out NetworkObject TransformNet))
        {
            if (CoinSpawnPoint.childCount <= 0)
                return;

            Transform coin = CoinSpawnPoint.GetChild(0);
            coin.SetParent(null);
            coin.SetParent(TransformNet.transform);
            coin.localPosition = Vector3.zero; 
        }

    }
    public IEnumerator DeleteCoins(int numberOfCoinsToDelete)
    {
        for(int i = 0; i < numberOfCoinsToDelete; i++)
        {
            yield return new WaitForSeconds(1);

            DeleteSenderRpc();
        }

    }
    [Rpc(SendTo.Server)]
    public void DeleteSenderRpc( RpcParams netParams = default)
    {
        if (CoinSpawnPoint.childCount <= 0)
            return;

        Transform coin = CoinSpawnPoint.GetChild(0);
        coin.GetComponent<NetworkObject>().Despawn();
    }
    private void AskForATrade()
    {
        HudManager.instance.ShowHideTradeSenderUi(true);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EndTurnRpc(NetworkObjectReference player, RpcParams Sender = default)
    {
        NetworkObject playerNetworkObject = null;

        if (player.TryGet(out NetworkObject playerObject))
            playerNetworkObject = playerObject;

        if (IsHost)
        {
            IsTurnToPlay.Value = false;
            if (playerNetworkObject.gameObject.GetComponent<CardPlayerNet>().GetPlayerState().IsAngry)
                playerNetworkObject.gameObject.GetComponent<CardPlayerNet>().DisablePlayerStateRpc(TargetStat.angry);
            else if (playerNetworkObject.gameObject.GetComponent<CardPlayerNet>().GetPlayerState().IsAngry)
            {
                NumberOFTurnCharmed += 1;

                if(NumberOFTurnCharmed > 2 )
                {
                    NumberOFTurnCharmed = 0;
                    playerNetworkObject.gameObject.GetComponent<CardPlayerNet>().DisablePlayerStateRpc(TargetStat.charmed);
                }
            }
        }

        OnPlayerEndTurn?.Invoke(this, new OnPlayerEndTurnsArgs
        {
            player = playerNetworkObject.gameObject
        });
    }

    public void BeginTurn ()
    {
        if (!IsOwner)
            return;

        currentAmountCardDraw = 0;

        if(!playerState.Value.Inflationned)
        {
            if (currentAmountCardDraw >= AmountOfDrawableCards || CurrentCoins.Value < 1)
                BuyCardButton.interactable = false;
            else
                BuyCardButton.interactable = true;
        }
        else
        {
            if (currentAmountCardDraw >= AmountOfDrawableCards || CurrentCoins.Value < 2)
                BuyCardButton.interactable = false;
            else
                BuyCardButton.interactable = true;
        }


        HudManager.instance.showHud();
        HudManager.instance.ShowHideInterruptionButton(false);

        CanPassTurn(false);

        BeginTurnRpc(gameObject.GetComponent<NetworkObject>());

        EnableCharacterCapacity(true);

        gameObject.GetComponent<PlayerHand>().NewTurnBegin();

        if (playerState.Value.TakeBath)
            DisablePlayerStateRpc(SideEffect.takeBath);

        if (playerState.Value.HaveToPass|| NumberOFTurnInPrison>0)
        {
            EndTurn();

            EnableCharacterCapacity(false);

            if (playerState.Value.HaveToPass)
                DisablePlayerStateRpc(SideEffect.passNextTurn);

            if(NumberOFTurnInPrison > 0)
                NumberOFTurnInPrison--;
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    public void BeginTurnRpc(NetworkObjectReference player, RpcParams sender = default)
    {
        if (IsHost)
            IsTurnToPlay.Value = true;

        GameObject CurrentBeginner = null;

        if (player.TryGet(out NetworkObject playerObject))
            CurrentBeginner = playerObject.gameObject;
        
        if (CurrentBeginner == gameObject && IsOwner)
            return;


        foreach (var CardPlayer in GameMaster.Instance.GetPlayers())
        {
            if (CardPlayer != CurrentBeginner)
                CardPlayer.GetComponent<PlayerHand>().ShowTutoResponseButton();
        }

    }

    [Rpc(SendTo.Server)]
    public void IncrementGoldRpc(int goldAdded, RpcParams Sender = default)
    {
        CurrentCoins.Value += goldAdded;
    }
    [Rpc(SendTo.Server)]
    public void decremenGoldRpc(int goldSubstract, RpcParams Sender = default)
    {
        if (CurrentCoins.Value - goldSubstract > 0)
            CurrentCoins.Value -= goldSubstract;
        else
        {
            goldSubstract -= CurrentCoins.Value;
            CurrentCoins.Value = 0;
            NumberOFTurnInPrison += goldSubstract;
        }


    }

    public void SetCoinSpawnPoint(Transform SpawnPoint)
    {
        CoinSpawnPoint = SpawnPoint;    
    }

    public void EnableCharacterCapacity(bool enable)
    {
        if(!gameObject.GetComponent<CharacterFunctionnality>().IsFirstCapacityUsed())
            CharacterCapacityOne.interactable = enable;

        if (!gameObject.GetComponent<CharacterFunctionnality>().IsSecondCapacityUsed())
            CharacterCapacityTwo.interactable = enable;
    }

    [Rpc(SendTo.Server)]
    public void EnablePlayerStateRpc(TargetStat affliction, ulong playerCharmer = 0, RpcParams sender = default)
    {
        PlayerState currentState = playerState.Value;

        if (affliction == TargetStat.angry)
            currentState.IsAngry = true;
        else if (affliction == TargetStat.poisonned)
        {
            if (!playerState.Value.IsHealed)
                currentState.IsPoisonned = true;
        }
        else if (affliction == TargetStat.injured)
        { 
            currentState.IsInjured = true; 
        }
        else if (affliction == TargetStat.charmed)
        {
            currentState.IsCharmed = true;
            currentState.PlayerCharmerId = playerCharmer;
        }
        else if (affliction == TargetStat.humiliated)
            PlayerHumiliatedRpc(gameObject.GetComponent<NetworkObject>());

        playerState.Value = currentState;
    }

    [Rpc(SendTo.Server)]
    public void EnablePlayerStateRpc(SideEffect effect)
    {
        PlayerState currentState = playerState.Value;

        if (effect == SideEffect.FreezeWork)
            currentState.WorkIsfreezed = true;
        else if (effect == SideEffect.overwatch)
            currentState.IsOverWatching = true;
        else if (effect == SideEffect.watchBank)
            currentState.IsWatchingBank = true;
        else if (effect == SideEffect.counting)
            currentState.IsCounting = true;
        else if (effect == SideEffect.passNextTurn)
            currentState.HaveToPass = true;
        else if (effect == SideEffect.takeBath)
            currentState.TakeBath = true;
        else if (effect == SideEffect.masked)
            currentState.IsMasked = true;
        else if (effect == SideEffect.heal)
        {
            currentState.IsHealed = true;

            if (playerState.Value.IsPoisonned)
                DisablePlayerStateRpc(TargetStat.poisonned);

            if (playerState.Value.IsInjured)
                DisablePlayerStateRpc(TargetStat.injured);
        }
        else if (effect == SideEffect.intimidate)
            IntimidationRpc(gameObject.GetComponent<NetworkObject>());
        else if (effect == SideEffect.AvoidAggression)
            currentState.IsAggressionProtected = true;
        else if (effect == SideEffect.AvoidTrick)
            currentState.IsTrickProtected = true;
        else if (effect == SideEffect.AvoidEvent)
            currentState.IsEventProtected = true;
        else if (effect == SideEffect.AvoidInspection)
            currentState.IsGardProtected = true;
        else if (effect == SideEffect.AvoidCrimeEffect)
            currentState.IsCrimeProtected = true;

        playerState.Value = currentState;

    }

    [Rpc(SendTo.Server)]
    public void DisablePlayerStateRpc(TargetStat affliction)
    {
        PlayerState currentState = playerState.Value;

        if (affliction == TargetStat.angry)
            currentState.IsAngry = false;
        else if (affliction == TargetStat.poisonned)
            currentState.IsPoisonned = false;
        else if (affliction == TargetStat.injured)
            currentState.IsInjured = false;
        else if (affliction == TargetStat.charmed)
        {
            currentState.IsCharmed = false;
        }
        playerState.Value = currentState;

    }

    [Rpc(SendTo.Server)]
    public void DisablePlayerStateRpc(SideEffect effect)
    {
        PlayerState currentState = playerState.Value;

        if (effect == SideEffect.FreezeWork)
            currentState.WorkIsfreezed = false;
        else if (effect == SideEffect.overwatch)
            currentState.IsOverWatching = false;
        else if (effect == SideEffect.watchBank)
            currentState.IsWatchingBank = false;
        else if (effect == SideEffect.counting)
            currentState.IsCounting = false;
        else if (effect == SideEffect.passNextTurn)
            currentState.HaveToPass = false;
        else if (effect == SideEffect.takeBath)
            currentState.TakeBath = false;
        else if (effect == SideEffect.masked)
            currentState.IsMasked = false;
        else if (effect == SideEffect.heal)
            currentState.IsHealed = false;
        else if (effect == SideEffect.AvoidAggression)
            currentState.IsAggressionProtected = false;
        else if (effect == SideEffect.AvoidTrick)
            currentState.IsTrickProtected = false;
        else if (effect == SideEffect.AvoidEvent)
            currentState.IsEventProtected = false;
        else if (effect == SideEffect.AvoidInspection)
            currentState.IsGardProtected = false;
        else if (effect == SideEffect.AvoidCrimeEffect)
            currentState.IsCrimeProtected = false;

        playerState.Value = currentState;

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerHumiliatedRpc(NetworkObjectReference player, RpcParams sender = default)
    {
        GameObject playerNetworkObject = null;

        if (player.TryGet(out NetworkObject playerObject))
            playerNetworkObject = playerObject.gameObject;


        playerNetworkObject.GetComponent<PlayerHand>().Humiliation();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void IntimidationRpc(NetworkObjectReference player, RpcParams sender = default)
    {
        NetworkObject playerNetworkObject = null;

        if (player.TryGet(out NetworkObject playerObject))
            playerNetworkObject = playerObject;


        playerNetworkObject.GetComponent<PlayerHand>().intimidation();
    }

    public bool playerIsBeerPoisonned()
    {
        return isBeerPoisoned;
    }

    public bool IsAlreadySeat()
    {
        return AlreadySeat;
    }
    public bool IsMyTurnToPlay()
    {
        return IsTurnToPlay.Value;
    }

    public int GetCurrentCoins()
    {
        return CurrentCoins.Value;
    }

    public Camera GetPlayerCamera()
    {
        return PlayerCamrera;
    }

    public Transform GetCoinsSpawnPoint()
    {
        return CoinSpawnPoint;
    }
    public PlayerState GetPlayerState()
    {
        return playerState.Value;
    }
}
