using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUi : MonoBehaviour
{
    [Header("refs")]
    public GameObject hostPanel;
    public TMP_Dropdown mapSel, skinSel;

    private ModelManager? localModelManager = null;

    void Start()
    {
        Debug.Log("Is host? " + NetworkManager.Singleton.IsHost);
        hostPanel.SetActive(NetworkManager.Singleton.IsHost);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(getMap(), LoadSceneMode.Single);
    }

    public void ChangeSkin()
    {
        if (localModelManager == null)
            FindLocalPlayer();

        localModelManager.RequestModelChange(skinSel.value);
    }

    private string getMap()
    {
        return mapSel.value switch {
            0 => "Castle",
            1 => "City",
            2 => "Dimension",

            _ => "Castle"
        };
    }

    private void FindLocalPlayer()
    {
        foreach (var player in GameObject.FindObjectsByType<ModelManager>(FindObjectsSortMode.None))
        {
            if (player.IsOwner)
            {
                localModelManager = player;
                break;
            }
        }
    }
}
