using UnityEngine;
using System.Collections;

public class TrafficLightController : MonoBehaviour
{
    public Renderer redBox;
    public Renderer yellowBox;
    public Renderer greenBox;

    public Material matRed;
    public Material matYellow;
    public Material matGreen;
    public Material matOff;

    public float redDuration = 5f;
    public float greenDuration = 5f;
    public float yellowDuration = 2f;

    private enum LightState { Red, Green, Yellow }
    private LightState currentState = LightState.Red;

    private void Start()
    {
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
        redBox.material = (state == LightState.Red) ? matRed : matOff;
        yellowBox.material = (state == LightState.Yellow) ? matYellow : matOff;
        greenBox.material = (state == LightState.Green) ? matGreen : matOff;
    }
}
