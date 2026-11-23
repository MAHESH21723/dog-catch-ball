using UnityEngine;

// Sits on the dog. When the dog touches a ball, the ball is caught: both disappear and you score.
// (Checking for the Ball component also stops two balls from destroying each other.)
public class DetectCollisions : MonoBehaviour
{
    private bool done;

    void OnTriggerEnter(Collider other)
    {
        if (done) return;
        var ball = other.GetComponentInParent<Ball>();
        if (ball == null) return;

        done = true;
        if (GameManager.Instance != null) GameManager.Instance.BallCaught(ball.points, ball.transform.position);
        Destroy(ball.gameObject);
        Destroy(gameObject);
    }
}
