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
    public int z = 100;
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
        Stopwatch worldBuildTime = Stopwatch.StartNew();
        worldBuildTime.Start();
        world = generateWorld(x, z);
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

    private World generateWorld(int x, int z)
    {
        loadGeneralFiles();
        string xText = GameObject.Find("XDimensionInput").GetComponent<InputField>().text;
        string zText = GameObject.Find("YDimensionInput").GetComponent<InputField>().text;
        x = !xText.Equals("") ? int.Parse(xText) : 100;
        z = !zText.Equals("") ? int.Parse(zText) : 80;
        Dictionary<string, object> defaults = fetchDefaults();
        UnityEngine.Debug.Log("Generating World of size (" + x + ", " + z + ")");
        World world = new World(x, z, defaults);
        return world;
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

    private void saveToCsv()
    {
        saveArrayToCsvFiles("high_temps", ArrayPrinter.printIntArray(world.temps.highTemps));
    }

    private void saveArrayToCsvFiles(string filename, string arrayString)
    {
        string filePath = @"C:/Users/Owner/Documents/Wills Projects/CavemanGameA/test_files/" + filename + ".csv";
        File.WriteAllText(filePath, arrayString);
    }

    private string loadJsonFileToString(string pathname)
    {
        return MyJsonFileInteractor.loadDataFileToString(pathname);
    }
}
