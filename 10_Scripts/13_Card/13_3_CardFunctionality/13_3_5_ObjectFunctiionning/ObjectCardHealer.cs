public class ObjectCardhealer : ObjectCardBase
{
    public override void ActivateCard()
    {
        removeStatusToPlayer(playerOwner, StatAffliction);

        base.ActivateCard();
    }
}
