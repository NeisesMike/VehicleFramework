using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.VehicleComponents
{
    // This component can be added to a steering wheel
    // in order to make it animate in a way that corresponds
    // with the movements of the vehicle.
    public class SteeringWheel : MonoBehaviour
    {
        public Rigidbody useRigidbody => GetComponentInParent<ModVehicle>()?.useRigidbody;
        // Store the current Z rotation and the velocity used by SmoothDamp
        private float currentYawRotation = 0f;
        private float rotationVelocity = 0f;
        public float smoothTime = 0.1f;
        public YawAxis yawAxis = YawAxis.z;
        public float maxExpectedAngularVelocity = 7f;
        public float maxSteeringWheelAngle = 45f;
        public enum YawAxis
        {
            x,
            minusX,
            y,
            minusY,
            z,
            minusZ
        }
        public void Update()
        {
            if (useRigidbody == null)
            {
                return;
            }
            float percentAng = useRigidbody.angularVelocity.y / maxExpectedAngularVelocity;
            float targetYawRotation = percentAng * maxSteeringWheelAngle;

            // Smoothly update the Z rotation using SmoothDamp
            currentYawRotation = Mathf.SmoothDamp(currentYawRotation, -targetYawRotation, ref rotationVelocity, smoothTime);

            // Apply the smoothed rotation to the transform
            switch(yawAxis)
            {
                case YawAxis.x:
                    transform.localEulerAngles = new Vector3(currentYawRotation, 0f, 0f);
                    break;
                case YawAxis.y:
                    transform.localEulerAngles = new Vector3(0f, currentYawRotation, 0f);
                    break;
                case YawAxis.z:
                    transform.localEulerAngles = new Vector3(0f, 0f, currentYawRotation);
                    break;
                case YawAxis.minusX:
                    transform.localEulerAngles = new Vector3(-currentYawRotation, 0f, 0f);
                    break;
                case YawAxis.minusY:
                    transform.localEulerAngles = new Vector3(0f, -currentYawRotation, 0f);
                    break;
                case YawAxis.minusZ:
                    transform.localEulerAngles = new Vector3(0f, 0f, -currentYawRotation);
                    break;
            }
        }
    }
}
