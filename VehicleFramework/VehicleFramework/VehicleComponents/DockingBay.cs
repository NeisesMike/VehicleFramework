using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

namespace VehicleFramework.VehicleComponents
{
    // TODO: disallow undocking when not enough space
    public class DockingBay : MonoBehaviour
    {
        public List<TechType> whitelist = new List<TechType>();
        public Vehicle currentDockedVehicle { get; protected set; }
        private Func<Vehicle, Transform> vehicleDockedPosition = null;
        private Transform dockedVehicleExitPosition = null;
        private Transform vehicleDockingTrigger = null;
        private Coroutine dockAnimation = null;
        private float dockingDistanceThreshold = 6f;
        private Action<Vehicle> internalUndockAction;
        private bool isInitialized = false;

        public bool Initialize(Func<Vehicle, Transform> getDockedPosition, Transform exit, Transform dockTrigger, List<TechType> inputWhitelist, Action<Vehicle> UndockAction)
        {
            const string failMessagePrefix = "Dockingbay Initialization Error: Input transform was null:";
            string failTransformPrefix = $"{failMessagePrefix} Input transform was null:";
            if (getDockedPosition == null)
            {
                Logger.Log($"{failTransformPrefix} dockedPosition");
                return false;
            }
            if (exit == null)
            {
                Logger.Log($"{failTransformPrefix} exit");
                return false;
            }
            if (dockTrigger == null)
            {
                Logger.Log($"{failTransformPrefix} dockTrigger");
                return false;
            }
            if(inputWhitelist != null)
            {
                inputWhitelist.ForEach(x => whitelist.Add(x));
            }
            vehicleDockedPosition = getDockedPosition;
            dockedVehicleExitPosition = exit;
            vehicleDockingTrigger = dockTrigger;
            void defaultUndockAction(Vehicle dockingVehicle)
            {
                dockingVehicle.useRigidbody.AddRelativeForce(Vector3.down);
            }
            internalUndockAction = UndockAction ?? defaultUndockAction;
            isInitialized = true;
            return true;
        }
        public void Detach(bool withPlayer)
        {
            if (dockAnimation == null && IsSufficientSpace())
            {
                UWE.CoroutineHost.StartCoroutine(InternalDetach(withPlayer));
            }
        }
        protected virtual bool IsSufficientSpace()
        {
            return true;
        }
        public virtual bool IsTargetValid(Vehicle thisPossibleTarget)
        {
            return true;
        }
        protected virtual void TryRechargeDockedVehicle() { }
        protected virtual Vehicle GetDockingTarget()
        {
            Vehicle target = null;
            if(whitelist.Count > 0)
            {
                float closestTargetDistance = 99999;
                foreach (TechType tt in whitelist)
                {
                    Vehicle innerTarget = Admin.GameObjectManager<Vehicle>.FindNearestSuch(transform.position, x => x.GetTechType() == tt);
                    if(innerTarget == null)
                    {
                        continue;
                    }
                    float innerDistance = Vector3.Distance(vehicleDockingTrigger.transform.position, innerTarget.transform.position);
                    if (innerDistance < closestTargetDistance)
                    {
                        target = innerTarget;
                        closestTargetDistance = innerDistance;
                    }
                }
                return target;
            }
            else
            {
                bool IsValidDockingTarget(Vehicle thisPossibleTarget)
                {
                    return thisPossibleTarget != GetComponent<Vehicle>() && IsTargetValid(thisPossibleTarget);
                }
                return Admin.GameObjectManager<Vehicle>.FindNearestSuch(transform.position, IsValidDockingTarget);
            }
        }
        protected virtual void UpdateDockedVehicle()
        {
            currentDockedVehicle.transform.position = vehicleDockedPosition(currentDockedVehicle).position;
            currentDockedVehicle.transform.rotation = vehicleDockedPosition(currentDockedVehicle).rotation;
            currentDockedVehicle.liveMixin.shielded = true;
            currentDockedVehicle.useRigidbody.detectCollisions = false;
            currentDockedVehicle.crushDamage.enabled = false;
            currentDockedVehicle.UpdateCollidersForDocking(true);
            if (currentDockedVehicle is SeaMoth)
            {
                (currentDockedVehicle as SeaMoth).toggleLights.SetLightsActive(false);
                currentDockedVehicle.GetComponent<SeaMoth>().enabled = true; // why is this necessary?
            }
            else if(currentDockedVehicle is ModVehicle mv)
            {
                if (mv.headlights.IsLightsOn)
                {
                    mv.headlights.Toggle();
                }
            }
        }
        protected virtual void HandleDockDoors(TechType dockedVehicle, bool open)
        {
        }
        protected virtual bool ValidateAttachment(Vehicle dockTarget)
        {
            if (Vector3.Distance(vehicleDockingTrigger.position, dockTarget.transform.position) >= dockingDistanceThreshold)
            {
                // dockTarget is too far away
                return false;
            }
            return true;
        }
        protected virtual void OnDockUpdate() { }
        protected virtual void OnStartedDocking()
        {
        }
        protected virtual void OnFinishedDocking(Vehicle dockingVehicle)
        {
            bool isPlayerSeamoth = dockingVehicle is SeaMoth && Player.main.inSeamoth;
            bool isPlayerPrawn = dockingVehicle is Exosuit && Player.main.inExosuit;
            if (isPlayerSeamoth || isPlayerPrawn)
            {
                Player.main.rigidBody.velocity = Vector3.zero;
                Player.main.ToNormalMode(false);
                Player.main.rigidBody.angularVelocity = Vector3.zero;
                Player.main.ExitLockedMode(false, false);
                Player.main.SetPosition(dockedVehicleExitPosition.position);
                Player.main.ExitSittingMode();
                Player.main.SetPosition(dockedVehicleExitPosition.position);
                ModVehicle.TeleportPlayer(dockedVehicleExitPosition.position);
            }
            dockingVehicle.transform.SetParent(transform);
        }
        protected virtual void OnStartedUndocking(bool withPlayer)
        {
            HandleDockDoors(currentDockedVehicle.GetTechType(), true);
            currentDockedVehicle.useRigidbody.velocity = Vector3.zero;
            currentDockedVehicle.transform.SetParent(this.transform.parent);
            if (withPlayer)
            {
                currentDockedVehicle.EnterVehicle(Player.main, true, true);
                // disabling the avatarinputhandler is a brutish way to do this
                // but I want the vehicle to continue drifting down,
                // until it leaves the docking trigger area
                // otherwise, the player could take advantage of their lack of collision,
                // since vehicle collisions aren't re-enabled until undocking is complete
                AvatarInputHandler.main.gameObject.SetActive(false);
            }
        }
        protected virtual IEnumerator DoUndockingAnimations()
        {
            internalUndockAction?.Invoke(currentDockedVehicle);
            // wait until the vehicle is "just" far enough away.
            // Should probably disallow undocking if a collider is too close, lest we clip into it.
            yield return new WaitUntil(() => Vector3.Distance(currentDockedVehicle.transform.position, vehicleDockingTrigger.position) > dockingDistanceThreshold);
        }
        protected virtual void OnFinishedUndocking(bool hasPlayer)
        {
            currentDockedVehicle.liveMixin.shielded = false;
            currentDockedVehicle.useRigidbody.detectCollisions = true;
            currentDockedVehicle.crushDamage.enabled = true;
            currentDockedVehicle.UpdateCollidersForDocking(false);
            if (hasPlayer)
            {
                AvatarInputHandler.main.gameObject.SetActive(true);
            }
        }
        protected virtual IEnumerator DoDockingAnimations(Vehicle dockingVehicle, float duration, float duration2)
        {
            yield return UWE.CoroutineHost.StartCoroutine(MoveAndRotate(dockingVehicle, vehicleDockingTrigger, duration));
            yield return UWE.CoroutineHost.StartCoroutine(MoveAndRotate(dockingVehicle, vehicleDockedPosition(dockingVehicle), duration2));
        }

        private void Update()
        {
            if (!isInitialized)
            {
                return;
            }
            OnDockUpdate();
            if (dockAnimation != null)
            {
                return;
            }
            else if (currentDockedVehicle == null)
            {
                TryAttachVehicle();
            }
            else
            {
                HandleDockDoors(currentDockedVehicle.GetTechType(), false);
                TryRechargeDockedVehicle();
                UpdateDockedVehicle();
            }
        }
        private void TryAttachVehicle()
        {
            Vehicle dockTarget = GetDockingTarget();
            if (dockTarget == null)
            {
                HandleDockDoors(TechType.None, false);
                return;
            }
            if (Vector3.Distance(vehicleDockedPosition(dockTarget).position, dockTarget.transform.position) < 20)
            {
                HandleDockDoors(dockTarget.GetTechType(), true);
            }
            else
            {
                HandleDockDoors(dockTarget.GetTechType(), false);
            }
            if (ValidateAttachment(dockTarget))
            {
                UWE.CoroutineHost.StartCoroutine(InternalAttach(dockTarget));
            }
        }
        private IEnumerator InternalAttach(Vehicle dockTarget)
        {
            if(dockTarget == null)
            {
                yield break;
            }
            OnStartedDocking();
            dockAnimation = UWE.CoroutineHost.StartCoroutine(DoDockingAnimations(dockTarget, 1f, 1f));
            yield return dockAnimation;
            OnFinishedDocking(dockTarget);
            currentDockedVehicle = dockTarget;
            dockAnimation = null;
        }
        private IEnumerator InternalDetach(bool withPlayer)
        {
            if (currentDockedVehicle == null)
            {
                yield break;
            }
            OnStartedUndocking(withPlayer);
            dockAnimation = UWE.CoroutineHost.StartCoroutine(DoUndockingAnimations());
            yield return dockAnimation;
            OnFinishedUndocking(withPlayer);
            currentDockedVehicle = null;
            dockAnimation = null;
        }
        private IEnumerator MoveAndRotate(Vehicle objectToMove, Transform firstTarget, float duration)
        {
            // Get starting position and rotation
            Vector3 startPosition = objectToMove.transform.position;
            Quaternion startRotation = objectToMove.transform.rotation;
            // Get target position and rotation
            Vector3 midPosition = firstTarget.position;
            Quaternion midRotation = firstTarget.rotation;
            float elapsedTime = 0f;
            // Move and rotate over the given duration
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                // Interpolate position and rotation
                objectToMove.transform.position = Vector3.Lerp(startPosition, midPosition, elapsedTime / duration);
                objectToMove.transform.rotation = Quaternion.Slerp(startRotation, midRotation, elapsedTime / duration);
                yield return null; // Wait for the next frame
            }
            // Ensure the final position and rotation are exactly the target's
            objectToMove.transform.position = midPosition;
            objectToMove.transform.rotation = midRotation;
        }
    }
}
