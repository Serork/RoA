using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Players;
using RoA.Core;
using RoA.Core.Utility;
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
    }

    private void PlayerCommon_AlwaysHeadDrawEvent(ref PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        if (!player.GetCommon().IsHereticsVeilEffectActive) {
            return;
        }
        Vector2 drawPosition = new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect;
        DrawData drawData = new DrawData(TextureAssets.Players[drawinfo.skinVar, 0].Value, drawPosition, drawinfo.drawPlayer.bodyFrame,
            drawinfo.colorHead.MultiplyRGB(Color.Black), drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect);
        DrawData item = drawData;
        drawinfo.DrawDataCache.Add(item);
        //item = new DrawData(TextureAssets.Players[drawinfo.skinVar, 1].Value, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, drawinfo.drawPlayer.bodyFrame,
        //    drawinfo.colorEyeWhites, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect);
        //drawinfo.DrawDataCache.Add(item);
        //item = new DrawData(TextureAssets.Players[drawinfo.skinVar, 2].Value, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, drawinfo.drawPlayer.bodyFrame,
        //    drawinfo.colorEyes.MultiplyRGB(Color.Black), drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect);
        //drawinfo.DrawDataCache.Add(item);

        if (_flameTexture.IsLoaded is not true) {
            return;
        }

        if (_seed == 0f) {
            _seed = Main.rand.NextFloat(100f);
        }
        ulong seed = (ulong)_seed;
        for (int i = 0; i < 2; i++) {
            Texture2D flameTexture = _flameTexture.Value;
            Vector2 flamePosition = drawPosition;
            int frame = (int)((Main.GlobalTimeWrappedHourly * 15f + Utils.RandomInt(ref seed, -3, 4)) % 8);
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
            Vector2 offset = new(Utils.RandomInt(ref seed, -2, 3) * -player.direction, Utils.RandomInt(ref seed, -2, 3));
            flamePosition += offset;
            flamePosition.Y += 1f;
            if (drawinfo.drawPlayer.gravDir < 0) {
                flamePosition.Y += 31f;
            }
            DrawData drawData2 = new DrawData(flameTexture, flamePosition, flameClip,
                flameColor, flameRotation, flameOrigin, 1f, drawinfo.playerEffect);
            drawinfo.DrawDataCache.Add(drawData2);
        }
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

        player.GetCommon().IsHereticsVeilEffectActive = true;

        player.GetCommon().StopHeadDrawing = true;

        player.endurance += 0.2f;
        player.lifeRegen += 4;

        foreach (NPC nPC in Main.ActiveNPCs) {
            float num = TileHelper.TileSize * 31;
            if (nPC.CanBeChasedBy(this) && !(player.Distance(nPC.Center) > num) && Collision.CanHitLine(player.position, player.width, player.height, nPC.position, nPC.width, nPC.height)) {
                nPC.AddBuff(BuffID.OnFire, 90);
            }
        }
    }
}
