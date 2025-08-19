using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework.VehicleComponents
{
    public struct MaterialReactorData
    {
        public TechType inputTechType;
        public float totalEnergy;
        public float energyPerSecond;
        public TechType outputTechType;
    }
    internal class ReactorBattery : MonoBehaviour, IBattery
    {
        private float totalCapacity;
        private float currentCharge;
        float IBattery.charge
        {
            get
            {
                return currentCharge;
            }
            set
            {
                currentCharge = value;
            }
        }
        float IBattery.capacity => totalCapacity;
        string IBattery.GetChargeValueText()
        {
            float arg = currentCharge / totalCapacity;
            return Language.main.GetFormat<float, int, float>("BatteryCharge", arg, Mathf.RoundToInt(currentCharge), totalCapacity);
        }
        internal void SetCapacity(float capacity)
        {
            totalCapacity = capacity;
        }
        internal float GetCharge()
        {
            return currentCharge;
        }
        internal float SetCharge(float iCharge)
        {
            return currentCharge = iCharge;
        }
    }
    public class MaterialReactor : HandTarget, IHandTarget, IProtoTreeEventListener
    {
        private ModVehicle mv = null!;
        private ReactorBattery reactorBattery = null!;
        private float capacity = 0;
        public string interactText = "Material Reactor";
        public string cannotRemoveMessage = "Can't remove items being consumed by a Material Reactor";
        public bool canViewWhitelist = true;
        public bool listPotentials = true;
        public Action<PDA>? OnClosePDAAction = null;
        public Action<int>? UpdateVisuals = null;
        private readonly Dictionary<TechType, float> maxEnergies = new();
        private readonly Dictionary<TechType, float> rateEnergies = new();
        private readonly Dictionary<InventoryItem, float> currentEnergies = new();
        private readonly Dictionary<TechType, TechType> spentMaterialIndex = new();
        private ItemsContainer container = null!;
        private Coroutine? OutputReactorDataCoroutine = null;
        private bool isInitialized = false;
        private const string saveFileName = "MaterialReactor";
        private const string newSaveFileName = "Reactor";

        public void Initialize(ModVehicle modVehicle, int height, int width, string label, float totalCapacity, List<MaterialReactorData> iMaterialData)
        {
            if (modVehicle.GetComponentsInChildren<MaterialReactor>().Where(x => x.mv == modVehicle).Any())
            {
                ErrorMessage.AddWarning($"A ModVehicle may (for now) only have one material reactor!");
                return;
            }
            if (modVehicle == null)
            {
                ErrorMessage.AddWarning($"Material Reactor {label} must be given a non-null ModVehicle.");
                return;
            }
            if (height < 0 || width < 0 || height * width == 0)
            {
                ErrorMessage.AddWarning($"Material Reactor {label} cannot have non-positive size.");
                return;
            }
            if (totalCapacity < 0)
            {
                ErrorMessage.AddWarning($"Material Reactor {label} cannot have negative capacity.");
                return;
            }
            if (iMaterialData.Count == 0)
            {
                ErrorMessage.AddWarning($"Material Reactor {label} must be able to accept some material.");
                return;
            }
            foreach (var datum in iMaterialData)
            {
                if (datum.totalEnergy <= 0)
                {
                    ErrorMessage.AddWarning($"Material Reactor {label} cannot process a material that has non-positive potential: {datum.inputTechType.AsString()}");
                    return;
                }
                if (datum.energyPerSecond <= 0)
                {
                    ErrorMessage.AddWarning($"Material Reactor {label} cannot process a material that provides non-positive energy: {datum.inputTechType.AsString()}");
                    return;
                }
            }
            mv = modVehicle;
            if (mv == null)
            {
                throw Admin.SessionManager.Fatal($"Material Reactor {label} could not be initialized! mv is null.");
            }
            capacity = totalCapacity;
            container = new(width, height, transform, label, null);
            container.onAddItem += OnAddItem;
            container.isAllowedToRemove = IsAllowedToRemove;
            container.SetAllowedTechTypes(iMaterialData.SelectMany(x => new List<TechType>() { x.inputTechType, x.outputTechType }).ToArray());
            if (container == null)
            {
                throw Admin.SessionManager.Fatal($"Material Reactor {label} could not be initialized! ItemsContainer is null.");
            }
            foreach (var reagent in iMaterialData)
            {
                maxEnergies.Add(reagent.inputTechType, reagent.totalEnergy);
                rateEnergies.Add(reagent.inputTechType, reagent.energyPerSecond);
                spentMaterialIndex.Add(reagent.inputTechType, reagent.outputTechType);
            }
            gameObject.SetActive(false);
            EnergyMixin eMix = gameObject.AddComponent<EnergyMixin>();
            eMix.batterySlot = new(transform);
            gameObject.SetActive(true);
            GameObject batteryObj = new("ReactorBattery");
            batteryObj.transform.SetParent(transform);
            reactorBattery = batteryObj.AddComponent<ReactorBattery>();
            reactorBattery.SetCapacity(capacity);
            eMix.battery = reactorBattery;
            if (reactorBattery == null)
            {
                throw Admin.SessionManager.Fatal($"Material Reactor {label} could not be initialized! reactorBattery is null.");
            }
            mv.energyInterface.sources = mv.energyInterface.sources.Append(eMix).ToArray();
            gameObject.AddComponent<ChildObjectIdentifier>();
            isInitialized = true;
        }
        private void Update()
        {
            if (!isInitialized)
            {
                string errorMsg = $"Material Reactor must be manually initialized. Destroying.";
                Logger.Error(errorMsg);
                ErrorMessage.AddWarning(errorMsg);
                Component.DestroyImmediate(this);
                return;
            }
            var reactants = currentEnergies.Keys.ToList();
            foreach (var reactant in reactants)
            {
                float rate = rateEnergies[reactant.techType] * Time.deltaTime;
                float consumed = mv.energyInterface.AddEnergy(rate);
                currentEnergies[reactant] -= consumed;
            }
            List<InventoryItem> spentMaterials = new();
            foreach (var reactantPair in currentEnergies.ToList().Where(x => x.Value <= 0))
            {
                spentMaterials.Add(reactantPair.Key);
                if (container._items.TryGetValue(reactantPair.Key.techType, out ItemsContainer.ItemGroup itemGroup))
                {
                    List<InventoryItem> items = itemGroup.items;
                    if (items.Remove(reactantPair.Key))
                    {
                        if (items.Count == 0)
                        {
                            container._items.Remove(reactantPair.Key.techType);
                            container.NotifyRemoveItem(reactantPair.Key);
                            TechType toAdd = spentMaterialIndex[reactantPair.Key.techType];
                            if(toAdd != TechType.None)
                            {
                                Admin.SessionManager.StartCoroutine(AddMaterial(toAdd));
                            }
                        }
                        reactantPair.Key.container = null;
                        GameObject.DestroyImmediate(reactantPair.Key.item.gameObject);
                    }
                }
            }
            spentMaterials.ForEach(x => currentEnergies.Remove(x));
        }
        private IEnumerator AddMaterial(TechType toAdd)
        {
            TaskResult<GameObject> result = new();
            yield return CraftData.InstantiateFromPrefabAsync(toAdd, result, false);
            GameObject spentMaterial = result.Get();
            spentMaterial.SetActive(false);
            try
            {
                container.AddItem(spentMaterial.GetComponent<Pickupable>());
            }
            catch(Exception e)
            {
                string errorMessage = $"Could not add material {toAdd.AsString()} to the material reactor!";
                Logger.LogException(errorMessage, e, true);
            }
        }
        private void OnAddItem(InventoryItem item)
        {
            if (maxEnergies.Keys.ToList().Contains(item.techType))
            {
                currentEnergies.Add(item, maxEnergies[item.techType]);
            }
            UpdateVisuals?.Invoke(container.count);
            // check if it can rot, and disable all that
            Eatable eatable = item.item.gameObject.GetComponent<Eatable>();
            if(eatable != null)
            {
                eatable.decomposes = false;
            }
        }
        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            if (spentMaterialIndex.Values.ToList().Contains(pickupable.inventoryItem.techType))
            {
                return true;
            }
            if (verbose)
            {
                ErrorMessage.AddMessage(cannotRemoveMessage);
            }
            return false;
        }
        void IHandTarget.OnHandClick(GUIHand hand)
        {
            if (container != null)
            {
                PDA pda = Player.main.GetPDA();
                Inventory.main.SetUsedStorage(container, false);
                if (!pda.Open(PDATab.Inventory, transform, new PDA.OnClose(OnClosePDA)))
                {
                    OnClosePDA(pda);
                }
            }
        }
        void IHandTarget.OnHandHover(GUIHand hand)
        {
            if (container != null)
            {
                HandReticle main = HandReticle.main;
                string chargeValue = capacity == 0 ? string.Empty : $"({Language.main.Get("VFMaterialReactorHint4")}: {(int)reactorBattery.GetCharge()}/{capacity})";
                string finalText = $"{interactText} {chargeValue}";
                main.SetText(HandReticle.TextType.Hand, finalText, true, GameInput.Button.LeftHand);
                if (canViewWhitelist)
                {
                    main.SetText(HandReticle.TextType.HandSubscript, Language.main.Get("VFMaterialReactorHint3"), false, GameInput.Button.RightHand);
                    if (GameInput.GetButtonDown(GameInput.Button.RightHand) && OutputReactorDataCoroutine == null)
                    {
                        OutputReactorDataCoroutine = Admin.SessionManager.StartCoroutine(OutputReactorData());
                    }
                }
                else
                {
                    main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false, GameInput.Button.None);
                }
                main.SetIcon(HandReticle.IconType.Hand, 1f);
            }
        }
        private IEnumerator OutputReactorData()
        {
            if (listPotentials)
            {
                Logger.PDANote(Language.main.Get("VFMaterialReactorHint2"), 4f);
            }
            else
            {
                Logger.PDANote(Language.main.Get("VFMaterialReactorHint1"), 4f);
            }
            foreach (var pair in maxEnergies)
            {
                if(5 < Vector3.Distance(Player.main.transform.position, transform.position))
                {
                    break;
                }
                yield return new WaitForSeconds(0.75f);
                if (listPotentials)
                {
                    Logger.PDANote($"{pair.Key.AsString()} : {pair.Value}", 4f);
                }
                else
                {
                    Logger.PDANote(pair.Key.AsString(), 4f);
                }
            }
            OutputReactorDataCoroutine = null;
        }
        private void OnClosePDA(PDA pda)
        {
            OnClosePDAAction?.Invoke(pda);
        }
        public float GetFuelPotential()
        {
            return currentEnergies.Values.ToList().Sum();
        }
        public static List<MaterialReactorData> GetBioReactorData()
        {
            return BaseBioReactor.charge.Select(x =>
                new MaterialReactorData {
                    inputTechType = x.Key,
                    totalEnergy = x.Value,
                    energyPerSecond = 1f,
                    outputTechType = TechType.None
                }
            ).ToList();
        }
        public static List<MaterialReactorData> GetNuclearReactorData()
        {
            return new()
            {
                new MaterialReactorData
                {
                    inputTechType = TechType.ReactorRod,
                    totalEnergy = 20000f,
                    energyPerSecond = 5f,
                    outputTechType = TechType.DepletedReactorRod
                }
            };
        }

        void IProtoTreeEventListener.OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            SaveLoad.JsonInterface.Write<List<Tuple<TechType, float>>>(mv, newSaveFileName, GetSaveDict());
        }
        void IProtoTreeEventListener.OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            List<Tuple<TechType, float>>? saveDict = SaveLoad.JsonInterface.Read<List<Tuple<TechType, float>>>(mv, newSaveFileName);
            if(saveDict == default)
            {
                saveDict = SaveLoad.JsonInterface.Read<List<Tuple<TechType, float>>>(mv, saveFileName);
            }
            if(saveDict != null)
            {
                Admin.SessionManager.StartCoroutine(LoadSaveDict(saveDict));
            }
        }
        private List<Tuple<TechType, float>> GetSaveDict()
        {
            List<Tuple<TechType, float>> result = new()
            {
                new(TechType.None, reactorBattery.GetCharge())
            };
            foreach (var reactant in currentEnergies)
            {
                result.Add(new(reactant.Key.techType, reactant.Value));
            }
            foreach(var item in container)
            {
                if (currentEnergies.ContainsKey(item))
                {
                    continue;
                }
                result.Add(new(item.techType, 0));
            }
            return result;
        }
        private IEnumerator LoadSaveDict(List<Tuple<TechType, float>> saveDict)
        {
            foreach(var reactant in saveDict)
            {
                if(reactant.Item1 == TechType.None)
                {
                    reactorBattery.SetCharge(reactant.Item2);
                    continue;
                }
                yield return Admin.SessionManager.StartCoroutine(AddMaterial(reactant.Item1));
            }
            Dictionary<InventoryItem, float> changesPending = new();
            foreach (var reactant in currentEnergies)
            {
                Tuple<TechType, float>? selectedReactant = default;
                foreach (var savedReactant in saveDict)
                {
                    if (reactant.Key.techType == savedReactant.Item1)
                    {
                        selectedReactant = savedReactant;
                        changesPending.Add(reactant.Key, savedReactant.Item2);
                        break;
                    }
                }
                if(selectedReactant == null)
                {
                    continue;
                }
                saveDict.Remove(selectedReactant);
            }
            foreach (var reactantToLoad in changesPending)
            {
                currentEnergies[reactantToLoad.Key] = reactantToLoad.Value;
            }
        }
    }
}
