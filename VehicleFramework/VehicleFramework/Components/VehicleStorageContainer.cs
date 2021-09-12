using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using UWE;

namespace VehicleFramework
{
	public class VehicleStorageContainer : MonoBehaviour, ICraftTarget//, IProtoEventListener, IProtoTreeEventListener
	{
		public ItemsContainer container { get; private set; }

		public void Awake()
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
			this.container.isAllowedToRemove = null;
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

		public string storageLabel = "StorageLabel";

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
