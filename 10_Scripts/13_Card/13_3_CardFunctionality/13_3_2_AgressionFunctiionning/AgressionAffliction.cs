
public class AgressionAffliction : AgressionCardBase
{
    public override void ActivateCard()
    {
        base.ActivateCard();

        GiveStatusToPlayer(playerTarget, StatAffliction);

        OutCard();
    }

}
