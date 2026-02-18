using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    [Header("refs")]
    public Movement move;
    public GrappleHook hook;
    public WeaponManager mana;
    public Melee mele;
    public SideArm sidearm;
    public Rifle rif;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        SceneManager.sceneLoaded += SceneChange;
        StartCoroutine(sceneLoaded());
    }

    void SceneChange(Scene arg1, LoadSceneMode arg2)
    {
        if (!IsOwner) return; 

        if (arg1.name == "Lobby")
            InactiveServerRpc();
        else
            ActiveServerRpc();
    }

    IEnumerator sceneLoaded()
    {
        while (GameUi.Instance == null)
            yield return null;

        GameUi.Instance.BindToPlayer(gameObject);
    }

    [ServerRpc]
    void ActiveServerRpc()
    {
        ActiveClientRpc();
    }

    [ClientRpc]
    void ActiveClientRpc()
    {
        if (!IsOwner) return;

        move.enabled = true;
        hook.enabled = true;
        mana.enabled = true;
        mele.enabled = true;
        sidearm.enabled = true;
        rif.enabled = true;
    }

    [ServerRpc]
    void InactiveServerRpc()
    {
        InactiveClientRpc();
    }

    [ClientRpc]
    void InactiveClientRpc()
    {
        if (!IsOwner) return;

        move.enabled = false;
        hook.enabled = false;
        mana.enabled = false;
        mele.enabled = false;
        sidearm.enabled = false;
        rif.enabled = false;
    }
}
