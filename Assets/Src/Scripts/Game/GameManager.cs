using Unity.Netcode;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [Header("Game Settings")]
    public int duration; //seconds
    public int maxScore;

    private float timer;

    private NetworkVariable<bool> gameActive = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> secondsLeft = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    [Header("Player Management")]
    public static Transform[] spawnPoints;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        StartGameServerRpc();
    }

    private void Update()
    {
        // Only server runs game timer logic
        if (!IsServer)
            return;

        if (gameActive.Value)
        {
            timer -= Time.deltaTime;
            secondsLeft.Value = (int)timer;
            if (timer <= 0f)
            {
                StopGame();
            }
        }
    }

    public static Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    public void StartGame()
    {
        if (!IsServer) return;

        timer = duration;
        gameActive.Value = true;

        OnGameStartedClientRpc();
    }

    public void StopGame()
    {
        if (!IsServer) return;

        gameActive.Value = false;

        OnGameStoppedClientRpc();
    }

    [ServerRpc]
    private void StartGameServerRpc()
    {
        StartGame();
    }

    [ClientRpc]
    private void OnGameStartedClientRpc()
    {
        Debug.Log("Game Started");
        // Enable player controls, UI, etc.
    }

    [ClientRpc]
    private void OnGameStoppedClientRpc()
    {
        Debug.Log("Game Ended");

        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        // Disable controls, show scoreboard, etc.
    }

    public bool IsGameActive()
    {
        return gameActive.Value;
    }
}
