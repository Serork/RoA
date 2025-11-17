using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Content.Buffs;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.Collections.Generic;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class GodFeather : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 600;
    public static ushort DEBUFFTIME => 60;

    public enum GodFeatherRequstedTextureType : byte {
        Base,
        Glow,
        Eye,
        Eye2
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)GodFeatherRequstedTextureType.Base, ResourceManager.NatureProjectileTextures + "GodFeather"),
         ((byte)GodFeatherRequstedTextureType.Glow, ResourceManager.NatureProjectileTextures + "GodFeather_Glow"),
         ((byte)GodFeatherRequstedTextureType.Eye, ResourceManager.NatureProjectileTextures + "GodFeather_Eye"),
         ((byte)GodFeatherRequstedTextureType.Eye2, ResourceManager.NatureProjectileTextures + "GodFeather_Eye2")];

    public ref float WaveValue => ref Projectile.localAI[1];
    public ref float RotationLerpValue => ref Projectile.localAI[2];

    public ref float ActivatedValue => ref Projectile.ai[0];
    public ref float Activated2Value => ref Projectile.ai[1];

    public bool Activated {
        get => ActivatedValue > 0f;
        set {
            if (ActivatedValue < 0.5f) {
                ActivatedValue = 1f;
            }
        }
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;

        Projectile.manualDirectionChange = true;

        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCs.Add(index);
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void AI() {
        ActivatedValue = Helper.Approach(ActivatedValue, 0f, TimeSystem.LogicDeltaTime);
        Activated2Value = Helper.Approach(Activated2Value, ActivatedValue, TimeSystem.LogicDeltaTime * 5f);

        Player owner = Projectile.GetOwnerAsPlayer();
        owner.SyncMousePosition();
        Vector2 mousePosition = owner.GetWorldMousePosition();
        Vector2 center = owner.Center;
        float distance = 50f;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, center.DirectionTo(mousePosition) * distance, 0.05f);
        Projectile.Center = Utils.Floor(center) + Projectile.velocity + Vector2.UnitY * owner.gfxOffY;

        int direction = Projectile.DirectionTo(owner.Center).X.GetDirection();
        Projectile.SetDirection(direction);

        RotationLerpValue = Helper.Approach(RotationLerpValue, 0.0375f * (1f + 0.5f * Activated2Value) * Projectile.direction, TimeSystem.LogicDeltaTime);
        float rotationValue = RotationLerpValue;
        Projectile.rotation += rotationValue;

        WaveValue += TimeSystem.LogicDeltaTime;

        int minDistance = (int)TileHelper.TileSize * 8;
        foreach (NPC activeNPC in Main.ActiveNPCs) {
            if (!activeNPC.CanBeChasedBy()) {
                continue;
            }
            if (activeNPC.Distance(Projectile.Center) < minDistance) {
                Activated = true;
                activeNPC.AddBuff<GodDescent>(DEBUFFTIME);
            }
        }
        // TODO: pvp support?
        foreach (Player activePlayer in Main.ActivePlayers) {
            if (activePlayer.whoAmI == owner.whoAmI) {
                continue;
            }
            if (activePlayer.Distance(Projectile.Center) < minDistance) {
                Activated = true;
                activePlayer.AddBuff<GodDescent>(DEBUFFTIME);
            }
        }
        minDistance = (int)TileHelper.TileSize * 3;
        foreach (Projectile activeProjectile in Main.ActiveProjectiles) {
            if (activeProjectile.type == Type) {
                continue;
            }
            if (activeProjectile.friendly) {
                continue;
            }
            if (activeProjectile.Distance(Projectile.Center) < minDistance) {
                Activated = true;
                activeProjectile.GetGlobalProjectile<GodDescent.GodDescent_ProjectileHandler>().IsEffectActive = true;
            }
        }

        float spawnProgress = Ease.CircOut(Utils.GetLerpValue(TIMELEFT, TIMELEFT - 20, Projectile.timeLeft, true));
        int count = 8;
        for (int i = 0; i < count; i++) {
            float rotation = Utils.AngleLerp(Projectile.velocity.ToRotation() - MathHelper.PiOver2, (float)i / count * MathHelper.TwoPi, spawnProgress);
            rotation += Utils.AngleLerp(Projectile.rotation, 0f, 1f - spawnProgress);
            Vector2 position = Projectile.Center;
            float distance2 = 14f;
            position += Vector2.UnitY.RotatedBy(rotation) * distance2;
            float fadeOutProgress = Utils.GetLerpValue(0, 25, Projectile.timeLeft, true);
            float fadeOutProgress3 = Utils.GetLerpValue(0f, 0.375f, fadeOutProgress, true);
            Lighting.AddLight(position, Color.LightYellow.ToVector3() * spawnProgress * MathHelper.Lerp(0.5f, 0.75f, Activated2Value) * fadeOutProgress3);
        }
    }

    public override void OnKill(int timeLeft) {
        
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<GodFeather>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        float spawnProgress = Ease.CircOut(Utils.GetLerpValue(TIMELEFT, TIMELEFT - 20, Projectile.timeLeft, true));
        float fadeOutProgress = Utils.GetLerpValue(0, 30, Projectile.timeLeft, true);
        float fadeOutProgress2 = Utils.GetLerpValue(0.75f, 0.2f, fadeOutProgress, true);
        float fadeOutProgress3 = Utils.GetLerpValue(0f, 0.375f, fadeOutProgress, true);
        float fadeOutProgress4 = Utils.GetLerpValue(0f, 0.2f, fadeOutProgress, true);
        float lightingModifier = MathHelper.Lerp(0.375f, 0.625f, Activated2Value);

        Texture2D baseTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Base].Value,
                  glowTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Glow].Value,
                  closedEyeTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Eye].Value;
        SpriteBatch batch = Main.spriteBatch;
        int count = 8;
        float waveOffset = Projectile.whoAmI;
        for (int i = 0; i < count; i++) {
            float rotation = Utils.AngleLerp(Projectile.velocity.ToRotation() - MathHelper.PiOver2, (float)i / count * MathHelper.TwoPi, spawnProgress);
            rotation += Utils.AngleLerp(Projectile.rotation, 0f, 1f - spawnProgress);
            Vector2 position = Projectile.Center;
            float distance = 6f;
            position += Vector2.UnitY.RotatedBy(rotation) * distance;
            Rectangle clip = baseTexture.Bounds;
            Vector2 origin = clip.BottomCenter();
            Color getColor(bool glow = false) {
                Color color = Color.Lerp(Lighting.GetColor(position.ToTileCoordinates()), Color.White, lightingModifier * glow.ToInt()) * 0.875f;
                color.A = (byte)MathHelper.Lerp(255, 185, Activated2Value);
                color.A = (byte)MathHelper.Lerp(color.A, 100, fadeOutProgress2);
                color *= spawnProgress;
                color *= fadeOutProgress3;
                return color;
            }
            Color color = getColor();
            rotation += MathHelper.Pi;
            Vector2 scale = Vector2.One * fadeOutProgress4;
            DrawInfo drawInfo = DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Rotation = rotation,
                Scale = scale
            };

            float maxRotation = 0.3f;

            bool second = i % 2 == 0;
            if (second) {
                batch.Draw(glowTexture, position, drawInfo with {
                    Scale = scale * 1.5f,
                    Color = Color.LightYellow.MultiplyRGB(getColor(true)).MultiplyAlpha(Helper.Wave(WaveValue, 0.25f, 0.75f, 10f, waveOffset)) * 0.5f * spawnProgress * 0.375f,
                    Rotation = rotation + Helper.Wave(WaveValue, -maxRotation, maxRotation, 5f, waveOffset)
                });
            }
            batch.Draw(baseTexture, position, drawInfo with {
                Scale = scale * Helper.Wave(WaveValue, 0.9f, 1.1f, 2.5f, waveOffset + i * count)
            });
            batch.Draw(glowTexture, position, drawInfo with {
                Color = getColor(true) * 0.5f,
                Scale = scale * Helper.Wave(WaveValue, 0.9f, 1.1f, 2.5f, waveOffset + i * count)
            });
            if (!second) {
                batch.Draw(glowTexture, position, drawInfo with {
                    Scale = scale * 1.5f * Helper.Wave(WaveValue, 0.75f, 1.25f, 5f, waveOffset + i * count),
                    Color = getColor(true).MultiplyAlpha(Helper.Wave(WaveValue, 0.25f, 0.75f, 10f, waveOffset)) * 0.625f * spawnProgress * 0.375f,
                    Rotation = rotation + MathHelper.PiOver4 + Helper.Wave(WaveValue, -maxRotation, maxRotation, 5f, waveOffset)
                });
            }
        }

        for (int i = 0; i < count; i++) {
            float rotation = Utils.AngleLerp(Projectile.velocity.ToRotation() - MathHelper.PiOver2, (float)i / count * MathHelper.TwoPi, spawnProgress);
            rotation += Utils.AngleLerp(Projectile.rotation, 0f, 1f - spawnProgress);
            Vector2 position = Projectile.Center;
            float distance = 6f;
            position += Vector2.UnitY.RotatedBy(rotation) * distance;
            Rectangle clip = baseTexture.Bounds;
            Vector2 origin = clip.BottomCenter();
            Color color = Color.Lerp(new Color(61, 72, 73), Color.Yellow, Activated2Value).MultiplyRGB(Color.Lerp(Lighting.GetColor(position.ToTileCoordinates()), Color.White, lightingModifier));
            color.A = (byte)MathHelper.Lerp(color.A, 100, fadeOutProgress2);
            color *= spawnProgress;
            color *= fadeOutProgress3;
            rotation += MathHelper.Pi;
            Vector2 scale = Vector2.One * fadeOutProgress4;
            DrawInfo drawInfo = DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Rotation = rotation,
                Scale = scale
            };
            origin += Vector2.UnitY * -16f;
            position += Vector2.UnitY.RotatedBy(rotation) * 25.5f;
            scale *= Helper.Wave(WaveValue, 1f, 1.5f, 5f, waveOffset + i * count);
            batch.Draw(closedEyeTexture, position, drawInfo with {
                Scale = scale,
                Origin = origin
            });
        }
    }
}
