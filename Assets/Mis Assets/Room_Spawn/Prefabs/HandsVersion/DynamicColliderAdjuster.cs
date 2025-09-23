using UnityEngine;
using UnityEngine.UI;

public class DynamicColliderAdjuster : MonoBehaviour
{
    private RectTransform contentRectTransform;
    private BoxCollider boxCollider;
    private VerticalLayoutGroup verticalLayoutGroup;

    void Start()
    {
        // Buscar el Content dentro del hierarchy
        contentRectTransform = transform.Find("Content") as RectTransform;
        if (contentRectTransform == null)
        {
            Debug.LogError("No se encontró el objeto 'Content' como hijo de este objeto");
            return;
        }

        boxCollider = GetComponent<BoxCollider>();
        verticalLayoutGroup = contentRectTransform.GetComponent<VerticalLayoutGroup>();

        AdjustCollider();
    }

    void Update()
    {
        // Ajusta el collider si el contenido cambia
        AdjustCollider();
    }

    void AdjustCollider()
    {
        if (contentRectTransform == null || boxCollider == null) return;

        // Calcular el tamaño total del contenido
        float totalWidth = 0f;
        float totalHeight = 0f;
        int childCount = contentRectTransform.childCount;

        if (childCount > 0)
        {
            // Para múltiples imágenes: calcular el tamaño total considerando el layout
            foreach (RectTransform child in contentRectTransform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    Image img = child.GetComponent<Image>();
                    if (img != null && img.sprite != null)
                    {
                        totalWidth = Mathf.Max(totalWidth, img.sprite.rect.width);
                        totalHeight += img.sprite.rect.height;

                        // Agregar spacing del layout group si existe
                        if (verticalLayoutGroup != null)
                        {
                            totalHeight += verticalLayoutGroup.spacing;
                        }
                    }
                }
            }

            // Si hay múltiples hijos, usar el ancho máximo y la altura acumulada
            if (childCount > 1)
            {
                // Remover el spacing extra del último elemento
                if (verticalLayoutGroup != null)
                {
                    totalHeight -= verticalLayoutGroup.spacing;
                }
            }
        }
        else
        {
            // Si no hay hijos, usar el tamaño del Content
            totalWidth = contentRectTransform.rect.width;
            totalHeight = contentRectTransform.rect.height;
        }

        // Asegurar un tamaño mínimo
        totalWidth = Mathf.Max(totalWidth, 100f);
        totalHeight = Mathf.Max(totalHeight, 100f);

        // Ajustar el collider al contenido
        boxCollider.size = new Vector3(totalWidth, totalHeight, 0.1f);

        // Centrar el collider en el contenido
        Vector3 contentLocalPos = contentRectTransform.localPosition;
        boxCollider.center = new Vector3(contentLocalPos.x, contentLocalPos.y, 0f);
    }

    // Método público para forzar un reajuste cuando se añaden nuevas imágenes
    public void ForceAdjustment()
    {
        AdjustCollider();
    }
}