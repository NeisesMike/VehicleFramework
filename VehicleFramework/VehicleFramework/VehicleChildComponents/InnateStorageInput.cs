using VehicleFramework.Admin;

namespace VehicleFramework.StorageComponents
{
    public class InnateStorageInput : StorageInput
    {
		public override void OpenFromExternal()
		{
			if(mv == null)
			{
				throw Admin.SessionManager.Fatal($"{transform.name} has no ModVehicle component! Please set the ModVehicle before calling OpenFromExternal.");
            }
            ItemsContainer? storageInSlot = mv.ModGetStorageInSlot(slotID, EnumHelper.GetInnateStorageType());
			if (storageInSlot != null)
			{
				PDA pda = Player.main.GetPDA();
				Inventory.main.SetUsedStorage(storageInSlot, false);
				pda.Open(PDATab.Inventory, null, null);
			}
		}
		protected override void OpenPDA()
		{
			if (mv == null)
			{
				throw Admin.SessionManager.Fatal($"{transform.name} has no ModVehicle component! Please set the ModVehicle before calling OpenPDA.");
			}
			ItemsContainer? storageInSlot = mv.ModGetStorageInSlot(slotID, EnumHelper.GetInnateStorageType());
			if (storageInSlot != null)
			{
				PDA pda = Player.main.GetPDA();
				Inventory.main.SetUsedStorage(storageInSlot, false);
				if (!pda.Open(PDATab.Inventory, transform, new PDA.OnClose(this.OnClosePDA)))
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
