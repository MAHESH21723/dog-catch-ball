using UnityEngine;
using UnityEngine.EventSystems;

// Sends a dog running when you press Space or click / tap the field.
// A short cooldown stops you from spamming dogs.
public class PlayerController : MonoBehaviour
{
    public GameObject dogPrefab;
    public Vector3 spawnPos = new Vector3(13f, 0f, 2f);
    public float cooldown = 0.9f;

    private float readyAt;

    // 0 = just sent a dog, 1 = ready to send the next one.
    public float Ready
    {
        get { return cooldown <= 0f ? 1f : Mathf.Clamp01(1f - (readyAt - Time.time) / cooldown); }
    }

    public void ResetCooldown()
    {
        readyAt = 0f;
    }

    void Update()
    {
        var game = GameManager.Instance;
        if (game == null || !game.IsPlaying || game.JustStarted) return;
        if (Time.time < readyAt) return;

        bool pressed = Input.GetKeyDown(KeyCode.Space);
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUi()) pressed = true;
        if (!pressed) return;

        readyAt = Time.time + cooldown;
        Instantiate(dogPrefab, spawnPos, dogPrefab.transform.rotation);
    }

    static bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
