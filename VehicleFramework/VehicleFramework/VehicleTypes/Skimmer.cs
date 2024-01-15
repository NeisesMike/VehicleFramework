using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Engines;

namespace VehicleFramework.VehicleTypes
{
    public abstract class Skimmer : ModVehicle
    {
        public abstract List<VehicleParts.VehicleHatchStruct> Hatches { get; }
        public abstract List<VehicleParts.VehiclePilotSeat> PilotSeats { get; }
        public virtual GameObject SteeringWheelLeftHandTarget { get; }
        public virtual GameObject SteeringWheelRightHandTarget { get; }
        protected bool isPlayerInside = false;

        public bool IsPlayerInside()
        {
            // this one is correct ?
            return isPlayerInside;
        }
    }
}
