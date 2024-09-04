using Microsoft.Xna.Framework;

using RoA.Common.WorldEvents;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Commands;

sealed class ActivateFog : ModCommand {
    public override CommandType Type => CommandType.World;
    public override string Command => "togglebackwoodsfog";
    public override string Usage => "/togglebackwoodsfog";

    public override void Action(CommandCaller caller, string input, string[] args) {
        BackwoodsFogHandler.IsFogActive = !BackwoodsFogHandler.IsFogActive;
        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.WorldData);
        }
    }
}
