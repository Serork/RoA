using Microsoft.Xna.Framework;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.LiquidsSpecific;

sealed class TarfallBlock : ModTile {
    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileBrick[Type] = true;

        AnimationFrameHeight = 90;
        DustType = DustID.Glass;

        AddMapEntry(new Color(62, 53, 70));
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        if (!fail) {
            SoundEngine.PlaySound(SoundID.Shatter, new Vector2(i * 16, j * 16));
        }
    }

    public override void AnimateTile(ref int frame, ref int frameCounter) {
        frameCounter++;
        if (frameCounter >= 12) {
            frameCounter = 0;
            frame++;
            if (frame > 7)
                frame = 0;
        }
    }
}
