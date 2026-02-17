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
        currentHealth = maxHealth;

        SceneManager.activeSceneChanged += sceneChange;
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

    void sceneChange (Scene arg0, Scene arg1)
    {
        if (!IsOwner) return;

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
        transform.position = new Vector3(0, 0, 0);
        rb.linearVelocity = Vector3.zero;

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

        if (wm.enabled)
        {
            wm.weapon.currentCount = wm.weapon.maxCount;
            wm.weapon.enabled = true;
        }

        transform.position = new Vector3(0, 0, 0);
        playerObj.SetActive(true);
        gh.enabled = true;
        pc.enabled = true;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;

        pm.enabled = true;
    }
}
