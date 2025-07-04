using UnityEngine;
using UnityEngine.XR;

public class CanvasCameraSetter : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (XRSettings.enabled)
        {
            // Para VR: Busca la cámara XR
            canvas.worldCamera = Camera.main;
        }
        else
        {
            // Para modo editor
            canvas.worldCamera = Camera.main;
        }
    }
}