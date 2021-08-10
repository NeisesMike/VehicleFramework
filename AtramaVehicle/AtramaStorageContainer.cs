using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using UWE;

namespace AtramaVehicle
{
	public class AtramaStorageContainer : MonoBehaviour, IProtoEventListener, IProtoTreeEventListener, ICraftTarget
	{
		public ItemsContainer container { get; private set; }

		private void Awake()
		{
			this.Init();
		}

		private void Init()
		{
			if (this.container != null)
			{
				return;
			}
			this.container = new ItemsContainer(this.width, this.height, this.storageRoot.transform, this.storageLabel, null);
			this.container.SetAllowedTechTypes(this.allowedTech);
		}

		public void OnProtoSerialize(ProtobufSerializer serializer)
		{
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			this.Init();
			this.container.Clear(false);
			if (this.serializedStorage != null)
			{
				StorageHelper.RestoreItems(serializer, this.serializedStorage, this.container);
				this.serializedStorage = null;
			}
		}

		public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
		{
		}

		public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
		{
			if (this.version < 2)
			{
				foreach (StoreInformationIdentifier storeInformationIdentifier in base.gameObject.GetComponentsInChildren<StoreInformationIdentifier>(true))
				{
					if (storeInformationIdentifier && storeInformationIdentifier.transform.parent == base.transform)
					{
						UnityEngine.Object.Destroy(storeInformationIdentifier.gameObject);
					}
				}
				this.version = 2;
			}
			else
			{
				StorageHelper.TransferItems(this.storageRoot.gameObject, this.container);
			}
			if (this.version < 3)
			{
				CoroutineHost.StartCoroutine(this.CleanUpDuplicatedStorage());
			}
		}

		private IEnumerator CleanUpDuplicatedStorage()
		{
			yield return StorageHelper.DestroyDuplicatedItems(base.gameObject);
			this.version = Mathf.Max(this.version, 3);
			yield break;
		}

		public void OnCraftEnd(TechType techType)
		{
			this.Init();
			if (techType == TechType.SeamothTorpedoModule || techType == TechType.ExosuitTorpedoArmModule)
			{
				for (int i = 0; i < 2; i++)
				{
					GameObject gameObject = CraftData.InstantiateFromPrefab(TechType.WhirlpoolTorpedo, false);
					if (gameObject != null)
					{
						Pickupable pickupable = gameObject.GetComponent<Pickupable>();
						if (pickupable != null)
						{
							pickupable = pickupable.Pickup(false);
							if (this.container.AddItem(pickupable) == null)
							{
								UnityEngine.Object.Destroy(pickupable.gameObject);
							}
						}
					}
				}
			}
		}

		public string storageLabel = "AtramaStorageLabel";

		public int width = 6;

		public int height = 8;

		public TechType[] allowedTech = new TechType[0];

		[AssertNotNull]
		public ChildObjectIdentifier storageRoot;

		private const int currentVersion = 3;


		public int version = 3;

		[NonSerialized]
		public byte[] serializedStorage;
	}
}
