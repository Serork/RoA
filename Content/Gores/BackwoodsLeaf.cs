using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace RoA.Content.Gores;

sealed class BackwoodsLeaf : ModGore {
	public override void OnSpawn(Gore gore, IEntitySource source) {
		ChildSafety.SafeGore[gore.type] = true;

		gore.velocity = new Vector2(Main.rand.NextFloat() - 0.5f, Main.rand.NextFloat() * MathHelper.TwoPi);
		gore.numFrames = 8;
		gore.frame = (byte)Main.rand.Next(8);
		gore.frameCounter = (byte)Main.rand.Next(8);

		UpdateType = GoreID.TreeLeaf_Normal;
	}
}