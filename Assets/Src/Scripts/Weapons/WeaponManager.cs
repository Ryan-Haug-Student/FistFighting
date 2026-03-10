using Unity.Netcode;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    public BaseWeapon[] equiped = new BaseWeapon[3];
    public BaseWeapon weapon;

    public PlayerHealth ph;
    public Animator animator;
    string trigger = "";

    private void Start()
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

        if (Input.GetKeyDown(KeyCode.Mouse0))
            animator.SetTrigger(trigger);
    }

    public void UpdateEquied(int target)
    {
        foreach (var w in equiped)
        {
            if (equiped[target] == w)
            {
                w.enabled = true;
                trigger = w.GetType().Name switch 
                {
                   "Melee" => "Melee",

                    _ => "Melee"
                };
            }
            else 
                w.enabled = false;
        }
        weapon = equiped[target];
    }
}
