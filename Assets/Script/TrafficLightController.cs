using UnityEngine;
using System.Collections;

public class TrafficLightController : MonoBehaviour
{
    public Renderer lightRenderer;

    public Material redMaterial;
    public Material yellowMaterial;
    public Material greenMaterial;

    public Light pointLight;  // ��һ��Light���

    public float redDuration = 5f;
    public float yellowDuration = 2f;
    public float greenDuration = 5f;

    public float lightIntensity = 2f;  // ��ǿ�ȣ�ͳһ���ã�Ҳ���Էֱ�д���

    private enum LightState { Red, Green, Yellow }
    private LightState currentState = LightState.Green;

    private void Start()
    {
        if (lightRenderer == null)
            lightRenderer = GetComponent<Renderer>();

        if (pointLight == null)
            pointLight = GetComponentInChildren<Light>();

        SetLightState(currentState);
        StartCoroutine(CycleLights());
    }

    private IEnumerator CycleLights()
    {
        while (true)
        {
            switch (currentState)
            {
                case LightState.Red:
                    SetLightState(LightState.Red);
                    yield return new WaitForSeconds(redDuration);
                    currentState = LightState.Green;
                    break;

                case LightState.Green:
                    SetLightState(LightState.Green);
                    yield return new WaitForSeconds(greenDuration);
                    currentState = LightState.Yellow;
                    break;

                case LightState.Yellow:
                    SetLightState(LightState.Yellow);
                    yield return new WaitForSeconds(yellowDuration);
                    currentState = LightState.Red;
                    break;
            }
        }
    }

    private void SetLightState(LightState state)
    {
        currentState = state;

        switch (state)
        {
            case LightState.Red:
                lightRenderer.material = redMaterial;
                if (pointLight != null)
                {
                    pointLight.color = Color.red;
                    pointLight.intensity = lightIntensity;
                }
                break;
            case LightState.Yellow:
                lightRenderer.material = yellowMaterial;
                if (pointLight != null)
                {
                    pointLight.color = Color.yellow;
                    pointLight.intensity = lightIntensity;
                }
                break;
            case LightState.Green:
                lightRenderer.material = greenMaterial;
                if (pointLight != null)
                {
                    pointLight.color = Color.green;
                    pointLight.intensity = lightIntensity;
                }
                break;
        }
    }
}
