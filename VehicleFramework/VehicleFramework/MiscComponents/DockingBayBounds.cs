using System;
using UnityEngine;

namespace VehicleFramework.MiscComponents
{
    public static class DockingBayBoundsExtensions
    {
        public static DockingBayBounds WithX(this DockingBayBounds bounds, float x)
        {
            bounds.x = x;
            return bounds;
        }
        public static DockingBayBounds WithY(this DockingBayBounds bounds, float y)
        {
            bounds.y = y;
            return bounds;
        }
        public static DockingBayBounds WithZ(this DockingBayBounds bounds, float z)
        {
            bounds.z = z;
            return bounds;
        }
        public static DockingBayBounds WithIsVehicleSmallEnoughOverride(this DockingBayBounds bounds, Func<VehicleDockingBay, ModVehicle, bool> method)
        {
            bounds.IsVehicleSmallEnoughOverride = method;
            return bounds;
        }
    }
    public class DockingBayBounds : MonoBehaviour
    {
        internal float x = 0;
        internal float y = 0;
        internal float z = 0;
        internal Func<VehicleDockingBay, ModVehicle, bool>? IsVehicleSmallEnoughOverride = null;

        public bool IsVehicleSmallEnough(VehicleDockingBay bay, ModVehicle mv)
        {
            if (bay == null) return false;
            if (mv == null) return false;

            if(IsVehicleSmallEnoughOverride != null)
            {
                return IsVehicleSmallEnoughOverride(bay, mv);
            }

            Vector3 boundingDimensions = mv.GetBoundingDimensions();
            if (boundingDimensions == Vector3.zero)
            {
                return false;
            }
            if (boundingDimensions.x > x)
            {
                return false;
            }
            else if (boundingDimensions.y > y)
            {
                return false;
            }
            else if (boundingDimensions.z > z)
            {
                return false;
            }
            return true;
        }
    }
}
