using System;
using System.Collections.Generic;
using UnityEngine;
using CavemanLand.Models;
using CavemanLand.Models.GenericModels;
using CavemanLand.Utility;
using CavemanLand.Generators;

namespace CavemanLand.Models
{
    public class World
    {
        // World Generation Constants
        public const int NUM_OF_PREHISTORY_YEARS = 20;
        public const double UNLIMITED_MIN = -1000.0;
        public const double UNLIMITED_MAX = -UNLIMITED_MIN;
        public const int TILE_SIDE_LENGTH = 400;
        // Terrain
            private double[] landPercentageRestrictions;
            private List<string> requiredMinerals;
            // Temperature
            private Dictionary<string, int> defaultTempRequirements;
            // Precipitation
            private Dictionary<string, int> defaultPrecipRequirements;
            // Habitats
            public const int YEARS_TO_FULL_HABITAT_REGROWTH = NUM_OF_PREHISTORY_YEARS;
            // General Constants
            public const int ROUND_TO = 2;

        public static int X;
        public static int Z;
        public static LayerGenerator layerGenerator;
        public static Animal[] animals;
        public static Dictionary<string, Animal> animalSpecies;
        public static Plant[] plants;
        public static Dictionary<string, Plant> plantSpecies;

        public WorldDate currentDate;
        public WorldTerrains terrains;
        public WorldTemps temps;
        public WorldPrecipitation precips;
        public WorldHabitats habitats;

        public World(int x, int z, Dictionary<string, object> defaults)
        {
            X = x;
            Z = z;
            Coordinates.setWorldSize(x, z);
            layerGenerator = new LayerGenerator(x, z, ROUND_TO);
            setDefaultRequirements(defaults);
            LayerGenerator.mapPoles poleSetting = (LayerGenerator.mapPoles) defaults["poleSetting"];
            generateNewWorld(layerGenerator, poleSetting);
        }

        public static void setAnimalSpecies(Animal[] animalInfo)
        {
            animals = animalInfo;
            animalSpecies = new Dictionary<string, Animal>();
            foreach (Animal animal in animalInfo)
            {
                animalSpecies[animal.name] = animal;
            }
        }

        public static void setPlantSpecies(Plant[] plantInfo)
        {
            plants = plantInfo;
            plantSpecies = new Dictionary<string, Plant>();
            foreach (Plant plant in plantInfo)
            {
                plantSpecies[plant.name] = plant;
            }
        }

        public void saveGameFiles(string worldName)
        {
            Debug.Log("Save worlds has not yet been implemented");
        }

        public string displayInfo()
        {
            string output = "Size: X = " + X + ", Y = " + Z + "\n\n";
            output += "Land %: " + terrains.landPercentage * 100.0 + " %\n\n";
            output += "Minerals: " + ArrayPrinter.printList<string>(terrains.getAllMineralsInWorld()) + "\n\n";
            output += HashPrinter.printHash<string, double>("Habitat % Distribution", habitats.getWorldHabitatDistribution());

            return output;
        }

        // PRIVATES

        private void generateNewWorld(LayerGenerator layerGenerator, LayerGenerator.mapPoles poleSetting)
        {
            currentDate = new WorldDate(1, 1);
            // create terrain first
            terrains = new WorldTerrains(layerGenerator, landPercentageRestrictions, requiredMinerals);
            // then create temperatures
            temps = new WorldTemps(layerGenerator, poleSetting, defaultTempRequirements);
            int[][][,] dailyTempsPrehistory = temps.generatePrehistory();
            // then precipitation as the most complex part
            precips = new WorldPrecipitation(layerGenerator, terrains, dailyTempsPrehistory, defaultPrecipRequirements);
            Dictionary<int, Dictionary<string, double[][,]>> dailyRainPrehistory = precips.generatePrehistory(terrains, dailyTempsPrehistory, temps.dailyTemps);
            // Then based on prehistory grow habitats to fill the world tiles
            habitats = new WorldHabitats(terrains.oceanPercents, dailyTempsPrehistory, dailyRainPrehistory);
        }

        private void setDefaultRequirements(Dictionary<string, object> defaults)
        {
            // terrain
            landPercentageRestrictions = (double[]) defaults["landPercentageRestrictions"];
            requiredMinerals = (List<string>) defaults["requiredMinerals"];
            // temps
            defaultTempRequirements = new Dictionary<string, int>();
            /* defaultTempRequirements["high_above"] = 60;
            defaultTempRequirements["low_below"] = 32; */

            // precipitation
            defaultPrecipRequirements = new Dictionary<string, int>();
        }

    }
}
