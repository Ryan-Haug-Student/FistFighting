using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_InputField clientIpInput;

    public void StartHost()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(getLocalIp(), (ushort)7777);

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(clientIpInput.text, (ushort)7777);

        NetworkManager.Singleton.StartClient();
    }

    public void Quit()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    // Helper Funcs

    private string getLocalIp()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                Debug.Log(ip.ToString());
                return ip.ToString();
            }
        }

        Debug.Log("No local Ip found?");
        return "";
    }
}
