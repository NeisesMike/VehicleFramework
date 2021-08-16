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
		// Token: 0x17000178 RID: 376
		// (get) Token: 0x06002150 RID: 8528 RVA: 0x000B7DEF File Offset: 0x000B5FEF
		public override string[] slotIDs
		{
			get
			{
				return AtramaVehicle._slotIDs;
			}
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x06002151 RID: 8529 RVA: 0x000B7DF8 File Offset: 0x000B5FF8
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

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x06002152 RID: 8530 RVA: 0x000B7E28 File Offset: 0x000B6028
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

		// Token: 0x06002153 RID: 8531 RVA: 0x000B7EC4 File Offset: 0x000B60C4
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

		// Token: 0x06002154 RID: 8532 RVA: 0x000B7F07 File Offset: 0x000B6107
		public override void Awake()
		{
			Logger.Log("AtramaVehicle Awake!");
			base.Awake();
			base.modules.isAllowedToRemove = new IsAllowedToRemove(this.IsAllowedToRemove);
			updateModules();
			Logger.Log("End AtramaVehicle Awake!");
		}

		// Token: 0x06002155 RID: 8533 RVA: 0x000B7F28 File Offset: 0x000B6128
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

		// Token: 0x06002156 RID: 8534 RVA: 0x000B8007 File Offset: 0x000B6207
		public override void SetPlayerInside(bool inside)
		{
			base.SetPlayerInside(inside);
			Player.main.inSeamoth = inside;
		}

		// Token: 0x06002157 RID: 8535 RVA: 0x000B801B File Offset: 0x000B621B
		public override bool GetAllowedToEject()
		{
			return !base.docked;
		}

		// Token: 0x06002158 RID: 8536 RVA: 0x000B8026 File Offset: 0x000B6226
		private void UpdateDockedAnim()
		{
			/*
			 // TODO
			this.animator.SetBool("docked", base.docked);
			*/
		}

		// Token: 0x06002159 RID: 8537 RVA: 0x000B8040 File Offset: 0x000B6240
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

		// Token: 0x0600215A RID: 8538 RVA: 0x000B8128 File Offset: 0x000B6328
		public void GetHUDValues(out float health, out float power)
		{
			health = this.liveMixin.GetHealthFraction();
			float num;
			float num2;
			base.GetEnergyValues(out num, out num2);
			power = ((num > 0f && num2 > 0f) ? (num / num2) : 0f);
		}

		// Token: 0x0600215B RID: 8539 RVA: 0x000B8168 File Offset: 0x000B6368
		public void SubConstructionComplete()
		{
			this.lightsParent.SetActive(true);
		}

		// Token: 0x0600215C RID: 8540 RVA: 0x000B8176 File Offset: 0x000B6376
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

		// Token: 0x0600215D RID: 8541 RVA: 0x000B81A8 File Offset: 0x000B63A8
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

		// Token: 0x0600215E RID: 8542 RVA: 0x000B82B4 File Offset: 0x000B64B4
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

		// Token: 0x0600215F RID: 8543 RVA: 0x0005698F File Offset: 0x00054B8F
		public override bool CanPilot()
		{
			return !FPSInputModule.current.lockMovement && base.IsPowered();
		}

		// Token: 0x06002160 RID: 8544 RVA: 0x000B83C0 File Offset: 0x000B65C0
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

		// Token: 0x06002161 RID: 8545 RVA: 0x000B8464 File Offset: 0x000B6664
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

		// Token: 0x06002162 RID: 8546 RVA: 0x000B84DC File Offset: 0x000B66DC
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
					Logger.Log(keyValuePair.Value.item.name);
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
			Logger.Log(slotID.ToString() + " : " + techType.ToString() + " : " + added.ToString());
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

		// Token: 0x06002164 RID: 8548 RVA: 0x000B869C File Offset: 0x000B689C
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

		// Token: 0x06002165 RID: 8549 RVA: 0x000B86F8 File Offset: 0x000B68F8
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

		// Token: 0x06002166 RID: 8550 RVA: 0x000B8870 File Offset: 0x000B6A70
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

		// Token: 0x06002167 RID: 8551 RVA: 0x000B8918 File Offset: 0x000B6B18
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

		// Token: 0x06002168 RID: 8552 RVA: 0x000B8985 File Offset: 0x000B6B85
		private void OnPlayerModeChanged(Player.Mode mode)
		{
			this.timeLastPlayerModeChange = Time.time;
		}

		// Token: 0x06002169 RID: 8553 RVA: 0x000B8994 File Offset: 0x000B6B94
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

		// Token: 0x0600216A RID: 8554 RVA: 0x000B89F4 File Offset: 0x000B6BF4
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

		// Token: 0x0600216B RID: 8555 RVA: 0x000B8A53 File Offset: 0x000B6C53
		private void UpdateLootSensorMetal()
		{
			this.CheckLootSensor(TechType.ScrapMetal);
		}

		// Token: 0x0600216C RID: 8556 RVA: 0x000B8A5C File Offset: 0x000B6C5C
		private void UpdateLootSensorLithium()
		{
			this.CheckLootSensor(TechType.Lithium);
		}

		// Token: 0x0600216D RID: 8557 RVA: 0x000B8A66 File Offset: 0x000B6C66
		private void UpdateLootSensorFragment()
		{
			this.CheckLootSensor(SeaMoth.fragmentTechTypes);
		}

		// Token: 0x0600216E RID: 8558 RVA: 0x000B8A73 File Offset: 0x000B6C73
		public void OnHoverTorpedoStorage(HandTargetEventData eventData)
		{
			if (base.modules.GetCount(TechType.SeamothTorpedoModule) > 0)
			{
				HandReticle.main.SetInteractText("SeamothTorpedoStorage");
				HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
			}
		}

		// Token: 0x0600216F RID: 8559 RVA: 0x000B8AA7 File Offset: 0x000B6CA7
		public void OnOpenTorpedoStorage(HandTargetEventData eventData)
		{
			this.OpenTorpedoStorage(eventData.transform);
		}

		// Token: 0x06002170 RID: 8560 RVA: 0x000B8AB8 File Offset: 0x000B6CB8
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

		// Token: 0x04001E6C RID: 7788
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

		// Token: 0x04001E6D RID: 7789
		[AssertNotNull]
		public GameObject[] torpedoSilos;

		// Token: 0x04001E6E RID: 7790
		public Transform torpedoTubeLeft;

		// Token: 0x04001E6F RID: 7791
		public Transform torpedoTubeRight;

		// Token: 0x04001E70 RID: 7792
		public FMOD_CustomEmitter sonarSound;

		// Token: 0x04001E71 RID: 7793
		public FMOD_CustomEmitter enterSeamoth;

		// Token: 0x04001E72 RID: 7794
		public GameObject seamothElectricalDefensePrefab;

		// Token: 0x04001E73 RID: 7795
		public FMOD_StudioEventEmitter pulseChargeSound;

		// Token: 0x04001E74 RID: 7796
		public FMOD_StudioEventEmitter ambienceSound;

		// Token: 0x04001E75 RID: 7797
		public EngineRpmSFXManager engineSound;

		// Token: 0x04001E76 RID: 7798
		public ParticleSystem bubbles;

		// Token: 0x04001E77 RID: 7799
		public string onSpawnGoalText = "";

		// Token: 0x04001E78 RID: 7800
		public GameObject screenEffectModel;

		// Token: 0x04001E79 RID: 7801
		public Gradient gradientInner;

		// Token: 0x04001E7A RID: 7802
		public Gradient gradientOuter;

		// Token: 0x04001E7B RID: 7803
		public FMODAsset torpedoArmed;

		// Token: 0x04001E7C RID: 7804
		public FMODAsset torpedoDisarmed;

		// Token: 0x04001E7D RID: 7805
		public Animator animator;

		// Token: 0x04001E7E RID: 7806
		public ToggleLights toggleLights;

		// Token: 0x04001E7F RID: 7807
		[AssertNotNull]
		public VFXVolumetricLight[] volumeticLights;

		// Token: 0x04001E80 RID: 7808
		public GameObject lightsParent;

		// Token: 0x04001E81 RID: 7809
		private bool lightsActive = true;

		// Token: 0x04001E82 RID: 7810
		private float timeLastPlayerModeChange;

		// Token: 0x04001E83 RID: 7811
		public AtramaStorageInput[] storageInputs;

		// Token: 0x04001E84 RID: 7812
		public float enginePowerConsumption = 0.06666667f;

		// Token: 0x04001E85 RID: 7813
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

		// Token: 0x04001E86 RID: 7814
		private float _smoothedMoveSpeed;

		// Token: 0x04001E87 RID: 7815
		private int fmodIndexSpeed = -1;

		// Token: 0x04001E88 RID: 7816
		private Material rendererMaterial0;

		// Token: 0x04001E89 RID: 7817
		private Material rendererMaterial1;
	}
}
