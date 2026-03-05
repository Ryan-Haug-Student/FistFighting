using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class ModelManager : NetworkBehaviour
{
    public GameObject[] models;

    private NetworkVariable<int> currentModel =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        currentModel.OnValueChanged += OnModelChanged;
        ChangeModel(currentModel.Value);

        if (!IsOwner) return;
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("LocalPlayer"));
    }

    private void OnModelChanged(int oldVal, int newVal)
    {
        ChangeModel(newVal);
    }

    public void RequestModelChange(int index)
    {
        if (!IsOwner) return;
        if (index < 0 || index >= models.Length) return;

        currentModel.Value = index;
    }

    private void ChangeModel(int index)
    {
        for (int i = 0; i < models.Length; i++)
            models[i].SetActive(i == index);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
