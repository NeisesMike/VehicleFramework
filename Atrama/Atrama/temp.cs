using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework;

namespace Atrama
{
    class temp : VehicleFramework.ModVehicle
    {
        public override GameObject VehicleModel => throw new NotImplementedException();

        public override List<VehiclePilotSeat> PilotSeats => throw new NotImplementedException();

        public override List<VehicleHatchStruct> Hatches => throw new NotImplementedException();

        public override List<VehicleStorage> Storages => throw new NotImplementedException();

        public override List<VehicleUpgrades> Upgrades => throw new NotImplementedException();

        public override List<VehicleBattery> Batteries => throw new NotImplementedException();

        public override List<VehicleLight> Lights => throw new NotImplementedException();

        public override List<GameObject> WalkableInteriors => throw new NotImplementedException();
    }
}
