using UnityEngine;
using System.Collections;

public class WheelchairBumpFeedbackNonVR : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip bumpClip;

    public Transform nonVrCamera;  // 非VR时的摄像机
    public float bobAmount = 0.03f;
    public float bobDuration = 0.09f;
    bool shaking;

    void OnCollisionEnter(Collision c)
    {
        if (!c.collider.CompareTag("Bump")) return;
        if (audioSource && bumpClip) audioSource.PlayOneShot(bumpClip, 0.6f);
        if (nonVrCamera && !shaking) StartCoroutine(Bob());
    }

    IEnumerator Bob()
    {
        shaking = true;
        Vector3 a = nonVrCamera.localPosition, b = a + Vector3.up * bobAmount;
        float t = 0;
        while (t < bobDuration) { t += Time.deltaTime; nonVrCamera.localPosition = Vector3.Lerp(a, b, t / bobDuration); yield return null; }
        t = 0;
        while (t < bobDuration) { t += Time.deltaTime; nonVrCamera.localPosition = Vector3.Lerp(b, a, t / bobDuration); yield return null; }
        shaking = false;
    }
}
