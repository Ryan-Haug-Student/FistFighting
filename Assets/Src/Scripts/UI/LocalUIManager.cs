using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class LocalUIManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) gameObject.SetActive(false);
    }
}
