//using Terraria;
//using Terraria.ModLoader;

//namespace RoA.Common.Lavas;

//abstract class LavaStyle : ModWaterStyle {
//	public string texturePath;

//	public override string Texture => texturePath;

//	public override void Load() {
//		string name = "";
//		string texture = "";
//		bool value = SafeAutoload(ref name, ref texture);

//        texturePath = texture;
//        LavaLoader.RegisterLavaStyle(this);
//	}

//	public virtual bool SafeAutoload(ref string name, ref string texture) => true;

//	public virtual bool DrawEffects(int x, int y) => false;

//	public virtual void DrawBlockEffects(int x, int y, Tile up, Tile left, Tile right, Tile down) { }

//	public virtual bool ChooseLavaStyle() => false;

//	public virtual ushort SplashDustType() => 35;
//}