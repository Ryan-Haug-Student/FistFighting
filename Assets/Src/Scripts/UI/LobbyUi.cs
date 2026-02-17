using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUi : MonoBehaviour
{
    [Header("refs")]
    public GameObject hostPanel;

    void Start()
    {
        Debug.Log("Is host? " + NetworkManager.Singleton.IsHost);
        hostPanel.SetActive(NetworkManager.Singleton.IsHost);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("TestScene", LoadSceneMode.Single);
    }

    //networking ==================================================================================
}
