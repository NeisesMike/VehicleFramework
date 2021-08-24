using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using ProtoBuf;
using UWE;

namespace AtramaVehicle
{
	public class AtramaVehicle : Vehicle
    {
		/*
		public override string[] slotIDs
		{
			get
			{
				return AtramaVehicle._slotIDs;
			}
		}


		public override Vector3[] vehicleDefaultColors
		{
			get
			{
				return new Vector3[]
				{
				new Vector3(0f, 0f, 1f),
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, 0f, 1f),
				new Vector3(0.577f, 0.447f, 0.604f),
				new Vector3(0.114f, 0.729f, 0.965f)
				};
			}
		}



		public override void Awake()
		{
			base.Awake();
			base.modules.isAllowedToRemove = new IsAllowedToRemove(this.IsAllowedToRemove);
			updateModules();
		}

		public override void Start()
		{
			this.liveMixin = this.gameObject.GetComponent<LiveMixin>();
			base.Start();
			if (this.onSpawnGoalText != "")
			{
				GoalManager.main.OnCustomGoalEvent(this.onSpawnGoalText);
			}
			if (this.screenEffectModel != null)
			{
				this.screenEffectModel.GetComponent<Renderer>().materials[0].SetColor(ShaderPropertyID._Color, this.gradientInner.Evaluate(0f));
				this.screenEffectModel.GetComponent<Renderer>().materials[1].SetColor(ShaderPropertyID._Color, this.gradientOuter.Evaluate(0f));
			}
			this.animator = base.GetComponentInChildren<Animator>();
			global::Utils.GetLocalPlayerComp().playerModeChanged.AddHandler(base.gameObject, new Event<Player.Mode>.HandleFunction(this.OnPlayerModeChanged));

			//set upgrade panel name
			modules.SetLabel("AtramaUpgradeModules");
		}

		public override void SetPlayerInside(bool inside)
		{
			base.SetPlayerInside(inside);
			Player.main.inSeamoth = inside;
		}

		public override bool GetAllowedToEject()
		{
			return !base.docked;
		}

		public override void Update()
		{
			base.Update();
			this.UpdateSounds();
			if (base.GetPilotingMode())
			{
				string buttonFormat = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit);
				HandReticle.main.SetUseTextRaw(buttonFormat, string.Empty);
				Vector3 vector = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
				if (vector.magnitude > 0.1f)
				{
					base.ConsumeEngineEnergy(Time.deltaTime * this.enginePowerConsumption * vector.magnitude);
				}
			}
			this.UpdateScreenFX();
		}

		public void SubConstructionComplete()
		{
			this.lightsParent.SetActive(true);
		}

		private void UpdateScreenFX()
		{
			 // TODO
			if (base.GetPilotingMode())
			{
				float to = Mathf.Clamp(base.transform.InverseTransformDirection(this.useRigidbody.velocity).z / 10f, 0f, 1f);
				this._smoothedMoveSpeed = UWE.Utils.Slerp(this._smoothedMoveSpeed, to, Time.deltaTime);
			}
			else
			{
				this._smoothedMoveSpeed = 0f;
			}
			if (this.screenEffectModel != null)
			{
				if (this.rendererMaterial0 == null)
				{
					Renderer component = this.screenEffectModel.GetComponent<Renderer>();
					this.rendererMaterial0 = component.materials[0];
					this.rendererMaterial1 = component.materials[1];
				}
				Color value = this.gradientInner.Evaluate(this._smoothedMoveSpeed);
				Color value2 = this.gradientOuter.Evaluate(this._smoothedMoveSpeed);
				this.rendererMaterial0.SetColor(ShaderPropertyID._Color, value);
				this.rendererMaterial1.SetColor(ShaderPropertyID._Color, value2);
			}
			this.screenEffectModel.SetActive(this._smoothedMoveSpeed > 0f);
		}

		private void UpdateSounds()
		{
			 // TODO
			Vector3 vector = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
			if (this.CanPilot() && vector.magnitude > 0f && base.GetPilotingMode())
			{
				this.engineSound.AccelerateInput(1f);
				if (this.fmodIndexSpeed < 0)
				{
					this.fmodIndexSpeed = this.ambienceSound.GetParameterIndex("speed");
				}
				if (this.ambienceSound && this.ambienceSound.GetIsPlaying())
				{
					this.ambienceSound.SetParameterValue(this.fmodIndexSpeed, this.useRigidbody.velocity.magnitude);
				}
			}
			bool flag = false;
			int i = 0;
			int num = this.quickSlotCharge.Length;
			while (i < num)
			{
				if (this.quickSlotCharge[i] > 0f)
				{
					flag = true;
					break;
				}
				i++;
			}
			if (this.pulseChargeSound.GetIsStartingOrPlaying() != flag)
			{
				if (flag)
				{
					this.pulseChargeSound.StartEvent();
					return;
				}
				this.pulseChargeSound.Stop(true);
			}
		}



		public override void OnPilotModeBegin()
		{
			base.OnPilotModeBegin();
			UWE.Utils.EnterPhysicsSyncSection();
			//Player.main.inSeamoth = true;
			//this.bubbles.Play();
			this.ambienceSound.PlayUI();
			//TODO set these IK targets
			//Player.main.armsController.SetWorldIKTarget(this.leftHandPlug, this.rightHandPlug);
		}

		public override void OnPilotModeEnd()
		{
			base.OnPilotModeEnd();
			UWE.Utils.ExitPhysicsSyncSection();
			//Player.main.inSeamoth = false;
			//this.bubbles.Stop();
			this.ambienceSound.Stop(true);
			Player.main.armsController.SetWorldIKTarget(null, null);

			// Atrama stuff
			gameObject.GetComponentInParent<Atrama>().isPlayerPiloting = false;
			gameObject.GetComponentInParent<Atrama>().isPlayerInside = true;
		}




		private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
		{
			if (pickupable.GetTechType() == TechType.VehicleStorageModule)
			{
				SeamothStorageContainer component = pickupable.GetComponent<SeamothStorageContainer>();
				if (component != null)
				{
					bool flag = component.container.count == 0;
					if (verbose && !flag)
					{
						ErrorMessage.AddDebug(Language.main.Get("SeamothStorageNotEmpty"));
					}
					return flag;
				}
				Debug.LogError("No SeamothStorageContainer found on SeamothStorageModule item");
			}
			return true;
		}

		public void updateModules()
		{
			//UpdateModuleSlots();
			var equipment = upgradesInput.equipment.GetEquipment();
			while (equipment.MoveNext())
			{
				KeyValuePair<string, InventoryItem> keyValuePair = equipment.Current;
				if (keyValuePair.Value == null)
				{
					continue;
				}

				// handle storage modules
				if (keyValuePair.Key == "AtramaModule1")
				{
					storageInputs[0].gameObject.SetActive(keyValuePair.Value.item.name.Contains("Storage"));
				}
				else if (keyValuePair.Key == "AtramaModule2")
				{
					storageInputs[1].gameObject.SetActive(keyValuePair.Value.item.name.Contains("Storage"));
				}
				else if (keyValuePair.Key == "AtramaModule3")
				{
					storageInputs[2].gameObject.SetActive(keyValuePair.Value.item.name.Contains("Storage"));
				}
				else if (keyValuePair.Key == "AtramaModule4")
				{
					storageInputs[3].gameObject.SetActive(keyValuePair.Value.item.name.Contains("Storage"));
				}
				else if (keyValuePair.Key == "AtramaModule5")
				{
					storageInputs[4].gameObject.SetActive(keyValuePair.Value.item.name.Contains("Storage"));
				}
				else if (keyValuePair.Key == "AtramaModule6")
				{
					storageInputs[5].gameObject.SetActive(keyValuePair.Value.item.name.Contains("Storage"));
				}
			};
		}


		public override void OnUpgradeModuleChange(int slotID, TechType techType, bool added)
		{
			//Logger.Log(slotID.ToString() + " : " + techType.ToString() + " : " + added.ToString());
			//TODO why doesn't this disappear the chests?
			updateModules();
		}

		public override void OnUpgradeModuleUse(TechType techType, int slotID)
		{
			bool flag = true;
			float num = 0f;
			if (techType != TechType.SeamothElectricalDefense)
			{
				if (techType != TechType.SeamothTorpedoModule)
				{
					if (techType == TechType.SeamothSonarModule)
					{
						this.sonarSound.Stop();
						this.sonarSound.Play();
						SNCameraRoot.main.SonarPing();
						num = 5f;
					}
				}
				else
				{
					Transform muzzle = (slotID == base.GetSlotIndex("SeamothModule1") || slotID == base.GetSlotIndex("SeamothModule3")) ? this.torpedoTubeLeft : this.torpedoTubeRight;
					ItemsContainer storageInSlot = base.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
					TorpedoType torpedoType = null;
					for (int i = 0; i < this.torpedoTypes.Length; i++)
					{
						if (storageInSlot.Contains(this.torpedoTypes[i].techType))
						{
							torpedoType = this.torpedoTypes[i];
							break;
						}
					}
					flag = Vehicle.TorpedoShot(storageInSlot, torpedoType, muzzle);
					if (flag)
					{
						if (storageInSlot.count == 0)
						{
							global::Utils.PlayFMODAsset(this.torpedoDisarmed, base.transform, 1f);
						}
						num = 5f;
					}
					else
					{
						ErrorMessage.AddError(Language.main.Get("VehicleTorpedoNoAmmo"));
					}
				}
			}
			else
			{
				float charge = this.quickSlotCharge[slotID];
				float slotCharge = this.GetSlotCharge(slotID);
				ElectricalDefense component = global::Utils.SpawnZeroedAt(this.seamothElectricalDefensePrefab, base.transform, false).GetComponent<ElectricalDefense>();
				component.charge = charge;
				component.chargeScalar = slotCharge;
				num = 5f;
			}
			if (flag)
			{
				this.quickSlotTimeUsed[slotID] = Time.time;
				this.quickSlotCooldown[slotID] = num;
			}
		}

		public override void OnUpgradeModuleToggle(int slotID, bool active)
		{
			switch (base.modules.GetTechTypeInSlot(this.slotIDs[slotID]))
			{
				case TechType.LootSensorMetal:
					if (active)
					{
						base.InvokeRepeating("UpdateLootSensorMetal", 1f, 1f);
						return;
					}
					base.CancelInvoke("UpdateLootSensorMetal");
					return;
				case TechType.LootSensorLithium:
					if (active)
					{
						base.InvokeRepeating("UpdateLootSensorLithium", 1f, 1f);
						return;
					}
					base.CancelInvoke("UpdateLootSensorLithium");
					return;
				case TechType.LootSensorFragment:
					if (active)
					{
						base.InvokeRepeating("UpdateLootSensorFragment", 1f, 1f);
						return;
					}
					base.CancelInvoke("UpdateLootSensorFragment");
					return;
				default:
					return;
			}
		}

		private void UpdateSolarRecharge()
		{
			DayNightCycle main = DayNightCycle.main;
			if (main == null)
			{
				return;
			}
			int count = base.modules.GetCount(TechType.SeamothSolarCharge);
			float num = Mathf.Clamp01((200f + base.transform.position.y) / 200f);
			float localLightScalar = main.GetLocalLightScalar();
			float amount = 1f * localLightScalar * num * (float)count;
			base.AddEnergy(amount);
		}

		private void OnPlayerModeChanged(Player.Mode mode)
		{
			this.timeLastPlayerModeChange = Time.time;
		}

		private void CheckLootSensor(TechType scanType)
		{
			bool flag = LootSensor.IsLootDetected(scanType, base.transform.position, 300);
			if (flag)
			{
				ErrorMessage.AddMessage("LootSensor detected " + scanType.AsString(false));
			}
			if (flag)
			{
				FMODUWE.PlayOneShot("event:/interface/on_long", base.gameObject.transform.position, 0.5f);
			}
		}

		private void CheckLootSensor(TechType[] scanTypes)
		{
			TechType techType;
			bool flag = LootSensor.IsLootDetected(scanTypes, base.transform.position, 300, out techType);
			if (flag)
			{
				ErrorMessage.AddMessage("LootSensor detected " + techType.AsString(false));
			}
			if (flag)
			{
				FMODUWE.PlayOneShot("event:/interface/on_long", base.gameObject.transform.position, 0.5f);
			}
		}

		private void UpdateLootSensorMetal()
		{
			this.CheckLootSensor(TechType.ScrapMetal);
		}

		private void UpdateLootSensorLithium()
		{
			this.CheckLootSensor(TechType.Lithium);
		}

		private void UpdateLootSensorFragment()
		{
			this.CheckLootSensor(SeaMoth.fragmentTechTypes);
		}
		public void OnHoverTorpedoStorage(HandTargetEventData eventData)
		{
			if (base.modules.GetCount(TechType.SeamothTorpedoModule) > 0)
			{
				HandReticle.main.SetInteractText("SeamothTorpedoStorage");
				HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
			}
		}

		public void OnOpenTorpedoStorage(HandTargetEventData eventData)
		{
			this.OpenTorpedoStorage(eventData.transform);
		}

		public void OpenTorpedoStorage(Transform useTransform)
		{
			if (base.modules.GetCount(TechType.SeamothTorpedoModule) > 0)
			{
				Inventory.main.ClearUsedStorage();
				int num = this.slotIDs.Length;
				for (int i = 0; i < num; i++)
				{
					ItemsContainer storageInSlot = base.GetStorageInSlot(i, TechType.SeamothTorpedoModule);
					Inventory.main.SetUsedStorage(storageInSlot, true);
				}
				Player.main.GetPDA().Open(PDATab.Inventory, useTransform, null, -1f);
			}
		}

		private static readonly string[] _slotIDs = new string[]
		{
			"AtramaModule1",
			"AtramaModule2",
			"AtramaModule3",
			"AtramaModule4",
			"AtramaModule5",
			"AtramaModule6",
			"AtramaArmLeft",
			"AtramaArmRight"
		};

		[AssertNotNull]
		public GameObject[] torpedoSilos;

		public Transform torpedoTubeLeft;

		public Transform torpedoTubeRight;

		public FMOD_CustomEmitter sonarSound;

		public FMOD_CustomEmitter enterSeamoth;

		public GameObject seamothElectricalDefensePrefab;

		public FMOD_StudioEventEmitter pulseChargeSound;

		public FMOD_StudioEventEmitter ambienceSound;

		public EngineRpmSFXManager engineSound;

		public ParticleSystem bubbles;

		public string onSpawnGoalText = "";

		public GameObject screenEffectModel;

		public Gradient gradientInner;

		public Gradient gradientOuter;

		public FMODAsset torpedoArmed;

		public FMODAsset torpedoDisarmed;

		public Animator animator;

		public GameObject lightsParent;

		private bool lightsActive = true;

		private float timeLastPlayerModeChange;

		public AtramaStorageInput[] storageInputs;
		
		public float enginePowerConsumption = 0.06666667f;

		private static readonly TechType[] fragmentTechTypes = new TechType[]
		{
		TechType.SeamothFragment,
		TechType.StasisRifleFragment,
		TechType.ExosuitFragment,
		TechType.TransfuserFragment,
		TechType.TerraformerFragment,
		TechType.ReinforceHullFragment,
		TechType.WorkbenchFragment,
		TechType.PropulsionCannonFragment,
		TechType.BioreactorFragment,
		TechType.ThermalPlantFragment,
		TechType.NuclearReactorFragment,
		TechType.MoonpoolFragment,
		TechType.BaseFiltrationMachineFragment,
		TechType.BaseBioReactorFragment,
		TechType.BaseNuclearReactorFragment,
		TechType.ExosuitDrillArmFragment,
		TechType.ExosuitGrapplingArmFragment,
		TechType.ExosuitPropulsionArmFragment,
		TechType.ExosuitTorpedoArmFragment
		};

		private float _smoothedMoveSpeed;

		private int fmodIndexSpeed = -1;

		private Material rendererMaterial0;

		private Material rendererMaterial1;
	*/
	}
}
