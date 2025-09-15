using Microsoft.Xna.Framework;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.LiquidsSpecific;

sealed class TarfallWall : ModWall {
    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        DustType = DustID.Glass;

        AddMapEntry(new Color(46, 39, 52));
    }

    public override void KillWall(int i, int j, ref bool fail) {
        /*if (!fail) */{
            SoundEngine.PlaySound(SoundID.Shatter, new Vector2(i * 16, j * 16));
        }
    }

    public override void AnimateWall(ref byte frame, ref byte frameCounter) {
        frameCounter++;
        if (frameCounter >= 12) {
            frameCounter = 0;
            frame++;
            if (frame > 7)
                frame = 0;
        }
    }
}
