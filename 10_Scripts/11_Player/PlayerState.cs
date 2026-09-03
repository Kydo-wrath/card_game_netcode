
using Unity.Netcode;

public struct PlayerState : INetworkSerializable
{
    public bool IsInjured;

    public bool IsAngry;

    public bool IsPoisonned;

    public bool IsCharmed;

    public ulong PlayerCharmerId;

    public bool WorkIsfreezed;

    public bool IsOverWatching;

    public bool IsWatchingBank;

    public bool IsCounting;

    public bool HaveToPass;

    public bool TakeBath;

    public bool IsMasked;

    public bool IsHealed;

    public bool Inflationned;

    public bool IsAggressionProtected;

    public bool IsTrickProtected;

    public bool IsEventProtected;

    public bool IsGardProtected;

    public bool IsCrimeProtected;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref IsInjured);

        serializer.SerializeValue(ref IsAngry);
 
        serializer.SerializeValue(ref IsPoisonned);
        
        serializer.SerializeValue(ref IsCharmed);
        
        serializer.SerializeValue(ref PlayerCharmerId);
        
        serializer.SerializeValue(ref WorkIsfreezed);
        
        serializer.SerializeValue(ref IsOverWatching);
        
        serializer.SerializeValue(ref IsWatchingBank);
        
        serializer.SerializeValue(ref IsCounting);
        
        serializer.SerializeValue(ref HaveToPass);
        
        serializer.SerializeValue(ref TakeBath);
        
        serializer.SerializeValue(ref IsMasked);
        
        serializer.SerializeValue(ref IsHealed);

        serializer.SerializeValue(ref Inflationned);

        serializer.SerializeValue(ref IsAggressionProtected);
        
        serializer.SerializeValue(ref IsTrickProtected);
        
        serializer.SerializeValue(ref IsEventProtected);

        serializer.SerializeValue(ref IsGardProtected);

        serializer.SerializeValue(ref IsCrimeProtected);
    }
}
