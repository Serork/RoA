using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using static Terraria.GameContent.Animations.Actions.Sprites;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class TectonicCane : BaseRodItem<TectonicCane.TectonicCaneBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<TectonicCaneProjectile>();

    protected override void SafeSetDefaults() {
        Item.SetSize(36, 38);
        Item.SetDefaultToUsable(-1, 30, useSound: SoundID.Item7);
        Item.SetWeaponValues(5, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 15);
        NatureWeaponHandler.SetFillingRate(Item, 0.35f);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class TectonicCaneBase : BaseRodProjectile {
        protected override bool ShouldWaitUntilProjDespawns() => false;
    }
}

sealed class TectonicCaneProjectile : NatureProjectile {
    protected override void SafeSetDefaults() {
        Projectile.Size = Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;
        Projectile.hide = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        Projectile.netImportant = true;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.owner != Main.myPlayer) {
            return;
        }

        Projectile.spriteDirection = Main.rand.NextBool().ToDirectionInt();

        EvilBranch.GetPos(Main.player[Projectile.owner], out Point point, out Point point2);
        Projectile.Center = point2.ToWorldCoordinates();

        Projectile.netUpdate = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

    public override bool PreDraw(ref Color lightColor) {
        SpriteFrame frame = new(1, 4);
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Color drawColor = Lighting.GetColor((Projectile.Center - Vector2.UnitY * 20f).ToTileCoordinates());
        SpriteEffects spriteEffects = (SpriteEffects)(Projectile.spriteDirection == 1).ToInt();
        Vector2 position = Projectile.Center - Main.screenPosition;
        Rectangle sourceRectangle = frame.GetSourceRectangle(texture);
        sourceRectangle.Height += 2;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, drawColor, Projectile.rotation, sourceRectangle.BottomCenter(), Projectile.scale, spriteEffects);

        return false;
    }
}