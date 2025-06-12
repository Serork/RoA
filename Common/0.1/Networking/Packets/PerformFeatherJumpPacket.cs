using RoA.Core.Utility;

using System.IO;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Networking.Packets;

sealed class PerformFeatherJumpPacket : NetPacket {
    public PerformFeatherJumpPacket(Player player, int jumpType) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(jumpType);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int jumpType = reader.ReadInt32();
        var jump = ExtraJumpLoader.OrderedJumps.FirstOrDefault(x => x.Type == jumpType);
        ref ExtraJumpState state = ref player.GetJumpState(jump);
        PerformJump(jump, player);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new PerformFeatherJumpPacket(player, jumpType), ignoreClient: sender);
        }
    }

    private static void PerformJump(ExtraJump jump, Player player) {
        // Set velocity and jump duration
        float duration = jump.GetDurationMultiplier(player);
        PlayerLoader.ModifyExtraJumpDurationMultiplier(jump, player, ref duration);

        player.velocity.Y = -Player.jumpSpeed * player.gravDir;
        player.jump = (int)(Player.jumpHeight * duration);

        bool playSound = true;
        jump.OnStarted(player, ref playSound);
        PlayerLoader.OnExtraJumpStarted(jump, player, ref playSound);

        if (playSound)
            SoundEngine.PlaySound(SoundID.DoubleJump, player.position);
    }
}
