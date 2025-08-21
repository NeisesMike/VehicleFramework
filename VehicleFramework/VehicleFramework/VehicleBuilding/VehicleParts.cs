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
        public VehiclePilotSeat(GameObject iSeat, GameObject iSitLocation, Transform iLeftHand, Transform iRightHand, Transform iExit)
        {
            Seat = iSeat;
            SitLocation = iSitLocation;
            LeftHandLocation = iLeftHand;
            RightHandLocation = iRightHand;
            ExitLocation = iExit;
        }
    }
    public struct VehicleHatchStruct
    {
        public GameObject Hatch;
        public Transform EntryLocation;
        public Transform ExitLocation;
        public Transform SurfaceExitLocation;
        public VehicleHatchStruct(GameObject iHatch, Transform iEntry, Transform iExit, Transform iSurfaceExit)
        {
            Hatch = iHatch;
            EntryLocation = iEntry;
            ExitLocation = iExit;
            SurfaceExitLocation = iSurfaceExit;
        }
    }

    public struct VehicleStorage
    {
        public GameObject Container;
        public int Height;
        public int Width;
        public VehicleStorage(GameObject iContainer, int iHeight = 4, int iWidth = 4)
        {
            Container = iContainer;
            Height = iHeight;
            Width = iWidth;
        }
    }

    public struct VehicleAnchor
    {
        public GameObject Anchor;
        public float AnchorLength;
        public int AnchorSpeed;
        public VehicleAnchor(GameObject iAnchor, float iAnchorLength, int IAnchorSpeed)
        {
            Anchor = iAnchor;
            AnchorLength = iAnchorLength;
            AnchorSpeed = IAnchorSpeed;
        }
    }
}
    public struct VehicleUpgrades
    {
        public GameObject Interface;
        public GameObject Flap;
        public Vector3 AnglesOpened;
        public Vector3 AnglesClosed;
        public List<Transform> ModuleProxies;
        public VehicleUpgrades(GameObject iInterface, GameObject iFlap, Vector3 iOpenAngles, Vector3 iClosedAngles, List<Transform> iProxies = null)
        {
            Interface = iInterface;
            Flap = iFlap;
            AnglesOpened = iOpenAngles;
            AnglesClosed = iClosedAngles;
            ModuleProxies = iProxies;
        }
    }
    public struct VehicleBattery
    {
        public GameObject BatterySlot;
        public Transform BatteryProxy;
        public VehicleBattery(GameObject iBatterySlot, Transform iBatteryProxy)
        {
            BatterySlot = iBatterySlot;
            BatteryProxy = iBatteryProxy;
        }
    }
    public struct VehicleFloodLight
    {
        public GameObject Light;
        public float Intensity;
        public float Range;
        public Color Color;
        public float Angle;
        public VehicleFloodLight(GameObject iLight, float iIntensity, float iRange, Color iColor, float iAngle)
        {
            Light = iLight;
            Intensity = iIntensity;
            Range = iRange;
            Color = iColor;
            Angle = iAngle;
        }
    }

    public struct VehicleArmsProxy
    {
        public GameObject originalLeftArm;
        public GameObject originalRightArm;
        public Transform leftArmPlacement;
        public Transform rightArmPlacement;
        public VehicleArmsProxy(GameObject originalLeft, GameObject originalRight, Transform leftArmPlace, Transform rightArmPlace)
        {
            originalLeftArm = originalLeft;
            originalRightArm = originalRight;
            leftArmPlacement = leftArmPlace;
            rightArmPlacement = rightArmPlace;
        }
    }
    public struct VehicleCamera
    {
        public string name;
        public Transform camera;
        public VehicleCamera(Transform cameraTransform, string cameraName)
        {
            name = cameraName;
            camera = cameraTransform;
        }
    }
}
