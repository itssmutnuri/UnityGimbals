using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Controller : MonoBehaviour
{

    public Transform target;
    public LayerMask translateGimbalLayer, rotateGimbalLayer, scaleGimbalLayer;
    public float minScale, maxScale;

    public TextMeshProUGUI currentModeText;

    public Material translucentMaterial, selectedMaterial;
    
    private Dictionary<ModeType, Mode> modes;
    private Mode currentMode = null;
    private Transform selectedGimbal = null;
    private Renderer selectedGimbalRenderer = null;
    private Color originalColor;

    // hold a list of all gimbal renderers to handle material assignment
    private List<Renderer> gimbalRenderers = new List<Renderer>();

    // init details to use during reset
    private Vector3 initPosition;
    private Vector3 initScale;
    private Quaternion initRotation;

    readonly float sensitivity = 10f;
    readonly string gimbalTag = "Gimbal";
    readonly string defaultStatus = "No Mode Selected";

    private void Start()
    {
        initPosition = target.position;
        initRotation = target.rotation;
        initScale = target.localScale;

        // create a dictionary for each mode, handling things from the Mode class (see that script)
        // all modes need a type, a layer name and a highlight color.
        // scale is the only one with a min/max value to avoid weird stuff from happening
        modes = new Dictionary<ModeType, Mode>
        {
            { ModeType.Translate, new Mode(ModeType.Translate, translateGimbalLayer, Color.blue) },
            { ModeType.Rotate, new Mode(ModeType.Rotate, rotateGimbalLayer, Color.yellow) },
            { ModeType.Scale, new Mode(ModeType.Scale, scaleGimbalLayer, Color.red, minScale, maxScale) }
        };

        // Get all renderers with the "Gimbal" tag
        GameObject[] gimbalObjects = GameObject.FindGameObjectsWithTag(gimbalTag);
        foreach (GameObject gimbalObject in gimbalObjects)
        {
            Renderer gimbalRenderer = gimbalObject.GetComponent<Renderer>();
            if (gimbalRenderer != null)
            {
                gimbalRenderers.Add(gimbalRenderer);
            }
        }

        currentModeText.text = defaultStatus;
    }

    private void Update()
    {
        // only do stuff when buttons down.
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                foreach (KeyValuePair<ModeType, Mode> entry in modes)
                {
                    // compare layer tags to see if we should do something.
                    // we have different layers for each mode, so handle things per that. 
                    if ((1 << hit.collider.gameObject.layer) == entry.Value.layer)
                    {
                        currentMode = entry.Value;
                        selectedGimbal = hit.transform;

                        print(currentMode.type);

                        // Highlight selected gimbal
                        if (selectedGimbalRenderer != null)
                            selectedGimbalRenderer.material.color = originalColor;

                        // do color/material change when selected
                        selectedGimbalRenderer = hit.collider.GetComponent<Renderer>();
                        originalColor = selectedGimbalRenderer.material.color;
                        selectedGimbalRenderer.material = selectedMaterial;
                        selectedGimbalRenderer.material.color = currentMode.highlightColor;

                        // Set the material to translucent for all objects
                        // with the "Gimbal" tag that are not selected
                        foreach (Renderer gimbalRenderer in gimbalRenderers)
                        {
                            if (gimbalRenderer != selectedGimbalRenderer)
                            {
                                gimbalRenderer.material = translucentMaterial;
                            }
                        }
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // make everything default

            currentMode = null;
            selectedGimbal = null;

            // Restore color of selected gimbal
            if (selectedGimbalRenderer != null)
            {
                selectedGimbalRenderer.material.color = originalColor;
                selectedGimbalRenderer = null;
            }
            // make everything default (translucent)
            SetDefaultMaterial();

            currentModeText.text = defaultStatus;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Quit the application
            Application.Quit();
        }
    }

    private void LateUpdate()
    {
        // only do when we have a mode selected
        if (currentMode != null && selectedGimbal != null && Input.GetMouseButton(0))
        {
            float mouseDelta = Input.GetAxis("Mouse X") * sensitivity;
            currentModeText.text = currentMode.type.ToString();
            // handle each mode
            switch (currentMode.type)
            {
                case ModeType.Translate:
                    Vector3 translateDirection = selectedGimbal.forward;
                    target.Translate(translateDirection * mouseDelta, Space.World);
                    break;

                case ModeType.Rotate:
                    Quaternion rotation = Quaternion.AngleAxis(mouseDelta, selectedGimbal.forward);
                    target.rotation = rotation * target.rotation;
                    break;

                case ModeType.Scale:
                    Vector3 scaleChange = Vector3.one * mouseDelta;
                    Vector3 newScale = target.localScale + scaleChange;
                    // scale can be weird, so lets clamp it to a min/max
                    newScale = ClampVector3(newScale, currentMode.minValue.Value, currentMode.maxValue.Value);
                    target.localScale = newScale;
                    break;
            }
        }
    }

    // handle the scale clamping
    private Vector3 ClampVector3(Vector3 vector, float min, float max)
    {
        vector.x = Mathf.Clamp(vector.x, min, max);
        vector.y = Mathf.Clamp(vector.y, min, max);
        vector.z = Mathf.Clamp(vector.z, min, max);

        return vector;
    }

    // make sure gimbal materials are set to default
    private void SetDefaultMaterial()
    {
        foreach (Renderer gimbalRenderer in gimbalRenderers)
        {
            if (gimbalRenderer.material != translucentMaterial)
            {
                gimbalRenderer.material = translucentMaterial;
            }
        }
    }

    // reset the cubes transforms and set to default.
    public void ResetPlayable()
    {
        print("resetting cube");
        target.position = initPosition;
        target.rotation = initRotation;
        target.localScale = initScale;

        currentModeText.text = defaultStatus;

        SetDefaultMaterial();
    }
}