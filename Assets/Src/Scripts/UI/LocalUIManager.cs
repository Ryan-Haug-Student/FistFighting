using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class LocalUIManager : NetworkBehaviour
{
    public TMP_Text speedText;
    public TMP_Text stateText;
    public TMP_Text healthText;
    public TMP_Text consText;

    public PlayerMovement pm;
    public PlayerHealth ph;
    public BaseWeapon bh;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) gameObject.SetActive(false);

        var canvas = GameObject.Find("Canvas");
        if (canvas)
            canvas.SetActive(false);
    }

    private void Update()
    {
        speedText.text = $"Speed | {Mathf.Round(pm.rb.linearVelocity.magnitude * 10) /10}";
        stateText.text = $"State   | {pm.state}";
        healthText.text = $"Health | {ph.currentHealth}/{ph.maxHealth}";
        consText.text = $"Consumables | {bh.currentCount}/{bh.maxCount}";
    }
}
