using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CavemanLand.Models;
using CavemanLand.Models.GenericModels;

public class TilemapManager : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("If true, tries to load sprites from Resources. If false or sprites not found, uses colored squares.")]
    public bool useSprites = false;

    private World world;
    private Dictionary<string, Tilemap> layers = new Dictionary<string, Tilemap>();
    private Sprite whiteSquareSprite;

    // Cached sprites (loaded from Resources folder)
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    // Layer names
    private const string GROUND_LAYER = "Ground";
    private const string VEGETATION_LAYER = "Vegetation";
    private const string RIVER_LAYER = "River";
    private const string TERRAIN_SYMBOL_LAYER = "TerrainSymbol";
    private const string GATHERABLES_LAYER = "Gatherables";
    private const string WEATHER_LAYER = "Weather";
    private const string SMALL_GAME_LAYER = "SmallGame";
    private const string HERDS_LAYER = "Herds";

    void Start()
    {
        // Create a white square sprite for coloring tiles
        whiteSquareSprite = CreateWhiteSquareSprite();

        // Get world data from WorldController1
        WorldController1 worldController = FindAnyObjectByType<WorldController1>();
        if (worldController != null)
        {
            world = worldController.GetWorld();
        }

        if (world == null)
        {
            Debug.LogError("TilemapManager: Could not find World data!");
            return;
        }

        Debug.Log("TilemapManager: Creating tile layers...");

        // Create all layers
        CreateFullSquareLayer(GROUND_LAYER, 0);
        CreateFullSquareLayer(VEGETATION_LAYER, 1);
        CreateFullSquareLayer(RIVER_LAYER, 2);
        CreateFullSquareLayer(TERRAIN_SYMBOL_LAYER, 3);

        // Corner layers with offsets
        CreateCornerLayer(GATHERABLES_LAYER, -0.25f, 0.25f, 4);  // Upper left
        CreateCornerLayer(WEATHER_LAYER, 0.25f, 0.25f, 5);       // Upper right
        CreateCornerLayer(SMALL_GAME_LAYER, -0.25f, -0.25f, 6);  // Lower left
        CreateCornerLayer(HERDS_LAYER, 0.25f, -0.25f, 7);        // Lower right

        Debug.Log("TilemapManager: Populating layers with world data...");

        // Populate layers with world data
        PopulateGroundLayer();
        PopulateVegetationLayer();
        PopulateRiverLayer();
        PopulateTerrainSymbolLayer();

        Debug.Log("TilemapManager: Tile display complete!");
    }

    private void CreateFullSquareLayer(string layerName, int sortingOrder)
    {
        GameObject layerObj = new GameObject(layerName + "Layer");
        layerObj.transform.parent = transform;
        layerObj.transform.localPosition = Vector3.zero;

        Tilemap tilemap = layerObj.AddComponent<Tilemap>();
        TilemapRenderer renderer = layerObj.AddComponent<TilemapRenderer>();

        renderer.sortingOrder = sortingOrder;

        layers[layerName] = tilemap;
    }

    private void CreateCornerLayer(string layerName, float offsetX, float offsetY, int sortingOrder)
    {
        GameObject layerObj = new GameObject(layerName + "Layer");
        layerObj.transform.parent = transform;
        layerObj.transform.localPosition = new Vector3(offsetX, offsetY, 0);

        Tilemap tilemap = layerObj.AddComponent<Tilemap>();
        TilemapRenderer renderer = layerObj.AddComponent<TilemapRenderer>();

        renderer.sortingOrder = sortingOrder;

        layers[layerName] = tilemap;
    }

    private void PopulateGroundLayer()
    {
        Tilemap groundLayer = layers[GROUND_LAYER];

        for (int x = 0; x < World.X; x++)
        {
            for (int z = 0; z < World.Z; z++)
            {
                Vector3Int tilePos = new Vector3Int(x, z, 0);

                double oceanPercent = world.terrains.oceanPercents[x, z];
                Color groundColor;
                string spritePath;

                if (oceanPercent >= 1.0)
                {
                    // Full ocean - deep blue
                    groundColor = new Color(0.2f, 0.4f, 0.8f, 1.0f);
                    spritePath = "Sprites/Ground/ocean";
                }
                else
                {
                    // Land tile - determine ground type by dominant habitat
                    int dominantHabitat = GetDominantHabitatIndex(x, z);

                    // Desert types: Arctic Desert (0), Desert (4), Hot Desert (8)
                    if (dominantHabitat == 0 || dominantHabitat == 4 || dominantHabitat == 8)
                    {
                        // Desert - sandy yellow
                        groundColor = new Color(0.85f, 0.75f, 0.45f, 1.0f);
                        spritePath = "Sprites/Ground/desert";
                    }
                    // Wet types: Arctic Marsh (3), Swamp (7), Rainforest (11)
                    else if (dominantHabitat == 3 || dominantHabitat == 7 || dominantHabitat == 11)
                    {
                        // Swamp/Marsh/Rainforest - muddy brown
                        groundColor = new Color(0.4f, 0.35f, 0.25f, 1.0f);
                        spritePath = "Sprites/Ground/swamp";
                    }
                    else
                    {
                        // Default land - dirt brown
                        groundColor = new Color(0.55f, 0.4f, 0.25f, 1.0f);
                        spritePath = "Sprites/Ground/land";
                    }
                }

                // Create tile with sprite (or white square) and color
                Tile tile = CreateTile(LoadSprite(spritePath), groundColor);

                groundLayer.SetTile(tilePos, tile);
            }
        }
    }

    private int GetDominantHabitatIndex(int x, int z)
    {
        if (world.habitats == null || world.habitats.habitats[x, z] == null)
        {
            return -1;
        }

        var habitat = world.habitats.habitats[x, z];
        int[] typePercents = habitat.typePercents;

        // Find dominant LAND habitat (indices 0-12, SKIP ocean at index 13)
        int dominantIndex = -1;
        int maxPercent = 0;

        for (int i = 0; i < 13; i++) // Loop 0-12 only (not 13 which is ocean)
        {
            if (typePercents[i] > maxPercent)
            {
                maxPercent = typePercents[i];
                dominantIndex = i;
            }
        }

        // Return -1 if no significant land habitat found
        if (maxPercent < 1)
        {
            return -1;
        }

        return dominantIndex;
    }

    private void PopulateVegetationLayer()
    {
        Tilemap vegLayer = layers[VEGETATION_LAYER];

        for (int x = 0; x < World.X; x++)
        {
            for (int z = 0; z < World.Z; z++)
            {
                Vector3Int tilePos = new Vector3Int(x, z, 0);

                // Skip ONLY 100% ocean tiles - partial ocean tiles still show land habitat
                if (world.terrains.oceanPercents[x, z] >= 1.0)
                    continue;

                // Get dominant NON-OCEAN habitat (excludes index 13)
                int dominantIndex = GetDominantHabitatIndex(x, z);

                if (dominantIndex < 0)
                    continue;

                // Show vegetation for any land (even 1% is enough)
                // No threshold check - if there's any land habitat, show it

                // Get habitat color (from reference images)
                Color vegColor = GetHabitatColor(dominantIndex);

                // Full opacity for dominant habitat (no transparency blending)
                vegColor.a = 1.0f;

                // Determine sprite path by habitat name
                string habitatName = Habitats.habitatMapping[dominantIndex];
                string spritePath = $"Sprites/Vegetation/{habitatName}";

                Tile tile = CreateTile(LoadSprite(spritePath), vegColor);

                vegLayer.SetTile(tilePos, tile);
            }
        }
    }

    private void PopulateRiverLayer()
    {
        Tilemap riverLayer = layers[RIVER_LAYER];

        for (int x = 0; x < World.X; x++)
        {
            for (int z = 0; z < World.Z; z++)
            {
                double flowRate = world.precips.flowRates[x, z];

                // Only show rivers with significant flow
                if (flowRate < 0.5)
                    continue;

                Vector3Int tilePos = new Vector3Int(x, z, 0);

                // River color - light blue, opacity based on flow rate
                Color riverColor = new Color(0.3f, 0.7f, 1.0f, 1.0f);
                riverColor.a = Mathf.Clamp01((float)flowRate / 5.0f) * 0.8f;

                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = whiteSquareSprite;
                tile.color = riverColor;

                riverLayer.SetTile(tilePos, tile);
            }
        }
    }

    private void PopulateTerrainSymbolLayer()
    {
        Tilemap symbolLayer = layers[TERRAIN_SYMBOL_LAYER];

        for (int x = 0; x < World.X; x++)
        {
            for (int z = 0; z < World.Z; z++)
            {
                // Skip ocean tiles
                if (world.terrains.oceanPercents[x, z] >= 1.0)
                    continue;

                double hillPercent = world.terrains.hillPercents[x, z];

                // Skip flat terrain
                if (hillPercent < 0.4)
                    continue;

                Vector3Int tilePos = new Vector3Int(x, z, 0);

                Sprite letterSprite;
                Color symbolColor;

                if (hillPercent > 0.75)
                {
                    // Mountains - "M"
                    letterSprite = CreateLetterSprite("M");
                    symbolColor = new Color(0.2f, 0.2f, 0.2f, 0.9f); // Dark gray/black
                }
                else
                {
                    // Hills - "H"
                    letterSprite = CreateLetterSprite("H");
                    symbolColor = new Color(0.4f, 0.3f, 0.2f, 0.8f); // Brown
                }

                Tile tile = CreateTile(letterSprite, symbolColor);

                symbolLayer.SetTile(tilePos, tile);
            }
        }
    }

    private Sprite CreateLetterSprite(string letter)
    {
        // Try to load custom sprite first
        if (useSprites)
        {
            Sprite customSprite = LoadSprite($"Sprites/Terrain/{letter}");
            if (customSprite != whiteSquareSprite)
            {
                return customSprite;
            }
        }

        // Generate simple letter sprite
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Fill with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        // Draw the letter in white
        if (letter == "H")
        {
            DrawH(pixels, size);
        }
        else if (letter == "M")
        {
            DrawM(pixels, size);
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point; // Crisp pixels

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private void DrawH(Color[] pixels, int size)
    {
        int thickness = 3;
        int startX = 6;
        int endX = size - 6;
        int startY = 4;
        int endY = size - 4;
        int midY = size / 2;

        // Left vertical bar
        for (int y = startY; y < endY; y++)
        {
            for (int t = 0; t < thickness; t++)
            {
                pixels[y * size + (startX + t)] = Color.white;
            }
        }

        // Right vertical bar
        for (int y = startY; y < endY; y++)
        {
            for (int t = 0; t < thickness; t++)
            {
                pixels[y * size + (endX - thickness + t)] = Color.white;
            }
        }

        // Horizontal crossbar
        for (int x = startX; x <= endX; x++)
        {
            for (int t = 0; t < thickness; t++)
            {
                pixels[(midY + t) * size + x] = Color.white;
            }
        }
    }

    private void DrawM(Color[] pixels, int size)
    {
        int thickness = 3;
        int startX = 4;
        int endX = size - 4;
        int startY = 4;
        int endY = size - 4;
        int midX = size / 2;

        // Left vertical bar
        for (int y = startY; y < endY; y++)
        {
            for (int t = 0; t < thickness; t++)
            {
                pixels[y * size + (startX + t)] = Color.white;
            }
        }

        // Right vertical bar
        for (int y = startY; y < endY; y++)
        {
            for (int t = 0; t < thickness; t++)
            {
                pixels[y * size + (endX - thickness + t)] = Color.white;
            }
        }

        // Left diagonal (top-left to center)
        for (int i = 0; i < (endY - startY) / 2; i++)
        {
            int x = startX + thickness + i / 2;
            int y = startY + i;
            for (int t = 0; t < thickness; t++)
            {
                if (x + t < size && y >= 0 && y < size)
                {
                    pixels[y * size + (x + t)] = Color.white;
                }
            }
        }

        // Right diagonal (center to top-right)
        for (int i = 0; i < (endY - startY) / 2; i++)
        {
            int x = midX + i / 2;
            int y = startY + i;
            for (int t = 0; t < thickness; t++)
            {
                if (x + t < size && y >= 0 && y < size)
                {
                    pixels[y * size + (x + t)] = Color.white;
                }
            }
        }
    }

    private Sprite CreateWhiteSquareSprite()
    {
        // Create a simple 1x1 white texture
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        // Create sprite from texture
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    // Helper method to load sprites from Resources folder
    private Sprite LoadSprite(string path)
    {
        // If sprites are disabled, always use white square
        if (!useSprites)
        {
            return whiteSquareSprite;
        }

        // Check cache first
        if (spriteCache.ContainsKey(path))
        {
            return spriteCache[path];
        }

        // Try to load from Resources folder
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite != null)
        {
            spriteCache[path] = sprite;
            return sprite;
        }

        // Fallback to white square if sprite not found
        Debug.LogWarning($"TilemapManager: Could not find sprite at Resources/{path}, using colored square fallback");
        return whiteSquareSprite;
    }

    // Helper to create a tile with sprite and color
    private Tile CreateTile(Sprite sprite, Color color)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.color = color;
        return tile;
    }

    // Example: How to use sprites instead of colored squares
    // Uncomment this when you have sprites ready:
    /*
    private Tile CreateGroundTile(double oceanPercent)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();

        if (oceanPercent >= 1.0)
        {
            // Use ocean sprite
            tile.sprite = LoadSprite("Sprites/Ground/ocean");
        }
        else if (oceanPercent > 0.0)
        {
            // Use coast sprite
            tile.sprite = LoadSprite("Sprites/Ground/coast");
        }
        else
        {
            // Use land sprite
            tile.sprite = LoadSprite("Sprites/Ground/land");
        }

        return tile;
    }
    */

    private Color GetHabitatColor(int habitatIndex)
    {
        // Color coding for different habitat types (from reference images)
        switch (habitatIndex)
        {
            case 0: return new Color(0.93f, 0.90f, 0.75f, 1.0f);  // Arctic Desert - pale beige
            case 1: return new Color(0.60f, 0.60f, 0.60f, 1.0f);  // Tundra - gray
            case 2: return new Color(0.70f, 0.85f, 0.65f, 1.0f);  // Boreal - pale green
            case 3: return new Color(0.85f, 0.60f, 0.50f, 1.0f);  // Arctic Marsh - peachy/salmon
            case 4: return new Color(1.0f, 0.95f, 0.30f, 1.0f);   // Desert - bright yellow
            case 5: return new Color(0.60f, 0.90f, 0.30f, 1.0f);  // Plains - bright lime green
            case 6: return new Color(0.45f, 0.55f, 0.30f, 1.0f);  // Forest - dark olive green
            case 7: return new Color(0.45f, 0.20f, 0.15f, 1.0f);  // Swamp - dark reddish brown
            case 8: return new Color(0.75f, 0.70f, 0.35f, 1.0f);  // Hot Desert - olive/tan yellow
            case 9: return new Color(0.85f, 0.70f, 0.25f, 1.0f);  // Savannah - golden/mustard
            case 10: return new Color(0.35f, 0.55f, 0.25f, 1.0f); // Monsoon Forest - medium-dark green
            case 11: return new Color(0.20f, 0.40f, 0.20f, 1.0f); // Rainforest - very dark green
            case 12: return new Color(0.90f, 0.95f, 1.0f, 1.0f);  // Ice Sheet - pale blue-white
            default: return new Color(0.5f, 0.7f, 0.3f, 1.0f);    // Default green
        }
    }

    // Toggle layer visibility (useful for debugging)
    public void ToggleLayer(string layerName, bool visible)
    {
        if (layers.ContainsKey(layerName))
        {
            layers[layerName].gameObject.SetActive(visible);
        }
    }

    // Get a specific layer (for external access)
    public Tilemap GetLayer(string layerName)
    {
        if (layers.ContainsKey(layerName))
        {
            return layers[layerName];
        }
        return null;
    }

    // Debug helper: Print all data for a specific tile
    public void PrintTileDebugInfo(int x, int z)
    {
        if (world == null)
        {
            Debug.Log($"No world data loaded");
            return;
        }

        if (x < 0 || x >= World.X || z < 0 || z >= World.Z)
        {
            Debug.Log($"Coordinates ({x}, {z}) out of bounds");
            return;
        }

        Debug.Log($"=== TILE DATA FOR ({x}, {z}) ===");
        Debug.Log($"Elevation: {world.terrains.elevations[x, z]}");
        Debug.Log($"Ocean %: {world.terrains.oceanPercents[x, z] * 100}%");
        Debug.Log($"Hill %: {world.terrains.hillPercents[x, z] * 100}%");
        Debug.Log($"Flow Rate: {world.precips.flowRates[x, z]}");

        // Print all habitat percentages
        var habitat = world.habitats.habitats[x, z];
        Debug.Log("Habitat breakdown:");
        for (int i = 0; i < habitat.typePercents.Length; i++)
        {
            if (habitat.typePercents[i] > 0)
            {
                string habitatName = Habitats.habitatMapping.ContainsKey(i) ? Habitats.habitatMapping[i] : $"Index {i}";
                Debug.Log($"  {habitatName}: {habitat.typePercents[i]}%");
            }
        }

        int dominant = GetDominantHabitatIndex(x, z);
        if (dominant >= 0)
        {
            Debug.Log($"Dominant Land Habitat: {Habitats.habitatMapping[dominant]}");
        }
        else
        {
            Debug.Log("No dominant land habitat");
        }
    }
}
