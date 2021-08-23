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
		public override string[] slotIDs
		{
			get
			{
				return AtramaVehicle._slotIDs;
			}
		}

		public override string vehicleDefaultName
		{
			get
			{
				Language main = Language.main;
				if (!(main != null))
				{
					return "ATRAMA";
				}
				return main.Get("AtramaDefaultName");
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

		public override void OnDockedChanged(bool docked, Vehicle.DockType dockType)
		{
			base.OnDockedChanged(docked, dockType);
			int i = 0;
			int num = this.storageInputs.Length;
			while (i < num)
			{
				AtramaStorageInput seamothStorageInput = this.storageInputs[i];
				if (seamothStorageInput != null)
				{
					seamothStorageInput.SetDocked(dockType);
				}
				i++;
			}
		}

		public override void EnterVehicle(Player player, bool teleport, bool playEnterAnimation = true)
		{
			if (player != null)
			{
				player.SetCurrentSub(null);
				player.playerController.UpdateController();
				player.EnterLockedMode(transform, false);
				player.sitting = true;
				player.currentMountedVehicle = this;
				player.playerController.ForceControllerSize();
				if (!string.IsNullOrEmpty(this.customGoalOnEnter))
				{
					GoalManager.main.OnCustomGoalEvent(this.customGoalOnEnter);
				}
				/*
				if (!GetComponent<EnergyInterface>().hasCharge)
				{
					if (this.noPowerWelcomeNotification)
					{
						this.noPowerWelcomeNotification.Play();
					}
				}
				else if (this.welcomeNotification)
				{
					this.welcomeNotification.Play();
				}
				*/
				this.pilotId = player.GetComponent<UniqueIdentifier>().Id;
				//this.mainAnimator.SetBool("enterAnimation", playEnterAnimation);
			}
		}

		public override void GetDepth(out int depth, out int crushDepth)
		{
			depth = 0;
			crushDepth = 0;
			depth = Mathf.FloorToInt(GetComponent<CrushDamage>().GetDepth());
			crushDepth = Mathf.FloorToInt(GetComponent<CrushDamage>().crushDepth);
		}

		public override void Awake()
		{
			Logger.Log("AtramaVehicle Awake!");
			base.Awake();
			base.modules.isAllowedToRemove = new IsAllowedToRemove(this.IsAllowedToRemove);
			updateModules();
			Logger.Log("End AtramaVehicle Awake!");
		}

		public override void Start()
		{
			Logger.Log("Atrama Start!");
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
			//TODO why does this fail?
			//this.toggleLights.lightsCallback += this.onLightsToggled;
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

		private void UpdateDockedAnim()
		{
			/*
			 // TODO
			this.animator.SetBool("docked", base.docked);
			*/
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
				this.toggleLights.CheckLightToggle();
			}
			this.UpdateScreenFX();
			this.UpdateDockedAnim();
		}

		public void GetHUDValues(out float health, out float power)
		{
			health = this.liveMixin.GetHealthFraction();
			float num;
			float num2;
			base.GetEnergyValues(out num, out num2);
			power = ((num > 0f && num2 > 0f) ? (num / num2) : 0f);
		}

		public void SubConstructionComplete()
		{
			this.lightsParent.SetActive(true);
		}

		public void onLightsToggled(bool active)
		{
			if (Application.isConsolePlatform)
			{
				if (active && this.volumeticLights.Length != 0)
				{
					PlatformUtils.SetLightbarColor(this.volumeticLights[0].color, 0);
					return;
				}
				PlatformUtils.ResetLightbarColor(0);
			}
		}

		private void UpdateScreenFX()
		{
			/*
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
			*/
		}

		private void UpdateSounds()
		{
			/*
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
			*/
		}

		public override bool CanPilot()
		{
			return !FPSInputModule.current.lockMovement && base.IsPowered();
		}

		public override void OnPilotModeBegin()
		{
			gameObject.GetComponentInParent<Atrama>().isPlayerPiloting = true;
			gameObject.GetComponentInParent<Atrama>().isPlayerInside = true;

			base.OnPilotModeBegin();
			UWE.Utils.EnterPhysicsSyncSection();
			//Player.main.inSeamoth = true;
			//this.bubbles.Play();
			this.ambienceSound.PlayUI();
			//TODO set these IK targets
			//Player.main.armsController.SetWorldIKTarget(this.leftHandPlug, this.rightHandPlug);

			/*
			Logger.Log("pmb8");

			Logger.Log("pmb10");
			this.onLightsToggled(this.toggleLights.GetLightsActive());
			Logger.Log("pmb11");
			*/
		}

		public override void OnPilotModeEnd()
		{
			base.OnPilotModeEnd();
			UWE.Utils.ExitPhysicsSyncSection();
			//Player.main.inSeamoth = false;
			//this.bubbles.Stop();
			this.ambienceSound.Stop(true);
			Player.main.armsController.SetWorldIKTarget(null, null);
			int num = this.volumeticLights.Length;
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					this.volumeticLights[i].RestoreVolume();
				}
			}
			this.onLightsToggled(false);

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


			/*
			Dictionary<TechType, float> depthDictionary = new Dictionary<TechType, float>
		{
			{
				TechType.AtramaHullModule1,
				600f
			},
			{
				TechType.AtramaHullModule2,
				1200f
			},
			{
				TechType.AtramaHullModule3,
				1800f
			}
		};
			float num = 0f;
			for (int i = 0; i < this.slotIDs.Length; i++)
			{
				string slot = this.slotIDs[i];
				TechType techTypeInSlot = base.modules.GetTechTypeInSlot(slot);
				if (depthDictionary.ContainsKey(techTypeInSlot))
				{
					float num2 = depthDictionary[techTypeInSlot];
					if (num2 > num)
					{
						num = num2;
					}
				}
			}
			this.crushDamage.SetExtraCrushDepth(num);
			*/
		}

		private void PlayTorpedoSound(int slotID, bool selected)
		{
			ItemsContainer storageInSlot = base.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
			TechType techType = TechType.WhirlpoolTorpedo;
			if (selected && storageInSlot.GetCount(techType) > 0)
			{
				global::Utils.PlayFMODAsset(this.torpedoArmed, base.transform, 1f);
				return;
			}
			global::Utils.PlayFMODAsset(this.torpedoDisarmed, base.transform, 1f);
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

		// Protobuf serializer stuff
		public override void OnProtoSerialize(ProtobufSerializer serializer)
		{
		}

		public override void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			Logger.Log("Deserialize Atrama Vehicle");
			base.OnProtoDeserialize(serializer);

			this.serializedModuleSlots = this.modules.SaveEquipment();
			foreach (KeyValuePair<string, string> pair in this.serializedModuleSlots)
			{
				Logger.Log(pair.Key + " : " + pair.Value);
			}

			/*
			this.LazyInitialize();
			Vector3[] vehicleDefaultColors = this.vehicleDefaultColors;
			if (this.vehicleName == null)
			{
				this.vehicleName = this.vehicleDefaultName;
			}
			if (this.vehicleColors == null)
			{
				this.vehicleColors = vehicleDefaultColors;
			}
			else if (this.vehicleColors.Length != vehicleDefaultColors.Length)
			{
				int num = Mathf.Min(this.vehicleColors.Length, vehicleDefaultColors.Length);
				for (int i = 0; i < num; i++)
				{
					vehicleDefaultColors[i] = this.vehicleColors[i];
				}
				this.vehicleColors = vehicleDefaultColors;
			}
			if (this.subName)
			{
				this.subName.DeserializeName(this.vehicleName);
				this.subName.DeserializeColors(this.vehicleColors);
			}
			//this.modules.Clear();
			if (this.serializedModules != null && this.serializedModuleSlots != null)
			{
				StorageHelper.RestoreEquipment(serializer, this.serializedModules, this.serializedModuleSlots, this.modules);
				this.serializedModules = null;
				this.serializedModuleSlots = null;
			}
			Transform parent = base.transform.parent;
			if (parent && parent.GetComponent<SubRoot>())
			{
				this.ReAttach(base.transform.position);
			}
			*/
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

		public ToggleLights toggleLights;

		[AssertNotNull]
		public VFXVolumetricLight[] volumeticLights;

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
	}
}
