using UnityEngine;

namespace VehicleFramework.Interfaces
{
    public interface ISteeringWheel
    {
        public GameObject? GetSteeringWheelLeftHandTarget();
        public GameObject? GetSteeringWheelRightHandTarget();
    }
}
