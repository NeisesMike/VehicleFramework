using UnityEngine;
using System.Collections;

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
        public Vehicle currentDockedVehicle { get; protected set; }
        private Coroutine dockAnimation = null;

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
                Admin.Utils.StartCoroutine(InternalDetach(withPlayer));
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
        protected virtual void UpdateDockedVehicle()
        {
            currentDockedVehicle.transform.position = GetDockedPosition(currentDockedVehicle).position;
            currentDockedVehicle.transform.rotation = GetDockedPosition(currentDockedVehicle).rotation;
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
            if(GetComponent<VehicleTypes.Submarine>() != null)
            {
                GetComponent<VehicleTypes.Submarine>().PlayerEntry();
            }
        }
        protected virtual void OnStartedUndocking(bool withPlayer)
        {
            HandleDockDoors(currentDockedVehicle.GetTechType(), true);
            currentDockedVehicle.useRigidbody.velocity = Vector3.zero;
            currentDockedVehicle.transform.SetParent(this.transform.parent);
            if (withPlayer)
            {
                // disabling the avatarinputhandler is a brutish way to do this
                // but I want the vehicle to continue drifting down,
                // until it leaves the docking trigger area
                // otherwise, the player could take advantage of their lack of collision,
                // since vehicle collisions aren't re-enabled until undocking is complete
                AvatarInputHandler.main.gameObject.SetActive(false);
            }
            if(currentDockedVehicle is SeaMoth || currentDockedVehicle is Exosuit)
            {
                currentDockedVehicle.EnterVehicle(Player.main, true, true);
            }
            if(currentDockedVehicle is ModVehicle mv)
            {
                mv.OnVehicleUndocked();
                mv.useRigidbody.detectCollisions = false;
            }
        }
        protected virtual IEnumerator DoUndockingAnimations()
        {
            UndockAction(currentDockedVehicle);
            // wait until the vehicle is "just" far enough away.
            // Should probably disallow undocking if a collider is too close, lest we clip into it.
            yield return new WaitUntil(() => Vector3.Distance(currentDockedVehicle.transform.position, DockTrigger.position) > DockingDistanceThreshold);
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
            yield return Admin.Utils.StartCoroutine(MoveAndRotate(dockingVehicle, DockTrigger, duration));
            yield return Admin.Utils.StartCoroutine(MoveAndRotate(dockingVehicle, GetDockedPosition(dockingVehicle), duration2));
        }

        private void Update()
        {
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
                Admin.Utils.StartCoroutine(InternalAttach(dockTarget));
            }
        }
        private IEnumerator InternalAttach(Vehicle dockTarget)
        {
            if(dockTarget == null)
            {
                yield break;
            }
            OnStartedDocking();
            dockAnimation = Admin.Utils.StartCoroutine(DoDockingAnimations(dockTarget, 1f, 1f));
            yield return dockAnimation;
            dockTarget.docked = true;
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
            currentDockedVehicle.docked = false;
            dockAnimation = Admin.Utils.StartCoroutine(DoUndockingAnimations());
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
