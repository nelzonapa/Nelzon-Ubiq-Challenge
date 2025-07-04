// DetailsPanel.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DetailsPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private GameObject panel;

    void Start() => HideDetails();

    public void ShowDetails(string nodeId, int group, List<string> entities, Vector3 nodePosition)
    {
        string details = $"<b>Nodo: {nodeId}</b>\n";
        details += $"Grupo: {group}\n\n";
        details += "<b>Entidades:</b>\n";

        foreach (string entity in entities)
        {
            details += $"- {entity}\n";
        }

        contentText.text = details;
        panel.SetActive(true);

        contentText.text = details;
        panel.SetActive(true);

        // Posiciona el panel al lado del nodo
        Vector3 panelPosition = nodePosition + new Vector3(0.2f, 0.2f, 0);
        transform.position = panelPosition;

        // Orientación hacia la cámara
        transform.LookAt(Camera.main.transform);
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    public void HideDetails() => panel.SetActive(false);
}