using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public static HudManager instance;

    [SerializeField] private Button PassButton;
    [SerializeField] private Button DrawButton;
    [SerializeField] private Button TradeButton;
    [SerializeField] private Button characterCapacityOneButton;
    [SerializeField] private Button characterCapacityTwoButton;
    [SerializeField] private Button HostButton;
    [SerializeField] private Button ClientButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button ActivateButton;
    [SerializeField] private Button SellButton;
    [SerializeField] private Button ResponceButton;
    [SerializeField] private Button InteruptionButton;
    [SerializeField] private Button ChoicePropositionOne;
    [SerializeField] private Button ChoicePropositionTwo;

    [SerializeField] private GameObject TradeSenderUi;
    [SerializeField] private GameObject TradeOfferUi;
    [SerializeField] private GameObject PlayerTargetSelection;
    [SerializeField] private GameObject PlayerChoiceSelection;

    [SerializeField] private Transform CardUseSelection;

    [SerializeField] private TMP_Text PriceDisplay;
    [SerializeField] private TMP_Text offerPriceDisplay;
    [SerializeField] private TMP_Text alertLevelDisplay;
    [SerializeField] private TMP_Text TextChoicePropositionOne;
    [SerializeField] private TMP_Text TextChoicePropositionTwo;

    [SerializeField] private Image SelectionIcone;
    [SerializeField] private Image OfferIcone;
    [SerializeField] private Image CharacterIcone;

    [SerializeField] private Color WorkColor;
    [SerializeField] private Color AgressionColor;
    [SerializeField] private Color CrimeColor;
    [SerializeField] private Color TrickColor;
    [SerializeField] private Color ObjectColor;

    [SerializeField] private List<ButtonSlotPlayer> ButtonPlayers;

    [SerializeField] private string serverIp = "192.168.1.194";

    private CardType tradeTypeSelection;
    private int TradePrice;

    private CardObject cardSellSelection;

    private bool IsOfferSender;
    private bool HasAlreadyOffer;

    public enum SelectionTiming
    {
        None,
        CardPlayable,
        ObjectToActivate,
        SellingCard
    }

    [System.Serializable] 
    public class ButtonSlotPlayer
    {
        public Button button;
        public Image image;
        public GameObject player; 
    }

    public event EventHandler<OnTradeOfferSendArgs> OnTradeOfferSend;

    public class OnTradeOfferSendArgs : EventArgs
    {
        public CardType TypeToTrade;
        public int priceToTrade;
    }

    public event EventHandler<OnSellOfferSendArgs> OnSellOfferSend;

    public class OnSellOfferSendArgs : EventArgs
    {
        public Texture CardIllustation;
        public int CardPrice;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        HostButton.onClick.AddListener(StartHost);
        ClientButton.onClick.AddListener(StartClient);
    }

    public void StartHost()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        bool success = NetworkManager.Singleton.StartHost();

        HostButton.gameObject.SetActive(false);
        ClientButton.gameObject.SetActive(false);
    }

    public void StartClient()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetConnectionData(serverIp, 9000);

        bool success = NetworkManager.Singleton.StartClient();

        HostButton.gameObject.SetActive(false);
        ClientButton.gameObject.SetActive(false);
    }

    public void CardSelection(SelectionTiming Selection, Transform Card, Camera Cam, UnityAction PlayAction )
    {
        CardUseSelection.gameObject.SetActive(true);
        CardUseSelection.position = Cam.WorldToScreenPoint(Card.position);

        if (Selection == SelectionTiming.CardPlayable)
        {
            playButton.gameObject.SetActive(true);

            playButton.onClick.AddListener(PlayAction);
            playButton.onClick.AddListener(CardUnselection);

            ActivateButton.gameObject.SetActive(false);
            SellButton.gameObject.SetActive(false);
        }
        else if(Selection  == SelectionTiming.SellingCard)
        {
            SellButton.gameObject.SetActive(true);
            SellButton.onClick.AddListener(PlayAction);
            SellButton.onClick.AddListener(CardUnselection);
            
            ActivateButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(false);
        }
    }
    public void CardSelection(SelectionTiming Selection, Transform Card, CardBaseFunctionning CardAttacker, bool responding, Camera Cam)
    {
        if (Selection == SelectionTiming.ObjectToActivate)
        {
            ObjectCardBase ObjectCard = Card.GetComponent<ObjectCardBase>();

            if (!ObjectCard.CardCanBeActivate())
                return;

            CardUseSelection.gameObject.SetActive(true);
            CardUseSelection.position = Cam.WorldToScreenPoint(Card.position);

            SellButton.gameObject.SetActive(true);
            SellButton.onClick.AddListener(SendSellOffer);
            SellButton.onClick.AddListener(CardUnselection);
            cardSellSelection = Card.GetComponent<CardBaseFunctionning>().GetCardData() as CardObject;

            playButton.gameObject.SetActive(false);

            if (!responding && ObjectCard.IsProtection())
            {
                return;
            }
            else if (responding && CardAttacker != null)
            {

                if (!ObjectCard.IsProtection())
                {

                    if (ObjectCard.GetCardData().id != 42)
                        return;
                }
                
                if (CardAttacker.GetCardData().CardFamily == CardType.Agression )
                {

                    if (ObjectCard.GetCardData().id != 42 && ObjectCard.GetCardData().id != 47)
                        return;
                }
                else if (CardAttacker.GetCardData().CardFamily == CardType.Trick )
                {

                    if (ObjectCard.GetCardData().id != 42 && ObjectCard.GetCardData().id != 45)
                        return;
                }
            }
            else if (responding && CardAttacker == null)
            {

                if (ObjectCard.IsProtection())
                    return;
            }

            if (ObjectCard.GetCardData().id == 40 && !ObjectCard.GetPlayerOwner().GetComponent<CardPlayerNet>().GetPlayerState().IsPoisonned)
                return;

            if(ObjectCard.GetCardData().id == 41 && !ObjectCard.GetPlayerOwner().GetComponent<CardPlayerNet>().GetPlayerState().IsInjured)
                return;

            ActivateButton.gameObject.SetActive(true);
            ActivateButton.onClick.AddListener(Card.GetComponent<CardBaseFunctionning>().ActivateCard);
            ActivateButton.onClick.AddListener(CardUnselection);
        }
    }

    public void CardUnselection()
    {
        CardUseSelection.gameObject.SetActive(false);

        playButton.onClick.RemoveAllListeners();
        SellButton.onClick.RemoveAllListeners();
        ActivateButton.onClick.RemoveAllListeners();
    }

    public void binaryChoice(string FirstChoice, string SecondChoice, UnityAction firstAction, UnityAction SecondAction, UnityAction GeneralAction)
    {
        PlayerChoiceSelection.SetActive(true);

        TextChoicePropositionOne.text = FirstChoice;
        TextChoicePropositionTwo.text = SecondChoice;

        ChoicePropositionOne.onClick.AddListener(()=>  
        { 
            PlayerChoiceSelection.SetActive(false); 

            firstAction.Invoke();
            GeneralAction.Invoke();

            ChoicePropositionOne.onClick.RemoveAllListeners();
        });

        ChoicePropositionTwo.onClick.AddListener(()=>  
        { 
            PlayerChoiceSelection.SetActive(false);

            SecondAction.Invoke();
            GeneralAction.Invoke();

            ChoicePropositionTwo.onClick.RemoveAllListeners();
        });
    }

    public Button GetPassButton()
    {
        return PassButton;
    }

    public Button GetDrawButton() 
    { 
        return DrawButton; 
    }

    public Button GetTradeButton()
    {
        return TradeButton; 
    }

    public Button GetCapacityOneButton()
    {
        return characterCapacityOneButton; 
    }

    public Button GetCapacityTwoButton()
    {
        return characterCapacityTwoButton;
    }

    public Button GetResponseButton()
    {
        return ResponceButton;  
    }

    public Button GetInteruptionButton()
    {
        return InteruptionButton;  
    }

    public Image GetCharacterIcone()
    {
        return CharacterIcone;  
    }

    public void ActivateSlotButton(Button slotButton , Sprite CharacterIcone, GameObject TargetPlayer)
    {
        foreach (var ButtonsSlot in ButtonPlayers)
        {
            if (ButtonsSlot.button == slotButton)
            {
                ButtonsSlot.image.sprite = CharacterIcone;
                ButtonsSlot.player = TargetPlayer;
                ButtonsSlot.button.interactable = true; 
                break;
            } 
        }
    }

    public void ShowTargetPlayerChoice(GameObject PlayerAimer, CardBaseFunctionning Card)
    {
        PlayerTargetSelection.gameObject.SetActive(true);

        foreach (var ButtonSlot in ButtonPlayers)
        {
            if (ButtonSlot.player == PlayerAimer || ButtonSlot.player == null)
            {
                ButtonSlot.button.interactable = false;
            }
            else 
            {
                ButtonSlot.button.interactable = true;
                ButtonSlot.button.onClick.AddListener(() =>
                {

                    Card.SetPlayerTarget(ButtonSlot.player);
                    Card.DoSpecialEffect();

                    PlayerTargetSelection.gameObject.SetActive(false);
                    foreach (var ButtonSlot in ButtonPlayers)
                        ButtonSlot.button.onClick.RemoveAllListeners();

                });
            }
        }
    }

    public void ShowTargetPlayerChoice(GameObject PlayerAimer, CardBaseFunctionning Card, GameObject NextPlayer, GameObject PreviousPlayer)
    {
        PlayerTargetSelection.gameObject.SetActive(true);

        foreach (var ButtonSlot in ButtonPlayers)
        {
            if (ButtonSlot.player == PlayerAimer || ButtonSlot.player == null || (ButtonSlot.player != NextPlayer && ButtonSlot.player != PreviousPlayer))
            {
                ButtonSlot.button.interactable = false;
            }
            else 
            {
                ButtonSlot.button.interactable = true;
                ButtonSlot.button.onClick.AddListener(() =>
                {

                    Card.SetPlayerTarget(ButtonSlot.player);
                    Card.DoSpecialEffect();

                    PlayerTargetSelection.gameObject.SetActive(false);
                    foreach (var ButtonSlot in ButtonPlayers)
                        ButtonSlot.button.onClick.RemoveAllListeners();

                });
            }
        }
    }

    public void ShowTargetPlayerChoice(GameObject PlayerAimer, CardBaseFunctionning Card, GameObject Charmer)
    {
        PlayerTargetSelection.gameObject.SetActive(true);

        foreach (var ButtonSlot in ButtonPlayers)
        {
            if (ButtonSlot.player == PlayerAimer || ButtonSlot.player == null|| ButtonSlot.player != Charmer)
            {
                ButtonSlot.button.interactable = false;
            }
            else 
            {
                ButtonSlot.button.interactable = true;
                ButtonSlot.button.onClick.AddListener(() =>
                {

                    Card.SetPlayerTarget(ButtonSlot.player);
                    Card.DoSpecialEffect();

                    PlayerTargetSelection.gameObject.SetActive(false);
                    foreach (var ButtonSlot in ButtonPlayers)
                        ButtonSlot.button.onClick.RemoveAllListeners();

                });
            }
        }
    }

    public void ShowHideTradeSenderUi(bool Showing)
    {
        TradeSenderUi.SetActive(Showing);
        PriceDisplay.text = TradePrice.ToString();
    }
    public void ShowHideResponceButton(bool Showing)
    {
        ResponceButton.gameObject.SetActive(Showing);

        if(Showing)
            hideHud();
    }

    public void ShowHideInterruptionButton(bool Showing)
    {
        InteruptionButton.gameObject.SetActive(Showing);
    }

    public void ShowHideTradeOfferUi(bool Showing)
    {
        TradeOfferUi.SetActive(Showing);
    }

    public void ShowHidePlayerTarget(bool Showing)
    {
        PlayerTargetSelection.SetActive(Showing);
    }

    public void hideHud()
    {
        PassButton.gameObject.SetActive(false);
        DrawButton.gameObject.SetActive(false);
        TradeButton.gameObject.SetActive(false);
        InteruptionButton.gameObject.SetActive(false);

        TradeSenderUi.SetActive(false);
        TradeOfferUi.SetActive(false);
        PlayerTargetSelection.SetActive(false);

    }

    public void showHud()
    {
        PassButton.gameObject.SetActive(true);
        DrawButton.gameObject.SetActive(true);
        TradeButton.gameObject.SetActive(true);
    }

    public void SetTradeType( int type)
    {
        tradeTypeSelection = (CardType)type;
    }

    public void SetSelectionIcone(Button btn)
    {
        SelectionIcone.color = btn.gameObject.GetComponent<Image>().color;
    }

    public void SetPrice(int value)
    {
        TradePrice += value;
        
        if (TradePrice < 0) 
            TradePrice = 0;
        
        PriceDisplay.text = TradePrice.ToString();
    }

    public void SendTradeOffer()
    {
        if(HasAlreadyOffer)
            return;

        TradeSenderUi.SetActive(false);

        HasAlreadyOffer = true;
        IsOfferSender = true;

        OnTradeOfferSend?.Invoke(this, new OnTradeOfferSendArgs
        {
            TypeToTrade = tradeTypeSelection,
            priceToTrade = TradePrice
        });
    }

    public void SendSellOffer()
    {
        CardUnselection();

        OnSellOfferSend?.Invoke(this, new OnSellOfferSendArgs
        {
            CardIllustation = cardSellSelection.CardIllustration.mainTexture,
            CardPrice = cardSellSelection.Cost
        });
    }


    public void ReceiveTradeOffer(CardType type, int offer)
    {
        if (IsOfferSender == true)
        {
            IsOfferSender = false;
            return;
        }

        TradeOfferUi.SetActive(true);
        offerPriceDisplay.text = offer.ToString();

        Debug.Log(type);

        if (type == CardType.Work)
            OfferIcone.color = WorkColor;
        else if (type == CardType.Agression)
            OfferIcone.color = AgressionColor;
        else if (type == CardType.Crime)
            OfferIcone.color = CrimeColor;
        else if (type == CardType.Trick)
            OfferIcone.color = TrickColor;
        else if (type == CardType.Object)
            OfferIcone.color = ObjectColor;
    }
    public void RefuseTradeOffer()
    {
        TradeOfferUi.SetActive(false);
        offerPriceDisplay.text = 0.ToString();
        OfferIcone.color = Color.white;
    }

    public void SetAlertLevelDisplay(int  level)
    {
        alertLevelDisplay.text = level.ToString();
    }
}
