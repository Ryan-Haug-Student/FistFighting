using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth;
    public int currentHealth;

    public override void OnNetworkSpawn()
    {
        currentHealth = maxHealth;
    }

    public void TryTakeDamage(int damage)
    {
        if (!IsOwner) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            DeathServerRpc();
    }

    //networking

    [ServerRpc]
    private void DeathServerRpc ()
    {
        DeathClientRpc();
    }

    [ClientRpc]
    private void DeathClientRpc()
    {

    }
}
