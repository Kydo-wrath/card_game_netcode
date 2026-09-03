using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BankManager : NetworkBehaviour
{
    [SerializeField] private int BankBeginCoins;
    [SerializeField] private GameObject CoinPrefab;
    [SerializeField] private NetworkVariable<int> BankCurrentCoins = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private GameObject CurrentCoinToSpawn;

    public override void OnNetworkSpawn()
    {
        if (!IsHost)
            return;

        StartCoroutine(CreateBeginCoins());
    }

    IEnumerator CreateBeginCoins()
    {
        for (int i = 0; i < BankBeginCoins; i++)
        {
            yield return new WaitForSeconds(1);

            SpawnCoinsRpc();
        }
    }
    public IEnumerator CreateCoins(int numberOfCoins)
    {
        for (int i = 0; i < numberOfCoins; i++)
        {
            yield return new WaitForSeconds(1);

            SpawnCoinsRpc();
        }
    }
    [Rpc(SendTo.Server)]
    public void SpawnCoinsRpc(RpcParams netParams = default)
    {
        
        CurrentCoinToSpawn = Instantiate(CoinPrefab, gameObject.transform.position, Random.rotation);
        CurrentCoinToSpawn.GetComponent<NetworkObject>().Spawn(true);
        CurrentCoinToSpawn.transform.SetParent(gameObject.transform);
        CurrentCoinToSpawn = null;

        BankCurrentCoins.Value++;
    }

    [Rpc(SendTo.Server)]
    public void IncrementCoinsRpc(int numberOfCoins,RpcParams netParams = default)
    {
        BankCurrentCoins.Value += numberOfCoins;
    }
    [Rpc(SendTo.Server)]
    public void DecrementCoinsRpc(int numberOfCoins,RpcParams netParams = default)
    {
        BankCurrentCoins.Value -= numberOfCoins;
    }

   public int GetCurrentbankCoins()
   { 
        return BankCurrentCoins.Value; 
   }

    public IEnumerator SendCoins(int numberOfCoinsToSent, Transform PositionToSend)
    {
        for (int i = 0; i < numberOfCoinsToSent; i++)
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
            if (transform.childCount <= 0)
                return;

            Transform coin = transform.GetChild(0);
            coin.SetParent(null);
            coin.SetParent(TransformNet.transform);
            coin.localPosition = Vector3.zero;
        }

    }
}
