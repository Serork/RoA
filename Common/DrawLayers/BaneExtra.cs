//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//using RoA.Content.Items.Weapons.Magic;
//using RoA.Core;
//using RoA.Core.Utility;

//using Terraria;
//using Terraria.DataStructures;
//using Terraria.ModLoader;

//namespace RoA.Common.DrawLayers;

//sealed class BaneExtra : PlayerDrawLayer {
//    private float _rotation;

//    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FirstVanillaLayer);

//    protected override void Draw(ref PlayerDrawSet drawInfo) {
//        return;

//        Player player = drawInfo.drawPlayer;
//        if (player.dead || !player.active || drawInfo.shadow != 0f) {
//            return;
//        }

//        if (player.GetSelectedItem().type != ModContent.ItemType<Bane>()) {
//            return;
//        }

//        _rotation += 0.05f;

//        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.Textures + "BaneRune2");
//        Color color = Lighting.GetColor((int)drawInfo.Center.X / 16, (int)drawInfo.Center.Y / 16).MultiplyRGB(Color.DarkViolet);
//        Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
//        DrawData item = new(texture, Utils.Floor(drawInfo.Center) - Main.screenPosition, new Rectangle?(),
//            color, _rotation, origin, 1f, SpriteEffects.None, 0);
//        drawInfo.DrawDataCache.Add(item);
//    }
//}
