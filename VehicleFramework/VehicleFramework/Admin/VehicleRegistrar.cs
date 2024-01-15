using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Nautilus.Json;
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
        public static IEnumerator RegisterVehicle(ModVehicle mv)
        {
            bool isNewEntry = true;
            foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
            {
                if (ve.name == mv.gameObject.name)
                {
                    Logger.Warn(mv.gameObject.name + " was already registered.");
                    isNewEntry = false;
                    break;
                }
            }
            if (isNewEntry)
            {
                VehiclesRegistered++;
                if (RegistrySemaphore)
                {
                    Logger.Log("The " + mv.gameObject.name + " is waiting for Registration.");
                }
                while (RegistrySemaphore)
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                }
                RegistrySemaphore = true;
                Logger.Log("The " + mv.gameObject.name + " is beginning Registration.");
                PingType registeredPingType = VehicleManager.RegisterPingType((PingType)121);
                if(mv as Submarine != null)
                {
                    if (!ValidateRegistration(mv as Submarine))
                    {
                        Logger.Error("Invalid Submarine Registration for the " + mv.gameObject.name + ". Next.");
                        RegistrySemaphore = false;
                        yield break;
                    }
                }
                if (mv as Submersible != null)
                {
                    if (!ValidateRegistration(mv as Submersible))
                    {
                        Logger.Error("Invalid Submersible Registration for the " + mv.gameObject.name + ". Next.");
                        RegistrySemaphore = false;
                        yield break;
                    }
                }
                if (mv as Drone != null)
                {
                    if (!ValidateRegistration(mv as Drone))
                    {
                        Logger.Error("Invalid Submersible Registration for the " + mv.gameObject.name + ". Next.");
                        RegistrySemaphore = false;
                        yield break;
                    }
                }
                if (mv as Walker != null)
                {
                    if (!ValidateRegistration(mv as Walker))
                    {
                        Logger.Error("Invalid Submersible Registration for the " + mv.gameObject.name + ". Next.");
                        RegistrySemaphore = false;
                        yield break;
                    }
                }
                if (mv as Skimmer != null)
                {
                    if (!ValidateRegistration(mv as Skimmer))
                    {
                        Logger.Error("Invalid Submersible Registration for the " + mv.gameObject.name + ". Next.");
                        RegistrySemaphore = false;
                        yield break;
                    }
                }
                yield return UWE.CoroutineHost.StartCoroutine(VehicleBuilder.Prefabricate(mv, registeredPingType));
                RegistrySemaphore = false;
                mv.gameObject.SetActive(false);

                Logger.Log("Registered the " + mv.gameObject.name + ".");
            }
            yield break;
        }

        public static bool ValidateRegistration(ModVehicle mv)
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
                Logger.Log("Validating the Registration of the " + mv.name);
                thisName = mv.name + ": ";
                if (mv.VehicleModel is null)
                {
                    Logger.Error(thisName + " A null ModVehicle.VehicleModel was passed for registration.");
                    return false;
                }
                if (mv.Recipe is null)
                {
                    Logger.Warn(thisName + " An empty recipe was passed for registration. The default recipe will be used.");
                }
                if (mv.PingSprite is null)
                {
                    Logger.Warn(thisName + " An empty ping sprite was passed for registration. The default ping sprite will be used.");
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
                    Logger.Warn(thisName + " No ModVehicle.InnateStorages were provided. These are lockers the vehicle always has.");
                }
                if (mv.ModularStorages is null || mv.ModularStorages.Count == 0)
                {
                    Logger.Log(thisName + " No ModVehicle.ModularStorages were provided. These are lockers that can be unlocked with upgrades.");
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
                    Logger.Log(thisName + " No ModVehicle.BackupBatteries were provided. This collection of batteries belong to the AI and will be used exclusively for life support, auto-leveling, and other AI tasks. The AI will use the main batteries instead.");
                }
                if (mv.HeadLights is null || mv.HeadLights.Count == 0)
                {
                    Logger.Warn(thisName + " No ModVehicle.HeadLights were provided. These lights would be activated when the player right clicks while piloting.");
                }
                if (mv.WaterClipProxies is null || mv.WaterClipProxies.Count == 0)
                {
                    Logger.Warn(thisName + " No ModVehicle.WaterClipProxies were provided. These are necessary to keep the ocean surface out of the vehicle.");
                }
                if (mv.CanopyWindows is null || mv.CanopyWindows.Count == 0)
                {
                    Logger.Warn(thisName + " No ModVehicle.CanopyWindows were provided. These must be specified to handle window transparencies.");
                }
                if (mv.BoundingBox is null)
                {
                    Logger.Error(thisName + " A null ModVehicle.BoundingBox was provided. This is necessary for the build bots to animate well.");
                    return false;
                }
                if (mv.CollisionModel is null)
                {
                    Logger.Warn(thisName + " A null ModVehicle.CollisionModel was provided. This is necessary for leviathans to grab the vehicle.");
                }
                foreach (VehicleParts.VehicleStorage vs in (mv.InnateStorages ?? Enumerable.Empty<VehicleParts.VehicleStorage>()).Concat(mv.ModularStorages ?? Enumerable.Empty<VehicleParts.VehicleStorage>()))
                {
                    if (vs.Container is null)
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
                    if (vu.Interface is null)
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
                        Logger.Log(thisName + " A null VehicleUpgrades.ModuleProxies was provided. VehicleFramework will not provide a model for this upgrade slot.");
                    }
                }
                foreach (VehicleParts.VehicleBattery vb in mv.Batteries.Concat(mv.BackupBatteries ?? Enumerable.Empty<VehicleParts.VehicleBattery>()))
                {
                    if (vb.BatterySlot is null)
                    {
                        Logger.Error(thisName + " A null VehicleBattery.BatterySlot was provided. There would be no way to access this battery.");
                        return false;
                    }
                    if (vb.BatteryProxy is null)
                    {
                        Logger.Log(thisName + " A null VehicleBattery.BatteryProxy was provided. VehicleFramework will not provide a model for this battery slot.");
                    }
                }
                foreach (VehicleParts.VehicleFloodLight vfl in (mv.HeadLights ?? Enumerable.Empty<VehicleParts.VehicleFloodLight>()))
                {
                    if (vfl.Light is null)
                    {
                        Logger.Error(thisName + " A null VehicleFloodLight.Light was provided. There would be nothing from which to emit light.");
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

                if (mv.StorageRootObject is null)
                {
                    Logger.Error(thisName + " A null ModVehicle.StorageRootObject was provided. There would be no way to store things in this vehicle.");
                    return false;
                }
                if (mv.ModulesRootObject is null)
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
                if (mv.Description is null)
                {
                    Logger.Error(thisName + " A null ModVehicle.GetDescription was provided. This is a brief description of the vehicle.");
                    return false;
                }
                if (mv.EncyclopediaEntry is null)
                {
                    Logger.Error(thisName + " A null ModVehicle.GetEncyEntry was provided. This is a possibly lengthy encyclopedia entry for the vehicle.");
                    return false;
                }
                if(mv.LeviathanGrabPoint is null)
                {
                    Logger.Warn(thisName + " A null ModVehicle.LeviathanGrabPoint was provided. This is where leviathans attach to the vehicle. The root object will be used instead.");
                }
            }
            catch (Exception e)
            {
                Logger.Error(thisName + " Exception Caught. Likely this ModVehicle is not implementing something it must. Check the abstract features of ModVehicle.\n\n" + e.ToString() + "\n\n");
                return false;
            }

            Logger.Log("The Registration of the " + mv.name + " as a ModVehicle has been Validated.");
            return true;
        }
        public static bool ValidateRegistration(Submarine mv)
        {
            if(!ValidateRegistration(mv as ModVehicle))
            {
                return false;
            }
            string thisName = "";
            try
            {
                thisName = mv.name + ": ";
                if (mv.Engine is null)
                {
                    Logger.Warn(thisName + " A null ModVehicle.ModVehicleEngine was passed for registration. The AtramaEngine will be used.");
                }
                if (mv.PilotSeats is null || mv.PilotSeats.Count == 0)
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
                    Logger.Warn(thisName + " No ModVehicle.FloodLights were provided. These lights would be activated on the control panel.");
                }
                if (mv.NavigationPortLights is null || mv.NavigationPortLights.Count == 0)
                {
                    Logger.Log(thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationStarboardLights is null || mv.NavigationStarboardLights.Count == 0)
                {
                    Logger.Log(thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationPositionLights is null || mv.NavigationPositionLights.Count == 0)
                {
                    Logger.Log(thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationWhiteStrobeLights is null || mv.NavigationWhiteStrobeLights.Count == 0)
                {
                    Logger.Log(thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationRedStrobeLights is null || mv.NavigationRedStrobeLights.Count == 0)
                {
                    Logger.Log(thisName + " Some navigation lights were missing.");
                }
                if (mv.TetherSources is null || mv.TetherSources.Count == 0)
                {
                    Logger.Error(thisName + " No ModVehicle.TetherSources were provided. These are necessary to keep the player 'grounded' within the vehicle.");
                    return false;
                }
                if (mv.ColorPicker is null)
                {
                    Logger.Log(thisName + " A null ModVehicle.ColorPicker was provided. You only need this if you implement the necessary painting functions.");
                }
                if (mv.Fabricator is null)
                {
                    Logger.Warn(thisName + " A null ModVehicle.Fabricator was provided. You know what this is.");
                }
                if (mv.ControlPanel is null)
                {
                    Logger.Warn(thisName + " A null ModVehicle.ControlPanel was provided. This is necessary to toggle floodlights.");
                }
                if (mv.SteeringWheelLeftHandTarget is null)
                {
                    Logger.Log(thisName + " A null ModVehicle.SteeringWheelLeftHandTarget was provided. This is what the player's left hand will 'grab' while you pilot.");
                }
                if (mv.SteeringWheelRightHandTarget is null)
                {
                    Logger.Log(thisName + " A null ModVehicle.SteeringWheelRightHandTarget was provided.  This is what the player's right hand will 'grab' while you pilot.");
                }
                foreach (VehicleParts.VehiclePilotSeat ps in mv.PilotSeats)
                {
                    if (ps.Seat is null)
                    {
                        Logger.Error(thisName + " A null PilotSeat.Seat was provided. There would be no way to pilot this vehicle.");
                        return false;
                    }
                    if (ps.SitLocation is null)
                    {
                        Logger.Error(thisName + " A null PilotSeat.SitLocation was provided. There would be no way to pilot this vehicle.");
                        return false;
                    }
                    if (ps.LeftHandLocation is null)
                    {
                        Logger.Log(thisName + " A null PilotSeat.LeftHandLocation was provided. (It's unused anyway)");
                    }
                    if (ps.RightHandLocation is null)
                    {
                        Logger.Log(thisName + " A null PilotSeat.RightHandLocation was provided. (It's unused anyway)");
                    }
                    if (ps.ExitLocation is null)
                    {
                        Logger.Warn(thisName + " A null PilotSeat.ExitLocation was provided. You might need this if you exit from piloting into a weird place.");
                    }
                }
                foreach (VehicleParts.VehicleHatchStruct vhs in mv.Hatches)
                {
                    if (vhs.Hatch is null)
                    {
                        Logger.Error(thisName + " A null VehicleHatchStruct.Hatch was provided. There would be no way to enter/exit this vehicle.");
                        return false;
                    }
                    if (vhs.EntryLocation is null)
                    {
                        Logger.Error(thisName + " A null VehicleHatchStruct.EntryLocation was provided. There would be no way to enter/exit this vehicle.");
                        return false;
                    }
                    if (vhs.ExitLocation is null)
                    {
                        Logger.Error(thisName + " A null VehicleHatchStruct.ExitLocation was provided. There would be no way to enter/exit this vehicle.");
                        return false;
                    }
                    if (vhs.SurfaceExitLocation is null)
                    {
                        Logger.Warn(thisName + " A null VehicleHatchStruct.SurfaceExitLocation was provided. You might need this if you exit weirdly near the surface.");
                    }
                }
                foreach (VehicleParts.VehicleStorage vs in mv.ModularStorages ?? Enumerable.Empty<VehicleParts.VehicleStorage>())
                {
                    if (vs.Container is null)
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
                    if (vfl.Light is null)
                    {
                        Logger.Error(thisName + " A null VehicleFloodLight.Light was provided. There would be nothing from which to emit light.");
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
                Logger.Error(thisName + " Exception Caught. Likely this ModVehicle is not implementing something it must. Check the abstract features of ModVehicle.\n\n" + e.ToString() + "\n\n");
                return false;
            }

            Logger.Log("The Registration of the " + mv.name + " as a Submarine has been Validated.");
            return true;
        }
        public static bool ValidateRegistration(Submersible mv)
        {
            if (!ValidateRegistration(mv as ModVehicle))
            {
                return false;
            }
            string thisName = "";
            try
            {
                thisName = mv.name + ": ";
                if (mv.Engine is null)
                {
                    Logger.Warn(thisName + " A null ModVehicle.ModVehicleEngine was passed for registration. The AtramaEngine will be used.");
                }
                if (mv.Hatches is null || mv.Hatches.Count == 0)
                {
                    Logger.Error(thisName + " No ModVehicle.Hatches were provided. These specify how the player will enter and exit the vehicle.");
                    return false;
                }
                if (mv.SteeringWheelLeftHandTarget is null)
                {
                    Logger.Log(thisName + " A null ModVehicle.SteeringWheelLeftHandTarget was provided. This is what the player's left hand will 'grab' while you pilot.");
                }
                if (mv.SteeringWheelRightHandTarget is null)
                {
                    Logger.Log(thisName + " A null ModVehicle.SteeringWheelRightHandTarget was provided.  This is what the player's right hand will 'grab' while you pilot.");
                }
                if (mv.PilotSeat.Seat is null)
                {
                    Logger.Error(thisName + " A null PilotSeat.Seat was provided. There would be no way to pilot this vehicle.");
                    return false;
                }
                if (mv.PilotSeat.SitLocation is null)
                {
                    Logger.Error(thisName + " A null PilotSeat.SitLocation was provided. There would be no way to pilot this vehicle.");
                    return false;
                }
                if (mv.PilotSeat.LeftHandLocation is null)
                {
                    Logger.Log(thisName + " A null PilotSeat.LeftHandLocation was provided. (It's unused anyway)");
                }
                if (mv.PilotSeat.RightHandLocation is null)
                {
                    Logger.Log(thisName + " A null PilotSeat.RightHandLocation was provided. (It's unused anyway)");
                }
                if (mv.PilotSeat.ExitLocation is null)
                {
                    Logger.Warn(thisName + " A null PilotSeat.ExitLocation was provided. You might need this if you exit from piloting into a weird place.");
                }
                foreach (VehicleParts.VehicleHatchStruct vhs in mv.Hatches)
                {
                    if (vhs.Hatch is null)
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
                        Logger.Warn(thisName + " A null VehicleHatchStruct.SurfaceExitLocation was provided. You might need this if you exit weirdly near the surface.");
                    }
                }
                foreach (VehicleParts.VehicleStorage vs in mv.ModularStorages ?? Enumerable.Empty<VehicleParts.VehicleStorage>())
                {
                    if (vs.Container is null)
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
                Logger.Error(thisName + " Exception Caught. Likely this ModVehicle is not implementing something it must. Check the abstract features of ModVehicle.\n\n" + e.ToString() + "\n\n");
                return false;
            }

            Logger.Log("The Registration of the " + mv.name + " as a Submersible has been Validated.");
            return true;
        }


        /*
        public static bool ValidateRegistration(ModVehicle mv, ModVehicleEngine engine, Dictionary<TechType, int> recipe, PingType pt, Atlas.Sprite sprite, int modules, int arms, int baseCrushDepth, int maxHealth, int mass)
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
                Logger.Log("Validating the Registration of the " + mv.name);
                thisName = mv.name + ": ";
                if (mv.VehicleModel is null)
                {
                    Logger.Error(thisName + " A null ModVehicle.VehicleModel was passed for registration.");
                    return false;
                }
                if (engine is null)
                {
                    Logger.Warn(thisName + " A null ModVehicle.ModVehicleEngine was passed for registration. The AtramaEngine will be used.");
                }
                if (recipe is null)
                {
                    Logger.Warn(thisName + " An empty recipe was passed for registration. The default recipe will be used.");
                }
                if (sprite is null)
                {
                    Logger.Warn(thisName + " An empty sprite was passed for registration. The default ping sprite will be used.");
                }
                if (baseCrushDepth < 0)
                {
                    Logger.Error(thisName + " A negative crush depth was passed for registration. This vehicle would take crush damage even out of water.");
                    return false;
                }
                if (maxHealth <= 0)
                {
                    Logger.Error(thisName + " A non-positive max health was passed for registration. This vehicle would be destroyed as soon as it awakens.");
                    return false;
                }
                if (mass <= 0)
                {
                    Logger.Error(thisName + " A non-positive mass was passed for registration. Don't do that.");
                    return false;
                }
                if (mv.PilotSeats is null || mv.PilotSeats.Count == 0)
                {
                    Logger.Error(thisName + " No ModVehicle.PilotSeats were provided. These specify what the player will click on to begin piloting the vehicle.");
                    return false;
                }
                if (mv.Hatches is null || mv.Hatches.Count == 0)
                {
                    Logger.Error(thisName + " No ModVehicle.Hatches were provided. These specify how the player will enter and exit the vehicle.");
                    return false;
                }
                if (mv.InnateStorages is null || mv.InnateStorages.Count == 0)
                {
                    Logger.Warn(thisName + " No ModVehicle.InnateStorages were provided. These are lockers the vehicle always has.");
                }
                if (mv.ModularStorages is null || mv.ModularStorages.Count == 0)
                {
                    Logger.Log(thisName + " No ModVehicle.ModularStorages were provided. These are lockers that can be unlocked with upgrades.");
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
                    Logger.Log(thisName + " No ModVehicle.BackupBatteries were provided. This collection of batteries belong to the AI and will be used exclusively for life support, auto-leveling, and other AI tasks. The AI will use the main batteries instead.");
                }
                if (mv.HeadLights is null || mv.HeadLights.Count == 0)
                {
                    Logger.Warn(thisName + " No ModVehicle.HeadLights were provided. These lights would be activated when the player right clicks while piloting.");
                }
                if (mv.FloodLights is null || mv.FloodLights.Count == 0)
                {
                    Logger.Warn(thisName + " No ModVehicle.FloodLights were provided. These lights would be activated on the control panel.");
                }
                if (mv.NavigationPortLights is null || mv.NavigationPortLights.Count == 0)
                {
                    Logger.Log(thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationStarboardLights is null || mv.NavigationStarboardLights.Count == 0)
                {
                    Logger.Log(thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationPositionLights is null || mv.NavigationPositionLights.Count == 0)
                {
                    Logger.Log(thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationWhiteStrobeLights is null || mv.NavigationWhiteStrobeLights.Count == 0)
                {
                    Logger.Log(thisName + " Some navigation lights were missing.");
                }
                if (mv.NavigationRedStrobeLights is null || mv.NavigationRedStrobeLights.Count == 0)
                {
                    Logger.Log(thisName + " Some navigation lights were missing.");
                }
                if (mv.WaterClipProxies is null || mv.WaterClipProxies.Count == 0)
                {
                    Logger.Warn(thisName + " No ModVehicle.WaterClipProxies were provided. These are necessary to keep the ocean surface out of the vehicle.");
                }
                if (mv.CanopyWindows is null || mv.CanopyWindows.Count == 0)
                {
                    Logger.Warn(thisName + " No ModVehicle.CanopyWindows were provided. These must be specified to handle window transparencies.");
                }
                if (mv.TetherSources is null || mv.TetherSources.Count == 0)
                {
                    Logger.Error(thisName + " No ModVehicle.TetherSources were provided. These are necessary to keep the player 'grounded' within the vehicle.");
                    return false;
                }
                if (mv.ColorPicker is null)
                {
                    Logger.Log(thisName + " A null ModVehicle.ColorPicker was provided. You only need this if you implement the necessary painting functions.");
                }
                if (mv.Fabricator is null)
                {
                    Logger.Warn(thisName + " A null ModVehicle.Fabricator was provided. You know what this is.");
                }
                if (mv.BoundingBox is null)
                {
                    Logger.Error(thisName + " A null ModVehicle.BoundingBox was provided. This is necessary for the build bots to animate well.");
                    return false;
                }
                if (mv.ControlPanel is null)
                {
                    Logger.Warn(thisName + " A null ModVehicle.ControlPanel was provided. This is necessary to toggle floodlights.");
                }
                if (mv.CollisionModel is null)
                {
                    Logger.Warn(thisName + " A null ModVehicle.CollisionModel was provided. This is necessary for leviathans to grab the vehicle.");
                }
                if (mv.SteeringWheelLeftHandTarget is null)
                {
                    Logger.Log(thisName + " A null ModVehicle.SteeringWheelLeftHandTarget was provided. This is what the player's left hand will 'grab' while you pilot.");
                }
                if (mv.SteeringWheelRightHandTarget is null)
                {
                    Logger.Log(thisName + " A null ModVehicle.SteeringWheelRightHandTarget was provided.  This is what the player's right hand will 'grab' while you pilot.");
                }
                foreach (VehicleParts.VehiclePilotSeat ps in mv.PilotSeats)
                {
                    if (ps.Seat is null)
                    {
                        Logger.Error(thisName + " A null PilotSeat.Seat was provided. There would be no way to pilot this vehicle.");
                        return false;
                    }
                    if (ps.SitLocation is null)
                    {
                        Logger.Error(thisName + " A null PilotSeat.SitLocation was provided. There would be no way to pilot this vehicle.");
                        return false;
                    }
                    if (ps.LeftHandLocation is null)
                    {
                        Logger.Log(thisName + " A null PilotSeat.LeftHandLocation was provided. (It's unused anyway)");
                    }
                    if (ps.RightHandLocation is null)
                    {
                        Logger.Log(thisName + " A null PilotSeat.RightHandLocation was provided. (It's unused anyway)");
                    }
                    if (ps.ExitLocation is null)
                    {
                        Logger.Warn(thisName + " A null PilotSeat.ExitLocation was provided. You might need this if you exit from piloting into a weird place.");
                    }
                }
                foreach (VehicleParts.VehicleHatchStruct vhs in mv.Hatches)
                {
                    if (vhs.Hatch is null)
                    {
                        Logger.Error(thisName + " A null VehicleHatchStruct.Hatch was provided. There would be no way to enter/exit this vehicle.");
                        return false;
                    }
                    if (vhs.EntryLocation is null)
                    {
                        Logger.Error(thisName + " A null VehicleHatchStruct.EntryLocation was provided. There would be no way to enter/exit this vehicle.");
                        return false;
                    }
                    if (vhs.ExitLocation is null)
                    {
                        Logger.Error(thisName + " A null VehicleHatchStruct.ExitLocation was provided. There would be no way to enter/exit this vehicle.");
                        return false;
                    }
                    if (vhs.SurfaceExitLocation is null)
                    {
                        Logger.Warn(thisName + " A null VehicleHatchStruct.SurfaceExitLocation was provided. You might need this if you exit weirdly near the surface.");
                    }
                }
                foreach (VehicleParts.VehicleStorage vs in (mv.InnateStorages ?? Enumerable.Empty<VehicleParts.VehicleStorage>()).Concat(mv.ModularStorages ?? Enumerable.Empty<VehicleParts.VehicleStorage>()))
                {
                    if (vs.Container is null)
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
                    if (vu.Interface is null)
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
                        Logger.Log(thisName + " A null VehicleUpgrades.ModuleProxies was provided. VehicleFramework will not provide a model for this upgrade slot.");
                    }
                }
                foreach (VehicleParts.VehicleBattery vb in mv.Batteries.Concat(mv.BackupBatteries ?? Enumerable.Empty<VehicleParts.VehicleBattery>()))
                {
                    if (vb.BatterySlot is null)
                    {
                        Logger.Error(thisName + " A null VehicleBattery.BatterySlot was provided. There would be no way to access this battery.");
                        return false;
                    }
                    if (vb.BatteryProxy is null)
                    {
                        Logger.Log(thisName + " A null VehicleBattery.BatteryProxy was provided. VehicleFramework will not provide a model for this battery slot.");
                    }
                }
                foreach (VehicleParts.VehicleFloodLight vfl in (mv.HeadLights ?? Enumerable.Empty<VehicleParts.VehicleFloodLight>()).Concat(mv.FloodLights ?? Enumerable.Empty<VehicleParts.VehicleFloodLight>()))
                {
                    if (vfl.Light is null)
                    {
                        Logger.Error(thisName + " A null VehicleFloodLight.Light was provided. There would be nothing from which to emit light.");
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

                if (mv.StorageRootObject is null)
                {
                    Logger.Error(thisName + " A null ModVehicle.StorageRootObject was provided. There would be no way to store things in this vehicle.");
                    return false;
                }
                if (mv.ModulesRootObject is null)
                {
                    Logger.Error(thisName + " A null ModVehicle.ModulesRootObject was provided. There would be no way to upgrade this vehicle.");
                    return false;
                }
                if (mv.GetDescription() is null)
                {
                    Logger.Error(thisName + " A null ModVehicle.GetDescription was provided. This is a brief description of the vehicle.");
                    return false;
                }
                if (mv.GetEncyEntry() is null)
                {
                    Logger.Error(thisName + " A null ModVehicle.GetEncyEntry was provided. This is a possibly lengthy encyclopedia entry for the vehicle.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.Error(thisName + " Exception Caught. Likely this ModVehicle is not implementing something it must. Check the abstract features of ModVehicle.\n\n" + e.ToString() + "\n\n");
                return false;
            }

            Logger.Log("The Registration of the " + mv.name + " has been Validated.");
            return true;
        }
         
         */

    }
}
