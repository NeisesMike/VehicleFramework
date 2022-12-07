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
        public GameObject Flap;
        public Vector3 AnglesOpened;
        public Vector3 AnglesClosed;
        public List<Transform> ModuleProxies;
    }
    public struct VehicleBattery
    {
        public GameObject BatterySlot;
        public Transform BatteryProxy;
    }
    public struct VehicleFloodLight
    {
        public GameObject Light;
        public float Intensity;
        public float Range;
        public Color Color;
        public float Angle;
    }
}
