// BallSpawner.cs
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform spawnPoint;
    private GameObject currentBall;

    void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;
        SpawnBall();
    }

    public void SpawnBall()
    {
        if (currentBall != null)
        {
            Destroy(currentBall);
        }
        if (ballPrefab != null)
        {
            currentBall = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
        }
    }

    // Optional: Call this from a UI button or after a certain time
    public void RequestNewBall()
    {
        SpawnBall();
    }
}