
public class  Bribe: TrickCardBase
{
    public override void DoSpecialEffect()
    {
        playerOwner.GetComponent<CardPlayerNet>().decremenGoldRpc(6);
        StartCoroutine(playerOwner.GetComponent<CardPlayerNet>().DeleteCoins(6));
        GardManager.Instance.DecrementAlertRpc(6);

        base.DoSpecialEffect();
    }

}
