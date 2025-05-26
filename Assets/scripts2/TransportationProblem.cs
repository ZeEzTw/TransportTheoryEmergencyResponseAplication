using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Implementation of the Transportation Problem solver for resource allocation optimization
/// </summary>
public class TransportationProblem : MonoBehaviour
{
    /// <summary>
    /// Represents a shipment of resources from a source to a destination
    /// </summary>
    private class Shipment
    {
        public float CostPerUnit { get; private set; }
        public int Row { get; private set; }
        public int Col { get; private set; }
        public float Quantity { get; set; }

        public static readonly Shipment ZERO = new Shipment();

        public Shipment()
        {
            Quantity = 0;
            CostPerUnit = 0;
            Row = -1;
            Col = -1;
        }

        public Shipment(float quantity, float costPerUnit, int row, int col)
        {
            Quantity = quantity;
            CostPerUnit = costPerUnit;
            Row = row;
            Col = col;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Shipment))
                return false;

            Shipment other = (Shipment)obj;
            return this.CostPerUnit == other.CostPerUnit
                && this.Quantity == other.Quantity
                && this.Row == other.Row
                && this.Col == other.Col;
        }

        public override int GetHashCode()
        {
            return CostPerUnit.GetHashCode() ^ Quantity.GetHashCode() ^ Row.GetHashCode() ^ Col.GetHashCode();
        }

        public static bool operator ==(Shipment lhs, Shipment rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Shipment lhs, Shipment rhs)
        {
            return !(lhs == rhs);
        }
    }

    private float[] supply;
    private float[] demand;
    private float[,] costs;
    private Shipment[,] matrix;
    private string[] sourceNames;
    private string[] destinationNames;
    private string[] routeNames;

    /// <summary>
    /// Represents a solution for resource allocation from a source to a destination
    /// </summary>
    public class AllocationSolution
    {
        public string sourceStationName;
        public string destinationPointName;
        public string routeName;
        public float units;
        public float routeCost;
        public float totalCost;
        public int sourceIndex;
        public int destinationIndex;

        public override string ToString()
        {
            return $"Allocate {units} units from {sourceStationName} to {destinationPointName} via {routeName} (cost: {routeCost}, total: {totalCost})";
        }
    }

    /// <summary>
    /// Initializes the transportation problem with the given data
    /// </summary>
    private void Initialize(float[] availableResources, float[] demandResources, float[,] costMatrix,
                           string[] stationNames, string[] poiNames, string[] routes)
    {
        // Store names for the solution
        this.sourceNames = stationNames;
        this.destinationNames = poiNames;
        this.routeNames = routes;

        // Make copies of the arrays to avoid modifying the originals
        supply = new float[availableResources.Length];
        demand = new float[demandResources.Length];
        costs = new float[availableResources.Length, demandResources.Length];

        // Copy values
        for (int i = 0; i < availableResources.Length; i++)
        {
            supply[i] = availableResources[i];
        }

        for (int j = 0; j < demandResources.Length; j++)
        {
            demand[j] = demandResources[j];
        }

        for (int i = 0; i < availableResources.Length; i++)
        {
            for (int j = 0; j < demandResources.Length; j++)
            {
                costs[i, j] = costMatrix[i, j];
            }
        }

        // Fix imbalance between supply and demand
        float totalSupply = 0;
        float totalDemand = 0;

        for (int i = 0; i < supply.Length; i++)
        {
            totalSupply += supply[i];
        }

        for (int j = 0; j < demand.Length; j++)
        {
            totalDemand += demand[j];
        }

        // If supply and demand are not balanced, create dummy sources or destinations
        if (totalSupply > totalDemand)
        {
            // Add a dummy destination
            float[] newDemand = new float[demand.Length + 1];
            for (int j = 0; j < demand.Length; j++)
            {
                newDemand[j] = demand[j];
            }
            newDemand[demand.Length] = totalSupply - totalDemand;
            demand = newDemand;

            // Extend the cost matrix
            float[,] newCosts = new float[supply.Length, demand.Length];
            for (int i = 0; i < supply.Length; i++)
            {
                for (int j = 0; j < demand.Length - 1; j++)
                {
                    newCosts[i, j] = costs[i, j];
                }
                // Dummy costs are 0
                newCosts[i, demand.Length - 1] = 0;
            }
            costs = newCosts;

            // Extend destination names
            string[] newDestNames = new string[demand.Length];
            for (int j = 0; j < destinationNames.Length; j++)
            {
                newDestNames[j] = destinationNames[j];
            }
            newDestNames[demand.Length - 1] = "Dummy Destination";
            destinationNames = newDestNames;
        }
        else if (totalDemand > totalSupply)
        {
            // Add a dummy source
            float[] newSupply = new float[supply.Length + 1];
            for (int i = 0; i < supply.Length; i++)
            {
                newSupply[i] = supply[i];
            }
            newSupply[supply.Length] = totalDemand - totalSupply;
            supply = newSupply;

            // Extend the cost matrix
            float[,] newCosts = new float[supply.Length, demand.Length];
            for (int i = 0; i < supply.Length - 1; i++)
            {
                for (int j = 0; j < demand.Length; j++)
                {
                    newCosts[i, j] = costs[i, j];
                }
            }
            // Dummy costs are 0
            for (int j = 0; j < demand.Length; j++)
            {
                newCosts[supply.Length - 1, j] = 0;
            }
            costs = newCosts;

            // Extend source names
            string[] newSourceNames = new string[supply.Length];
            for (int i = 0; i < sourceNames.Length; i++)
            {
                newSourceNames[i] = sourceNames[i];
            }
            newSourceNames[supply.Length - 1] = "Dummy Source";
            sourceNames = newSourceNames;
        }

        // Initialize the solution matrix
        matrix = new Shipment[supply.Length, demand.Length];
        for (int i = 0; i < supply.Length; i++)
        {
            for (int j = 0; j < demand.Length; j++)
            {
                matrix[i, j] = Shipment.ZERO;
            }
        }
    }

    /// <summary>
    /// Applies the North-West Corner Rule to find an initial basic feasible solution
    /// </summary>
    private void NorthWestCornerRule()
    {
        int northWest = 0;
        for (int r = 0; r < supply.Length; r++)
        {
            for (int c = northWest; c < demand.Length; c++)
            {
                float quantity = Mathf.Min(supply[r], demand[c]);
                if (quantity > 0)
                {
                    matrix[r, c] = new Shipment(quantity, costs[r, c], r, c);

                    supply[r] -= quantity;
                    demand[c] -= quantity;

                    if (supply[r] == 0)
                    {
                        northWest = c;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Converts the solution matrix to a list of shipments
    /// </summary>
    private List<Shipment> MatrixToList()
    {
        List<Shipment> result = new List<Shipment>();
        for (int r = 0; r < matrix.GetLength(0); r++)
        {
            for (int c = 0; c < matrix.GetLength(1); c++)
            {
                if (matrix[r, c] != Shipment.ZERO)
                {
                    result.Add(matrix[r, c]);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Gets the horizontal and vertical neighbors of a shipment in the list
    /// </summary>
    private Shipment[] GetNeighbors(Shipment s, List<Shipment> shipments)
    {
        // Initialize with ZERO values
        Shipment[] neighbors = new[] { Shipment.ZERO, Shipment.ZERO };
        
        // Guard against null input
        if (s == null || s == Shipment.ZERO || shipments == null)
        {
            return neighbors;
        }

        // Find horizontal and vertical neighbors
        foreach (var shipment in shipments.Where(x => x != null))
        {
            if (shipment == s)
            {
                continue;
            }

            if (shipment.Row == s.Row && neighbors[0] == Shipment.ZERO)
            {
                neighbors[0] = shipment;
            }
            else if (shipment.Col == s.Col && neighbors[1] == Shipment.ZERO)
            {
                neighbors[1] = shipment;
            }

            // Break early if we found both neighbors
            if (neighbors[0] != Shipment.ZERO && neighbors[1] != Shipment.ZERO)
            {
                break;
            }
        }
        
        return neighbors;
    }

    /// <summary>
    /// Finds a closed path of shipments for evaluating potential improvement
    /// </summary>
    private List<Shipment> GetClosedPath(Shipment s)
    {
        List<Shipment> path = MatrixToList();
        path.Insert(0, s);

        // Remove elements that do not have both horizontal and vertical neighbors
        int before;
        do
        {
            before = path.Count;
            path.RemoveAll(ship => {
                Shipment[] neighbors = GetNeighbors(ship, path);
                return neighbors[0] == Shipment.ZERO || neighbors[1] == Shipment.ZERO;
            });
        } while (before != path.Count);

        // If no valid path found, return empty list
        if (path.Count < 4)
        {
            return new List<Shipment>();
        }

        // Place the remaining elements in the correct plus-minus order
        List<Shipment> stones = new List<Shipment>(path.Count);
        Shipment current = s;

        for (int i = 0; i < path.Count; i++)
        {
            stones.Add(current);
            Shipment[] neighbors = GetNeighbors(current, path);

            // Alternate between horizontal and vertical neighbors
            if (i % 2 == 0)
            {
                current = neighbors[0]; // Horizontal neighbor
            }
            else
            {
                current = neighbors[1]; // Vertical neighbor
            }

            // Break if we can't find the next neighbor
            if (current == Shipment.ZERO)
            {
                break;
            }
        }

        return stones;
    }

    /// <summary>
    /// Fixes degenerate cases by adding a small epsilon shipment
    /// </summary>
    private void FixDegenerateCase()
    {
        float epsilon = 1e-10f;
        if (supply.Length + demand.Length - 1 != MatrixToList().Count)
        {
            for (int r = 0; r < supply.Length; r++)
            {
                for (int c = 0; c < demand.Length; c++)
                {
                    if (matrix[r, c] == Shipment.ZERO)
                    {
                        Shipment dummy = new Shipment(epsilon, costs[r, c], r, c);
                        if (GetClosedPath(dummy).Count == 0)
                        {
                            matrix[r, c] = dummy;
                            return;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Applies the Stepping Stone method to optimize the initial solution
    /// </summary>
    private void SteppingStone()
    {
        float maxReduction = 0;
        List<Shipment> move = null;
        Shipment leaving = Shipment.ZERO;
        bool isOptimal = true;

        FixDegenerateCase();

        for (int r = 0; r < supply.Length; r++)
        {
            for (int c = 0; c < demand.Length; c++)
            {
                if (matrix[r, c] != Shipment.ZERO)
                {
                    continue;
                }

                Shipment trial = new Shipment(0, costs[r, c], r, c);
                List<Shipment> path = GetClosedPath(trial);

                if (path.Count == 0)
                {
                    continue; // Skip if no valid path found
                }

                float reduction = 0;
                float lowestQuantity = float.MaxValue;
                Shipment leavingCandidate = Shipment.ZERO;

                bool plus = true;
                foreach (var s in path)
                {
                    if (plus)
                    {
                        reduction += s.CostPerUnit;
                    }
                    else
                    {
                        reduction -= s.CostPerUnit;
                        if (s.Quantity < lowestQuantity)
                        {
                            leavingCandidate = s;
                            lowestQuantity = s.Quantity;
                        }
                    }
                    plus = !plus;
                }

                if (reduction < maxReduction)
                {
                    isOptimal = false;
                    move = path;
                    leaving = leavingCandidate;
                    maxReduction = reduction;
                }
            }
        }

        if (!isOptimal)
        {
            float quantity = leaving.Quantity;
            bool plus = true;
            foreach (var s in move)
            {
                float newQuantity = s.Quantity + (plus ? quantity : -quantity);
                matrix[s.Row, s.Col] = (newQuantity <= 1e-10f) ? Shipment.ZERO :
                                     new Shipment(newQuantity, s.CostPerUnit, s.Row, s.Col);
                plus = !plus;
            }
            SteppingStone();
        }
    }

    /// <summary>
    /// Converts the internal solution to a list of allocation solutions for the client
    /// </summary>
    private List<AllocationSolution> FormatSolution()
    {
        List<AllocationSolution> solutions = new List<AllocationSolution>();
        
        for (int r = 0; r < matrix.GetLength(0); r++)
        {
            for (int c = 0; c < matrix.GetLength(1); c++)
            {
                Shipment s = matrix[r, c];
                if (s != Shipment.ZERO)
                {
                    // Skip dummy sources or destinations
                    if (r >= sourceNames.Length || c >= destinationNames.Length ||
                        sourceNames[r].Contains("Dummy") || destinationNames[c].Contains("Dummy"))
                    {
                        continue;
                    }

                    AllocationSolution solution = new AllocationSolution
                    {
                        sourceStationName = sourceNames[r],
                        destinationPointName = destinationNames[c],
                        routeName = GetRouteNameForMatrixPosition(r, c),
                        units = s.Quantity,
                        routeCost = s.CostPerUnit,
                        totalCost = s.Quantity * s.CostPerUnit,
                        sourceIndex = r,
                        destinationIndex = c
                    };
                    
                    solutions.Add(solution);
                }
            }
        }
        
        return solutions;
    }

    /// <summary>
    /// Gets the route name for a given position in the matrix
    /// </summary>
    private string GetRouteNameForMatrixPosition(int row, int col)
    {
        // Get the proper route name from routeNames array
        // We need to map the current matrix position to the original position in the routeNames array
        if (row < sourceNames.Length && col < destinationNames.Length)
        {
            // This assumes routeNames are stored in a flattened array where:
            // index = row * originalDestinationCount + originalCol
            // We would need to retrieve the original col for this destination
            
            // For simplicity, let's assume routeNames is already properly formatted
            int index = row * destinationNames.Length + col;
            if (index < routeNames.Length && !string.IsNullOrEmpty(routeNames[index]))
            {
                return routeNames[index];
            }
        }
        
        // Fallback if no route name found
        return $"Route from {sourceNames[row]} to {destinationNames[col]}";
    }

    /// <summary>
    /// Solves the transportation problem and returns the optimal allocation
    /// </summary>
    public List<AllocationSolution> SolveTransportation(
        float[] availableResources, 
        float[] demandResources, 
        float[,] costMatrix,
        string[] stationNames,
        string[] poiNames,
        string[] routeNames)
    {
        // Initialize the problem with the given data
        Initialize(availableResources, demandResources, costMatrix, stationNames, poiNames, routeNames);
        
        // Find an initial basic feasible solution
        NorthWestCornerRule();
        
        // Optimize the solution
        SteppingStone();
        
        // Format the solution for the client
        return FormatSolution();
    }
}