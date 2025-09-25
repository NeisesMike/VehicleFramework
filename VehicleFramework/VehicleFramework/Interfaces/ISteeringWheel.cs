using UnityEngine;

namespace VehicleFramework.Interfaces
{
    internal interface ISteeringWheel
    {
        public GameObject? SteeringWheelLeftHandTarget();
        public GameObject? SteeringWheelRightHandTarget();
    }
}
