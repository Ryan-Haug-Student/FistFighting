using Unity.Netcode;
using Unity.VectorGraphics;
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
    public GameObject ui;


    private bool activeated = false;

    public override void OnNetworkSpawn()
    {
    }

    private void Update()
    {
        if (!IsOwner) return;
        //if (!IsHost) return;

        if (SceneManager.GetActiveScene().name == "TestScene" && activeated == false)
        {
            ActiveServerRpc();
            activeated = true;
        }
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
        ui.SetActive(true);
    }
}
