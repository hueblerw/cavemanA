using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CavemanLand.Models.ViewModels;
using CavemanLand.Models.GenericModels;
using CavemanLand.Models;


public class MapView2D : MonoBehaviour
{
    public GameObject tilePrefab;
    public int mapWidth = 80;
    public int mapLength = 60;
    public float tileSize = 1.0f;

    private GameObject[,] tileGrid;
    private World world;

    void Start()
    {
        // Find the world from the WorldController1
        WorldController1 worldController = FindObjectOfType<WorldController1>();
        if (worldController != null)
        {
            world = worldController.GetWorld();
        }

        if (world != null)
        {
            // Use world dimensions instead of hardcoded values
            mapWidth = World.X;
            mapLength = World.Z;
        }

        tileGrid = new GameObject[mapWidth, mapLength];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapLength; y++)
            {
                Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);
                GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity, transform);

                // Assign initial data to the tile
                Tile2D tile = newTile.GetComponent<Tile2D>();
                tile.Initialize(GetTileData(x, y)); // GetTileData fetches TileData2D from your 2D array

                tileGrid[x, y] = newTile;
            }
        }
    }

    public void UpdateTile(int x, int y, TileData2D newData)
    {
        if (tileGrid != null && x >= 0 && x < mapWidth && y >= 0 && y < mapLength)
        {
            Tile2D tile = tileGrid[x, y].GetComponent<Tile2D>();
            tile.Initialize(newData); // Reinitialize the tile with new data
        }
    }

    public void RefreshAllTiles()
    {
        if (tileGrid == null) return;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapLength; y++)
            {
                Tile2D tile = tileGrid[x, y].GetComponent<Tile2D>();
                tile.Initialize(GetTileData(x, y));
            }
        }
    }

    public void SetWorld(World newWorld)
    {
        world = newWorld;
        RefreshAllTiles();
    }

    // Helper methods for herd testing
    public bool IsTilePassable(int x, int y)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapLength) return false;
        
        TileData2D tileData = GetTileData(x, y);
        if (tileData == null) return false;
        
        // Simple passability: only water blocks movement for now
        return tileData.groundType != "water";
    }

    public TileData2D GetTileDataAt(int x, int y)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapLength) return null;
        return GetTileData(x, y);
    }

    public void HighlightTile(int x, int y, Color highlightColor)
    {
        if (tileGrid != null && x >= 0 && x < mapWidth && y >= 0 && y < mapLength)
        {
            Tile2D tile = tileGrid[x, y].GetComponent<Tile2D>();
            // You can add a highlight method to Tile2D later
            // tile.SetHighlight(highlightColor);
        }
    }

    private TileData2D GetTileData(int x, int y)
    {
        // Return actual world data if available, otherwise default
        if (world != null && x < World.X && y < World.Z)
        {
            return ConvertWorldTileToTileData(x, y);
        }
        
        // Fallback to default data
        return new TileData2D
        {
            groundType = "grass",
            vegetationType = "tree",
            vegetationAmount = 20,
            terrainSymbol = "hills",
            riverSystem = "none"
        };
    }

    private TileData2D ConvertWorldTileToTileData(int x, int y)
    {
        TileData2D tileData = new TileData2D();

        // Extract basic terrain information
        double oceanPercent = world.terrains.oceanPercents[x, y];
        double elevation = world.terrains.elevations[x, y];

        // Store raw data for future use
        tileData.elevation = elevation;
        tileData.oceanPercent = oceanPercent;

        // Determine ground type: Water vs Land
        if (oceanPercent == 1.0)
        {
            tileData.groundType = "water";
        }
        else
        {
            // Land tile - determine base type from dominant habitat
            tileData.groundType = GetDominantHabitatGroundType(x, y);
        }

        // Clear other fields for now - we'll add these back in later iterations
        tileData.vegetationType = GetDominantHabitatName(x, y);
        tileData.vegetationAmount = GetDominantHabitatPercentage(x, y);
        tileData.terrainSymbol = GetTerrainSymbol(x, y);
        tileData.riverSystem = "none";

        return tileData;
    }

    private string GetTerrainSymbol(int x, int y)
    {
        if (world.terrains == null) return "none";

        double hillPercent = world.terrains.hillPercents[x, y];

        if (hillPercent < 0.4)
        {
            return "none";
        }
        else if (hillPercent > 0.75)
        {
            return "mountains";
        }
        else
        {
            return "hills";
        }
    }

    private string GetDominantHabitatName(int x, int y)
    {
        if (world.habitats == null || world.habitats.habitats[x, y] == null)
        {
            return "none"; // No habitat data
        }

        var habitat = world.habitats.habitats[x, y];
        int[] typePercents = habitat.typePercents;

        // Find the habitat with the highest percentage (excluding ocean at index 13)
        int dominantHabitatIndex = -1;
        int maxPercent = 0;

        for (int i = 0; i < 13; i++) // 0-12, excluding ocean (13)
        {
            if (typePercents[i] > maxPercent)
            {
                maxPercent = typePercents[i];
                dominantHabitatIndex = i;
            }
        }

        // No significant habitat found
        if (dominantHabitatIndex == -1 || maxPercent == 0)
        {
            return "none";
        }

        // Return the actual habitat name using the mapping
        return Habitats.habitatMapping[dominantHabitatIndex];
    }

    private int GetDominantHabitatPercentage(int x, int y)
    {
        if (world.habitats == null || world.habitats.habitats[x, y] == null)
        {
            return 0;
        }

        var habitat = world.habitats.habitats[x, y];
        int[] typePercents = habitat.typePercents;

        // Find the highest percentage (excluding ocean at index 13)
        int maxPercent = 0;

        for (int i = 0; i < 13; i++) // 0-12, excluding ocean (13)
        {
            if (typePercents[i] > maxPercent)
            {
                maxPercent = typePercents[i];
            }
        }

        return maxPercent;
    }

    private string GetDominantHabitatGroundType(int x, int y)
    {
        if (world.habitats == null || world.habitats.habitats[x, y] == null)
        {
            return "grass"; // Default if no habitat data
        }

        var habitat = world.habitats.habitats[x, y];
        int[] typePercents = habitat.typePercents;

        // Find the habitat with the highest percentage (excluding ocean at index 13)
        int dominantHabitatIndex = -1;
        int maxPercent = 0;

        for (int i = 0; i < 13; i++) // 0-12, excluding ocean (13)
        {
            if (typePercents[i] > maxPercent)
            {
                maxPercent = typePercents[i];
                dominantHabitatIndex = i;
            }
        }

        // No significant habitat found
        if (dominantHabitatIndex == -1 || maxPercent == 0)
        {
            return "grass"; // Default
        }

        // Determine ground type based on rainfall pattern (mod 4) and special cases
        if (dominantHabitatIndex == 12) // Ice Sheet
        {
            return "ice";
        }
        else if (dominantHabitatIndex % 4 == 0) // Desert rainfall pattern (0, 4, 8)
        {
            return "sand";
        }
        else if (dominantHabitatIndex % 4 == 3) // Wet/Swamp pattern (3, 7, 11)
        {
            return "swamp";
        }
        else // Everything else (dry and moderate rainfall)
        {
            return "grass";
        }
    }
}
