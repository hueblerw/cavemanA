using System;
using System.Collections.Generic;
using UnityEngine;
using CavemanLand.Models;

public class TileInspector : MonoBehaviour
{
    private TilemapManager tilemapManager;
    private World world;
    private Camera mainCamera;

    // Track last clicked tile for refreshing
    private int lastClickedX = -1;
    private int lastClickedZ = -1;

    // Selection indicator (red box)
    private GameObject selectionBox;
    private LineRenderer lineRenderer;

    void Start()
    {
        mainCamera = Camera.main;
        tilemapManager = FindAnyObjectByType<TilemapManager>();

        WorldController1 worldController = FindAnyObjectByType<WorldController1>();
        if (worldController != null)
        {
            world = worldController.GetWorld();
        }

        // Create selection box indicator
        CreateSelectionBox();

        // Subscribe to day change events
        if (DateController.Instance != null)
        {
            DateController.Instance.OnDayChanged += RefreshTileInfo;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (DateController.Instance != null)
        {
            DateController.Instance.OnDayChanged -= RefreshTileInfo;
        }

        // Clean up selection box
        if (selectionBox != null)
        {
            Destroy(selectionBox);
        }
    }

    void CreateSelectionBox()
    {
        // Create a GameObject for the selection indicator
        selectionBox = new GameObject("SelectionBox");
        selectionBox.transform.parent = transform;

        // Add LineRenderer component
        lineRenderer = selectionBox.AddComponent<LineRenderer>();

        // Configure LineRenderer
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 5; // 5 points to draw a closed square
        lineRenderer.loop = true;
        lineRenderer.sortingOrder = 100; // Render on top of everything

        // Hide initially
        selectionBox.SetActive(false);
    }

    void UpdateSelectionBox(int x, int z)
    {
        if (selectionBox == null || lineRenderer == null)
            return;

        // Show the box
        selectionBox.SetActive(true);

        // Define the four corners of the tile (1x1 square at tile position)
        // Tiles occupy x to x+1, z to z+1
        Vector3[] corners = new Vector3[5];
        corners[0] = new Vector3(x, z, -1);         // Bottom-left
        corners[1] = new Vector3(x + 1, z, -1);     // Bottom-right
        corners[2] = new Vector3(x + 1, z + 1, -1); // Top-right
        corners[3] = new Vector3(x, z + 1, -1);     // Top-left
        corners[4] = corners[0];                     // Close the loop

        lineRenderer.SetPositions(corners);
    }

    void Update()
    {
        // Left click to inspect tile
        if (Input.GetMouseButtonDown(0))
        {
            InspectTileAtMouse();
        }
    }

    void InspectTileAtMouse()
    {
        if (world == null || mainCamera == null)
        {
            Debug.LogWarning("TileInspector: No world data or camera found");
            return;
        }

        // Convert mouse position to world position
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Floor to get tile coordinates (tiles are 1x1 squares at integer positions)
        int x = Mathf.FloorToInt(worldPos.x);
        int z = Mathf.FloorToInt(worldPos.y);

        // Check bounds
        if (x < 0 || x >= World.X || z < 0 || z >= World.Z)
        {
            Debug.Log($"Clicked outside world bounds: ({x}, {z})");
            return;
        }

        // Store clicked tile coordinates
        lastClickedX = x;
        lastClickedZ = z;

        // Update selection box position
        UpdateSelectionBox(x, z);

        // Print comprehensive tile info
        PrintTileInfo(x, z);
    }

    // Refresh tile info for the last clicked tile (called when day changes)
    void RefreshTileInfo()
    {
        if (lastClickedX >= 0 && lastClickedZ >= 0)
        {
            PrintTileInfo(lastClickedX, lastClickedZ);
        }
    }

    void PrintTileInfo(int x, int z)
    {
        // Get viewing day from DateController if available, otherwise use world's current date
        int viewingDay;
        int viewingDayIndex;

        if (DateController.Instance != null)
        {
            viewingDay = DateController.Instance.GetViewingDay();
            viewingDayIndex = DateController.Instance.GetViewingDayIndex();
        }
        else
        {
            viewingDay = world.currentDate.day;
            viewingDayIndex = world.currentDate.day - 1;
        }

        int currentDay = viewingDayIndex;

        // Build formatted string for UI
        string output = "";
        output += $"<b>TILE ({x}, {z}) - Day {viewingDay}</b>\n\n";

        // Terrain
        output += "<b>TERRAIN</b>\n";
        output += $"  Elevation: {world.terrains.elevations[x, z]:F2}\n";
        output += $"  Hill %: {world.terrains.hillPercents[x, z] * 100:F1}%\n\n";

        // Habitat
        var habitat = world.habitats.habitats[x, z];
        output += "<b>HABITAT</b>\n";
        output += $"  Quality: {habitat.currentLevel:F1}\n";
        output += $"  Game Quality: {habitat.gameCurrentLevel:F1}\n";
        for (int i = 0; i < habitat.typePercents.Length; i++)
        {
            if (habitat.typePercents[i] > 0)
            {
                string habitatName = Habitats.habitatMapping[i];
                output += $"  {habitatName}: {habitat.typePercents[i]}%\n";
            }
        }
        output += "\n";

        // Minerals
        output += "<b>MINERALS</b>\n";
        output += $"  {world.terrains.minerals[x, z].ToString()}\n\n";

        // Daily data
        int tempToday = world.temps.GetTempForDay(x, z, currentDay);
        double precipToday = world.precips.GetPrecipForDay(x, z, currentDay);
        double surfaceWaterToday = world.precips.GetSurfaceWaterForDay(x, z, currentDay);

        output += "<b>DAILY DATA</b>\n";
        output += $"  Temperature: {tempToday}°\n";
        output += $"  Precipitation: {precipToday:F2}\n";
        output += $"  Surface Water: {surfaceWaterToday:F2}\n\n";

        // River
        output += "<b>RIVER</b>\n";
        output += $"  Flow Rate: {world.precips.flowRates[x, z]:F2}\n";
        output += $"  Flow Direction: {world.precips.flowDirections[x, z]}\n";
        output += $"  River Bed Depth: {world.precips.riverBanks[x, z].ToString()}\n\n";

        // Vegetation
        int trees = habitat.getTrees();
        double grass;
        double grazingToday = habitat.getGrazing(0, tempToday, out grass);
        double foliage = habitat.getFoilage(trees, tempToday);
        double seeds = habitat.getSeeds(grass, trees);

        output += "<b>VEGETATION</b>\n";
        output += $"  Trees: {trees}\n";
        output += $"  Grazing: {grazingToday:F2}\n";
        output += $"  Foliage: {foliage:F2}\n";
        output += $"  Seeds: {seeds:F2}\n\n";

        // Gatherables
        output += "<b>GATHERABLES</b>\n";
        Dictionary<string, double[]> crops = habitat.getCrops();
        if (crops != null && crops.Count > 0)
        {
            foreach (var kvp in crops)
            {
                double todayAmount = kvp.Value[currentDay];
                double totalYearly = 0;
                foreach (double dailyAmount in kvp.Value)
                {
                    totalYearly += dailyAmount;
                }
                output += $"  {kvp.Key}\n";
                output += $"    Today: {todayAmount:F2}, Yearly: {totalYearly:F2}\n";
            }
        }
        else
        {
            output += "  No crops data\n";
        }
        output += "\n";

        // Small game
        output += "<b>SMALL GAME</b>\n";
        try
        {
            Dictionary<string, double> vegetation = new Dictionary<string, double>();
            vegetation["grass"] = grass;
            vegetation["seeds"] = seeds;
            vegetation["trees"] = trees;

            Dictionary<string, int> gameToday = habitat.getGame(
                vegetation,
                world.terrains.elevations[x, z],
                surfaceWaterToday
            );

            foreach (var kvp in gameToday)
            {
                output += $"  {kvp.Key}: {kvp.Value}\n";
            }
        }
        catch (Exception e)
        {
            output += $"  Error: {e.Message}\n";
        }

        // Send to UI panel
        if (TileInfoPanel.Instance != null)
        {
            TileInfoPanel.Instance.SetText(output);
        }

        // Also log to console
        Debug.Log($"\n========== TILE INFO: ({x}, {z}) ==========\n{output}==========================================\n");
    }
}
