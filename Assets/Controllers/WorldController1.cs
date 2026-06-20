using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;
using CavemanLand.Models;
using CavemanLand.Generators;
using CavemanLand.Models.GenericModels;
using CavemanLand.Utility;

public class WorldController1 : MonoBehaviour
{
    public int x = 100;
    public int z = 80;
    private World world;

    private GameObject loadOrCreateMenu;
    private GameObject generateWorldMenu;
    private GameObject loadingScreen;
    private GameObject worldInfoDisplay;
    private Text worldInfoDataBox;

    void Start()
    {
        loadMenus();
    }

    public void clickGenerateWorld()
    {
        var parameters = fetchWorldGenerationParams();
        toggleLoadingScreen(true);
        StartCoroutine(GenerateWorldCoroutine(parameters.x, parameters.z, parameters.defaults));
    }

    private IEnumerator GenerateWorldCoroutine(int x, int z, Dictionary<string, object> defaults)
    {
        Stopwatch worldBuildTime = Stopwatch.StartNew();
        yield return null; // Show loading screen

        world = generateWorld(x, z, defaults);

        worldBuildTime.Stop();
        UnityEngine.Debug.Log("World built in " + worldBuildTime.ElapsedMilliseconds + " millseconds");

        toggleLoadingScreen(false);
        loadWorldDisplayScreen();
    }

    public void saveWorld(string worldName)
    {
        world.saveGameFiles(worldName);
    }

    public World GetWorld()
    {
        return world;
    }

    public void goToGenerateWorldScreen()
    {
        loadOrCreateMenu.SetActive(false);
        loadingScreen.SetActive(false);
        worldInfoDisplay.SetActive(false);
        generateWorldMenu.SetActive(true);
    }

    public void goToLoadWorldScreen()
    {
        UnityEngine.Debug.Log("This is not implemented yet!");
    }

    public void goToMainMenu()
    {
        loadOrCreateMenu.SetActive(true);
        generateWorldMenu.SetActive(false);
        loadingScreen.SetActive(false);
        worldInfoDisplay.SetActive(false);
    }

    public void confirmWorld()
    {
        UnityEngine.Debug.Log("The Player has accepted this world!");
        UnityEngine.Debug.Log("Exporting world data to CSV files...");
        exportWorldDataToCSV();

        // Hide all UI before switching scenes
        loadOrCreateMenu.SetActive(false);
        generateWorldMenu.SetActive(false);
        loadingScreen.SetActive(false);
        worldInfoDisplay.SetActive(false);

        // Persist this GameObject (and the World data) across scene changes
        DontDestroyOnLoad(gameObject);

        UnityEngine.Debug.Log("Switching to 2D Map View");
        SceneManager.LoadScene("2DMapScene");
    }

    private void loadMenus()
    {
        UnityEngine.Debug.Log("Loading Menu ...");
        loadOrCreateMenu = GameObject.Find("LoadOrCreateMenu");
        loadOrCreateMenu.SetActive(true);
        generateWorldMenu = GameObject.Find("GenerateWorldMenu");
        generateWorldMenu.SetActive(false);
        loadingScreen = GameObject.Find("LoadingScreen");
        loadingScreen.SetActive(false);
        worldInfoDisplay = GameObject.Find("WorldInfoDisplay");
        worldInfoDataBox = GameObject.Find("WorldInfoDataBox").GetComponent<UnityEngine.UI.Text>();
        worldInfoDisplay.SetActive(false);
    }

    private void toggleLoadingScreen(bool isOn)
    {
        loadOrCreateMenu.SetActive(false);
        generateWorldMenu.SetActive(false);
        worldInfoDisplay.SetActive(false);
        loadingScreen.SetActive(isOn);
    }

    private void loadWorldDisplayScreen()
    {
        loadOrCreateMenu.SetActive(false);
        generateWorldMenu.SetActive(false);
        worldInfoDisplay.SetActive(true);

        worldInfoDataBox.text = world.displayInfo();
    }

    private Dictionary<string, object> fetchDefaults()
    {
        Dictionary<string, object> defaults = new Dictionary<string, object>();
        defaults["landPercentageRestrictions"] = fetchLandPercentageRange();
        defaults["requiredMinerals"] = fetchRequiredMinerals();
        defaults["poleSetting"] = fetchPoleSetting();

        return defaults;
    }

    private double[] fetchLandPercentageRange()
    {
        double[] array = new double[2];
        string lowerBound = GameObject.Find("LowerBoundInput").GetComponent<InputField>().text;
        string upperBound = GameObject.Find("UpperBoundInput").GetComponent<InputField>().text;
        array[0] = !lowerBound.Equals("") ? int.Parse(lowerBound) / 100.0 : 0.10;
        array[1] = !upperBound.Equals("") ? int.Parse(upperBound) / 100.0 : 0.90;
        return array;
    }

    private List<string> fetchRequiredMinerals()
    {
        List<string> list = new List<string>(new string[] { "Stone" });
        Toggle[] allCheckboxes = GameObject.Find("RequiredMineralOptions").GetComponentsInChildren<Toggle>();
        foreach(Toggle checkbox in allCheckboxes)
        {
            if (checkbox.isOn)
            {
                list.Add(checkbox.name);
            }
        }

        UnityEngine.Debug.Log("list " + ArrayPrinter.printList<string>(list));
        return list;
    }

    private LayerGenerator.mapPoles fetchPoleSetting()
    {
        Dropdown dropdown = GameObject.Find("PolarOptions").GetComponent<Dropdown>();
        int poleSetting = dropdown != null ? dropdown.value : -1;
        switch (poleSetting)
        {
            case 1:
                return LayerGenerator.mapPoles.North;
            case 2:
                return LayerGenerator.mapPoles.South;
            default:
                return LayerGenerator.mapPoles.None;
        }
    }

    private World generateWorld(int x, int z, Dictionary<string, object> defaults)
    {
        loadGeneralFiles();
        UnityEngine.Debug.Log("Generating World of size (" + x + ", " + z + ")");
        World world = new World(x, z, defaults);
        return world;
    }

    private (int x, int z, Dictionary<string, object> defaults) fetchWorldGenerationParams()
    {
        string xText = GameObject.Find("XDimensionInput").GetComponent<InputField>().text;
        string zText = GameObject.Find("YDimensionInput").GetComponent<InputField>().text;
        int x = !string.IsNullOrEmpty(xText) ? int.Parse(xText) : 100;
        int z = !string.IsNullOrEmpty(zText) ? int.Parse(zText) : 80;
        Dictionary<string, object> defaults = fetchDefaults();
        return (x, z, defaults);
    }

    private void loadGeneralFiles()
    {
        UnityEngine.Debug.Log("Loading Animal Files ...");
        string json = loadJsonFileToString("Animal");
        Animal[] animals = JsonConvert.DeserializeObject<Animal[]>(json);
        World.setAnimalSpecies(animals);

        UnityEngine.Debug.Log("Loading Plant Files ...");
        json = loadJsonFileToString("Plants");
        Plant[] plants = JsonConvert.DeserializeObject<Plant[]>(json);
        World.setPlantSpecies(plants);
    }

    private void logStats()
    {
        WorldDate date = world.currentDate;
        UnityEngine.Debug.Log("World Elevations:");
        UnityEngine.Debug.Log(ArrayPrinter.printDoubleArray(world.terrains.elevations));
        UnityEngine.Debug.Log("World Ocean Percents:");
        UnityEngine.Debug.Log(ArrayPrinter.printDoubleArray(world.terrains.oceanPercents));
        UnityEngine.Debug.Log("World Hill Percents:");
        UnityEngine.Debug.Log(ArrayPrinter.printDoubleArray(world.terrains.hillPercents));
        UnityEngine.Debug.Log("World Minerals:");
        UnityEngine.Debug.Log(ArrayPrinter.printArrayOf<Minerals>(world.terrains.minerals));
        UnityEngine.Debug.Log("All Minerals in the world:");
        UnityEngine.Debug.Log(ArrayPrinter.printList<string>(world.terrains.getAllMineralsInWorld()));
    }

    private void exportWorldDataToCSV()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string exportDir = Path.Combine(Application.dataPath, "..", "WorldExports", timestamp);

        try
        {
            // Create export directory
            Directory.CreateDirectory(exportDir);
            UnityEngine.Debug.Log("Exporting world data to: " + exportDir);

            // Export Terrain Data
            saveArrayToCsvFile(exportDir, "elevations", ArrayPrinter.printDoubleArray(world.terrains.elevations));
            saveArrayToCsvFile(exportDir, "ocean_percents", ArrayPrinter.printDoubleArray(world.terrains.oceanPercents));
            saveArrayToCsvFile(exportDir, "hill_percents", ArrayPrinter.printDoubleArray(world.terrains.hillPercents));
            saveArrayToCsvFile(exportDir, "bias_guide", ArrayPrinter.printDoubleArray(world.terrains.biasGuide));

            // Export Temperature Data
            saveArrayToCsvFile(exportDir, "high_temps", ArrayPrinter.printIntArray(world.temps.highTemps));
            saveArrayToCsvFile(exportDir, "low_temps", ArrayPrinter.printIntArray(world.temps.lowTemps));
            saveArrayToCsvFile(exportDir, "summer_lengths", ArrayPrinter.printIntArray(world.temps.summerLengths));
            saveArrayToCsvFile(exportDir, "temp_variances", ArrayPrinter.printDoubleArray(world.temps.variances));

            // Export Precipitation Data
            saveArrayToCsvFile(exportDir, "flow_rates", ArrayPrinter.printDoubleArray(world.precips.flowRates));

            // Export humidity layers
            for (int i = 0; i < world.precips.humidities.Length; i++)
            {
                saveArrayToCsvFile(exportDir, $"humidity_layer_{i}", ArrayPrinter.printDoubleArray(world.precips.humidities[i]));
            }

            // Export Habitat Data (dominant habitat per tile)
            saveArrayToCsvFile(exportDir, "dominant_habitat", exportDominantHabitats());
            saveArrayToCsvFile(exportDir, "habitat_percentages", exportHabitatPercentages());

            // Export Mineral Data (what minerals exist per tile)
            saveArrayToCsvFile(exportDir, "minerals_surface", exportMineralsData(true));
            saveArrayToCsvFile(exportDir, "minerals_mineable", exportMineralsData(false));

            // Export River/Water Data
            saveArrayToCsvFile(exportDir, "flow_directions", exportFlowDirections());
            saveArrayToCsvFile(exportDir, "river_bed_depth", exportRiverBedDepth());
            saveArrayToCsvFile(exportDir, "avg_surface_water", exportAverageSurfaceWater());

            // Export World Info Summary
            string summaryPath = Path.Combine(exportDir, "world_summary.txt");
            File.WriteAllText(summaryPath, world.displayInfo());

            UnityEngine.Debug.Log("World data export complete! Files saved to: " + exportDir);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Error exporting world data: " + e.Message);
        }
    }

    private void saveToCsv()
    {
        saveArrayToCsvFile(Application.dataPath, "high_temps", ArrayPrinter.printIntArray(world.temps.highTemps));
    }

    private void saveArrayToCsvFile(string directory, string filename, string arrayString)
    {
        string filePath = Path.Combine(directory, filename + ".csv");
        File.WriteAllText(filePath, arrayString);
    }

    private string exportDominantHabitats()
    {
        string output = "";
        // Transpose: swap loops so visual matches array semantics
        for (int z = 0; z < World.Z; z++)
        {
            for (int x = 0; x < World.X; x++)
            {
                var habitat = world.habitats.habitats[x, z];
                int[] typePercents = habitat.typePercents;

                // Find dominant habitat
                int dominantIndex = 0;
                int maxPercent = typePercents[0];
                for (int i = 1; i < typePercents.Length; i++)
                {
                    if (typePercents[i] > maxPercent)
                    {
                        maxPercent = typePercents[i];
                        dominantIndex = i;
                    }
                }

                string habitatName = Habitats.habitatMapping[dominantIndex];
                output += habitatName;
                if (x < World.X - 1)
                {
                    output += ", ";
                }
            }
            output += "\n";
        }
        return output;
    }

    private string exportHabitatPercentages()
    {
        string output = "";
        // Transpose: swap loops so visual matches array semantics
        for (int z = 0; z < World.Z; z++)
        {
            for (int x = 0; x < World.X; x++)
            {
                var habitat = world.habitats.habitats[x, z];
                int[] typePercents = habitat.typePercents;

                // Format: "Arctic:10|Tundra:30|Forest:60" (only non-zero)
                List<string> parts = new List<string>();
                for (int i = 0; i < typePercents.Length; i++)
                {
                    if (typePercents[i] > 0)
                    {
                        parts.Add($"{Habitats.habitatMapping[i]}:{typePercents[i]}");
                    }
                }

                output += "\"" + string.Join("|", parts) + "\"";
                if (x < World.X - 1)
                {
                    output += ", ";
                }
            }
            output += "\n";
        }
        return output;
    }

    private string exportMineralsData(bool isSurface)
    {
        string output = "";
        // Transpose: swap loops so visual matches array semantics
        for (int z = 0; z < World.Z; z++)
        {
            for (int x = 0; x < World.X; x++)
            {
                Minerals minerals = world.terrains.minerals[x, z];
                Dictionary<string, double> mineralDict = isSurface ? minerals.surface : minerals.mineable;

                // Format: "Iron:567.8|Gold:12.3" (exclude Stone since it's everywhere)
                List<string> parts = new List<string>();
                foreach (var kvp in mineralDict)
                {
                    if (kvp.Key != "Stone") // Skip Stone - it's on every land tile
                    {
                        parts.Add($"{kvp.Key}:{kvp.Value}");
                    }
                }

                output += "\"" + string.Join("|", parts) + "\"";
                if (x < World.X - 1)
                {
                    output += ", ";
                }
            }
            output += "\n";
        }
        return output;
    }

    private string exportFlowDirections()
    {
        string output = "";
        // Transpose: swap loops so visual matches array semantics
        for (int z = 0; z < World.Z; z++)
        {
            for (int x = 0; x < World.X; x++)
            {
                Direction.CardinalDirections flowDir = world.precips.flowDirections[x, z];
                string arrow = flowDir switch
                {
                    Direction.CardinalDirections.up => "↑",
                    Direction.CardinalDirections.down => "↓",
                    Direction.CardinalDirections.right => "→",
                    Direction.CardinalDirections.left => "←",
                    _ => "O" // none/pool/lake
                };
                output += arrow;
                if (x < World.X - 1)
                {
                    output += ", ";
                }
            }
            output += "\n";
        }
        return output;
    }

    private string exportRiverBedDepth()
    {
        string output = "";
        // Transpose: swap loops so visual matches array semantics
        for (int z = 0; z < World.Z; z++)
        {
            for (int x = 0; x < World.X; x++)
            {
                double depth = world.precips.riverBanks[x, z].dryBedDepth;
                output += depth;
                if (x < World.X - 1)
                {
                    output += ", ";
                }
            }
            output += "\n";
        }
        return output;
    }

    private string exportAverageSurfaceWater()
    {
        string output = "";
        // Transpose: swap loops so visual matches array semantics
        for (int z = 0; z < World.Z; z++)
        {
            for (int x = 0; x < World.X; x++)
            {
                // Calculate average surface water over the year
                double sum = 0;
                for (int day = 0; day < WorldDate.DAYS_PER_YEAR; day++)
                {
                    sum += world.precips.dailySurfaceWater[day][x, z];
                }
                double avg = Math.Round(sum / WorldDate.DAYS_PER_YEAR, World.ROUND_TO);
                output += avg;
                if (x < World.X - 1)
                {
                    output += ", ";
                }
            }
            output += "\n";
        }
        return output;
    }

    private string loadJsonFileToString(string pathname)
    {
        return MyJsonFileInteractor.loadDataFileToString(pathname);
    }
}
