using UnityEngine;
using TMPro;

public class NodeData : MonoBehaviour
{
    [Tooltip("Nombre del archivo JSON dentro de Resources (sin .json)")]
    public string jsonFileName = "miTexto";

    [Tooltip("El componente TextMeshProUGUI que mostrará el contenido")]
    public TextMeshProUGUI textComponent;

    void Start()
    {
        if (textComponent == null)
        {
            Debug.LogError($"[{nameof(NodeData)}] No se ha asignado textComponent en {gameObject.name}");
            return;
        }

        // Carga el TextAsset desde Resources/miTexto.json
        TextAsset ta = Resources.Load<TextAsset>(jsonFileName);
        if (ta == null)
        {
            Debug.LogError($"[{nameof(NodeData)}] No se encontró Resources/{jsonFileName}.json");
            return;
        }

        // Asigna el texto plano entero al TMP
        textComponent.text = ta.text;
    }
}
