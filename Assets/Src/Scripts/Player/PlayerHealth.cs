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
    public BaseWeapon bw;
    public Rigidbody rb;
    public PlayerCamera pc;

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
        bw.enabled = false;
        pc.enabled = false;

        rb.useGravity = false;
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        isDead = false;
        currentHealth = maxHealth;
        bw.currentCount = bw.maxCount;

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        playerObj.SetActive(true);
        gh.enabled = true;
        bw.enabled = true;
        pc.enabled = true;
        rb.useGravity = true;

        pm.enabled = true;
    }
}
