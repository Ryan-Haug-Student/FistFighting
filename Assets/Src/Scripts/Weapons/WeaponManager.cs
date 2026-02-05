using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public BaseWeapon[] equiped = new BaseWeapon[3];
    public BaseWeapon weapon;

    private void Start()
    {
        UpdateEquied(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            UpdateEquied(0);
        }
        if (Input.GetKeyDown("2"))
        {
            UpdateEquied(1);

        }
        if (Input.GetKeyDown("3"))
        {
            UpdateEquied(2);
        }
    }

    void UpdateEquied(int target)
    {
        foreach (var w in equiped)
        {
            if (equiped[target] == w)
                w.enabled = true;
            else 
                w.enabled = false;
        }
        weapon = equiped[target];
    }
}
