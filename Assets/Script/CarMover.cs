using UnityEngine;

public class CarMover : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 endPos;
    private float moveDuration = 5f;
    private float timer = 0f;
    private bool isMoving = false;

    private Renderer carRenderer;
    private float showAngleThreshold = 5f;

    public void InitializePath(Vector3 start, Vector3 end, float duration = 5f)
    {
        startPos = start;
        endPos = end;
        moveDuration = duration;
        timer = 0f;
        isMoving = true;

        transform.position = startPos;
        transform.rotation = Quaternion.LookRotation((endPos - startPos).normalized);

        carRenderer = GetComponentInChildren<Renderer>();
        if (carRenderer != null)
            carRenderer.enabled = false;
    }

    void Update()
    {
        if (!isMoving) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / moveDuration);

        Vector3 pos = Vector3.Lerp(startPos, endPos, t);
        pos.y = startPos.y;
        transform.position = pos;

        Vector3 moveDir = (endPos - startPos).normalized;
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }

        float angle = Vector3.Angle(moveDir, transform.forward);
        if (carRenderer != null)
            carRenderer.enabled = angle < showAngleThreshold;

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DeathHandler death = other.GetComponent<DeathHandler>();
            if (death != null)
                death.Die();
        }
    }
}
