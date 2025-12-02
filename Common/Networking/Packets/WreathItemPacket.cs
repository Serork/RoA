using RoA.Common.Players;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace RoA.Common.Networking.Packets;

sealed class WreathItemPacket : NetPacket {
    private bool _null, _null2;

    public WreathItemPacket(Player player, bool hideVisuals, Item wreathToShow = null, Item dyeItem = null) {
        Writer.TryWriteSenderPlayer(player);

        Writer.Write(hideVisuals);
        if (wreathToShow != null) {
            ItemIO.Send(wreathToShow, Writer, true);
        }
        else {
            _null = true;
        }
        if (dyeItem != null) {
            ItemIO.Send(dyeItem, Writer, true);
        }
        else {
            _null2 = true;
        }
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        bool hideVisuals = reader.ReadBoolean();
        Item wreathToShow = null;
        Item dyeItem = null;
        if (!_null) {
            wreathToShow = ItemIO.Receive(reader, true);
        }
        else {
            _null = false;
        }
        if (!_null2) {
            dyeItem = ItemIO.Receive(reader, true);
        }
        else {
            _null2 = false;
        }

        player.GetModPlayer<WreathItemToShowHandler>().ReceivePacket(hideVisuals, wreathToShow, dyeItem);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new WreathItemPacket(player, hideVisuals, wreathToShow, dyeItem), ignoreClient: sender);
        }
    }
}
