using System;
using System.Collections.Generic;

namespace CavemanLand.Models.GenericModels
{
    [Serializable]
    public class Animal
    {
		public string name;
		public int maxAge;
		public List<int> formsHerds = new List<int>(2);
		public List<string> habitats;
		public List<int> abundance;
		public List<string> foodType;
		public double foodEaten;
		public int defense;
		public int attack;
		public int speed;
		public int sneak;
		public List<int> temperatureTolerance = new List<int>(2);
		public double foodPerAnimal;
		public double weightPerUnit;
		public List<string> production;
		public List<double> productionPerUnit;
		public bool isRidable;
		public bool isBurden;

        public Animal()
        {
        }

		public override string ToString()
        {
            return name;
        }

    }
}
