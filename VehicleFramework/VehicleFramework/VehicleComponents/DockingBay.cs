using System.Collections;
using UnityEngine;
//using VehicleFramework.BaseVehicle;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Extensions;

namespace VehicleFramework.VehicleComponents
{
    // TODO: disallow undocking when not enough space

    // TODO: Is it an issue that we set vehicle.docked = true/false ?
    //       Previous iterations of DockingBay did not touch vehicle.docked,
    //       But if we do so, we get free compat with DockedVehicleStorageAccess...

    // ISSUE: hand animations (seamoth/prawn) when undocking are weird
    //        until AvatarInputHandler.main.gameObject.SetActive(true);
    //        But the input handler must be inactive until
    //        currentDockedVehicle.useRigidbody.detectCollisions = true;  
    //        Otherwise, the player could drive inside of the dock and get stuck.
    // SOLUTION: activate the input handler during OnStartedUndocking, and
    //           control exit more cleverly, e.g. by animation rather than physics

    public abstract class DockingBay : MonoBehaviour
    {
        public Vehicle? CurrentDockedVehicle { get; protected set; }
        private Coroutine? dockAnimation = null;

        public abstract Transform GetDockedPosition(Vehicle dockedVehicle);
        public abstract Transform PlayerExitLocation { get; }
        public abstract Transform DockTrigger { get; }
        public virtual void UndockAction(Vehicle dockedVehicle)
        {
            dockedVehicle.useRigidbody.AddRelativeForce(Vector3.down);
        }
        public virtual float DockingDistanceThreshold { get; set; } = 6f;

        public virtual void Awake()
        {
            const string failMessagePrefix = "Dockingbay Initialization Error:";
            string failTransformPrefix = $"{failMessagePrefix} Input transform was null:";
            if (PlayerExitLocation == null)
            {
                Logger.Log($"{failTransformPrefix} PlayerExitLocation. Destroying this component.");
                Component.DestroyImmediate(this);
                return;
            }
            if (DockTrigger == null)
            {
                Logger.Log($"{failTransformPrefix} DockTrigger. Destroying this component.");
                Component.DestroyImmediate(this);
                return;
            }
        }
        public void Detach(bool withPlayer)
        {
            if (dockAnimation == null && IsSufficientSpace())
            {
                Admin.SessionManager.StartCoroutine(InternalDetach(withPlayer));
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
        protected virtual void TryRechargeDockedVehicle(Vehicle cdVehicle) { }
        protected virtual Vehicle GetDockingTarget()
        {
            bool IsValidDockingTarget(Vehicle thisPossibleTarget)
            {
                if(thisPossibleTarget is Exosuit exo)
                {
                    if (exo.GetIsGrappling())
                    {
                        return false;
                    }
                }
                return thisPossibleTarget != GetComponent<Vehicle>() && IsTargetValid(thisPossibleTarget);
            }
            return Admin.GameObjectManager<Vehicle>.FindNearestSuch(transform.position, IsValidDockingTarget);
        }
        protected virtual void UpdateDockedVehicle(Vehicle cdVehicle)
        {
            cdVehicle.transform.position = GetDockedPosition(cdVehicle).position;
            cdVehicle.transform.rotation = GetDockedPosition(cdVehicle).rotation;
            cdVehicle.liveMixin.shielded = true;
            cdVehicle.useRigidbody.detectCollisions = false;
            cdVehicle.crushDamage.enabled = false;
            cdVehicle.UpdateCollidersForDocking(true);
            if (cdVehicle is SeaMoth seamoth)
            {
                seamoth.toggleLights.SetLightsActive(false);
                cdVehicle.GetComponent<SeaMoth>().enabled = true; // why is this necessary?
            }
            else if(cdVehicle is ModVehicle mv && mv.headlights != null)
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
            if (Vector3.Distance(DockTrigger.position, dockTarget.transform.position) >= DockingDistanceThreshold)
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
                Player.main.SetPosition(PlayerExitLocation.position);
                Player.main.ExitSittingMode();
                Player.main.SetPosition(PlayerExitLocation.position);
                ModVehicle.TeleportPlayer(PlayerExitLocation.position);
            }
            if(dockingVehicle is ModVehicle mv)
            {
                mv.DeselectSlots();
                Player.main.SetPosition(PlayerExitLocation.position);
                ModVehicle.TeleportPlayer(PlayerExitLocation.position);
                mv.OnVehicleDocked(Vector3.zero);
            }
            dockingVehicle.transform.SetParent(transform);
            GetComponent<VehicleTypes.Submarine>()?.PlayerEntry();
        }
        protected virtual void OnStartedUndocking(bool withPlayer, Vehicle cdVehicle)
        {
            HandleDockDoors(cdVehicle.GetTechType(), true);
            cdVehicle.useRigidbody.velocity = Vector3.zero;
            cdVehicle.transform.SetParent(this.transform.parent);
            if (withPlayer)
            {
                // disabling the avatarinputhandler is a brutish way to do this
                // but I want the vehicle to continue drifting down,
                // until it leaves the docking trigger area
                // otherwise, the player could take advantage of their lack of collision,
                // since vehicle collisions aren't re-enabled until undocking is complete
                AvatarInputHandler.main.gameObject.SetActive(false);
            }
            if(cdVehicle is SeaMoth || cdVehicle is Exosuit)
            {
                cdVehicle.EnterVehicle(Player.main, true, true);
            }
            if(cdVehicle is ModVehicle mv)
            {
                mv.OnVehicleUndocked();
                mv.useRigidbody.detectCollisions = false;
            }
        }
        protected virtual IEnumerator DoUndockingAnimations(Vehicle cdVehicle)
        {
            UndockAction(cdVehicle);
            // wait until the vehicle is "just" far enough away.
            // Should probably disallow undocking if a collider is too close, lest we clip into it.
            yield return new WaitUntil(() => Vector3.Distance(cdVehicle.transform.position, DockTrigger.position) > DockingDistanceThreshold);
        }
        protected virtual void OnFinishedUndocking(bool hasPlayer, Vehicle cdVehicle)
        {
            cdVehicle.liveMixin.shielded = false;
            cdVehicle.useRigidbody.detectCollisions = true;
            cdVehicle.crushDamage.enabled = true;
            cdVehicle.UpdateCollidersForDocking(false);
            if (hasPlayer)
            {
                AvatarInputHandler.main.gameObject.SetActive(true);
            }
        }
        protected virtual IEnumerator DoDockingAnimations(Vehicle dockingVehicle, float duration, float duration2)
        {
            yield return Admin.SessionManager.StartCoroutine(MoveAndRotate(dockingVehicle, DockTrigger, duration));
            yield return Admin.SessionManager.StartCoroutine(MoveAndRotate(dockingVehicle, GetDockedPosition(dockingVehicle), duration2));
        }

        private void Update()
        {
            OnDockUpdate();
            if (dockAnimation != null)
            {
                return;
            }
            else if (CurrentDockedVehicle == null)
            {
                TryAttachVehicle();
            }
            else
            {
                HandleDockDoors(CurrentDockedVehicle.GetTechType(), false);
                TryRechargeDockedVehicle(CurrentDockedVehicle);
                UpdateDockedVehicle(CurrentDockedVehicle);
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
            if (Vector3.Distance(GetDockedPosition(dockTarget).position, dockTarget.transform.position) < 20)
            {
                HandleDockDoors(dockTarget.GetTechType(), true);
            }
            else
            {
                HandleDockDoors(dockTarget.GetTechType(), false);
            }
            if (ValidateAttachment(dockTarget))
            {
                Admin.SessionManager.StartCoroutine(InternalAttach(dockTarget));
            }
        }
        private IEnumerator InternalAttach(Vehicle dockTarget)
        {
            if(dockTarget == null)
            {
                yield break;
            }
            OnStartedDocking();
            dockAnimation = Admin.SessionManager.StartCoroutine(DoDockingAnimations(dockTarget, 1f, 1f));
            yield return dockAnimation;
            dockTarget.docked = true;
            OnFinishedDocking(dockTarget);
            CurrentDockedVehicle = dockTarget;
            dockAnimation = null;
        }
        private IEnumerator InternalDetach(bool withPlayer)
        {
            if (CurrentDockedVehicle == null)
            {
                yield break;
            }
            OnStartedUndocking(withPlayer, CurrentDockedVehicle);
            CurrentDockedVehicle.docked = false;
            dockAnimation = Admin.SessionManager.StartCoroutine(DoUndockingAnimations(CurrentDockedVehicle));
            yield return dockAnimation;
            OnFinishedUndocking(withPlayer, CurrentDockedVehicle);
            CurrentDockedVehicle = null;
            dockAnimation = null;
        }
        private static IEnumerator MoveAndRotate(Vehicle objectToMove, Transform firstTarget, float duration)
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
