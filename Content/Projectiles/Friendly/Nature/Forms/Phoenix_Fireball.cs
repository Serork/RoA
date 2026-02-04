using Microsoft.Xna.Framework;

using Newtonsoft.Json.Linq;

using RoA.Common;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

[Tracked]
sealed class PhoenixFireball : FormProjectile {
    public enum StateType : byte {
        Idle,
        Shoot,
        Count
    }

    public ref float InitValue => ref Projectile.localAI[0];

    public ref float StateValue => ref Projectile.ai[0];
    public ref float OffsetXValue => ref Projectile.ai[1];
    public ref float OffsetYValue => ref Projectile.ai[2];

    public StateType State {
        get => (StateType)StateValue;
        set => StateValue = Utils.Clamp((byte)value, (byte)StateType.Idle, (byte)StateType.Count);
    }

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }

    public Vector2 PositionOffset => new(OffsetXValue, OffsetYValue);   

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(10);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(20);

        Projectile.friendly = true;
        Projectile.penetrate = -1;

        Projectile.tileCollide = false;
    }

    public override void AI() {
        Projectile.timeLeft = 2;

        Vector2 center = Projectile.GetOwnerAsPlayer().GetPlayerCorePoint();
        if (!Init) {
            Init = true;

            Projectile.Center = center;
        }
        switch (State) {
            case StateType.Idle:
                Projectile.Center = Vector2.Lerp(Projectile.Center, center + PositionOffset, 0.3f);
                break;
            case StateType.Shoot:
                break;
        }

        ref int frame = ref Projectile.frame;
        ref int frameCounter = ref Projectile.frameCounter;
        if (frameCounter++ > 4) {
            frameCounter = 0;
            frame++;
        }
        if (frame > 8) {
            frame = 5;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(Color.White * 1f * Projectile.Opacity);

        for (float num6 = 0f; num6 < 4f; num6 += 1f) {
            float num3 = ((float)(TimeSystem.TimeForVisualEffects * 60f + Projectile.whoAmI * 10) / 40f * ((float)Math.PI * 2f)).ToRotationVector2().X * 3f;
            Color color2 = new Color(80, 70, 40, 0) * (num3 / 8f + 0.5f) * 0.8f;
            Vector2 position = Projectile.Center + (num6 * ((float)Math.PI / 2f)).ToRotationVector2() * num3;

            Vector2 temp = Projectile.Center;
            Projectile.Center = position;
            Projectile.QuickDrawAnimated(color2 * Projectile.Opacity);
            Projectile.Center = temp;
        }

        return false;
    }
}
