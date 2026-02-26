using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (!IsOwner) return;

        currentHealth = maxHealth;

        NetworkManager.SceneManager.OnLoadComplete += sceneChange;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
            NetworkManager.SceneManager.OnLoadComplete -= sceneChange;
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

    void sceneChange(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (!IsOwner) return;

        if (clientId != OwnerClientId) return;

        RespawnServerRpc();
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
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Vector3 spawnPos = SpawnPointManager.instance.getSpawnPoint().position;

        rb.MovePosition(spawnPos);

        RespawnClientRpc(spawnPos);
    }

    [ClientRpc]
    private void DeathClientRpc()
    {
        isDead = true;
        pm.enabled = false;

        playerObj.SetActive(false);
        gh.enabled = false;
        if (wm.weapon)
            wm.weapon.enabled = false;
        pc.enabled = false;

        rb.useGravity = false;
    }

    [ClientRpc]
    private void RespawnClientRpc(Vector3 spawnPos)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.MovePosition(spawnPos);

        isDead = false;
        currentHealth = maxHealth;

        if (wm && wm.enabled && wm.weapon)
        {
            wm.UpdateEquied(0);
            wm.weapon.enabled = true;
            wm.weapon.currentCount = wm.weapon.maxCount;
        }

        playerObj.SetActive(true);
        gh.enabled = true;
        pc.enabled = true;
        rb.useGravity = true;

        pm.enabled = true;
    }
}
