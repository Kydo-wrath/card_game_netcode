using Unity.Netcode;
using UnityEngine;

public class CardBaseFunctionning : NetworkBehaviour
{
    [SerializeField] protected float CardDistanceSelect;
    [SerializeReference] protected CardBase CardData;

    [SerializeField] protected GameObject playerOwner;
    [SerializeField] protected GameObject playerTarget;

    [SerializeField] protected TargetStat StatAffliction;
    [SerializeField] protected SideEffect CardSideEffect;

    [SerializeField] protected bool IsAProtection;


    protected bool isPlayed = false;
    protected bool IsActivated = false;
    protected bool IsSelect = false;

    protected Vector3 SelecteDirection;
    protected int CurrentNumberOfTurnPassed = 0;



    private void Awake()
    {
        CardData = gameObject.GetComponent<CardAspectScript>().GetCardData();
    }

    

    public virtual void PlayCard()
    {
        playerOwner.GetComponent<PlayerHand>().CardPlayedRpc(CardData.CardFamily);

        if (playerOwner.GetComponent<CardPlayerNet>().GetPlayerState().IsPoisonned || playerOwner.GetComponent<CardPlayerNet>().playerIsBeerPoisonned())
            playerOwner.GetComponent<PlayerHand>().PoisonChoiceActivation();

        isPlayed = true;
    }
    public virtual void ActivateCard()
    {

    }

    public virtual void SelectCard() 
    {
        if (!IsSelect && !isPlayed)
        {
            IsSelect = true;
            MoveToSelectRpc();
        }
    }

    public virtual void UnselectCard() 
    {
        if (isPlayed)
            return;

        IsSelect = false;

        if (!playerOwner)
            return;
        NetworkObjectReference CardRef = gameObject.GetComponent<NetworkObject>();
        playerOwner.GetComponent<PlayerHand>().CardInHandRpc(CardRef);
    }

    public virtual void OutCard()
    {
        IsActivated = false;
        isPlayed = false;

        MoveToDeckOutRpc();
    }
    public virtual void DrawCard()
    {
    }
    public virtual void GiveStatusToPlayer(GameObject playerTarget, TargetStat affliction)
    {
        if (playerTarget == null)   
            return;

        if (affliction == TargetStat.charmed)
            playerTarget.GetComponent<CardPlayerNet>().EnablePlayerStateRpc(affliction, playerOwner.GetComponent<NetworkObject>().OwnerClientId);
        else
            playerTarget.GetComponent<CardPlayerNet>().EnablePlayerStateRpc(affliction);

    }
    public virtual void GiveStatusToPlayer(GameObject playerTarget, SideEffect effect)
    {
        if (playerTarget == null)
            return;

        playerTarget.GetComponent<CardPlayerNet>().EnablePlayerStateRpc(effect);
    }

    public virtual void removeStatusToPlayer(GameObject playerTarget, TargetStat affliction)
    {
        if (playerTarget == null)
            return;

        playerTarget.GetComponent<CardPlayerNet>().DisablePlayerStateRpc(affliction);
    }

    public virtual void removeStatusToPlayer(GameObject playerTarget, SideEffect effect)
    {
        if (playerTarget == null)
            return;

        playerTarget.GetComponent<CardPlayerNet>().DisablePlayerStateRpc(effect);
    }

    public virtual void IncreaseStat()
    {
    }
    public virtual void DecreaseStat()
    {

    }
    public virtual void DoSpecialEffect()
    {

    }
    public virtual void targetSelection()
    {

    }
    public virtual void SetPlayerOwner(GameObject player) 
    {
        playerOwner = player;

        SetNetOwnerRpc(playerOwner.GetComponent<NetworkObject>());
    }

    public virtual void SetPlayerTarget(GameObject player)
    {
        playerTarget = player;

        SetNetTargetRpc(playerTarget.GetComponent<NetworkObject>());
    }
    public virtual void ChooseTarget()
    {
        if(CardData.id == 28)
            HudManager.instance.ShowTargetPlayerChoice(playerOwner, this, GameMaster.Instance.SearchNextPlayer(playerOwner), GameMaster.Instance.SearchPreviousPlayer(playerOwner));
        else
            HudManager.instance.ShowTargetPlayerChoice(playerOwner, this);
    }
    public virtual void ATurnPassed()
    {

    }

    public virtual GameObject GetPlayerOwner() 
    { 
        return playerOwner; 
    }
    public virtual CardBase GetCardData() 
    { 
        return null; 
    }

    public bool IsProtection()
    {
        return IsAProtection;
    }

    [Rpc(SendTo.Server)]
    public void MoveToDeckOutRpc(RpcParams sender = default)
    {
        if (!playerOwner)
            return;
        gameObject.transform.SetParent(playerOwner.GetComponent<PlayerHand>().GetDeckOutSlot());
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
        SetNetOwnerRpc(new NetworkObjectReference());
    }

    [Rpc(SendTo.Server)]
    public void MoveToSelectRpc()
    {
        gameObject.transform.SetParent(null);
        Vector3 direction = (playerOwner.transform.position - gameObject.transform.position).normalized;
        gameObject.transform.position += direction * CardDistanceSelect;

    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PingPlayerResponceRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject playerObject))
            playerObject.gameObject.GetComponent<PlayerHand>().TutoResponce(this);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetNetTargetRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject playerObject))
            playerTarget = playerObject.gameObject;
        else
        {
            playerTarget = null;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetNetOwnerRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject playerObject))
        {
            playerOwner = playerObject.gameObject;
        }
        else
        {
            playerOwner = null;
        }
    }
}