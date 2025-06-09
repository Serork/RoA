using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Utility;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

abstract class NatureProjectile_NoTextureLoad : NatureProjectile {
    private class StopNoTextureLoadProjectileFromDrawingHandler : ILoadable {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawProj_DrawYoyoString")]
        public extern static void Main_DrawProj_DrawYoyoString(Main self, Projectile proj, Vector2 mountedCenter);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawProj_DrawExtras")]
        public extern static void Main_DrawProj_DrawExtras(Main self, Projectile proj, Vector2 mountedCenter, ref float polePosX, ref float polePosY);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawProj_Inner_DoDrawProj")]
        public extern static void Main_DrawProj_Inner_DoDrawProj(Main self, Projectile proj, Vector2 mountedCenter, float polePosX, float polePosY);

        void ILoadable.Load(Mod mod) {
            On_Main.DrawProj_Inner += On_Main_DrawProj_Inner;
        }

        private void On_Main_DrawProj_Inner(On_Main.orig_DrawProj_Inner orig, Main self, Projectile proj) {
            if (proj.IsModded(out ModProjectile modProjectile) && modProjectile is NatureProjectile_NoTextureLoad) {
                float polePosX = 0f;
                float polePosY = 0f;
                Vector2 mountedCenter = Main.player[proj.owner].MountedCenter;
                if (Main.player[proj.owner].mount.Active && Main.player[proj.owner].mount.Type == 52)
                    mountedCenter += new Vector2(Main.player[proj.owner].direction * 14, -10f);

                Main mainInstance = Main.instance;
                if (ProjectileLoader.PreDrawExtras(proj)) {
                    Main_DrawProj_DrawYoyoString(mainInstance, proj, mountedCenter);
                    Main_DrawProj_DrawExtras(mainInstance, proj, mountedCenter, ref polePosX, ref polePosY);
                }

                Main_DrawProj_Inner_DoDrawProj(mainInstance, proj, mountedCenter, polePosX, polePosY);

                return;
            }

            orig(self, proj);
        }

        void ILoadable.Unload() { }
    }

    public sealed override string Texture => ResourceManager.EmptyTexture;

    public sealed override void AutoStaticDefaults() {
        if (Projectile.hostile) {
            Main.projHostile[Type] = true;
        }
    }

    public sealed override bool PreDraw(ref Color lightColor) {
        Draw(ref lightColor);

        return false;
    }

    protected virtual void Draw(ref Color lightColor) { }
}
