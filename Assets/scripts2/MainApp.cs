using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Add using directive for LINQ

/// <summary>
/// Main application class that initializes the resource allocation system
/// </summary>
public class MainApp : MonoBehaviour
{
    [Header("Map Elements")]
    [SerializeField] private ResourceStations[] resourceStations;
    [SerializeField] private PointsOfInterest[] pointsOfInterest;
    
    [Header("UI Elements")]
    [SerializeField] private Button distributeButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private TextMeshProUGUI solutionText;
    [SerializeField] private GameObject solutionPanel;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private TransportationProblem transportationProblem;
      void Start()
    {
        // Find all resource stations and points of interest in the scene if not assigned
        if (resourceStations == null || resourceStations.Length == 0)
        {
            resourceStations = FindObjectsOfType<ResourceStations>();
            Debug.Log($"Found {resourceStations.Length} resource stations in the scene");
        }
        
        if (pointsOfInterest == null || pointsOfInterest.Length == 0)
        {
            pointsOfInterest = FindObjectsOfType<PointsOfInterest>();
            Debug.Log($"Found {pointsOfInterest.Length} points of interest in the scene");
        }
        
        // Initialize the transportation problem solver
        transportationProblem = GetComponent<TransportationProblem>();
        if (transportationProblem == null)
        {
            transportationProblem = gameObject.AddComponent<TransportationProblem>();
            Debug.Log("Added TransportationProblem component to MainApp");
        }
        
        // Set up button listeners
        if (distributeButton != null)
        {
            distributeButton.onClick.AddListener(RequestDistribution);
        }
        
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetRequests);
        }
        
        // Initialize solution text
        if (solutionText != null)
        {
            solutionText.text = "Select points of interest and request resources, then press Distribute Resources.";
        }
    }
    
    /// <summary>
    /// Initiates the resource distribution process when the distribute button is clicked
    /// </summary>
    public void RequestDistribution()
    {
        Debug.Log("Starting resource distribution...");
        
        // Arrays for the transportation problem
        float[] availableResources = new float[resourceStations.Length];
        
        // Count selected points of interest
        int selectedPoiCount = 0;
        foreach (PointsOfInterest poi in pointsOfInterest)
        {
            if (poi.pointSelected)
            {
                selectedPoiCount++;
            }
        }
        
        // If no points are selected, show a message and return
        if (selectedPoiCount == 0)
        {
            if (solutionText != null)
                solutionText.text = "No points of interest selected. Please select at least one point and request resources.";
            else
                Debug.LogWarning("No points of interest selected. Please select at least one point and request resources.");
            return;
        }
        
        float[] demandResources = new float[selectedPoiCount];
        float[,] costMatrix = new float[resourceStations.Length, selectedPoiCount];
        string[] stationNames = new string[resourceStations.Length];
        string[] poiNames = new string[selectedPoiCount];
        string[] routeNames = new string[resourceStations.Length * selectedPoiCount];
        
        // Retrieve information from resource stations and points of interest
        RetrieveInformation(availableResources, demandResources, costMatrix, stationNames, poiNames, routeNames);
        
        // Check if there are enough resources
        float totalAvailable = 0;
        float totalDemand = 0;
        
        for (int i = 0; i < availableResources.Length; i++)
        {
            totalAvailable += availableResources[i];
        }
        
        for (int j = 0; j < demandResources.Length; j++)
        {
            totalDemand += demandResources[j];
        }
        
        if (totalAvailable < totalDemand)
        {
            string warningMessage = $"Warning: Not enough resources available. Available: {totalAvailable}, Requested: {totalDemand}";
            if (solutionText != null)
                solutionText.text = warningMessage;
            Debug.LogWarning(warningMessage);
        }
        
        // Clear the solution text to prepare for the new solution
        if (solutionText != null)
            solutionText.text = "Calculating optimal solution...";
        
        // Solve the transportation problem
        List<TransportationProblem.AllocationSolution> solutions = 
            transportationProblem.SolveTransportation(
                availableResources, 
                demandResources, 
                costMatrix,
                stationNames,
                poiNames,
                routeNames);
        
        // Display the solution in the UI
        if (solutionText != null)
        {
            // Update the formatting of the detailed solution data in the .txt file
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("================ TRANSPORTATION PROBLEM SOLUTION ================");
            sb.AppendLine();
            
            // Display the allocation table
            sb.AppendLine("---------------- ALLOCATION TABLE ----------------");
            
            // Header
            sb.Append("Station/POI | ");
            for (int j = 0; j < demandResources.Length; j++)
            {
                if (!poiNames[j].Contains("Dummy"))
                    sb.Append($"{poiNames[j]} ({demandResources[j]}) | ");
            }
            sb.AppendLine();
            sb.AppendLine(new string('-', 50));
            
            // Rows
            for (int i = 0; i < availableResources.Length; i++)
            {
                if (!stationNames[i].Contains("Dummy"))
                {
                    sb.Append($"{stationNames[i]} ({availableResources[i]}) | ");

                    for (int j = 0; j < demandResources.Length; j++)
                    {
                        if (!poiNames[j].Contains("Dummy"))
                        {
                            var solution = solutions.Find(s => 
                                s.sourceStationName == stationNames[i] && 
                                s.destinationPointName == poiNames[j]);

                            if (solution != null)
                                sb.Append($"{solution.units} ({solution.routeCost:F2}) | ");
                            else
                                sb.Append($"- ({costMatrix[i, j]:F2}) | ");
                        }
                    }
                    sb.AppendLine();
                }
            }
            sb.AppendLine(new string('-', 50));
            
            // Allocations
            sb.AppendLine("---------------- ALLOCATIONS ----------------");
            foreach (var solution in solutions)
            {
                sb.AppendLine($"• {solution.units} units from {solution.sourceStationName} to {solution.destinationPointName} via {solution.routeName} (cost: {solution.routeCost:F2}, total: {solution.totalCost:F2})");
            }
            sb.AppendLine();
            
            // Total cost
            float totalCost = 0;
            foreach (var solution in solutions)
            {
                totalCost += solution.totalCost;
            }
            sb.AppendLine("---------------- SUMMARY ----------------");
            sb.AppendLine($"TOTAL COST: {totalCost:F2} minutes");
            sb.AppendLine(new string('=', 50));
            
            // Save the detailed solution to a .txt file
            string detailedSolutionPath = System.IO.Path.Combine(Application.dataPath, "DetailedSolution.txt");
            System.Text.StringBuilder fileContent = new System.Text.StringBuilder();

            fileContent.AppendLine("==============================");
            fileContent.AppendLine("  TRANSPORTATION PROBLEM SOLUTION");
            fileContent.AppendLine("==============================\n");

            fileContent.AppendLine("--- ALLOCATION TABLE ---");
            fileContent.AppendLine();

            // Header
            fileContent.Append("| ");
            for (int j = 0; j < demandResources.Length; j++)
            {
                if (!poiNames[j].Contains("Dummy"))
                    fileContent.Append($"{poiNames[j]} ({Mathf.RoundToInt(demandResources[j])}) | ");
            }
            fileContent.AppendLine();

            // Rows
            for (int i = 0; i < availableResources.Length; i++)
            {
                if (!stationNames[i].Contains("Dummy"))
                {
                    fileContent.Append($"{stationNames[i]} ({Mathf.RoundToInt(availableResources[i])}) | ");

                    for (int j = 0; j < demandResources.Length; j++)
                    {
                        if (!poiNames[j].Contains("Dummy"))
                        {
                            var solution = solutions.Find(s => 
                                s.sourceStationName == stationNames[i] && 
                                s.destinationPointName == poiNames[j]);

                            if (solution != null)
                                fileContent.Append($"{Mathf.RoundToInt(solution.units)} ({solution.routeCost:F2}) | ");
                            else
                                fileContent.Append($"- ({costMatrix[i, j]:F2}) | ");
                        }
                    }
                    fileContent.AppendLine();
                }
            }

            fileContent.AppendLine("\n--- ALLOCATIONS ---");
            foreach (var solution in solutions)
            {
                fileContent.AppendLine($"• {solution.units} units from {solution.sourceStationName} to {solution.destinationPointName} via {solution.routeName} (cost: {solution.routeCost:F2}, total: {solution.totalCost:F2})");
            }

            // Use the existing totalCost variable
            fileContent.AppendLine("\n==============================");
            fileContent.AppendLine("  SUMMARY");
            fileContent.AppendLine("==============================");
            fileContent.AppendLine($"Total Cost: {totalCost:F2} minutes\n");

            fileContent.AppendLine("File saved successfully.");
            fileContent.AppendLine("==============================");

            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            detailedSolutionPath = System.IO.Path.Combine(desktopPath, "DetailedSolution.txt");
            System.IO.File.WriteAllText(detailedSolutionPath, fileContent.ToString());
            Debug.Log("Fișier salvat pe desktop: " + detailedSolutionPath);

Debug.Log($"Detailed solution saved to: {detailedSolutionPath}");
            
            // Update the text box to display only the total cost
            solutionText.text = $"TOTAL COST: {totalCost:F2} minutes";
        }
        
        // Trigger animations and update UI for each allocation
        foreach (var solution in solutions)
        {
            // Find the index of the source station and destination point
            int stationIndex = System.Array.IndexOf(stationNames, solution.sourceStationName);
            int poiIndex = System.Array.IndexOf(poiNames, solution.destinationPointName);

            if (stationIndex >= 0 && poiIndex >= 0)
            {
                // Get the ResourceStations component of the station
                ResourceStations station = FindObjectsOfType<ResourceStations>()
                    .FirstOrDefault(s => s.stationName == solution.sourceStationName);

                if (station != null)
                {
                    // Start the route animation and update the police car count text
                    station.StartRouteAnimation(poiIndex, Mathf.RoundToInt(solution.units));
                }
            }
        }
        
        // Log results for debugging
        if (showDebugLogs)
        {
            LogDebugInfo(availableResources, demandResources, costMatrix, solutions);
        }
    }
    
    /// <summary>
    /// Retrieves information from resource stations and points of interest
    /// </summary>
    private void RetrieveInformation(
        float[] availableResources, 
        float[] demandResources, 
        float[,] costMatrix,
        string[] stationNames,
        string[] poiNames,
        string[] routeNames)
    {
        // Get available resources from stations
        for (int i = 0; i < resourceStations.Length; i++)
        {
            availableResources[i] = resourceStations[i].resourcesAvailable;
            stationNames[i] = resourceStations[i].stationName;
        }
        
        // Get demand and costs for selected points of interest
        int selectedPoiIndex = 0;
        for (int j = 0; j < pointsOfInterest.Length; j++)
        {
            if (pointsOfInterest[j].pointSelected)
            {
                // Store demand
                demandResources[selectedPoiIndex] = pointsOfInterest[j].resourcesRequested;
                poiNames[selectedPoiIndex] = pointsOfInterest[j].pointName;
                
                // Calculate costs (travel time) from each station to this POI
                for (int i = 0; i < resourceStations.Length; i++)
                {
                    // Fix the retrieval of cost information - we need to use the correct index for the point of interest
                    costMatrix[i, selectedPoiIndex] = resourceStations[i].GetCostToPoint(j);
                    
                    // Use a formula for routeNames that doesn't depend on the selectedPoiIndex value
                    int routeIndex = i * pointsOfInterest.Length + j;
                    routeNames[routeIndex] = resourceStations[i].GetRouteNameToPoint(j);
                    
                    Debug.Log($"Cost from {stationNames[i]} to {poiNames[selectedPoiIndex]}: {costMatrix[i, selectedPoiIndex]:F2}");
                }
                
                selectedPoiIndex++;
            }
        }
    }
      /// <summary>
    /// Resets all resource requests
    /// </summary>
    public void ResetRequests()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("All resource requests have been reset");
    }
    
    /// <summary>
    /// Logs debug information
    /// </summary>
    private void LogDebugInfo(
        float[] availableResources, 
        float[] demandResources, 
        float[,] costMatrix,
        List<TransportationProblem.AllocationSolution> solutions)
    {
        Debug.Log("=== Transportation Problem Debug Info ===");
        
        // Log available resources
        string availableStr = "Available Resources: ";
        for (int i = 0; i < availableResources.Length; i++)
        {
            availableStr += $"{availableResources[i]} ";
        }
        Debug.Log(availableStr);
        
        // Log demand resources
        string demandStr = "Demand Resources: ";
        for (int j = 0; j < demandResources.Length; j++)
        {
            demandStr += $"{demandResources[j]} ";
        }
        Debug.Log(demandStr);
        
        // Log cost matrix
        Debug.Log("Cost Matrix:");
        for (int i = 0; i < resourceStations.Length; i++)
        {
            string rowStr = "";
            for (int j = 0; j < demandResources.Length; j++)
            {
                rowStr += $"{costMatrix[i, j]:F2} ";
            }
            Debug.Log(rowStr);
        }
        
        // Log solutions
        Debug.Log($"Found {solutions.Count} allocation solutions");
        foreach (TransportationProblem.AllocationSolution solution in solutions)
        {
            Debug.Log(solution.ToString());
        }
    }
    
    /// <summary>
    /// Logs the solution to the console
    /// </summary>
    private void DisplaySolution(string solution)
    {
        // For debug purposes only, log the solution to the console
        Debug.Log(solution);
        
        // Don't update UI as we're now handling that in RequestDistribution
    }
}