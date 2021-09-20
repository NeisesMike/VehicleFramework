using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class ModularStorageInput : StorageInput
	{
		public override void OpenFromExternal()
		{
			ItemsContainer storageInSlot = mv.ModGetStorageInSlot(slotID, TechType.VehicleStorageModule);
			if (storageInSlot != null)
			{
				PDA pda = Player.main.GetPDA();
				Inventory.main.SetUsedStorage(storageInSlot, false);
				pda.Open(PDATab.Inventory, null, null, -1f);
			}
		}
		protected override void OpenPDA()
		{
			ItemsContainer storageInSlot = mv.ModGetStorageInSlot(slotID, TechType.VehicleStorageModule);
			if (storageInSlot == null)
			{
				storageInSlot = gameObject.GetComponent<SeamothStorageContainer>().container;
			}

			if (storageInSlot != null)
			{
				PDA pda = Player.main.GetPDA();
				Inventory.main.SetUsedStorage(storageInSlot, false);
				if (!pda.Open(PDATab.Inventory, this.tr, new PDA.OnClose(this.OnClosePDA), -1f))
				{
					this.OnClosePDA(pda);
					return;
				}
			}
			else
			{
				this.OnClosePDA(null);
			}
		}
	}
}
