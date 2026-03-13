using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
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
    string reloadTrigger = "";

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        StartCoroutine(updateEquipedStart());
    }

    IEnumerator updateEquipedStart()
    {
        yield return new WaitForSeconds(.2f);
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

        if (!ph.isDead)
            if (weapon.fullAuto)
            {
                if (Input.GetKey(KeyCode.Mouse0) && weapon.canAction && weapon.currentCount > 0 && !weapon.isRefilling)
                    networkAnimator.SetTrigger(trigger);
                if (Input.GetKeyUp(KeyCode.Mouse0))
                    networkAnimator.ResetTrigger(trigger);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && weapon.canAction && weapon.currentCount > 0 && !weapon.isRefilling)
                    networkAnimator.SetTrigger(trigger);
            }

        if (Input.GetKeyDown(KeyCode.R) && !weapon.isRefilling & !ph.isDead)
            networkAnimator.SetTrigger(reloadTrigger);
        if (weapon.isRefilling)
            networkAnimator.ResetTrigger(reloadTrigger);
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
                reloadTrigger = "";
                networkAnimator.SetTrigger("MeleeEquip");
                break;
            case "SideArm":
                trigger = "PistolShoot";
                reloadTrigger = "SideArmReload";
                networkAnimator.SetTrigger("SideArmEquip");
                break;
            case "Rifle":
                trigger = "RifleShoot";
                reloadTrigger = "RifleReload";
                networkAnimator.SetTrigger("RifleEquip");
                break;
        }
    }
}
