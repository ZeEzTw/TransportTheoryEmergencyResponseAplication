using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test script to verify the Transportation Problem solver is working correctly
/// </summary>
public class TransportationProblemTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private bool showDetailedOutput = true;
    
    void Start()
    {
        if (runTestOnStart)
        {
            RunTransportationTest();
        }
    }
    
    /// <summary>
    /// Run a simple test case to verify the transportation solver works
    /// </summary>
    public void RunTransportationTest()
    {
        Debug.Log("=== Transportation Problem Test ===");
        
        // Test case: 3 sources, 4 destinations
        float[] supply = { 300, 400, 500 };
        float[] demand = { 250, 350, 400, 200 };
        
        float[,] costs = new float[3, 4] {
            { 3, 1, 7, 4 },
            { 2, 6, 5, 9 },
            { 8, 3, 3, 2 }
        };
        
        string[] sourceNames = { "Station A", "Station B", "Station C" };
        string[] destinationNames = { "Point 1", "Point 2", "Point 3", "Point 4" };
        string[] routeNames = {
            "Route A-1", "Route A-2", "Route A-3", "Route A-4",
            "Route B-1", "Route B-2", "Route B-3", "Route B-4",
            "Route C-1", "Route C-2", "Route C-3", "Route C-4"
        };
        
        // Get or create the transportation problem solver
        TransportationProblem solver = GetComponent<TransportationProblem>();
        if (solver == null)
        {
            solver = gameObject.AddComponent<TransportationProblem>();
        }
        
        // Solve the problem
        List<TransportationProblem.AllocationSolution> solutions = solver.SolveTransportation(
            supply, demand, costs, sourceNames, destinationNames, routeNames);
        
        // Display results
        if (solutions.Count > 0)
        {
            Debug.Log("✅ Transportation Problem Solved Successfully!");
            
            if (showDetailedOutput)
            {
                float totalCost = 0;
                Debug.Log("\n📋 Allocation Results:");
                
                foreach (var solution in solutions)
                {
                    Debug.Log($"  • {solution.units:F0} units from {solution.sourceStationName} to {solution.destinationPointName}");
                    Debug.Log($"    Route: {solution.routeName} | Unit Cost: {solution.routeCost:F2} | Total: {solution.totalCost:F2}");
                    totalCost += solution.totalCost;
                }
                
                Debug.Log($"\n💰 Total Transportation Cost: {totalCost:F2} unit-minutes");
                
                // Verify solution constraints
                VerifySolution(solutions, supply, demand);
            }
        }
        else
        {
            Debug.LogError("❌ Transportation Problem Solver returned no solutions!");
        }
    }
    
    /// <summary>
    /// Verify that the solution satisfies supply and demand constraints
    /// </summary>
    private void VerifySolution(List<TransportationProblem.AllocationSolution> solutions, float[] supply, float[] demand)
    {
        Debug.Log("\n🔍 Verifying Solution Constraints:");
        
        // Check supply constraints
        for (int i = 0; i < supply.Length; i++)
        {
            float totalAllocated = 0;
            foreach (var solution in solutions)
            {
                if (solution.sourceIndex == i)
                {
                    totalAllocated += solution.units;
                }
            }
            
            bool supplyOk = Mathf.Abs(totalAllocated - supply[i]) < 0.1f;
            Debug.Log($"  Supply {i + 1}: {totalAllocated:F1}/{supply[i]:F1} {(supplyOk ? "✅" : "❌")}");
        }
        
        // Check demand constraints (excluding dummy destinations)
        for (int j = 0; j < demand.Length; j++)
        {
            float totalReceived = 0;
            foreach (var solution in solutions)
            {
                if (solution.destinationIndex == j)
                {
                    totalReceived += solution.units;
                }
            }
            
            bool demandOk = Mathf.Abs(totalReceived - demand[j]) < 0.1f;
            Debug.Log($"  Demand {j + 1}: {totalReceived:F1}/{demand[j]:F1} {(demandOk ? "✅" : "❌")}");
        }
    }
    
    /// <summary>
    /// Test with a different problem size
    /// </summary>
    [ContextMenu("Run Small Test")]
    public void RunSmallTest()
    {
        Debug.Log("=== Small Transportation Problem Test ===");
        
        // Smaller test case: 2 sources, 3 destinations
        float[] supply = { 100, 150 };
        float[] demand = { 80, 90, 80 };
        
        float[,] costs = new float[2, 3] {
            { 2, 3, 1 },
            { 5, 4, 9 }
        };
        
        string[] sourceNames = { "Source 1", "Source 2" };
        string[] destinationNames = { "Dest 1", "Dest 2", "Dest 3" };
        string[] routeNames = { "R1-1", "R1-2", "R1-3", "R2-1", "R2-2", "R2-3" };
        
        TransportationProblem solver = GetComponent<TransportationProblem>();
        if (solver == null)
        {
            solver = gameObject.AddComponent<TransportationProblem>();
        }
        
        List<TransportationProblem.AllocationSolution> solutions = solver.SolveTransportation(
            supply, demand, costs, sourceNames, destinationNames, routeNames);
        
        if (solutions.Count > 0)
        {
            Debug.Log("✅ Small test completed successfully!");
            foreach (var solution in solutions)
            {
                Debug.Log($"  {solution.units:F0} from {solution.sourceStationName} to {solution.destinationPointName} (cost: {solution.totalCost:F2})");
            }
        }
        else
        {
            Debug.LogError("❌ Small test failed!");
        }
    }
}
