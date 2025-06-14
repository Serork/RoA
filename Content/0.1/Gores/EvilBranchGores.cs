//using Terraria;
//using Terraria.DataStructures;
//using Terraria.GameContent;
//using Terraria.ModLoader;

//namespace RoA.Content.Gores;

//class EvilBranchGore2 : EvilBranchGore1 { }

//class EvilBranchGore1 : ModGore {
//	public override void OnSpawn(Gore gore, IEntitySource source) {
//		ChildSafety.SafeGore[gore.type] = true;

//		gore.UsedFrame = new SpriteFrame(1, 3) {
//			CurrentRow = (byte)Main.rand.Next(3)
//		};
//	}
//}