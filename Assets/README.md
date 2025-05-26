# Resource Allocation Application Guide

This application implements a transportation problem algorithm to solve resource allocation problems. The system optimally distributes resources (police, ambulance, fire departments, special forces) from their stations to points of interest based on requests, minimizing total travel time.

## Getting Started

1. Open the application in Unity
2. Navigate to the SampleScene
3. Press Play

## Map Elements

- **Blue Points**: Police Stations
- **Red Points**: Ambulance Stations
- **Yellow Points**: Fire Departments
- **Green Points**: Special Forces Stations
- **Magenta Points**: Points of Interest (destinations)
- **Gray Lines**: Routes between stations and points of interest

## Using the Application

### As a Dispatcher:

1. Select a Point of Interest from the dropdown menu and click "Select POI"
2. Enter the number of units needed for each resource type:
   - Police units
   - Ambulance units
   - Fire Department units
   - Special Forces units
3. Click "Confirm Request" to register the resource request
4. Repeat steps 1-3 for additional points of interest
5. When all requests are entered, click "Solve Allocation"
6. The system will display the optimal allocation of resources:
   - Which units should go where
   - Along which routes
   - Total travel time (unit-minutes)
7. To start over, click "Reset" to clear all requests

### Map Navigation:

- **Move Camera**: WASD or Arrow Keys
- **Fast Movement**: Hold Shift while moving
- **Rotate Camera**: Right-click and drag
- **Zoom**: Mouse wheel

## Setting Up Your Own Scenario

To create a custom scenario:

1. Add the `MapElementCreator` component to an empty GameObject
2. Use the context menu options to create:
   - Resource Stations
   - Points of Interest 
   - Routes between them
3. Alternatively, use the `SetupSampleScene` component to automatically generate a sample scenario

## Transportation Algorithm

The application uses the Northwest Corner method to solve the transportation problem, which:

1. Balances supply and demand (if necessary)
2. Creates a cost matrix based on travel times
3. Allocates resources to minimize the total travel time
4. Returns a solution indicating which resources should be sent from which stations to which points of interest

## Adding New Resource Types

To add new resource types:

1. Open the `ResourceType.cs` file
2. Add your new resource type to the enum
3. Update the UI to include the new resource type
4. Assign the new resource type to stations in the scene

## Customizing Routes

Routes between stations and points of interest have:

- A source point
- A destination point
- A travel time (in minutes)

You can create multiple routes between the same points to represent alternative paths with different travel times.
