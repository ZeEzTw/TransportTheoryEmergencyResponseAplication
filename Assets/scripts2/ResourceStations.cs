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
    [Header("Route Animations")]
    [Tooltip("The animation with police car to each point of interest")]
    public GameObject[] routeAnimations;
    public TextMeshProUGUI[] policeCarCountTexts;
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
    public void RemoveUnitsFromSection(int unitsToRemove)
    {
        if (unitsToRemove <= 0 || resourcesAvailable < unitsToRemove)
        {
            Debug.LogWarning($"Cannot remove {unitsToRemove} units from {stationName}. Not enough resources available.");
            return;
        }

        resourcesAvailable -= unitsToRemove;
        if (resourceCountText != null)
        {
            resourceCountText.text = $"{resourcesAvailable}";
        }
        
        Debug.Log($"Removed {unitsToRemove} units from {stationName}. Remaining: {resourcesAvailable}");
    }
    /// <summary>
    /// Starts the animation for a specific route, enables the route animation object, and updates the police car count text.
    /// </summary>
    /// <param name="pointIndex">The index of the point of interest.</param>
    /// <param name="unitsSent">The number of units sent to the point of interest.</param>
    public void StartRouteAnimation(int pointIndex, int unitsSent)
    {
        if (unitsSent <= 0 || Mathf.Abs(unitsSent) < 1e-5f) // Check for very small values
        {
            Debug.LogWarning($"No valid units to send to point {pointIndex} from {stationName}. Animation will not start.");
            return;
        }

        if (routeAnimations != null && pointIndex >= 0 && pointIndex < routeAnimations.Length)
        {
            // Enable the route animation object
            GameObject routeAnimation = routeAnimations[pointIndex];
            RemoveUnitsFromSection(unitsSent);
            if (routeAnimation != null)
            {
                routeAnimation.SetActive(true);
            }
        }

        if (policeCarCountTexts != null && pointIndex >= 0 && pointIndex < policeCarCountTexts.Length)
        {
            // Update the police car count text
            TextMeshProUGUI policeCarCountText = policeCarCountTexts[pointIndex];
            if (policeCarCountText != null)
            {
                policeCarCountText.text = unitsSent.ToString();
            }
        }
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
