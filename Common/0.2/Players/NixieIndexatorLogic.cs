using RoA.Content.Tiles.Mechanisms;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    public partial void WiresLoad() {
        On_Player.TileInteractionsUse += On_Player_TileInteractionsUse;
        On_Wiring.HitSwitch += On_Wiring_HitSwitch;
    }

    private void On_Wiring_HitSwitch(On_Wiring.orig_HitSwitch orig, int i, int j) {
        if (WorldGen.InWorld(i, j)) {
            int myX = i, myY = j;
            if (Main.tile[myX, myY].TileType == ModContent.TileType<NixieIndexator>()) {
                if (Main.tile[i, j].TileFrameX >= 18 * 8) {
                    Main.tile[i, j].TileFrameX = 0;
                }
                else {
                    Main.tile[i, j].TileFrameX += 18;
                }

                SoundEngine.PlaySound(SoundID.Mech, new Microsoft.Xna.Framework.Vector2(i * 16, j * 16));

                return;
            }
        }

        orig(i, j);
    }

    private void On_Player_TileInteractionsUse(On_Player.orig_TileInteractionsUse orig, Player self, int myX, int myY) {
        if (WorldGen.InWorld(myX, myY) && Main.tile[myX, myY].TileType == ModContent.TileType<NixieIndexator>()) {
            int i = myX, j = myY;
            bool flag = self.releaseUseTile;
            if (!self.tileInteractAttempted || !flag)
                return;

            Wiring.HitSwitch(myX, myY);
            NetMessage.SendData(MessageID.HitSwitch, -1, -1, null, myX, myY);

            self.tileInteractionHappened = true;

            return;
        }


        orig(self, myX, myY);
    }
}
