using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

[AutoloadEquip(EquipType.Back, EquipType.Front)]
sealed class HereticsVeil : ModItem {
    private static Asset<Texture2D> _flameTexture = null!;
    private static float _seed = 0f;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _flameTexture = ModContent.Request<Texture2D>(ResourceManager.MiscellaneousProjectileTextures + "HereticsVeil_Flame");
    }

    public override void Load() {
        PlayerCommon.AlwaysHeadDrawEvent += PlayerCommon_AlwaysHeadDrawEvent;
        ExtraDrawLayerSupport.PreProjectileOverArmDrawEvent += ExtraDrawLayerSupport_PreProjectileOverArmDrawEvent;
    }

    private void ExtraDrawLayerSupport_PreProjectileOverArmDrawEvent(ref PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        if (!player.GetCommon().IsHereticsVeilEffectActive) {
            return;
        }
        if (!player.IsAlive()) {
            return;
        }

        float opacity = 1f - player.GetCommon().HereticVeilEffectOpacity;
        Vector2 drawPosition = new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect;
        //drawPosition += player.MovementOffset();

        if (_seed == 0f) {
            _seed = Main.rand.NextFloat(100f);
        }
        ulong seed = (ulong)_seed;

        bool isAppearing = player.statLife <= 100;

        float startProgress = isAppearing ? Utils.GetLerpValue(0f, 0.625f, opacity, true) : 1f;

        for (int i = 0; i < 2; i++) {
            Texture2D flameTexture = _flameTexture.Value;
            Vector2 flamePosition = drawPosition;
            int frame = (int)((TimeSystem.TimeForVisualEffects * 15f + Utils.RandomInt(ref seed, -3, 4)) % 8);
            Rectangle flameClip = Utils.Frame(flameTexture, 1, 8, frameY: frame);
            Vector2 flameOrigin = flameClip.Centered();
            Color flameColor = Color.Lerp(new Color(255, 165, 53), new Color(255, 247, 147), Utils.RandomFloat(ref seed));
            flameColor.A = 60;
            flameColor *= 0.75f;
            if (i != 0) {
                flameColor = Color.Lerp(new Color(255, 53, 53), new Color(255, 147, 147), Utils.RandomFloat(ref seed));
                flameColor.A = 60;
                flameColor *= 0.75f;
            }
            float flameRotation = drawinfo.drawPlayer.headRotation;
            flamePosition.Y -= flameOrigin.Y / 2f;
            Vector2 offset = new(i != 0 ? (Utils.RandomInt(ref seed, -2, 3) * -player.direction) : 0f, Utils.RandomInt(ref seed, -2, 3));
            flamePosition += offset;
            flamePosition.Y += 1f;
            if (drawinfo.drawPlayer.gravDir < 0) {
                flamePosition.Y += 31f;
            }
            float scale2 = MathHelper.Lerp(1.5f, 1f, startProgress);
            Vector2 scale = Vector2.One * scale2;
            DrawData drawData2 = new DrawData(flameTexture, flamePosition, flameClip,
                flameColor * opacity, flameRotation, flameOrigin, scale, drawinfo.playerEffect);
            drawinfo.DrawDataCache.Add(drawData2);
        }
    }

    private void PlayerCommon_AlwaysHeadDrawEvent(ref PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        if (!player.GetCommon().IsHereticsVeilEffectActive) {
            return;
        }
        if (!player.IsAlive()) {
            return;
        }

        float opacity = 1f - player.GetCommon().HereticVeilEffectOpacity;
        Vector2 drawPosition = new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect;
        //drawPosition += player.MovementOffset();
        DrawData drawData = new DrawData(TextureAssets.Players[drawinfo.skinVar, 0].Value, drawPosition, drawinfo.drawPlayer.bodyFrame,
            Lighting.GetColor(drawPosition.ToTileCoordinates()).MultiplyRGB(Color.Black) * opacity, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect);
        DrawData item = drawData;
        drawinfo.DrawDataCache.Add(item);
        //item = new DrawData(TextureAssets.Players[drawinfo.skinVar, 1].Value, new Vector2((int)(drawinfo.Positions.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Positions.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, drawinfo.drawPlayer.bodyFrame,
        //    drawinfo.colorEyeWhites, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect);
        //drawinfo.DrawDataCache.Add(item);
        //item = new DrawData(TextureAssets.Players[drawinfo.skinVar, 2].Value, new Vector2((int)(drawinfo.Positions.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Positions.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, drawinfo.drawPlayer.bodyFrame,
        //    drawinfo.colorEyes.MultiplyRGB(Color.Black), drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect);
        //drawinfo.DrawDataCache.Add(item);
    }

    public override void SetDefaults() {
        Item.DefaultToAccessory(32, 32);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if (player.statLife > 100) {
            _seed = 0f;

            return;
        }

        bool isAppearing = player.statLife <= 100;
        float opacity = 1f - player.GetCommon().HereticVeilEffectOpacity;
        float startProgress = isAppearing ? Utils.GetLerpValue(0f, 0.4f, opacity, true) : 1f;

        Vector2 position = player.Top + Vector2.UnitY * player.height * 0.4f;
        if (startProgress == 0f) {
            for (int num130 = 0; num130 < 20; num130++) {
                Vector2 velocity = -Vector2.UnitY.RotatedBy(player.fullRotation);
                int num1020 = Math.Sign(velocity.Y);
                int num1021 = ((num1020 != -1) ? 1 : 0);
                int num1030 = Utils.SelectRandom<int>(Main.rand, 6, 259, 158);
                float num127 = Main.rand.NextFloat(0.75f, 1.25f);
                num127 *= Main.rand.NextFloat(1.25f, 1.5f);
                int width = 20;
                Color color = Color.Lerp(new Color(255, 165, 53), new Color(255, 247, 147), Main.rand.NextFloat());
                if (Main.rand.NextBool()) {
                    color = Color.Lerp(new Color(255, 53, 53), new Color(255, 147, 147), Main.rand.NextFloat());
                }
                if (num1030 != 6) {
                    color = default;
                    num127 = 1f;
                }
                int num131 = Dust.NewDust(new Vector2(position.X, position.Y), 6, 6, num1030, 0f, 0f, 0, color, num127);
                Main.dust[num131].position = position + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(velocity.ToRotation()) * width / 3f;
                Main.dust[num131].customData = num1021;
                if (num1020 == -1 && Main.rand.Next(4) != 0)
                    Main.dust[num131].velocity.Y -= 0.2f;
                Main.dust[num131].noGravity = true;
                Dust dust2 = Main.dust[num131];
                dust2.velocity *= 0.5f;
                dust2 = Main.dust[num131];
                dust2.velocity += position.DirectionTo(Main.dust[num131].position) * Main.rand.NextFloat(2f, 5f) * 0.8f;
                dust2.velocity.Y += velocity.Y * Main.rand.NextFloat(2f, 5f) * 0.625f * 0.8f;
            }
        }

        player.GetCommon().IsHereticsVeilEffectActive = true;

        //player.GetCommon().StopHeadDrawing = true;

        player.endurance += 0.2f;
        player.lifeRegen += 10;

        foreach (NPC nPC in Main.ActiveNPCs) {
            float num = TileHelper.TileSize * 31;
            if (nPC.CanBeChasedBy(player) && !(player.Distance(nPC.Center) > num) && Collision.CanHitLine(player.position, player.width, player.height, nPC.position, nPC.width, nPC.height)) {
                nPC.AddBuff(BuffID.OnFire, 90);
            }
        }
    }
}
