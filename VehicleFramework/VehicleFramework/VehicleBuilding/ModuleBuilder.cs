using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static VFXParticlesPool;

namespace VehicleFramework.VehicleBuilding
{
    public static class ModuleBuilder
    {
        internal static Dictionary<string, uGUI_EquipmentSlot> vehicleAllSlots = new();
        public const int MaxNumModules = 18;
        internal static bool areModulesReady = false;
        internal static bool slotExtenderIsPatched = false;
        internal static bool slotExtenderHasGreenLight = false;
        private static bool isPDAFirstOpenFixed = false;


        internal const string VFUpgradePrefix = "VehicleFrameworkUpgrade";
        internal const string ModVehicleModulePrefix = $"{VFUpgradePrefix}Module";
        internal const string LeftArmSlotName = $"{VFUpgradePrefix}LeftArm";
        internal const string RightArmSlotName = $"{VFUpgradePrefix}RightArm";

        private const string PresenceKey = $"{ModVehicleModulePrefix}0";

        private static uGUI_Equipment? Equipment => GameObject.Find("uGUI_PDAScreen(Clone)").transform.Find("Content/InventoryTab/Equipment")?.GetComponent<uGUI_Equipment>();

        private static GameObject? GenericModuleObject; // parent object of the regular module slot
        private static GameObject? ArmModuleObject; // parent object of the arm module slot
        private static GameObject? GenericModuleIconRect;
        private static GameObject? GenericModuleHint;

        private static GameObject? ModulesBackground; // background image parent object
        internal static Sprite? BackgroundSprite
        {
            set
            {
                Sprite? setSprite;
                if(value == null)
                {
                    setSprite = Assets.SpriteHelper.GetSprite("Sprites/VFModuleBackground.png");
                }
                else
                {
                    setSprite = value;
                }
                if(Equipment == null)
                {
                    throw Admin.SessionManager.Fatal("ModuleBuilder: BackgroundSprite set before equipment was initialized!");
                }
                Equipment.transform.Find($"{PresenceKey}/VehicleModuleBackground(Clone)").GetComponent<UnityEngine.UI.Image>().sprite = setSprite;
            }
        }

        private static Sprite? GenericModuleSlotSprite;
        private static Sprite? LeftArmModuleSlotSprite;
        private static Sprite? RightArmModuleSlotSprite;

        // These two materials might be the same
        private static Material? GenericModuleSlotMaterial;

        private static Transform? TopLeftSlot = null;
        private static Transform? BottomRightSlot = null;

        internal static void Reset()
        {
            vehicleAllSlots.Clear();
            areModulesReady = slotExtenderHasGreenLight = slotExtenderIsPatched = isPDAFirstOpenFixed = false;
        }
        internal static void SignalUpgradePDAOpened(VehicleUpgradeConsoleInput instance)
        {
            if(isPDAFirstOpenFixed)
            {
                return;
            }
            isPDAFirstOpenFixed = true;
            static IEnumerator SignalPDAOpened(VehicleUpgradeConsoleInput instance)
            {
                var pda = Player.main.GetPDA();
                yield return new WaitForSeconds(1);
                pda.Close();
                pda.isInUse = false;
                instance.OpenPDA();
            }
            Admin.SessionManager.StartCoroutine(SignalPDAOpened(instance));
        }
        internal static void BuildAllSlots()
        {
            if(vehicleAllSlots.Count == 0)
            {
                BuildAllSlotsInternal();
            }
        }
        private static void BuildAllSlotsInternal()
        {
            if(Equipment == null)
            {
                throw Admin.SessionManager.Fatal("ModuleBuilder: Equipment is null, cannot build all slots!");
            }
            BuildGenericModulesASAP();
            if (!vehicleAllSlots.ContainsKey(PresenceKey))
            {
                for (int i = 0; i < MaxNumModules; i++)
                {
                    vehicleAllSlots.Add($"{ModVehicleModulePrefix}{i}", Equipment.transform.Find($"{ModVehicleModulePrefix}{i}").GetComponent<uGUI_EquipmentSlot>());
                }
                vehicleAllSlots.Add(ModuleBuilder.LeftArmSlotName, Equipment.transform.Find(ModuleBuilder.LeftArmSlotName).GetComponent<uGUI_EquipmentSlot>());
                vehicleAllSlots.Add(ModuleBuilder.RightArmSlotName, Equipment.transform.Find(ModuleBuilder.RightArmSlotName).GetComponent<uGUI_EquipmentSlot>());
            }
            else
            {
                for (int i = 0; i < MaxNumModules; i++)
                {
                    vehicleAllSlots[$"{ModVehicleModulePrefix}{i}"] = Equipment.transform.Find($"{ModVehicleModulePrefix}{i}").GetComponent<uGUI_EquipmentSlot>();
                }
                vehicleAllSlots[ModuleBuilder.LeftArmSlotName] = Equipment.transform.Find(ModuleBuilder.LeftArmSlotName).GetComponent<uGUI_EquipmentSlot>();
                vehicleAllSlots[ModuleBuilder.RightArmSlotName] = Equipment.transform.Find(ModuleBuilder.RightArmSlotName).GetComponent<uGUI_EquipmentSlot>();
            }

            // Now that we've gotten the data we need,
            // we can let slot extender mangle it
            var type2 = Type.GetType("SlotExtender.Patches.uGUI_Equipment_Awake_Patch, SlotExtender", false, false);
            if (type2 != null)
            {
                ModuleBuilder.slotExtenderHasGreenLight = true;
                Equipment.Awake();
            }
        }
        private static void BuildGenericModulesASAP()
        {
            // this function is invoked by PDA.Awake,
            // so that we can access the same PDA here
            // Unfortunately this means we must wait for the player to open the PDA.
            // Maybe we can grab equipment from prefab?
            if(Equipment == null)
            {
                throw Admin.SessionManager.Fatal("ModuleBuilder: Equipment is null, cannot build generic modules!");
            }
            //equipment.Awake();
            foreach (uGUI_EquipmentSlot eSlot in Equipment.GetComponentsInChildren<uGUI_EquipmentSlot>(true))
            {
                switch (eSlot.slot)
                {
                    case "ExosuitModule1":
                        {
                            // get slot location
                            TopLeftSlot = eSlot.transform;

                            //===============================================================================
                            // get generic module components
                            //===============================================================================
                            GenericModuleObject = new("GenericVehicleModule");
                            GenericModuleObject.SetActive(false);
                            GenericModuleObject.transform.SetParent(Equipment.transform, false);

                            // set module position
                            GenericModuleObject.transform.localPosition = TopLeftSlot.localPosition;

                            // add background child gameobject and components
                            GameObject genericModuleBackground = new("Background");
                            genericModuleBackground.transform.SetParent(GenericModuleObject.transform, false);
                            VehicleBuilder.CopyComponent(TopLeftSlot.Find("Background").GetComponent<RectTransform>(), genericModuleBackground);
                            VehicleBuilder.CopyComponent(TopLeftSlot.Find("Background").GetComponent<CanvasRenderer>(), genericModuleBackground);
                            VehicleBuilder.CopyComponent(TopLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>(), genericModuleBackground);

                            // save these I guess?
                            GenericModuleSlotSprite = TopLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
                            GenericModuleSlotMaterial = TopLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

                            // configure slot background image
                            GenericModuleObject.EnsureComponent<uGUI_EquipmentSlot>().background = TopLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>();
                            GenericModuleObject.GetComponent<uGUI_EquipmentSlot>().background.sprite = TopLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite;
                            GenericModuleObject.GetComponent<uGUI_EquipmentSlot>().background.material = TopLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>().material;

                            // add iconrect child gameobject
                            GenericModuleIconRect = new("IconRect");
                            GenericModuleIconRect.transform.SetParent(GenericModuleObject.transform, false);
                            GenericModuleObject.GetComponent<uGUI_EquipmentSlot>().iconRect = VehicleBuilder.CopyComponent(TopLeftSlot.Find("IconRect").GetComponent<RectTransform>(), GenericModuleIconRect);

                            //===============================================================================
                            // get background image components
                            //===============================================================================
                            ModulesBackground = new("VehicleModuleBackground");
                            ModulesBackground.SetActive(false);
                            VehicleBuilder.CopyComponent(TopLeftSlot.Find("Exosuit").GetComponent<RectTransform>(), ModulesBackground);
                            VehicleBuilder.CopyComponent(TopLeftSlot.Find("Exosuit").GetComponent<CanvasRenderer>(), ModulesBackground);
                            VehicleBuilder.CopyComponent(TopLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>(), ModulesBackground);
                            //backgroundSprite = Assets.SpriteHelper.GetSpriteRaw("Sprites/VFModuleBackground.png");
                            //backgroundSprite = topLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>().sprite;
                            ModulesBackground.EnsureComponent<UnityEngine.UI.Image>().material = TopLeftSlot.Find("Exosuit").GetComponent<UnityEngine.UI.Image>().material;
                            // this can remain active, because its parent's Activity is controlled
                            ModulesBackground.SetActive(true);
                            break;
                        }
                    case "ExosuitModule4":
                        // get slot location
                        BottomRightSlot = eSlot.transform;
                        break;
                    case "ExosuitArmLeft":
                        {
                            // get slot location
                            ArmModuleObject = new("ArmVehicleModule");
                            ArmModuleObject.SetActive(false);
                            Transform arm = eSlot.transform;

                            // adjust the module transform
                            ArmModuleObject.transform.localPosition = arm.localPosition;

                            // add background child gameobject and components
                            GameObject genericModuleBackground = new("Background");
                            genericModuleBackground.transform.SetParent(ArmModuleObject.transform, false);

                            // configure background image
                            if(TopLeftSlot == null)
                            {
                                throw Admin.SessionManager.Fatal("ModuleBuilder: TopLeftSlot is null, cannot copy background components!");
                            }   
                            VehicleBuilder.CopyComponent(TopLeftSlot.Find("Background").GetComponent<RectTransform>(), genericModuleBackground);
                            VehicleBuilder.CopyComponent(TopLeftSlot.Find("Background").GetComponent<CanvasRenderer>(), genericModuleBackground);
                            VehicleBuilder.CopyComponent(TopLeftSlot.Find("Background").GetComponent<UnityEngine.UI.Image>(), genericModuleBackground);

                            // add iconrect child gameobject
                            GameObject thisModuleIconRect = new("IconRect");
                            thisModuleIconRect.transform.SetParent(ArmModuleObject.transform, false);
                            ArmModuleObject.EnsureComponent<uGUI_EquipmentSlot>().iconRect = VehicleBuilder.CopyComponent(TopLeftSlot.Find("IconRect").GetComponent<RectTransform>(), thisModuleIconRect);

                            // add 'hints' to show which arm is which (left vs right)
                            LeftArmModuleSlotSprite = arm.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite;
                            GenericModuleHint = new("Hint");
                            GenericModuleHint.transform.SetParent(ArmModuleObject.transform, false);
                            GenericModuleHint.transform.localScale = new(.75f, .75f, .75f);
                            GenericModuleHint.transform.localEulerAngles = new(0, 180, 0);
                            VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<RectTransform>(), GenericModuleHint);
                            VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<CanvasRenderer>(), GenericModuleHint);
                            VehicleBuilder.CopyComponent(arm.Find("Hint").GetComponent<UnityEngine.UI.Image>(), GenericModuleHint);
                            RightArmModuleSlotSprite = arm.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite;
                            break;
                        }
                    default:
                        break;
                }
            }
            BuildVehicleModuleSlots(MaxNumModules, true);
            areModulesReady = true;
        }
        private static void BuildVehicleModuleSlots(int modules, bool arms)
        {
            if(Equipment == null)
            {
                throw Admin.SessionManager.Fatal("ModuleBuilder: Equipment is null, cannot build vehicle module slots!");
            }   
            // build, link, and position modules
            for (int i=0; i<modules; i++)
            {
                GameObject thisModule = GetGenericModuleSlot();
                thisModule.name = $"{ModVehicleModulePrefix}{i}";
                thisModule.SetActive(false);
                thisModule.transform.SetParent(Equipment.transform, false);
                thisModule.transform.localScale = Vector3.one;
                thisModule.GetComponent<uGUI_EquipmentSlot>().slot = thisModule.name;
                thisModule.GetComponent<uGUI_EquipmentSlot>().manager = Equipment;

                LinkModule(ref thisModule);

                DistributeModule(ref thisModule, i);

                if (i == 0)
                {
                    AddBackgroundImage(ref thisModule, i);
                    thisModule.AddComponent<UpgradeSlotListener>().Privilege();
                }
                else
                {
                    thisModule.AddComponent<UpgradeSlotListener>();
                }
            }

            // build, link, and position left arm
            GameObject leftArm = GetLeftArmSlot();
            if (arms)
            {
                leftArm.name = ModuleBuilder.LeftArmSlotName;
                leftArm.SetActive(false);
                leftArm.transform.SetParent(Equipment.transform, false);
                leftArm.transform.localScale = new(-1, 1, 1); // need to flip this hand to look "left"
                leftArm.EnsureComponent<uGUI_EquipmentSlot>().slot = ModuleBuilder.LeftArmSlotName;
                leftArm.EnsureComponent<uGUI_EquipmentSlot>().manager = Equipment;
                LinkArm(ref leftArm);
                DistributeModule(ref leftArm, modules);
            }

            // build, link, and position right arm
            GameObject rightArm = GetRightArmSlot();
            if (arms)
            {
                rightArm.name = ModuleBuilder.RightArmSlotName;
                rightArm.SetActive(false);
                rightArm.transform.SetParent(Equipment.transform, false);
                rightArm.transform.localScale = Vector3.one;
                rightArm.EnsureComponent<uGUI_EquipmentSlot>().slot = ModuleBuilder.RightArmSlotName;
                rightArm.EnsureComponent<uGUI_EquipmentSlot>().manager = Equipment;
                LinkArm(ref rightArm);
                DistributeModule(ref rightArm, modules + 1);
            }
        }
        private static void LinkModule(ref GameObject thisModule)
        {
            // add background
            GameObject backgroundTop = thisModule.transform.Find("Background").gameObject;
            if (GenericModuleObject == null)
            {
                throw Admin.SessionManager.Fatal("ModuleBuilder: GenericModuleObject is null, cannot copy background components!");
            }
            VehicleBuilder.CopyComponent(GenericModuleObject.transform.Find("Background").GetComponent<RectTransform>(), backgroundTop);
            VehicleBuilder.CopyComponent(GenericModuleObject.transform.Find("Background").GetComponent<CanvasRenderer>(), backgroundTop);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background = VehicleBuilder.CopyComponent(GenericModuleObject.transform.Find("Background").GetComponent<UnityEngine.UI.Image>(), backgroundTop);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.sprite = GenericModuleSlotSprite;
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.material = GenericModuleSlotMaterial;
        }
        private static void DistributeModule(ref GameObject thisModule, int position)
        {
            int row_size = 4;
            int arrayX = position % row_size;
            int arrayY = position / row_size;

            if (TopLeftSlot == null)
            {
                throw Admin.SessionManager.Fatal("ModuleBuilder: TopLeftSlot is null, cannot copy background components!");
            }
            if (BottomRightSlot == null)
            {
                throw Admin.SessionManager.Fatal("ModuleBuilder: bottomRightSlot is null, cannot copy background components!");
            }
            float centerX = (TopLeftSlot.localPosition.x + BottomRightSlot.localPosition.x) / 2;
            float centerY = (TopLeftSlot.localPosition.y + BottomRightSlot.localPosition.y) / 2;

            float stepX = Mathf.Abs(TopLeftSlot.localPosition.x - centerX);
            float stepY = Mathf.Abs(TopLeftSlot.localPosition.y - centerY);

            Vector3 arrayOrigin = new(centerX - 2 * stepX, centerY - 2.5f * stepY, 0);

            float thisX = arrayOrigin.x + arrayX * stepX;
            float thisY = arrayOrigin.y + arrayY * stepY;

            thisModule.transform.localPosition = new(thisX, thisY, 0);
        }
        private static void AddBackgroundImage(ref GameObject parent, int index)
        {
            if(ModulesBackground == null)
            {
                throw Admin.SessionManager.Fatal("ModuleBuilder: ModulesBackground is null, cannot add background image!");
            }
            GameObject thisBackground = GameObject.Instantiate(ModulesBackground);
            thisBackground.transform.SetParent(parent.transform);
            thisBackground.transform.localRotation = Quaternion.identity;
            thisBackground.transform.localPosition = new(250,250,0);
            thisBackground.transform.localScale = 5 * Vector3.one;
            thisBackground.EnsureComponent<UnityEngine.UI.Image>().sprite = Assets.SpriteHelper.GetSprite("Sprites/VFModuleBackground.png");
        }
        private static void LinkArm(ref GameObject thisModule)
        {
            // add background
            GameObject backgroundTop = thisModule.transform.Find("Background").gameObject;
            if (GenericModuleObject == null)
            {
                throw Admin.SessionManager.Fatal("ModuleBuilder: GenericModuleObject is null, cannot copy background components!");
            }
            VehicleBuilder.CopyComponent(GenericModuleObject.transform.Find("Background").GetComponent<RectTransform>(), backgroundTop);
            VehicleBuilder.CopyComponent(GenericModuleObject.transform.Find("Background").GetComponent<CanvasRenderer>(), backgroundTop);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background = VehicleBuilder.CopyComponent(GenericModuleObject.transform.Find("Background").GetComponent<UnityEngine.UI.Image>(), backgroundTop);
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.sprite = GenericModuleSlotSprite;
            thisModule.GetComponent<uGUI_EquipmentSlot>().background.material = GenericModuleSlotMaterial;
        }
        private static GameObject GetGenericModuleSlot()
        {
            if (GenericModuleObject == null)
            {
                throw Admin.SessionManager.Fatal("ModuleBuilder: GenericModuleObject is null, cannot copy background components!");
            }
            return GameObject.Instantiate(GenericModuleObject);
        }
        private static GameObject GetLeftArmSlot()
        {
            if(ArmModuleObject == null)
            {
                throw Admin.SessionManager.Fatal("ModuleBuilder: ArmModuleObject is null, cannot copy background components!");
            }
            GameObject armSlot = GameObject.Instantiate(ArmModuleObject);
            armSlot.transform.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite = LeftArmModuleSlotSprite;
            return armSlot;
        }
        private static GameObject GetRightArmSlot()
        {
            if (ArmModuleObject == null)
            {
                throw Admin.SessionManager.Fatal("ModuleBuilder: ArmModuleObject is null, cannot copy background components!");
            }
            GameObject armSlot = GameObject.Instantiate(ArmModuleObject);
            armSlot.transform.Find("Hint").GetComponent<UnityEngine.UI.Image>().sprite = RightArmModuleSlotSprite;
            return armSlot;
        }
    }
}
