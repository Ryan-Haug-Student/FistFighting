using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth;
    public int currentHealth;
    public bool isDead;

    public GameObject playerObj;
    public PlayerMovement pm;
    public GrappleHook gh;

    public override void OnNetworkSpawn()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (isDead && Input.GetKeyDown(KeyCode.Q))
            RespawnServerRpc();
    }

    public void TryTakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0 && !isDead)
            DeathServerRpc();
    }

    //networking

    [ServerRpc]
    private void DeathServerRpc ()
    {
        DeathClientRpc();
    }

    [ServerRpc]
    private void RespawnServerRpc()
    {
        RespawnClientRpc();
    }

    [ClientRpc]
    private void DeathClientRpc()
    {
        isDead = true;

        playerObj.SetActive(false);
        pm.enabled = false;
        gh.enabled = false;
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        isDead = false;


        playerObj.SetActive(true);
        pm.enabled = true;
        gh.enabled = true;
    }
}
