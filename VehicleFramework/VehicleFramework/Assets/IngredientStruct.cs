using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework.Assets
{
    public struct Ingredient
    {
        public TechType techType;
        public int count;
        public Ingredient(TechType inTechType, int inCount)
        {
            techType = inTechType;
            count = inCount;
        }
        public CraftData.Ingredient Get()
        {
            return new CraftData.Ingredient(techType, count);
        }
    }
}
