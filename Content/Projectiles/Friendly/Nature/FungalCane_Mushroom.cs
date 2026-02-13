using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class FungalCaneMushroom : NatureProjectile {
    public ref float InitValue => ref Projectile.localAI[0];

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(2);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(16);
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;
        Projectile.hide = true;

        Projectile.tileCollide = false;

        Projectile.netImportant = true;

        Projectile.manualDirectionChange = true;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        => behindNPCsAndTiles.Add(index);

    public override void AI() {
        if (!Init) {
            Init = true;

            Player player = Main.player[Projectile.owner];
            EvilBranch.GetPos(player, out Point point, out Point point2, maxDistance: 800f);
            Projectile.Center = point2.ToWorldCoordinates();
            while (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
                Projectile.position.Y -= 1f;
            }
            Projectile.position.Y -= 5f;

            Projectile.frame = Main.rand.Next(2);
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor);

        return false;
    }
}
