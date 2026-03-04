using System;
using UnityEngine;

public class RespawnPlayer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) { other.gameObject.GetComponentInParent<PlayerHealth>().Respawn(); }
}
