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
	public class InnateStorageContainer : MonoBehaviour, ICraftTarget//, IProtoEventListener, IProtoTreeEventListener
	{
		private bool hasInit = false;

		private ItemsContainer _container;
		public ItemsContainer container 
		{ 
			get
            {
				return _container;
            }
			private set
			{
				_container = value;
            }
		}

		public void Awake()
		{
			this.Init();
		}

		private void Init()
		{
			if (this.container != null || hasInit)
			{
				return;
			}
			this.container = new ItemsContainer(this.width, this.height, this.storageRoot.transform, this.storageLabel, null);
			this.container.SetAllowedTechTypes(this.allowedTech);
			this.container.isAllowedToRemove = null;
			hasInit = true;
		}

		public void OnCraftEnd(TechType techType)
		{
			// NEWNEW
			IEnumerator GetAndSetTorpedoSlots()
			{
				if (techType == TechType.SeamothTorpedoModule || techType == TechType.ExosuitTorpedoArmModule)
				{
					for (int i = 0; i < 2; i++)
					{
						TaskResult<GameObject> result = new TaskResult<GameObject>();
						yield return CraftData.InstantiateFromPrefabAsync(techType, result, false);
						GameObject gameObject = result.Get();
						if (gameObject != null)
						{
							Pickupable pickupable = gameObject.GetComponent<Pickupable>();
							if (pickupable != null)
							{
								// NEWNEW
								// Why did we use to have this line?
								//pickupable = pickupable.Pickup(false);
								if (this.container.AddItem(pickupable) == null)
								{
									UnityEngine.Object.Destroy(pickupable.gameObject);
								}
							}
						}
					}
				}
				yield break;
			}
			this.Init();
			StartCoroutine(GetAndSetTorpedoSlots());
		}

		public string storageLabel = "StorageLabel";

		public int width = 6;
		public int height = 8;

		public TechType[] allowedTech = new TechType[0];

		[AssertNotNull]
		public ChildObjectIdentifier storageRoot;

		public int version = 3;

		[NonSerialized]
		public byte[] serializedStorage;
	}
}
