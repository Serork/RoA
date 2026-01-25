using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

// also see Hooks_Player.cs
sealed class ChromaticScarfDebuffPicker : GlobalItem {
    public static ushort CHANGETIMEINTICKS => MathUtils.SecondsToFrames(10);

    public static readonly ushort[] DebuffList = [BuffID.OnFire3, BuffID.CursedInferno, BuffID.Frostburn2];

    public ushort CurrentDebuff, NextDebuff;
    public ushort CurrentDebuffCounter;

    public void UpdateCurrentDebuff() {
        if (CurrentDebuffCounter++ > CHANGETIMEINTICKS) {
            CurrentDebuffCounter = 0;

            CurrentDebuff = NextDebuff;
            while (CurrentDebuff == NextDebuff) {
                NextDebuff = Main.rand.NextFromList(DebuffList);
            }
        }
    }

    public override void UpdateInventory(Item item, Player player) {
        if (player.GetSelectedItem() == item) {
            return;
        }
        UpdateCurrentDebuff();
    }

    public override void HoldItem(Item item, Player player) {
        UpdateCurrentDebuff();
    }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.IsAWeapon();
}
