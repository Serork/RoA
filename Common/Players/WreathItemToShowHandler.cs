using RoA.Common.InterfaceElements;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class WreathItemToShowHandler : ModPlayer {
    public bool HideVisuals { get; private set; }
    public Item WreathToShow { get; private set; }
    public Item DyeItem { get; private set; }

    public override void PostUpdateEquips() {
        ForcedUpdate();
    }

    public override void OnEnterWorld() {
        if (Player.whoAmI == Main.myPlayer && !BeltButton.IsUsed &&
            (!WreathSlot.GetFunctionalItem(Player).IsAir || !WreathSlot.GetVanityItem(Player).IsAir || !WreathSlot.GetDyeItem(Player).IsAir)) {
            BeltButton.ToggleTo(true);
        }
    }

    internal void ForcedUpdate() {
        if (!Player.isDisplayDollOrInanimate) {
            Item wreathVanityItem = WreathSlot.GetVanityItem(Player);
            Item wreathItem = wreathVanityItem.IsEmpty() ? WreathSlot.GetFunctionalItem(Player) : wreathVanityItem;

            WreathToShow = wreathItem;
            HideVisuals = WreathSlot.GetHideVisuals(Player);
            DyeItem = WreathSlot.GetDyeItem(Player);
        }
    }

    internal void ForcedUpdate2(Item wreathItem, Item dyeItem) {
        if (wreathItem != null) {
            WreathToShow = wreathItem;
        }
        HideVisuals = false;
        if (dyeItem != null) {
            DyeItem = dyeItem;
        }
    }

    public override void CopyClientState(ModPlayer targetCopy) {
        //WreathItemToShowHandler client = (WreathItemToShowHandler)targetCopy;
        //if (WreathToShow != null && client.WreathToShow != null) {
        //    WreathToShow.CopyNetStateTo(client.WreathToShow);
        //}
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
        //if ((WreathToShow != null || DyeItem != null) && Main.netMode == NetmodeID.MultiplayerClient) {
        //    MultiplayerSystem.SendPacket(new WreathItemPacket(Player, HideVisuals, WreathToShow, DyeItem), toWho, fromWho);
        //}
    }

    public override void SendClientChanges(ModPlayer clientPlayer) {
        //WreathItemToShowHandler client = (WreathItemToShowHandler)clientPlayer;
        //if (Main.netMode == NetmodeID.MultiplayerClient) {
        //    if ((WreathToShow != null && client.WreathToShow != null && WreathToShow.IsNetStateDifferent(client.WreathToShow)) ||
        //        HideVisuals != client.HideVisuals ||
        //        (DyeItem != null && client.DyeItem != null && DyeItem.IsNetStateDifferent(client.DyeItem))) {
        //        MultiplayerSystem.SendPacket(new WreathItemPacket(Player, HideVisuals, WreathToShow, DyeItem));
        //    }
        //}
    }

    internal void ReceivePacket(bool hideVisuals, Item wreathToShow, Item dyeItem) {
        HideVisuals = hideVisuals;
        WreathToShow = wreathToShow;
        DyeItem = dyeItem;
    }
}