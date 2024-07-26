using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Engines;
using UnityEngine.SceneManagement;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    public static class VehicleRegistrar
    {
        public static int VehiclesRegistered = 0;
        public static int VehiclesPrefabricated = 0;
        private static bool RegistrySemaphore = false;
        public enum LogType
        {
            Log,
            Warn
        }
        public static void VerboseLog(LogType type, bool verbose, string message)
        {
            if (verbose)
            {
                switch (type)
                {
                    case LogType.Log:
                        Logger.Log(message);
                        break;
                    case LogType.Warn:
                        Logger.Warn(message);
                        break;
                    default:
                        break;

                }
            }
        }
        public static void RegisterVehicleLater(ModVehicle mv, bool verbose=false)
        {
            UWE.CoroutineHost.StartCoroutine(RegisterVehicle(mv, verbose));
        }
        public static IEnumerator RegisterVehicle(ModVehicle mv, bool verbose=false)
        {
            bool isNewEntry = true;
            foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
            {
                if (ve.name == mv.gameObject.name)
                {
                    VerboseLog(LogType.Warn, verbose, mv.gameObject.name + " was already registered.");
                    isNewEntry = false;
                    break;
                }
            }
            if (isNewEntry)
            {
                VehiclesRegistered++;
                if (RegistrySemaphore)
                {
                    VerboseLog(LogType.Log, verbose, "The " + mv.gameObject.name + " is waiting for Registration.");
                }
                while (RegistrySemaphore)
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                }
                RegistrySemaphore = true;
                VerboseLog(LogType.Log, verbose, "The " + mv.gameObject.name + " is beginning Registration.");
                PingType registeredPingType = VehicleManager.RegisterPingType((PingType)121, verbose);
                if (mv as Submarine != null)
                {
                    if (!ValidateRegistration(mv as Submarine, verbose))
                    {
                        Logger.Error("Invalid Submarine Registration for the " + mv.gameObject.name + ". Next.");
                        RegistrySemaphore = false;
                        yield break;
                    }
                }
                if (mv as Submersible != null)
                {
                    if (!ValidateRegistration(mv as Submersible, verbose))
                    {
                        Logger.Error("Invalid Submersible Registration for the " + mv.gameObject.name + ". Next.");
                        RegistrySemaphore = false;
                        yield break;
                    }
                }
                if (mv as Drone != null)
                {
                    if (!ValidateRegistration(mv as Drone, verbose))
                    {
                        Logger.Error("Invalid Submersible Registration for the " + mv.gameObject.name + ". Next.");
                        RegistrySemaphore = false;
                        yield break;
                    }
                }
                if (mv as Walker != null)
                {
                    if (!ValidateRegistration(mv as Walker, verbose))
                    {
                        Logger.Error("Invalid Submersible Registration for the " + mv.gameObject.name + ". Next.");
                        RegistrySemaphore = false;
                        yield break;
                    }
                }
                if (mv as Skimmer != null)
                {
                    if (!ValidateRegistration(mv as Skimmer, verbose))
                    {
                        Logger.Error("Invalid Submersible Registration for the " + mv.gameObject.name + ". Next.");
                        RegistrySemaphore = false;
                        yield break;
                    }
                }
                yield return UWE.CoroutineHost.StartCoroutine(VehicleBuilder.Prefabricate(mv, registeredPingType, verbose));
                RegistrySemaphore = false;
                mv.gameObject.SetActive(true);

                Logger.Log("Registered the " + mv.gameObject.name + ".");
            }
            yield break;
        }
        public static IEnumerator RegisterVehicle(ModVehicle mv)
        {
            yield return RegisterVehicle(mv, false);
        }
        public static bool ValidateRegistration(ModVehicle mv, bool verbose)
        {
            string thisName = "";
            try
            {
                if (mv is null)
                {
                    Logger.Error("An null mod vehicle was passed for registration.");
                    return false;
                }
                if (mv.name == "")
                {
                    Logger.Error(thisName + " An empty name was provided for this vehicle.");
                    return false;
                }
                VerboseLog(LogType.Log, verbose, "Validating the Registration of the " + mv.name);
                thisName = mv.name + ": ";
                if (mv.VehicleModel == null)
                {
                    Logger.Error(thisName + " A null ModVehicle.VehicleModel was passed for registration.");
                    return false;
                }
                if (mv.BaseCrushDepth < 0)
                {
                    Logger.Error(thisName + " A negative crush depth was passed for registration. This vehicle would take crush damage even out of water.");
                    return false;
                }
                if (mv.MaxHealth <= 0)
                {
                    Logger.Error(thisName + " A non-positive max health was passed for registration. This vehicle would be destroyed as soon as it awakens.");
                    return false;
                }
                if (mv.Mass <= 0)
                {
                    Logger.Error(thisName + " A non-positive mass was passed for registration. Don't do that.");
                    return false;
                }
                if (mv.InnateStorages is null || mv.InnateStorages.Count == 0)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " No ModVehicle.InnateStorages were provided. These are lockers the vehicle always has.");
                }
                if (mv.ModularStorages is null || mv.ModularStorages.Count == 0)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " No ModVehicle.ModularStorages were provided. These are lockers that can be unlocked with upgrades.");
                }
                if (mv.Upgrades is null || mv.Upgrades.Count == 0)
                {
                    Logger.Error(thisName + " No ModVehicle.Upgrades were provided. These specify interfaces the player can click to insert and remove upgrades.");
                    return false;
                }
                if (mv.Batteries is null || mv.Batteries.Count == 0)
                {
                    Logger.Error(thisName + " No ModVehicle.Batteries were provided. These are necessary to power the engines.");
                    return false;
                }
                if (mv.BackupBatteries is null || mv.BackupBatteries.Count == 0)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " No ModVehicle.BackupBatteries were provided. This collection of batteries belong to the AI and will be used exclusively for life support, auto-leveling, and other AI tasks. The AI will use the main batteries instead.");
                }
                if (mv.HeadLights is null || mv.HeadLights.Count == 0)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " No ModVehicle.HeadLights were provided. These lights would be activated when the player right clicks while piloting.");
                }
                if (mv.WaterClipProxies is null || mv.WaterClipProxies.Count == 0)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " No ModVehicle.WaterClipProxies were provided. These are necessary to keep the ocean surface out of the vehicle.");
                }
                if (mv.CanopyWindows is null || mv.CanopyWindows.Count == 0)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " No ModVehicle.CanopyWindows were provided. These must be specified to handle window transparencies.");
                }
                if(mv.BoundingBoxCollider ?? mv.BoundingBox?.GetComponentInChildren<BoxCollider>(true) == null)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " No BoundingBox BoxCollider was provided. If a BoundingBox GameObject was provided, it did not have a BoxCollider. Tether range is 10 meters. This vehicle will not be able to dock in the Moonpool. The build bots will assume this vehicle is 6m x 8m x 12m.");
                }
                if (mv.CollisionModel == null)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " A null ModVehicle.CollisionModel was provided. This is necessary for leviathans to grab the vehicle.");
                }
                foreach (VehicleParts.VehicleStorage vs in (mv.InnateStorages ?? Enumerable.Empty<VehicleParts.VehicleStorage>()).Concat(mv.ModularStorages ?? Enumerable.Empty<VehicleParts.VehicleStorage>()))
                {
                    if (vs.Container == null)
                    {
                        Logger.Error(thisName + " A null VehicleStorage.Container was provided. There would be no way to access this storage.");
                        return false;
                    }
                    if (vs.Height < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleStorage.Height was provided. This storage would have no space.");
                        return false;
                    }
                    if (vs.Width < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleStorage.Width was provided. This storage would have no space.");
                        return false;
                    }
                }
                foreach (VehicleParts.VehicleUpgrades vu in mv.Upgrades)
                {
                    if (vu.Interface == null)
                    {
                        Logger.Error(thisName + " A null VehicleUpgrades.Interface was provided. There would be no way to upgrade this vehicle.");
                        return false;
                    }
                    if (vu.Flap is null)
                    {
                        Logger.Error(thisName + " A null VehicleUpgrades.Flap was provided. The upgrades interface requires this. It will be rotated by the angles in this struct when activated. You can set the rotation angle to zero to take no action.");
                        return false;
                    }
                    if (vu.ModuleProxies is null)
                    {
                        VerboseLog(LogType.Log, verbose, thisName + " A null VehicleUpgrades.ModuleProxies was provided. VehicleFramework will not provide a model for this upgrade slot.");
                    }
                }
                foreach (VehicleParts.VehicleBattery vb in mv.Batteries.Concat(mv.BackupBatteries ?? Enumerable.Empty<VehicleParts.VehicleBattery>()))
                {
                    if (vb.BatterySlot == null)
                    {
                        Logger.Error(thisName + " A null VehicleBattery.BatterySlot was provided. There would be no way to access this battery.");
                        return false;
                    }
                    if (vb.BatteryProxy == null)
                    {
                        VerboseLog(LogType.Log, verbose, thisName + " A null VehicleBattery.BatteryProxy was provided. VehicleFramework will not provide a model for this battery slot.");
                    }
                }
                foreach (VehicleParts.VehicleFloodLight vfl in (mv.HeadLights ?? Enumerable.Empty<VehicleParts.VehicleFloodLight>()))
                {
                    if (vfl.Light == null)
                    {
                        Logger.Error(thisName + " A null VehicleFloodLight.Light was provided. There would be nothing from which to emit light.");
                        return false;
                    }
                    if (vfl.Light.transform.Find("VolumetricLight") == null)
                    {
                        Logger.Error(thisName + " A headlight was missing its VolumetricLight.");
                        return false;
                    }
                    if (vfl.Intensity < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleFloodLight.Intensity was provided. The light would be totally dark.");
                        return false;
                    }
                    if (vfl.Range < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleFloodLight.Range was provided. The light would be totally dark.");
                        return false;
                    }
                }
                if (mv.StorageRootObject == null)
                {
                    Logger.Error(thisName + " A null ModVehicle.StorageRootObject was provided. There would be no way to store things in this vehicle.");
                    return false;
                }
                if (mv.ModulesRootObject == null)
                {
                    Logger.Error(thisName + " A null ModVehicle.ModulesRootObject was provided. There would be no way to upgrade this vehicle.");
                    return false;
                }
                if (mv.StorageRootObject == mv.gameObject)
                {
                    Logger.Error(thisName + " The StorageRootObject was the same as the Vehicle itself. These must be uniquely identifiable objects!");
                    return false;
                }
                if (mv.ModulesRootObject == mv.gameObject)
                {
                    Logger.Error(thisName + " The ModulesRootObject was the same as the Vehicle itself. These must be uniquely identifiable objects!");
                    return false;
                }
                if (mv.LeviathanGrabPoint == null)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " A null ModVehicle.LeviathanGrabPoint was provided. This is where leviathans attach to the vehicle. The root object will be used instead.");
                }
                if (mv.Engine is null)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " A null ModVehicle.ModVehicleEngine was passed for registration. A default engine will be chosen.");
                }
            }
            catch (Exception e)
            {
                Logger.Error(thisName + " Exception Caught. Likely this ModVehicle is not implementing something it must. Check the abstract features of ModVehicle.\n\n" + e.ToString() + "\n\n");
                return false;
            }

            VerboseLog(LogType.Log, verbose, "The Registration of the " + mv.name + " as a ModVehicle has been Validated.");
            return true;
        }
        public static bool ValidateRegistration(Submarine mv, bool verbose)
        {
            if (!ValidateRegistration(mv as ModVehicle, verbose))
            {
                return false;
            }
            string thisName = "";
            try
            {
                thisName = mv.name + ": ";
                if (mv.PilotSeats == null || mv.PilotSeats.Count == 0)
                {
                    Logger.Error(thisName + " No ModVehicle.PilotSeats were provided. These specify what the player will click on to begin piloting the vehicle.");
                    return false;
                }
                if (mv.Hatches is null || mv.Hatches.Count == 0)
                {
                    Logger.Error(thisName + " No ModVehicle.Hatches were provided. These specify how the player will enter and exit the vehicle.");
                    return false;
                }
                if (mv.FloodLights is null || mv.FloodLights.Count == 0)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " No ModVehicle.FloodLights were provided. These lights would be activated on the control panel.");
                }
                if (mv.NavigationPortLights is null || mv.NavigationPortLights.Count == 0)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationStarboardLights is null || mv.NavigationStarboardLights.Count == 0)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationPositionLights is null || mv.NavigationPositionLights.Count == 0)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationWhiteStrobeLights is null || mv.NavigationWhiteStrobeLights.Count == 0)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationRedStrobeLights is null || mv.NavigationRedStrobeLights.Count == 0)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " Some navigation lights were missing.");
                }
                if (mv.TetherSources is null || mv.TetherSources.Count == 0)
                {
                    Logger.Error(thisName + " No ModVehicle.TetherSources were provided. These are necessary to keep the player 'grounded' within the vehicle.");
                    return false;
                }
                if (mv.ColorPicker == null)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " A null ModVehicle.ColorPicker was provided. You only need this if you implement the necessary painting functions.");
                }
                if (mv.Fabricator == null)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " A null ModVehicle.Fabricator was provided. The Submarine will not come with a fabricator at construction-time.");
                }
                if (mv.ControlPanel == null)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " A null ModVehicle.ControlPanel was provided. This is necessary to toggle floodlights.");
                }
                if (mv.SteeringWheelLeftHandTarget == null)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " A null ModVehicle.SteeringWheelLeftHandTarget was provided. This is what the player's left hand will 'grab' while you pilot.");
                }
                if (mv.SteeringWheelRightHandTarget == null)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " A null ModVehicle.SteeringWheelRightHandTarget was provided.  This is what the player's right hand will 'grab' while you pilot.");
                }
                foreach (VehicleParts.VehiclePilotSeat ps in mv.PilotSeats)
                {
                    if (ps.Seat == null)
                    {
                        Logger.Error(thisName + " A null PilotSeat.Seat was provided. There would be no way to pilot this vehicle.");
                        return false;
                    }
                    if (ps.SitLocation == null)
                    {
                        Logger.Error(thisName + " A null PilotSeat.SitLocation was provided. There would be no way to pilot this vehicle.");
                        return false;
                    }
                    if (ps.LeftHandLocation == null)
                    {
                        VerboseLog(LogType.Log, verbose, thisName + " A null PilotSeat.LeftHandLocation was provided. (It's unused anyway)");
                    }
                    if (ps.RightHandLocation == null)
                    {
                        VerboseLog(LogType.Log, verbose, thisName + " A null PilotSeat.RightHandLocation was provided. (It's unused anyway)");
                    }
                    if (ps.ExitLocation == null)
                    {
                        VerboseLog(LogType.Warn, verbose, thisName + " A null PilotSeat.ExitLocation was provided. You might need this if you exit from piloting into a weird place.");
                    }
                }
                foreach (VehicleParts.VehicleHatchStruct vhs in mv.Hatches)
                {
                    if (vhs.Hatch == null)
                    {
                        Logger.Error(thisName + " A null VehicleHatchStruct.Hatch was provided. There would be no way to enter/exit this vehicle.");
                        return false;
                    }
                    if (vhs.EntryLocation == null)
                    {
                        Logger.Error(thisName + " A null VehicleHatchStruct.EntryLocation was provided. There would be no way to enter/exit this vehicle.");
                        return false;
                    }
                    if (vhs.ExitLocation == null)
                    {
                        Logger.Error(thisName + " A null VehicleHatchStruct.ExitLocation was provided. There would be no way to enter/exit this vehicle.");
                        return false;
                    }
                    if (vhs.SurfaceExitLocation == null)
                    {
                        VerboseLog(LogType.Warn, verbose, thisName + " A null VehicleHatchStruct.SurfaceExitLocation was provided. You might need this if you exit weirdly near the surface.");
                    }
                }
                foreach (VehicleParts.VehicleStorage vs in mv.ModularStorages ?? Enumerable.Empty<VehicleParts.VehicleStorage>())
                {
                    if (vs.Container == null)
                    {
                        Logger.Error(thisName + " A null VehicleStorage.Container was provided. There would be no way to access this storage.");
                        return false;
                    }
                    if (vs.Height < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleStorage.Height was provided. This storage would have no space.");
                        return false;
                    }
                    if (vs.Width < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleStorage.Width was provided. This storage would have no space.");
                        return false;
                    }
                }
                foreach (VehicleParts.VehicleFloodLight vfl in mv.FloodLights ?? Enumerable.Empty<VehicleParts.VehicleFloodLight>())
                {
                    if (vfl.Light == null)
                    {
                        Logger.Error(thisName + " A null VehicleFloodLight.Light was provided. There would be nothing from which to emit light.");
                        return false;
                    }
                    if (vfl.Light.transform.Find("VolumetricLight") == null)
                    {
                        Logger.Error(thisName + " A floodlight was missing its VolumetricLight.");
                        return false;
                    }
                    if (vfl.Intensity < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleFloodLight.Intensity was provided. The light would be totally dark.");
                        return false;
                    }
                    if (vfl.Range < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleFloodLight.Range was provided. The light would be totally dark.");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(thisName + " Exception Caught. Likely this Submarine is not implementing something it must. Check the abstract features of Submarine.\n\n" + e.ToString() + "\n\n");
                return false;
            }

            VerboseLog(LogType.Log, verbose, "The Registration of the " + mv.name + " as a Submarine has been Validated.");
            return true;
        }
        public static bool ValidateRegistration(Submersible mv, bool verbose)
        {
            if (!ValidateRegistration(mv as ModVehicle, verbose))
            {
                return false;
            }
            string thisName = "";
            try
            {
                thisName = mv.name + ": ";
                if (mv.Hatches is null || mv.Hatches.Count == 0)
                {
                    Logger.Error(thisName + " No ModVehicle.Hatches were provided. These specify how the player will enter and exit the vehicle.");
                    return false;
                }
                if (mv.SteeringWheelLeftHandTarget == null)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " A null ModVehicle.SteeringWheelLeftHandTarget was provided. This is what the player's left hand will 'grab' while you pilot.");
                }
                if (mv.SteeringWheelRightHandTarget == null)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " A null ModVehicle.SteeringWheelRightHandTarget was provided.  This is what the player's right hand will 'grab' while you pilot.");
                }
                if (mv.PilotSeat.Seat == null)
                {
                    Logger.Error(thisName + " A null PilotSeat.Seat was provided. There would be no way to pilot this vehicle.");
                    return false;
                }
                if (mv.PilotSeat.SitLocation == null)
                {
                    Logger.Error(thisName + " A null PilotSeat.SitLocation was provided. There would be no way to pilot this vehicle.");
                    return false;
                }
                if (mv.PilotSeat.LeftHandLocation == null)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " A null PilotSeat.LeftHandLocation was provided. (It's unused anyway)");
                }
                if (mv.PilotSeat.RightHandLocation == null)
                {
                    VerboseLog(LogType.Log, verbose, thisName + " A null PilotSeat.RightHandLocation was provided. (It's unused anyway)");
                }
                if (mv.PilotSeat.ExitLocation == null)
                {
                    VerboseLog(LogType.Warn, verbose, thisName + " A null PilotSeat.ExitLocation was provided. You might need this if you exit from piloting into a weird place.");
                }
                foreach (VehicleParts.VehicleHatchStruct vhs in mv.Hatches)
                {
                    if (vhs.Hatch == null)
                    {
                        Logger.Error(thisName + " A null VehicleHatchStruct.Hatch was provided. There would be no way to enter/exit this vehicle.");
                        return false;
                    }
                    if (vhs.ExitLocation is null)
                    {
                        Logger.Error(thisName + " A null VehicleHatchStruct.ExitLocation was provided. There would be no way to enter/exit this vehicle.");
                        return false;
                    }
                    if (vhs.SurfaceExitLocation is null)
                    {
                        VerboseLog(LogType.Warn, verbose, thisName + " A null VehicleHatchStruct.SurfaceExitLocation was provided. You might need this if you exit weirdly near the surface.");
                    }
                }
                foreach (VehicleParts.VehicleStorage vs in mv.ModularStorages ?? Enumerable.Empty<VehicleParts.VehicleStorage>())
                {
                    if (vs.Container == null)
                    {
                        Logger.Error(thisName + " A null VehicleStorage.Container was provided. There would be no way to access this storage.");
                        return false;
                    }
                    if (vs.Height < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleStorage.Height was provided. This storage would have no space.");
                        return false;
                    }
                    if (vs.Width < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleStorage.Width was provided. This storage would have no space.");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(thisName + " Exception Caught. Likely this Submersible is not implementing something it must. Check the abstract features of Submersible.\n\n" + e.ToString() + "\n\n");
                return false;
            }

            VerboseLog(LogType.Log, verbose, "The Registration of the " + mv.name + " as a Submersible has been Validated.");
            return true;
        }
        public static bool ValidateRegistration(Drone mv, bool verbose)
        {
            if (!ValidateRegistration(mv as ModVehicle, verbose))
            {
                return false;
            }
            string thisName = "";
            try
            {
                thisName = mv.name + ": ";
                if (mv.CameraLocation is null)
                {
                    Logger.Error(thisName + " No Drone.CameraLocation was provided. This is where VF will place the camera while the Drone is remotely piloted.");
                    return false;
                }
                foreach (VehicleParts.VehicleStorage vs in mv.ModularStorages ?? Enumerable.Empty<VehicleParts.VehicleStorage>())
                {
                    if (vs.Container == null)
                    {
                        Logger.Error(thisName + " A null VehicleStorage.Container was provided. There would be no way to access this storage.");
                        return false;
                    }
                    if (vs.Height < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleStorage.Height was provided. This storage would have no space.");
                        return false;
                    }
                    if (vs.Width < 0)
                    {
                        Logger.Error(thisName + " A negative VehicleStorage.Width was provided. This storage would have no space.");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(thisName + " Exception Caught. Likely this Drone is not implementing something it must. Check the abstract features of Drone.\n\n" + e.ToString() + "\n\n");
                return false;
            }

            VerboseLog(LogType.Log, verbose, "The Registration of the " + mv.name + " as a Drone has been Validated.");
            return true;


            /*
            public abstract Camera Camera { get; }
            */

        }

    }
}
