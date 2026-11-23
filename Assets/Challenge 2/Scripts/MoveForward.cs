using UnityEngine;

// Moves an object straight ahead (its local forward). Dogs run along the ground
// and balls fall, because the ball prefab is rotated to face down.
public class MoveForward : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
