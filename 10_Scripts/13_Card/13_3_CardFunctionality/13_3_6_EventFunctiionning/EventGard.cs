using UnityEngine;

public class EventGard : EventCardBase
{
    protected override void EventEffect()
    {
        GardManager.Instance.ActivateAlertRpc();

        base.EventEffect();
    }
}
