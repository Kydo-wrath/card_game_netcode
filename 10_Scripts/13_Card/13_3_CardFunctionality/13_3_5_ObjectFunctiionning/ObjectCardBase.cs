using Unity.Netcode;
using UnityEngine;

public class ObjectCardBase : CardBaseFunctionning
{
    protected CardObject ObjectCardData;
    protected bool CanBeACtivate = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (CardData is CardObject)
        {
            ObjectCardData = (CardObject)CardData;
        }
    }

    public override void PlayCard()
    {
        base.PlayCard();
        MoveToObjectSlotRpc();
    }

    public override CardBase GetCardData()
    {
        return ObjectCardData;
    }

    [Rpc(SendTo.Server)]
    public void MoveToObjectSlotRpc()
    {
        gameObject.transform.SetParent(playerOwner.GetComponent<PlayerHand>().GetObjectSlot());
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
    }

    public override void ATurnPassed()
    {
        CanBeACtivate = true;  
    }
    public override void ActivateCard()
    {
        IsActivated = true;

        OutCard();
    }

    public override void OutCard()
    {
        CanBeACtivate = false;

        base.OutCard();
    }

    public bool CardCanBeActivate()
    { 
        return CanBeACtivate; 
    }
}
