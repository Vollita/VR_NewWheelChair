using UnityEngine;

public class WheelchairBumpFeedbackXRI : MonoBehaviour
{
    public float hapticAmplitude = 0.6f;
    public float hapticDuration = 0.08f;

    void OnCollisionEnter(Collision c)
    {
        if (!c.collider.CompareTag("Bump")) return;
        SendHaptics();
    }

    void SendHaptics()
    {
#if UNITY_XR_MANAGEMENT && ENABLE_INPUT_SYSTEM
        var ctls = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.XRBaseController>();
        foreach (var c in ctls)
            c.SendHapticImpulse(hapticAmplitude, hapticDuration);
#endif
    }
}
