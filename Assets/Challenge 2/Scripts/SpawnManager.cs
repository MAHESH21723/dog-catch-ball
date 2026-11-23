using UnityEngine;

// Drops a random ball from a random spot at the top. The longer you last, the faster and more often they come.
public class SpawnManager : MonoBehaviour
{
    public GameObject[] ballPrefabs;

    public float spawnLimitXLeft = -18f;
    public float spawnLimitXRight = 8f;
    public float spawnPosY = 29f;
    public float spawnPosZ = 2.5f;

    public float startDelay = 1f;
    public float firstInterval = 3f;     // average gap between balls at the start
    public float fastestInterval = 1.2f; // ... and the smallest it gets
    public float startFallSpeed = 5f;
    public float maxFallSpeed = 9f;
    public float rampSeconds = 90f;      // time it takes to reach full difficulty

    private float timer;
    private float elapsed;
    private bool spawning;

    public void ResetSpawning()
    {
        elapsed = 0f;
        timer = startDelay;
        spawning = true;
    }

    public void StopSpawning()
    {
        spawning = false;
    }

    void Update()
    {
        if (!spawning) return;

        elapsed += Time.deltaTime;
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        float ramp = Mathf.Clamp01(elapsed / rampSeconds);
        float interval = Mathf.Lerp(firstInterval, fastestInterval, ramp);
        timer = interval * Random.Range(0.6f, 1.4f);   // random gap each time
        SpawnRandomBall(Mathf.Lerp(startFallSpeed, maxFallSpeed, ramp));
    }

    // Spawn a random ball at a random x position at the top of the play area.
    void SpawnRandomBall(float fallSpeed)
    {
        if (ballPrefabs == null || ballPrefabs.Length == 0) return;
        var spawnPos = new Vector3(Random.Range(spawnLimitXLeft, spawnLimitXRight), spawnPosY, spawnPosZ);
        var prefab = ballPrefabs[Random.Range(0, ballPrefabs.Length)];
        var ball = Instantiate(prefab, spawnPos, prefab.transform.rotation);
        var mover = ball.GetComponent<MoveForward>();
        if (mover != null) mover.speed = fallSpeed;
    }
}
