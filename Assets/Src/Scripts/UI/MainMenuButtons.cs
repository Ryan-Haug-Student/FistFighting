using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    public GameObject mainMenuHolder;
    public GameObject connectionMenuHolder;

    public GameObject networkManager;

    public TMP_InputField ipInput;
    public TMP_InputField portInput;

    public void PlayButtonPressed()
    {
        mainMenuHolder.SetActive(false);

        connectionMenuHolder.SetActive(true);
    }

    public void BackButtonPressed()
    {
        connectionMenuHolder.SetActive(false);

        mainMenuHolder.SetActive(true);
    }

    public void HostButtonClicked()
    {
        var transport = networkManager.GetComponent<UnityTransport>();
        var manager = networkManager.GetComponent<NetworkManager>();

        transport.SetConnectionData(ipInput.text, ushort.Parse(portInput.text));

        manager.StartHost();
        manager.SceneManager.LoadScene("TestScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void ClientButtonClicked()
    {
        var transport = networkManager.GetComponent<UnityTransport>();
        var manager = networkManager.GetComponent<NetworkManager>();

        transport.SetConnectionData(ipInput.text, ushort.Parse(portInput.text));

        manager.StartClient();
    }
}
