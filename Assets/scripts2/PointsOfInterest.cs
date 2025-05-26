using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a point of interest on the map that may require resources
/// </summary>
public class PointsOfInterest : MonoBehaviour
{
    public string pointName;
    public bool pointSelected;
    public int resourcesRequested;
    public Vector3 position;
    
    [Header("UI Elements")]
    public Toggle selectionToggle;
    public TMP_InputField resourceRequestField;
    public TextMeshProUGUI resourceCountText;
    public Button confirmRequestButton;
    
    [Header("Visualization")]
    public Color pointColor = Color.magenta;
    public float pointSize = 1.0f;
    
    void Start()
    {
        if (selectionToggle != null)
        {
            selectionToggle.onValueChanged.AddListener(OnPointSelected);
        }
        
        if (confirmRequestButton != null)
        {
            confirmRequestButton.onClick.AddListener(ConfirmResourceRequest);
        }
        
        position = transform.position;
    }
    
    /// <summary>
    /// Called when this point is selected or deselected
    /// </summary>
    public void OnPointSelected(bool isSelected)
    {
        pointSelected = isSelected;
    }
    
    /// <summary>
    /// Confirms a resource request for this point of interest
    /// </summary>
    public void ConfirmResourceRequest()
    {
        if (resourceRequestField != null && int.TryParse(resourceRequestField.text, out int count))
        {
            resourcesRequested = count;
            resourceCountText.text = $"{resourcesRequested}";
            Debug.Log($"Resource request at {pointName}: {resourcesRequested} units");
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = pointColor;
        Gizmos.DrawSphere(transform.position, pointSize);
        
        // Draw name above the point
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * pointSize * 1.5f, pointName);
        #endif
    }
}


