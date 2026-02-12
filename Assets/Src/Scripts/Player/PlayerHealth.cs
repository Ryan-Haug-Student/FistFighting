using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth;
    public int currentHealth;
    public bool isDead;

    public GameObject playerObj;
    public Movement pm;
    public GrappleHook gh;
    public WeaponManager wm;
    public Rigidbody rb;
    public PlayerCamera pc;

    public override void OnNetworkSpawn()
    {
        currentHealth = maxHealth;
        RespawnServerRpc();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (isDead && Input.GetKeyDown(KeyCode.Q))
            RespawnServerRpc();
    }

    public void TryTakeDamage(int damage)
    {
        if (!IsOwner) return;

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
        pm.enabled = false;

        playerObj.SetActive(false);
        gh.enabled = false;
        wm.weapon.enabled = false;
        pc.enabled = false;

        rb.useGravity = false;
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        isDead = false;
        currentHealth = maxHealth;
        wm.weapon.currentCount = wm.weapon.maxCount;

        transform.position = GameManager.GetSpawnPoint().position;

        playerObj.SetActive(true);
        gh.enabled = true;
        wm.weapon.enabled = true;
        pc.enabled = true;
        rb.useGravity = true;

        pm.enabled = true;
    }
}
