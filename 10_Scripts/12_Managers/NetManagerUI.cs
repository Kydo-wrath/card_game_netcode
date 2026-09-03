using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetManagerUI : MonoBehaviour
{
    [SerializeField] private Button ServerBtn;
    [SerializeField] private Button HostBtn;
    [SerializeField] private Button ClientBtn;


    private void Awake()
    {

        HostBtn.onClick.AddListener(() => 
        { 
            NetworkManager.Singleton.StartHost();
        });

        ClientBtn.onClick.AddListener(() => 
        { 
            NetworkManager.Singleton.StartClient();
        });
    }
}
