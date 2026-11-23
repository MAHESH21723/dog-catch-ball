using UnityEngine;

// Cleans up objects that leave the play area.
// A dog that runs past the left edge just disappears; a ball that reaches the ground costs a life.
public class DestroyOutOfBounds : MonoBehaviour
{
    public float leftLimit = -22f;
    public float bottomLimit = 1f;

    void Update()
    {
        if (GetComponent<Ball>() != null)
        {
            if (transform.position.y < bottomLimit)
            {
                if (GameManager.Instance != null) GameManager.Instance.BallMissed(transform.position);
                Destroy(gameObject);
            }
        }
        else if (transform.position.x < leftLimit)
        {
            Destroy(gameObject);
        }
    }
}
