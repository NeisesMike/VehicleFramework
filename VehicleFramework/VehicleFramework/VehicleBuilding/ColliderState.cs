using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class ColliderState
    {
        public Collider collider;
        public bool wasEnabled;
        public ColliderState(Collider collider, bool wasEnabled)
        {
            this.collider = collider;
            this.wasEnabled = wasEnabled;
        }
        public static List<ColliderState> DisableAllColliders(GameObject gameObject)
        {
            var colliderStates = new List<ColliderState>();
            var allColliders = gameObject.GetComponentsInChildren<Collider>();

            foreach (var collider in allColliders)
            {
                colliderStates.Add(new ColliderState(collider, collider.enabled));
                collider.enabled = false;
            }

            return colliderStates;
        }
        public static void RestoreColliderStates(List<ColliderState> colliderStates)
        {
            foreach (var state in colliderStates)
            {
                if (state.collider != null) // Check if the collider hasn't been destroyed
                {
                    state.collider.enabled = state.wasEnabled;
                }
            }
        }

    }
}
