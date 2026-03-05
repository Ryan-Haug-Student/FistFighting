using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class BaseWeapon : NetworkBehaviour
{
    [Header("Consumables")]
    public int maxCount;
    public int currentCount;

    public float refilTime;
    public bool canAction;
    public float actionRefilTime;

    public bool fullAuto;

    [Header("general stats")]
    public int damage;
    public int maxFireDist;

    [Header("handling")]
    public float dispersion;
    public float actualDispersion;
    public float movementMultiplier;

    public bool isRefilling;

    [Header("sounds")]
    public AudioSource weaponNoise, reload;

    [Header("refs")]
    public Camera playerCam;
    public Transform handPos;
    public LayerMask whatIsPlayer;
    public Movement pm;
    public PlayerHealth phealth;
    public LineRenderer lr;

    private void Start()
    {
        currentCount = maxCount;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (fullAuto)
            if (Input.GetKey(KeyCode.Mouse0))
                TryFiring();
            else { }
        else
            if (Input.GetKeyDown(KeyCode.Mouse0))
            TryFiring();

        if (Input.GetKeyDown(KeyCode.R))
            TryRefill();
    }

    private void TryFiring()
    {
        if (!canAction || isRefilling || currentCount <= 0)
            return;
        else if (currentCount <= 0)
            TryRefill();

        currentCount--;
        canAction = false;
        MakeNoiseServerRpc(1);

        ApplyDispersion();
        FireRayCast();
    }

    private void ApplyDispersion()
    {
        float movementFactor = pm.state != Movement.MS.idle && pm.state != Movement.MS.crouching
            ? movementMultiplier : 1f;

        actualDispersion = dispersion * movementFactor;
    }

    private void FireRayCast()
    {
        Vector3 dir = playerCam.transform.forward;

        //dispersion
        dir += playerCam.transform.right * Random.Range(-actualDispersion, actualDispersion) * 0.01f;
        dir += playerCam.transform.up * Random.Range(-actualDispersion, actualDispersion) * 0.01f;
        dir.Normalize();



        if (Physics.Raycast(playerCam.transform.position, dir, out RaycastHit hit, maxFireDist))
        {
            DrawVisualServerRpc(hit.point);

            if (((1 << hit.collider.gameObject.layer) & whatIsPlayer) != 0)
            {
                if (hit.transform.gameObject != transform.gameObject)
                    DamagePlayerServerRpc(hit.transform.gameObject.GetComponentInParent<NetworkObject>().NetworkObjectId);
            }
        }
        else
        {
            hit.point = playerCam.transform.position + dir * maxFireDist;
            DrawVisualServerRpc(hit.point);
        }

        Invoke(nameof(ResetAction), actionRefilTime);
    }

    private void ResetAction()
    {
        canAction = true;
    }

    private void TryRefill()
    {
        if (isRefilling || currentCount == maxCount)
            return;

        StartCoroutine(Refill());
    }

    private IEnumerator Refill()
    {
        isRefilling = true;
        MakeNoiseServerRpc(0);

        yield return new WaitForSeconds(refilTime);

        currentCount = maxCount;
        canAction = true;
        isRefilling = false;
        MakeNoiseServerRpc(0);
    }

    [ServerRpc]
    public void MakeNoiseServerRpc(int noise)
    {
        MakeNoiseClientRpc(noise);
    }

    [ClientRpc]
    public void MakeNoiseClientRpc(int noise)
    {
        switch (noise)
        {
            case 1:
                {
                    weaponNoise.Play();
                    break;
                }

            case 0:
                {
                    if (!reload.isPlaying)
                        reload.Play();
                    else
                        reload.Stop();
                    break;
                }
        }
    }


    [ServerRpc]
    private void DamagePlayerServerRpc(ulong objId)
    {
        DamagePlayerClientRpc(objId, damage);
    }

    [ClientRpc]
    private void DamagePlayerClientRpc(ulong targetReference, int damageAmount)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetReference, out NetworkObject networkObject))
        {
            PlayerHealth ph = networkObject.gameObject.GetComponentInChildren<PlayerHealth>();
            ph.TryTakeDamage(damage);

            if (ph.currentHealth <= 0)
                phealth.Kills++;
        }
    }

    [ServerRpc]
    private void DrawVisualServerRpc(Vector3 endPoint)
    {
        DrawVisualClientRpc(endPoint);
    }

    [ClientRpc]
    private void DrawVisualClientRpc(Vector3 endPoint)
    {
        StartCoroutine(DrawVisual(endPoint));
    }

    private IEnumerator DrawVisual(Vector3 endPoint)
    {
        lr.enabled = true;
        lr.SetPosition(0, handPos.position);
        lr.SetPosition(1, endPoint);

        yield return new WaitForSeconds(0.05f);

        lr.enabled = false;
    }
}
