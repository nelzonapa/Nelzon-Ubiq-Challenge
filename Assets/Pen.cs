using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Messaging;

[RequireComponent(typeof(XRGrabInteractable))] // Asegura que el componente exista
public class Pen : MonoBehaviour
{
    private NetworkContext context;
    private bool owner;


    [Header("Required References")]
    [SerializeField] private Transform nib; // Arrastra el Nib desde el Inspector
    [SerializeField] private Material drawingMaterial; // Opcional: material personalizado

    [Header("Drawing Settings")]
    [SerializeField] private float startWidth = 0.05f;
    [SerializeField] private float endWidth = 0.05f;
    [SerializeField] private float minVertexDistance = 0.02f;

    private GameObject currentDrawing;
    private XRGrabInteractable grabInteractable;



    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;

        public bool isDrawing;

        //constructor
        public Message(Transform transform, bool isDrawingAux)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.isDrawing = isDrawingAux;
        }
    }


    private void Awake()
    {
        // Obtener referencia segura al XRGrabInteractable
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (nib == null)
            Debug.LogError("Asigna el transform del Nib en el Inspector!", this);
    }

    private void Start()
    {
        // Verificar componentes críticos
        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable no encontrado. Asegúrate de que el objeto tiene este componente.", this);
            enabled = false; // Desactiva el script
            return;
        }

        // Configurar material si no está asignado
        if (drawingMaterial == null)
        {
            var shader = Shader.Find("Particles/Standard Unlit");
            if (shader != null)
            {
                drawingMaterial = new Material(shader);
            }
            else
            {
                Debug.LogError("Shader no encontrado. El dibujo no funcionará correctamente.", this);
            }
        }

        // Configurar eventos
        grabInteractable.activated.AddListener(BeginDrawing);
        grabInteractable.deactivated.AddListener(EndDrawing);


        grabInteractable.selectEntered.AddListener(XRGrabInteractable_SelectEntered);
        grabInteractable.selectExited.AddListener(XRGrabInteractable_SelectExited);


        context = NetworkScene.Register(this);


    }


    private void FixedUpdate()
    {
        if (owner)
        {
            context.SendJson(new Message(transform, isDrawingAux:currentDrawing));
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;

        if (data.isDrawing && !currentDrawing)
        {
            //BeginDrawing();
        }
        if (!data.isDrawing && currentDrawing)
        {
            //EndDrawing();
        }


    }


    private void XRGrabInteractable_SelectEntered(SelectEnterEventArgs eventArgs)
    {
        owner = true;
    }

    private void XRGrabInteractable_SelectExited(SelectExitEventArgs eventArgs)
    {
        owner = false;
    }


    private void BeginDrawing(ActivateEventArgs eventArgs)
    {
        if (nib == null) return;

        currentDrawing = new GameObject("Drawing");
        currentDrawing.transform.SetParent(nib, false); // false mantiene posición/rotación local

        var trail = currentDrawing.AddComponent<TrailRenderer>();
        trail.time = Mathf.Infinity;
        trail.material = drawingMaterial;
        trail.startWidth = startWidth;
        trail.endWidth = endWidth;
        trail.minVertexDistance = minVertexDistance;
    }

    private void EndDrawing(DeactivateEventArgs eventArgs)
    {
        if (currentDrawing == null) return;

        currentDrawing.transform.SetParent(null);
        if (currentDrawing.TryGetComponent(out TrailRenderer trail))
        {
            trail.emitting = false;
        }
        currentDrawing = null;
    }

    private void OnDestroy()
    {
        // Limpieza segura de eventos
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(BeginDrawing);
            grabInteractable.deactivated.RemoveListener(EndDrawing);
        }
    }
}