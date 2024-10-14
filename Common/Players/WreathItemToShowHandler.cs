using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class WreathItemToShowHandler : ModPlayer {
    public bool HideVisuals { get; private set; }
    public Item WreathToShow { get; private set; }
    public Item DyeItem { get; private set; }

    public override void PostUpdateEquips() {
        ModAccessorySlot wreathSlot = LoaderManager.Get<AccessorySlotLoader>().Get(ModContent.GetInstance<WreathSlot>().Type, Player);
        Item wreathVanityItem = wreathSlot.VanityItem;
        Item wreathItem = wreathVanityItem.IsEmpty() ? wreathSlot.FunctionalItem : wreathVanityItem;

        WreathToShow = wreathItem;
        HideVisuals = wreathSlot.HideVisuals;
        DyeItem = wreathSlot.DyeItem;
    }

    public override void CopyClientState(ModPlayer targetCopy) {
        var defaultInv = (WreathItemToShowHandler)targetCopy;
        if (WreathToShow != null && defaultInv.WreathToShow != null) {
            WreathToShow.CopyNetStateTo(defaultInv.WreathToShow);
        }
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new WreathItemPacket(Player, HideVisuals, WreathToShow, DyeItem), toWho, fromWho);
        }
    }

    public override void SendClientChanges(ModPlayer clientPlayer) {
        var clientInv = (WreathItemToShowHandler)clientPlayer;
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            if ((WreathToShow != null && clientInv.WreathToShow != null && WreathToShow.IsNetStateDifferent(clientInv.WreathToShow)) ||
                HideVisuals != clientInv.HideVisuals ||
                (DyeItem != null && clientInv.DyeItem != null && DyeItem.IsNetStateDifferent(clientInv.DyeItem))) {
                MultiplayerSystem.SendPacket(new WreathItemPacket(Player, HideVisuals, WreathToShow, DyeItem));
            }
        }
    }

    internal void ReceivePacket(bool hideVisuals, Item wreathToShow, Item dyeItem) {
        HideVisuals = hideVisuals;
        WreathToShow = wreathToShow;
        DyeItem = dyeItem;
    }
}