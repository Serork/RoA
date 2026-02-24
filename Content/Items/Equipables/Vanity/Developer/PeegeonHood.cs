using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Head)]
sealed class PeegeonHood : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Peegeon's Hood");
        //Tooltip.SetDefault("'Great for impersonating RoA devs?' Sure!");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 26; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }

    public override bool IsVanitySet(int head, int body, int legs)
       => head == EquipLoader.GetEquipSlot(Mod, nameof(PeegeonHood), EquipType.Head) &&
          body == EquipLoader.GetEquipSlot(Mod, nameof(PeegeonChestguard), EquipType.Body) &&
          legs == EquipLoader.GetEquipSlot(Mod, nameof(PeegeonGreaves), EquipType.Legs);

    /*public override void UpdateVanitySet(Player player) {
        int num = 0;
        num += player.bodyFrame.Y / 56;
        if (num >= Main.OffsetsPlayerHeadgear.Length)
            num = 0;

        Vector2 vector = Main.OffsetsPlayerHeadgear[num];
        vector *= player.Directions;
        Vector2 vector2 = new Vector2(player.width / 2, player.height / 2) + vector + (player.MountedCenter - player.Center);
        player.sitting.GetSittingOffsetInfo(player, out var posOffset, out var seatAdjustment);
        vector2 += posOffset + new Vector2(0f, seatAdjustment);

        //if (player.face == 19)
        //    vector2.Y -= 5f * player.gravDir;

        //if (head == 276)
        vector2.X -= 2.5f * (float)player.direction;

        if (player.mount.Active && player.mount.Type == 52) {
            vector2.X += 14f * (float)player.direction;
            vector2.Y -= 2f * player.gravDir;
        }

        float y = -11.5f * player.gravDir;
        Vector2 vector3 = new Vector2(3 * player.direction - ((player.direction == 1) ? 1 : 0), y) + Vector2.UnitY * player.gfxOffY + vector2;
        Vector2 vector4 = new Vector2(3 * player.shadowDirection[1] - ((player.direction == 1) ? 1 : 0), y) + vector2;
        Vector2 vector5 = Vector2.Zero;
        if (player.mount.Active && player.mount.Cart) {
            int num2 = Math.Sign(player.velocity.X);
            if (num2 == 0)
                num2 = player.direction;

            vector5 = new Vector2(MathHelper.Lerp(0f, -8f, player.fullRotation / ((float)Math.PI / 4f)), MathHelper.Lerp(0f, 2f, Math.Abs(player.fullRotation / ((float)Math.PI / 4f)))).RotatedBy(player.fullRotation);
            if (num2 == Math.Sign(player.fullRotation))
                vector5 *= MathHelper.Lerp(1f, 0.6f, Math.Abs(player.fullRotation / ((float)Math.PI / 4f)));
        }

        if (player.fullRotation != 0f) {
            vector3 = vector3.RotatedBy(player.fullRotation, player.fullRotationOrigin);
            vector4 = vector4.RotatedBy(player.fullRotation, player.fullRotationOrigin);
        }

        float num3 = 0f;
        Vector2 vector6 = player.position + vector3 + vector5;
        Vector2 vector7 = player.oldPosition + vector4 + vector5;
        vector7.Y -= num3 / 2f;
        vector6.Y -= num3 / 2f;
        float num4 = 1f;
        num4 = 0.5f;
        //switch (player.yoraiz0rEye % 10) {
        //    case 1:
        //        return;
        //    case 2:
        //        num4 = 0.5f;
        //        break;
        //    case 3:
        //        num4 = 0.625f;
        //        break;
        //    case 4:
        //        num4 = 0.75f;
        //        break;
        //    case 5:
        //        num4 = 0.875f;
        //        break;
        //    case 6:
        //        num4 = 1f;
        //        break;
        //    case 7:
        //        num4 = 1.1f;
        //        break;
        //}

        //if (player.yoraiz0rEye < 7) {
        //    DelegateMethods.v3_1 = Main.hslToRgb(Main.rgbToHsl(player.eyeColor).X, 1f, 0.5f).ToVector3() * 0.5f * num4;
        //    if (player.velocity != Vector2.Zero)
        //        Utils.PlotTileLine(player.Center, player.Center + player.velocity * 2f, 4f, DelegateMethods.CastLightOpen);
        //    else
        //        Utils.PlotTileLine(player.Left, player.Right, 4f, DelegateMethods.CastLightOpen);
        //}

        int num5 = (int)Vector2.Distance(vector6, vector7) / 3 + 1;
        if (Vector2.Distance(vector6, vector7) % 3f != 0f)
            num5++;

        for (float num6 = 1f; num6 <= (float)num5; num6 += 1f) {
            Dust obj = Main.dust[Dust.NewDust(player.Center, 0, 0, DustID.TheDestroyer)];
            obj.position = Vector2.Lerp(vector7, vector6, num6 / (float)num5);
            obj.noGravity = true;
            obj.velocity = Vector2.Zero;
            obj.customData = this;
            obj.scale = num4;
            obj.shader = GameShaders.Armor.GetSecondaryShader(player.cHead > 0 ? player.cHead : GameShaders.Armor.GetShaderIdFromItemId(1065), player);
        }
    }*/
    public class EyeTrail : PlayerDrawLayer {
        private class EyeTrailInfo : ModPlayer {
            public Vector2[] trailPos = new Vector2[10];
            public Vector2[] oldPos = new Vector2[10];
            public float idleCount;
        }

        private static Asset<Texture2D> _eyeTrailTexture = null!, _eyeTrailGlowTexture = null!;

        public override void Load() {
            if (Main.dedServ) {
                return;
            }

            _eyeTrailTexture = _eyeTrailGlowTexture = ModContent.Request<Texture2D>(ResourceManager.DeveloperEquipableTextures + "PeegeonHood_Glow");
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
            => true;

        public override Position GetDefaultPosition()
            => new AfterParent(PlayerDrawLayers.Head);

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            if (drawInfo.hideEntirePlayer) {
                return;
            }

            if (drawInfo.shadow != 0f || drawInfo.drawPlayer.dead)
                return;

            var handler = drawInfo.drawPlayer.GetModPlayer<EyeTrailInfo>();

            if (drawInfo.drawPlayer.face > 0) {
                return;
            }

            if (drawInfo.drawPlayer.head != EquipLoader.GetEquipSlot(RoA.Instance, nameof(PeegeonHood), EquipType.Head)) {
                if (handler.trailPos != new Vector2[10]) {
                    handler.trailPos = new Vector2[10];
                    handler.oldPos = new Vector2[10];
                    return;
                }
            }

            Player player = drawInfo.drawPlayer;
            Texture2D texture = _eyeTrailTexture.Value;
            Texture2D glow_texture = _eyeTrailGlowTexture.Value;
            Rectangle bodyFrame = player.bodyFrame;
            SpriteEffects effect = player.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;


            if (!Main.gamePaused) {
                for (int i = handler.trailPos.Length - 1; i > 0; i--) {
                    handler.trailPos[i] = player.MountedCenter + (handler.oldPos[3] - player.MountedCenter) * (i) / handler.trailPos.Length;
                    handler.trailPos[i].Y -= handler.idleCount * i * player.gravDir;
                    handler.trailPos[i] += new Vector2(0, handler.idleCount * i / 3).RotatedByRandom(360);
                }

                for (int i = handler.oldPos.Length - 1; i > 0; i--) {
                    handler.oldPos[i] = handler.oldPos[i - 1];
                }
                if ((handler.oldPos[0].X != player.MountedCenter.X || handler.oldPos[0].Y != player.MountedCenter.Y) && handler.idleCount > 0f) handler.idleCount -= 0.1f;
                else if (handler.idleCount < 1f) handler.idleCount += 0.025f;
                if (handler.idleCount < 0f) handler.idleCount = 0f;

                handler.oldPos[0] = handler.trailPos[0] = player.MountedCenter;
            }

            int offset = 0;
            int height = texture.Height / 20;
            int rate = 1;

            for (int i = 0; i < handler.trailPos.Length; i += rate) {

                float x = (int)(handler.trailPos[i].X);
                float y = (int)(handler.trailPos[i].Y - (float)(3 + offset));

                if (player.gravDir == -1f) {
                    y += player.height / 2 - 4;
                }


                float alpha = 15f;
                Color color = Lighting.GetColor((int)(x / 16f), (int)(y / 16f));
                color *= (handler.trailPos.Length - i) / alpha;

                Color color2 = Color.White;
                color2 *= (handler.trailPos.Length - i) / alpha;
                DrawData drawData = new DrawData(texture, new Vector2((float)x - Main.screenPosition.X, (float)y - Main.screenPosition.Y) + new Vector2(0f, player.gfxOffY), new Rectangle?(bodyFrame), color2, 0f, new Vector2((float)texture.Width / 2f, (float)height / 2f), 1f, effect, 0);
                drawInfo.DrawDataCache.Add(drawData);

                color = player.underShirtColor;
                color *= (handler.trailPos.Length - i) / alpha;
                DrawData drawDataGlow = new DrawData(glow_texture, new Vector2((float)x - Main.screenPosition.X, (float)y - Main.screenPosition.Y) + new Vector2(0f, player.gfxOffY), new Rectangle?(bodyFrame), color, 0f, new Vector2((float)texture.Width / 2f, (float)height / 2f), 1f, effect, 0);
                drawInfo.DrawDataCache.Add(drawDataGlow);
            }
        }
    }
}
