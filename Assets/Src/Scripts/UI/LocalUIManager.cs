using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LocalUIManager : NetworkBehaviour
{
    public TMP_Text speedText;
    public TMP_Text stateText;
    public TMP_Text healthText;
    public TMP_Text consText;
    public Slider slider;

    public GameObject pauseMenu;

    public Movement pm;
    public PlayerHealth ph;
    public PlayerCamera pc;
    public WeaponManager wm;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) gameObject.SetActive(false);

        var canvas = GameObject.Find("Canvas");
        if (canvas)
            canvas.SetActive(false);


    }

    private void Update()
    {
        UpdateOnScreen();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void UpdateOnScreen()
    {
        speedText.text = $"Speed | {Mathf.Round(pm.rb.linearVelocity.magnitude * 10) / 10}";
        stateText.text = $"State   | {pm.state}";
        healthText.text = $"Health | {ph.currentHealth}/{ph.maxHealth}";
        consText.text = $"Consumables | {wm.weapon.currentCount}/{wm.weapon.maxCount}";
    }

    // BUTTON FUNCTIONS ===========================================================================

    public void UpdateSens()
    {
        pc.sens = slider.value * 1000;
    }

    public void TogglePauseMenu()
    {
        if (pauseMenu.activeSelf == false)
        {
            pauseMenu.SetActive(true);

            pc.enabled = false;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            pauseMenu.SetActive(false);

            pc.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
