using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework.VehicleComponents
{
    public class ForeverBattery : EnergyMixin
    {
        // use fixed update because EnergyMixin has none
        public void FixedUpdate()
        {
            AddEnergy(capacity - charge);
        }
    }
}
