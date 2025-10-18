using System;
using System.Collections;
using UnityEngine;
using VehicleFramework.Assets;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework.StorageComponents
{
	public class InnateStorageContainer : MonoBehaviour, ICraftTarget//, IProtoEventListener, IProtoTreeEventListener
	{
		private ItemsContainer _container = null!;
		public ItemsContainer Container
		{
			get
			{
				if (_container == null)
				{
					_container = new(this.width, this.height, this.storageRoot.transform, this.storageLabel, null);
					_container.SetAllowedTechTypes(this.allowedTech);
					_container.isAllowedToRemove = null;
				}
				return _container;
			}
			private set
			{
				_container = value;
			}
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
						TaskResult<GameObject> result = new();
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
								if (this.Container.AddItem(pickupable) == null)
								{
									UnityEngine.Object.Destroy(pickupable.gameObject);
								}
							}
						}
					}
				}
				yield break;
			}
			StartCoroutine(GetAndSetTorpedoSlots());
		}

		internal static void Create(VehicleBuilding.VehicleStorage vs, ModVehicle mv, int storageID)
		{
			var cont = vs.Container.EnsureComponent<InnateStorageContainer>();
			cont.storageRoot = mv.StorageRootObject.GetComponent<ChildObjectIdentifier>();
			cont.storageLabel = "Vehicle Storage " + storageID.ToString();
			cont.height = vs.Height;
			cont.width = vs.Width;

			if (SeamothHelper.Seamoth == null)
			{
				throw Admin.SessionManager.Fatal("SeamothHelper.Seamoth is null when trying to create innate storage! This should never happen!");
			}

			FMODAsset storageCloseSound = SeamothHelper.Seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().closeSound;
			FMODAsset storageOpenSound = SeamothHelper.Seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().openSound;
			var inp = vs.Container.EnsureComponent<InnateStorageInput>();
			inp.mv = mv;
			inp.slotID = storageID;
			inp.model = vs.Container;
			if (vs.Container.GetComponentInChildren<Collider>() == null)
			{
				inp.collider = vs.Container.EnsureComponent<BoxCollider>();
			}
			inp.openSound = storageOpenSound;
			inp.closeSound = storageCloseSound;
			vs.Container.SetActive(true);

			SaveLoad.SaveLoadUtils.EnsureUniqueNameAmongSiblings(vs.Container.transform);
			vs.Container.EnsureComponent<SaveLoad.VFInnateStorageIdentifier>();
		}

		public string storageLabel = "StorageLabel";

		public int width = 6;
		public int height = 8;

		public TechType[] allowedTech = Array.Empty<TechType>();

		[AssertNotNull]
		public ChildObjectIdentifier storageRoot = null!;

		public int version = 3;

		[NonSerialized]
		public byte[] serializedStorage = null!;
		public static void SetContainerLabel(Submarine sub, string containerLabel)
		{
			sub.InnateStorages?.ForEach(x => x.Container.GetComponent<InnateStorageContainer>().Container._label = containerLabel);
		}
	}
}
