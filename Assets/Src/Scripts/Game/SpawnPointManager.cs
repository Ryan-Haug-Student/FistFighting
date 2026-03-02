using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager instance;
    public Transform[] spawnPoints;

    private void Awake() => instance = this;
    public Transform getSpawnPoint() => spawnPoints[Random.Range(0, spawnPoints.Length)];
}
