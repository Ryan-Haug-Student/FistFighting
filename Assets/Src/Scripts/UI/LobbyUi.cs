using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUi : MonoBehaviour
{
    [Header("refs")]
    public GameObject hostPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("TestScene", LoadSceneMode.Single);
    }





    //networking ==================================================================================
}
