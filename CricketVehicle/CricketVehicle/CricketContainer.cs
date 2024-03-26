using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework;
using System.IO;
using System.Reflection;

using UnityEngine.U2D;
using VehicleFramework.VehicleParts;
using VehicleFramework.VehicleTypes;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;

namespace CricketVehicle
{
	public class CricketStorageInput : HandTarget, IHandTarget
	{
		public int slotID = -1;
		public GameObject model;
		public Collider collider;
		public float timeOpen = 0.5f;
		public float timeClose = 0.25f;
		public FMODAsset openSound;
		public FMODAsset closeSound;
		protected Transform tr;
		protected Vehicle.DockType dockType;
		protected bool state;
		ItemsContainer myContainer;

		public void OpenFromExternal()
		{
			PDA pda = Player.main.GetPDA();
			Inventory.main.SetUsedStorage(myContainer, false);
			pda.Open(PDATab.Inventory, null, null);
		}

		protected void OpenPDA()
		{
			PDA pda = Player.main.GetPDA();
			Inventory.main.SetUsedStorage(myContainer, false);
			if (!pda.Open(PDATab.Inventory, this.tr, new PDA.OnClose(this.OnClosePDA)))
			{
				this.OnClosePDA(pda);
				return;
			}
		}

		public override void Awake()
		{
			base.Awake();
			this.tr = GetComponent<Transform>();
			this.UpdateColliderState();

			// go up in the transform heirarchy until we find
			SetEnabled(true);
		}
		public void Start()
		{
			myContainer = GetComponent<InnateStorageContainer>().container;
		}
		protected void OnDisable()
		{

		}
		protected void ChangeFlapState()
		{
			//Utils.PlayFMODAsset(open ? this.openSound : this.closeSound, base.transform, 1f);
			Utils.PlayFMODAsset(this.openSound, base.transform, 1f);
			OpenPDA();
		}
		protected void OnClosePDA(PDA pda)
		{
			seq.Set(0, false, null);
			Utils.PlayFMODAsset(this.closeSound, base.transform, 1f);
		}
		protected void UpdateColliderState()
		{
			if (this.collider != null)
			{
				this.collider.enabled = (this.state && this.dockType != Vehicle.DockType.Cyclops);
			}
		}
		public void SetEnabled(bool state)
		{
			if (this.state == state)
			{
				return;
			}
			this.state = state;
			this.UpdateColliderState();
			if (this.model != null)
			{
				this.model.SetActive(state);
			}
		}
		public void SetDocked(Vehicle.DockType dockType)
		{
			this.dockType = dockType;
			this.UpdateColliderState();
		}
		public void OnHandHover(GUIHand hand)
		{
			HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.OpenStorage));
			HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
		}
		public Sequence seq = new Sequence();
		public void Update()
		{
			seq.Update();
		}
		public void OnHandClick(GUIHand hand)
		{
			seq.Set(0, true, new SequenceCallback(ChangeFlapState));
		}
	}

	public class CricketContainer : MonoBehaviour
	{
		public InnateStorageContainer storageContainer;
		public float marginOfError = 0.9f;
		private bool wasJustBuilt = false;
		public static void ApplyShaders(GameObject mv)
		{
			// Add the marmoset shader to all renderers
			Shader marmosetShader = Shader.Find("MarmosetUBER");
			foreach (var renderer in mv.gameObject.GetComponentsInChildren<MeshRenderer>(true))
			{
				foreach (Material mat in renderer.materials)
				{
					mat.shader = marmosetShader;
				}
			}
			var ska = mv.gameObject.EnsureComponent<SkyApplier>();
			ska.anchorSky = Skies.Auto;
			ska.customSkyPrefab = null;
			ska.dynamic = true;
			ska.emissiveFromPower = false;
			//ska.environmentSky = null;
			var rends = mv.gameObject.GetComponentsInChildren<Renderer>();
			ska.renderers = new Renderer[rends.Count()];
			foreach (var rend in rends)
			{
				ska.renderers.Append(rend);
			}
		}
		public void SetupGameObjectPregame()
		{
			gameObject.SetActive(false);
			ApplyShaders(Cricket.storageContainer);

			var rb = gameObject.EnsureComponent<Rigidbody>();
			rb.isKinematic = false;
			rb.useGravity = false;
			rb.mass = 120;
			rb.drag = 10f;
			rb.angularDrag = 1f;

			gameObject.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;

			gameObject.SetActive(true);
		}
		public void SetupGameObjectWakeTime()
		{
			storageContainer = gameObject.EnsureComponent<InnateStorageContainer>();
			storageContainer.storageRoot = transform.Find("StorageRoot").gameObject.AddComponent<ChildObjectIdentifier>();
			storageContainer.storageLabel = "Cricket Container";
			storageContainer.height = 6;
			storageContainer.width = 5;

			FMODAsset storageCloseSound = SeamothHelper.Seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().closeSound;
			FMODAsset storageOpenSound = SeamothHelper.Seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().openSound;
			var inp = gameObject.EnsureComponent<CricketStorageInput>();

			inp.model = gameObject;
			inp.openSound = storageOpenSound;
			inp.closeSound = storageCloseSound;

			VehicleBuilder.CopyComponent<WorldForces>(SeamothHelper.Seamoth.GetComponent<SeaMoth>().worldForces, gameObject);
			var wf = gameObject.GetComponent<WorldForces>();
			wf.useRigidbody = GetComponent<Rigidbody>();
			wf.underwaterGravity = 0f;
			wf.aboveWaterGravity = 9.8f;
			wf.waterDepth = Ocean.GetOceanLevel();
			wf.handleGravity = false;
		}
		public void Awake()
		{
			gameObject.SetActive(false);
			SetupGameObjectWakeTime();
			gameObject.SetActive(true);
		}
		public void Start()
		{
			if (!wasJustBuilt)
			{
				UWE.CoroutineHost.StartCoroutine(MainPatcher.DeserializeStorage(this));
			}
			UWE.CoroutineHost.StartCoroutine(RegisterWithManager());
		}
		public IEnumerator RegisterWithManager()
		{
			while (!VehicleFramework.Admin.GameStateWatcher.IsPlayerStarted)
			{
				yield return null;
			}
			VehicleFramework.Admin.GameObjectManager<CricketContainer>.Register(this);
		}
		public void OnDestroy()
		{
			//CricketContainerManager.main.DeregisterCricketContainer(this);
		}

		public void CricketContainerConstructionBeginning()
		{
			wasJustBuilt = true;
			GetComponent<PingInstance>().enabled = false;
		}
		public void SubConstructionComplete()
		{
			GetComponent<WorldForces>().handleGravity = true;
			GetComponent<PingInstance>().enabled = true;
		}

		/*
        void IHandTarget.OnHandClick(GUIHand hand)
        {
            // Offer to rename the container
            return;
        }

        void IHandTarget.OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, storageContainer.storageLabel);
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
		*/

		public void FixedUpdate()
		{
			float zCorrection = Mathf.Abs(transform.eulerAngles.z - 180f);
			if (zCorrection <= 178f)
			{
				float d = Mathf.Clamp01(1f - zCorrection / 180f) * 20f;
				GetComponent<Rigidbody>().AddTorque(transform.forward * d * Time.fixedDeltaTime * Mathf.Sign(transform.eulerAngles.z - 180f), ForceMode.VelocityChange);
			}

			float xCorrection = Mathf.Abs(transform.eulerAngles.x - 180f);
			if (xCorrection <= 178f)
			{
				float d = Mathf.Clamp01(1f - xCorrection / 180f) * 20f;
				GetComponent<Rigidbody>().AddTorque(transform.right * d * Time.fixedDeltaTime * Mathf.Sign(transform.eulerAngles.x - 180f), ForceMode.VelocityChange);
			}
		}
	}
}
