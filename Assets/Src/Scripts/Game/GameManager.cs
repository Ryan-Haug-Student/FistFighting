using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int duration; //seconds
    public int maxScore;

    [Header("Player Management")]
    public Transform[] spawnPoints;


    public static GameManager i;

    private void Start()
    {
        if (i == null)
            i = this;
        else
            Destroy(this);
    }

    public Transform GetSpawnPoint()
    {
        //put this here incase I want to add safe spawn logic

        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
