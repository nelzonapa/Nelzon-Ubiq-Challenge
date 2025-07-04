using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class BoardAnimator : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private readonly List<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor> currentInteractors = new();

    private float initialDistance;
    private Vector3 initialScale;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        currentInteractors.Add(args.interactorObject);

        if (currentInteractors.Count == 2)
        {
            initialDistance = Vector3.Distance(
                currentInteractors[0].transform.position,
                currentInteractors[1].transform.position);
            initialScale = transform.localScale;
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        currentInteractors.Remove(args.interactorObject);
    }

    void Update()
    {
        if (currentInteractors.Count == 2)
        {
            float currentDistance = Vector3.Distance(
                currentInteractors[0].transform.position,
                currentInteractors[1].transform.position);
            float scaleFactor = currentDistance / initialDistance;
            transform.localScale = initialScale * scaleFactor;
        }
    }
}