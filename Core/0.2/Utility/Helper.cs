using Terraria;
using Terraria.ID;

namespace RoA.Core.Utility; 

static partial class Helper {
    public static bool SinglePlayerOrServer => Main.netMode != NetmodeID.MultiplayerClient;
}
