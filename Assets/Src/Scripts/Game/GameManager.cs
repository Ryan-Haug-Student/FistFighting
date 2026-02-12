using Unity.Netcode;
using UnityEngine;

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

    [Header("Player Management")]
    public static Transform[] spawnPoints;

    private void Start()
    {
        Debug.Log("GameManager Start");
    }

    private void Update()
    {
        // Input handled on ALL clients
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (IsServer)
                StartGame();
            else
                StartGameServerRpc();

        }

        // Only server runs game timer logic
        if (!IsServer)
            return;

        if (gameActive.Value)
        {
            timer -= Time.deltaTime;

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

    [ServerRpc(RequireOwnership = false)]
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
        // Disable controls, show scoreboard, etc.
    }

    public bool IsGameActive()
    {
        return gameActive.Value;
    }
}
