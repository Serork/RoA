using Microsoft.Xna.Framework;

using RoA.Common.Druid.Claws;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.VisualEffects;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Utilities;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class HellfireClawsSlash : ClawsSlash {
    private int _hitTimer, _oldTimeleft;
    private int _oldItemUse, _oldItemAnimation;
    private int _hitAmount;

    public bool Charged { get; private set; } = true;

    private bool Hit => _hitTimer > 0;

    public override string Texture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<ClawsSlash>()).Texture;

    protected override void SafeOnSpawn(IEntitySource source) {
        base.SafeOnSpawn(source);

        Projectile.localNPCHitCooldown = -2;
        Projectile.usesOwnerMeleeHitCD = false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        if (!Charged) {
            return;
        }
        _hitAmount++;
        target.immune[Projectile.owner] = 0;
        Projectile.localNPCImmunity[target.whoAmI] = 10 + _hitAmount;
        if (!Hit) {
            _oldTimeleft = Projectile.timeLeft;
            _hitTimer = 10;
            _oldItemUse = Owner.itemTime;
            _oldItemAnimation = Owner.itemAnimation;
            //UpdateMainCycle();
        }
    }

    protected override void UpdateMainCycle() {
        if (Hit) {
            return;
        }

        base.UpdateMainCycle();
    }

    public override void PostAI() {
        ClawsHandler clawsStats = Owner.GetModPlayer<ClawsHandler>();
        float fromValue = Helper.EaseInOut3(Projectile.localAI[0] / Projectile.ai[1]);
        Color color1 = Color.Lerp(new Color(255, 150, 20), new Color(137, 54, 6), fromValue),
              color2 = Color.Lerp(new Color(200, 80, 10), new Color(96, 36, 4), fromValue);
        clawsStats.SetColors(color1, color2);

        if (Hit) {
            _hitTimer--;
            if (_hitTimer <= 0) {
                for (int i = 0; i < 200; i++) {
                    if (!Main.npc[i].active)
                        continue;

                    bool flag5 = Projectile.usesLocalNPCImmunity && Projectile.localNPCImmunity[i] == 0;
                    if (!((!Main.npc[i].dontTakeDamage || NPCID.Sets.ZappingJellyfish[Main.npc[i].type]) && flag5))
                        continue;

                    Projectile.localNPCImmunity[i] = 0;
                }
            }
            Projectile.timeLeft = _oldTimeleft;
            Owner.itemTime = _oldItemUse;
            Owner.itemAnimation = _oldItemAnimation;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        float fromValue = Ease.QuintIn(Projectile.localAI[0] / Projectile.ai[1]);
        Color color1 = Color.Lerp(new Color(255, 150, 20), new Color(137, 54, 6), fromValue),
              color2 = Color.Lerp(new Color(200, 80, 10), new Color(96, 36, 4), fromValue);
        float angle = MathHelper.PiOver2;
        Vector2 offset = new(0.2f);
        Vector2 velocity = 1.5f * offset;
        Vector2 position = Main.rand.NextVector2Circular(4f, 4f) * offset;
        Color color = Color.Lerp(color1, color2, Main.rand.NextFloat())/*Lighting.GetColor(target.Center.ToTileCoordinates()).MultiplyRGB(Color.Lerp(FirstSlashColor, SecondSlashColor, Main.rand.NextFloat()))*/;
        color.A = 50;
        position = target.Center + target.velocity + position + Main.rand.NextVector2Circular(target.width / 3f, target.height / 3f);
        velocity = angle.ToRotationVector2() * velocity * 0.5f;
        int layer = VisualEffectLayer.ABOVENPCS;
        VisualEffectSystem.New<ClawsSlashHit>(layer).
            Setup(position,
                  velocity,
                  color);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new VisualEffectSpawnPacket(VisualEffectSpawnPacket.VisualEffectPacketType.ClawsHit, Owner, layer, position, velocity, color, 1f, 0f));
        }
    }
}
