using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Projectiles.Friendly;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

// same as InteractableProjectile.cs
abstract class InteractableProjectile_Nature : NatureProjectile {
    protected virtual Vector2 DrawOffset { get; }

    protected virtual SpriteEffects SetSpriteEffects() => Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

    protected abstract Asset<Texture2D> HoverTexture { get; }

    public override void Load() {
        On_Projectile.IsInteractible += On_Projectile_IsInteractable;
    }

    private bool On_Projectile_IsInteractable(On_Projectile.orig_IsInteractible orig, Projectile self) {
        if (self.type >= ProjectileID.Count && self.ModProjectile is InteractableProjectile_Nature) {
            return true;
        }

        return orig(self);
    }

    protected abstract void OnInteraction(Player player);

    protected abstract void OnHover(Player player);

    public sealed override void AI() {
        Main.CurrentFrameFlags.HadAnActiveInteractibleProjectile = true;

        SafeAI();
    }

    public virtual void SafeAI() { }

    public sealed override void PostDraw(Color lightColor) {
        int num417 = TryInteractingWithMe();
        if (num417 == 0)
            return;

        int num418 = (lightColor.R + lightColor.G + lightColor.B) / 3;
        if (num418 > 10) {
            Color selectionGlowColor = Colors.GetSelectionGlowColor(num417 == 2, num418);
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D texture = HoverTexture.Value;
            int height = texture.Height / Main.projFrames[Projectile.type];
            Vector2 drawOrigin = new(texture.Width / 2f, height * 0.5f);
            Vector2 position = Projectile.position + drawOrigin - Main.screenPosition;
            Rectangle rect = new(0, height * Projectile.frame, texture.Width, height);
            spriteBatch.Draw(texture, position + DrawOffset, rect, selectionGlowColor, Projectile.rotation, drawOrigin, Projectile.scale, SetSpriteEffects(), 0);
            spriteBatch.EndBlendState();
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindProjectiles.Add(index);
    }

    protected int TryInteractingWithMe() {
        if (Main.gamePaused || Main.gameMenu)
            return 0;

        bool flag = !Main.SmartCursorIsUsed && !PlayerInput.UsingGamepad;
        Projectile proj = Projectile;
        Player localPlayer = Main.player[proj.owner];
        if (localPlayer.whoAmI != Main.myPlayer) {
            return 0;
        }
        if (Main.HoveringOverAnNPC) {
            return 0;
        }
        Microsoft.Xna.Framework.Point point = proj.Center.ToTileCoordinates();
        Vector2 compareSpot = localPlayer.Center;
        if (!localPlayer.IsProjectileInteractibleAndInInteractionRange(proj, ref compareSpot))
            return 0;

        bool flag2 = proj.Hitbox.Contains(localPlayer.GetViableMousePosition().ToPoint());
        if (!((flag2 || Main.SmartInteractProj == proj.whoAmI) & !localPlayer.lastMouseInterface)) {
            if (!flag)
                return 1;

            return 0;
        }

        Main.HasInteractibleObjectThatIsNotATile = true;
        if (flag2) {
            OnHover(localPlayer);
        }

        if (PlayerInput.UsingGamepad)
            localPlayer.GamepadEnableGrappleCooldown();

        if (Main.mouseRight && Main.mouseRightRelease && Player.BlockInteractionWithProjectiles == 0) {
            Main.mouseRightRelease = false;
            localPlayer.tileInteractAttempted = true;
            localPlayer.tileInteractionHappened = true;
            localPlayer.releaseUseTile = false;
            OnInteraction(localPlayer);
        }

        if (!Main.SmartCursorIsUsed && !PlayerInput.UsingGamepad)
            return 0;

        if (!flag)
            return 2;

        return 0;
    }
}

abstract class InteractableProjectile : ModProjectile {
    protected virtual Vector2 DrawOffset { get; }

    protected virtual SpriteEffects SetSpriteEffects() => Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

    protected abstract Asset<Texture2D> HoverTexture { get; }

    public override void Load() {
        On_Projectile.IsInteractible += On_Projectile_IsInteractable;
    }

    private bool On_Projectile_IsInteractable(On_Projectile.orig_IsInteractible orig, Projectile self) {
        if (self.type >= ProjectileID.Count && self.ModProjectile is InteractableProjectile) {
            return true;
        }

        return orig(self);
    }

    protected abstract void OnInteraction(Player player);

    protected abstract void OnHover(Player player);

    public sealed override void AI() {
        Main.CurrentFrameFlags.HadAnActiveInteractibleProjectile = true;

        SafeAI();
    }

    public virtual void SafeAI() { }

    public sealed override void PostDraw(Color lightColor) {
        int num417 = TryInteractingWithMe();
        if (num417 == 0)
            return;

        int num418 = (lightColor.R + lightColor.G + lightColor.B) / 3;
        if (num418 > 10) {
            Color selectionGlowColor = Colors.GetSelectionGlowColor(num417 == 2, num418);
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D texture = HoverTexture.Value;
            int height = texture.Height / Main.projFrames[Projectile.type];
            Vector2 drawOrigin = new(texture.Width / 2f, height * 0.5f);
            Vector2 position = Projectile.position + drawOrigin - Main.screenPosition;
            Rectangle rect = new(0, height * Projectile.frame, texture.Width, height);
            spriteBatch.Draw(texture, position + DrawOffset, rect, selectionGlowColor, Projectile.rotation, drawOrigin, Projectile.scale, SetSpriteEffects(), 0);
            spriteBatch.EndBlendState();
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindProjectiles.Add(index);
    }

    protected int TryInteractingWithMe() {
        if (Main.gamePaused || Main.gameMenu)
            return 0;

        bool flag = !Main.SmartCursorIsUsed && !PlayerInput.UsingGamepad;
        Projectile proj = Projectile;
        Player localPlayer = Main.player[proj.owner];
        if (localPlayer.whoAmI != Main.myPlayer) {
            return 0;
        }
        if (Main.HoveringOverAnNPC) {
            return 0;
        }
        Microsoft.Xna.Framework.Point point = proj.Center.ToTileCoordinates();
        Vector2 compareSpot = localPlayer.Center;
        if (!localPlayer.IsProjectileInteractibleAndInInteractionRange(proj, ref compareSpot))
            return 0;

        bool flag2 = proj.Hitbox.Contains(localPlayer.GetViableMousePosition().ToPoint());
        if (!((flag2 || Main.SmartInteractProj == proj.whoAmI) & !localPlayer.lastMouseInterface)) {
            if (!flag)
                return 1;

            return 0;
        }

        Main.HasInteractibleObjectThatIsNotATile = true;
        if (flag2) {
            OnHover(localPlayer);
        }

        if (PlayerInput.UsingGamepad)
            localPlayer.GamepadEnableGrappleCooldown();

        if (Main.mouseRight && Main.mouseRightRelease && Player.BlockInteractionWithProjectiles == 0) {
            Main.mouseRightRelease = false;
            localPlayer.tileInteractAttempted = true;
            localPlayer.tileInteractionHappened = true;
            localPlayer.releaseUseTile = false;
            OnInteraction(localPlayer);
        }

        if (!Main.SmartCursorIsUsed && !PlayerInput.UsingGamepad)
            return 0;

        if (!flag)
            return 2;

        return 0;
    }
}
