using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.VehicleParts
{
    public struct VehiclePilotSeat
    {
        public GameObject Seat;
        public GameObject SitLocation;
        public Transform LeftHandLocation;
        public Transform RightHandLocation;
        public Transform ExitLocation;
    }
    public struct VehicleHatchStruct
    {
        public GameObject Hatch;
        public Transform EntryLocation;
        public Transform ExitLocation;
        public Transform SurfaceExitLocation;
    }
    public struct VehicleStorage
    {
        public GameObject Container;
        public int Height;
        public int Width;
    }
    public struct VehicleUpgrades
    {
        public GameObject Interface;
    }
    public struct VehicleBattery
    {
        public GameObject BatterySlot;
    }
    public struct VehicleHeadLight
    {
        public GameObject Light;
        public int Strength;
        public Color Color;
        public int Angle;
    }
}
