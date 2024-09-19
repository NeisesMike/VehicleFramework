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
        public Rigidbody useRigidbody => GetComponent<ModVehicle>()?.useRigidbody;

        // Store the current Z rotation and the velocity used by SmoothDamp
        private float currentZRotation = 0f;
        private float rotationVelocity = 0f;
        public float smoothTime = 0.1f;

        public void Update()
        {
            if (useRigidbody == null)
            {
                return;
            }
            const float maxYAng = 7f;
            float percentAng = useRigidbody.angularVelocity.y / maxYAng;
            const float maxZRotate = 45f;
            float targetZRotation = percentAng * maxZRotate;

            // Smoothly update the Z rotation using SmoothDamp
            currentZRotation = Mathf.SmoothDamp(currentZRotation, -targetZRotation, ref rotationVelocity, smoothTime);

            // Apply the smoothed rotation to the transform
            transform.localEulerAngles = new Vector3(0f, 0f, currentZRotation);
        }
    }
}
