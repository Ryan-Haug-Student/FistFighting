using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    public BaseWeapon[] equiped = new BaseWeapon[3];
    public BaseWeapon weapon;

    public PlayerHealth ph;
    public Animator animator;
    public NetworkAnimator networkAnimator;
    string trigger = "";

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        UpdateEquied(0);
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!ph.isDead && !weapon.isRefilling)
        {
            if (Input.GetKeyDown("1"))
                UpdateEquied(0);
            if (Input.GetKeyDown("2"))
                UpdateEquied(1);
            if (Input.GetKeyDown("3"))
                UpdateEquied(2);
        }

        if (weapon.fullAuto)
        {
            if (Input.GetKey(KeyCode.Mouse0) && weapon.canAction && weapon.currentCount > 0 && !weapon.isRefilling)
                networkAnimator.SetTrigger(trigger);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && weapon.canAction && weapon.currentCount > 0 && !weapon.isRefilling)
                networkAnimator.SetTrigger(trigger);
        }
    }

    public void UpdateEquied(int target)
    {
        foreach (var w in equiped)
        {
            if (equiped[target] == w)
            {
                w.enabled = true;
                AnimHandler(w);
            }
            else 
                w.enabled = false;
        }
        weapon = equiped[target];
    }

    private void AnimHandler(BaseWeapon w)
    {
        animator.ResetTrigger("MeleeEquip");
        animator.ResetTrigger("SideArmEquip");
        animator.ResetTrigger("RifleEquip");

        switch (w.GetType().Name) 
        {
            case "Melee":
                trigger = "Melee";
                networkAnimator.SetTrigger("MeleeEquip");
                break;
            case "SideArm":
                trigger = "PistolShoot";
                networkAnimator.SetTrigger("SideArmEquip");
                break;
            case "Rifle":
                trigger = "RifleShoot";
                networkAnimator.SetTrigger("RifleEquip");
                break;
        }
    }
}
