﻿using System;
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

        public override List<VehicleFramework.VehicleParts.VehiclePilotSeat> PilotSeats => throw new NotImplementedException();

        public override List<VehicleFramework.VehicleParts.VehicleHatchStruct> Hatches => throw new NotImplementedException();

        public override List<VehicleFramework.VehicleParts.VehicleStorage> Storages => throw new NotImplementedException();

        public override List<VehicleFramework.VehicleParts.VehicleStorage> ModularStorages => throw new NotImplementedException();

        public override List<VehicleFramework.VehicleParts.VehicleUpgrades> Upgrades => throw new NotImplementedException();

        public override List<VehicleFramework.VehicleParts.VehicleBattery> Batteries => throw new NotImplementedException();

        public override List<VehicleFramework.VehicleParts.VehicleLight> Lights => throw new NotImplementedException();

        public override List<GameObject> WalkableInteriors => throw new NotImplementedException();
    }
}
