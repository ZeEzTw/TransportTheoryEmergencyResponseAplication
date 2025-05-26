using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a resource station on the map which has a certain number of units available
/// </summary>
public class ResourceStations : MonoBehaviour
{
    public string stationName;
    public int resourcesAvailable;
    public Vector3 position;

    [Header("Route Information")]
    [Tooltip("Cost (in minutes) to travel from this station to each point of interest")]
    public float[] costInTimeTillPoint;
    [Tooltip("Names of routes from this station to each point of interest")]
    public string[] routeNames;

    [Header("UI Elements")]
    public TMP_InputField resourceInputField;
    public TextMeshProUGUI resourceCountText;
    public Button updateButton;
    
    [Header("Visualization")]
    public Color stationColor = Color.blue;
    public float pointSize = 1.0f;
    
    private void Start()
    {
        if (updateButton != null)
        {
            updateButton.onClick.AddListener(UpdateResourceCount);
        }
        
        position = transform.position;
    }
    
    /// <summary>
    /// Updates the resource count based on input field value
    /// </summary>
    public void UpdateResourceCount()
    {
        if (resourceInputField != null && int.TryParse(resourceInputField.text, out int count))
        {
            resourcesAvailable = count;
            if (resourceCountText != null)
            {
                resourceCountText.text = $"{resourcesAvailable}";
            }
            Debug.Log($"Updated {stationName} resource count to {resourcesAvailable}");
        }
    }
    
    /// <summary>
    /// Sets the cost to a specific point of interest
    /// </summary>
    public void SetCostToPoint(int pointIndex, float cost, string routeName)
    {
        // Initialize arrays if needed
        if (costInTimeTillPoint == null || routeNames == null)
        {
            // Find all points of interest in the scene to determine the array size
            PointsOfInterest[] allPois = FindObjectsOfType<PointsOfInterest>();
            int poiCount = allPois.Length;

            costInTimeTillPoint = new float[poiCount];
            routeNames = new string[poiCount];
        }

        // Store the cost and route information
        if (pointIndex >= 0 && pointIndex < costInTimeTillPoint.Length)
        {
            costInTimeTillPoint[pointIndex] = cost;
            routeNames[pointIndex] = routeName;
        }
    }
    
    /// <summary>
    /// Gets the cost to a specific point of interest by index
    /// </summary>
    public float GetCostToPoint(int pointIndex)
    {
        if (costInTimeTillPoint == null || pointIndex < 0 || pointIndex >= costInTimeTillPoint.Length)
            return 0;

        return costInTimeTillPoint[pointIndex];
    }
    
    /// <summary>
    /// Gets the route name to a specific point of interest by index
    /// </summary>
    public string GetRouteNameToPoint(int pointIndex)
    {
        if (routeNames == null || pointIndex < 0 || pointIndex >= routeNames.Length)
            return $"Route from {stationName} to Point {pointIndex}";

        return routeNames[pointIndex];
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = stationColor;
        Gizmos.DrawSphere(transform.position, pointSize);
        
        // Draw name above the point
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * pointSize * 1.5f, stationName);
        #endif
    }
}
