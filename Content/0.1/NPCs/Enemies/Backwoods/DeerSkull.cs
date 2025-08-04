using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.WorldEvents;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class DeerSkullHead : BaseHead {
    public override int BodyType => ModContent.NPCType<DeerSkullBody>();

    public override int TailType => ModContent.NPCType<DeerSkullTail>();

    public override void SetStaticDefaults() {
        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            CustomTexturePath = ResourceManager.BestiaryTextures + "DeerSkull_Bestiary",
            Position = new Vector2(70f, 10f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = 0f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
    }

    public override void SetDefaults() {
        NPC.lifeMax = 130;
        NPC.damage = 40;
        NPC.defense = 14;
        NPC.knockBackResist = 0f;

        int width = 50; int height = 50;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        NPC.npcSlots = 0.3f;
        NPC.value = Item.buyPrice(0, 0, 35, 0);

        NPC.HitSound = SoundID.NPCHit2;
        NPC.DeathSound = SoundID.NPCDeath2;

        NPC.noTileCollide = true;
        NPC.noGravity = true;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type, ModContent.GetInstance<BackwoodsBiomeFog>().Type];

        Banner = Type;
        BannerItem = ModContent.ItemType<DeerSkullBanner>();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.DeerSkull"),
            new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<BackwoodsBiomeFog>().ModBiomeBestiaryInfoElement)
        ]);
    }

    internal override void Init() {
        MinSegmentLength = 10;
        MaxSegmentLength = 10;
    }

    private float _aiTimer1, _aiTimer2;

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_aiTimer1);
        writer.Write(_aiTimer2);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _aiTimer1 = reader.ReadSingle();
        _aiTimer2 = reader.ReadSingle();
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if (NPC.life > 0) {
            for (int num681 = 0; (double)num681 < hit.Damage / (double)NPC.lifeMax * 50.0; num681++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, hit.HitDirection, -1f);
            }

            return;
        }

        for (int num682 = 0; num682 < 20; num682++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, 2.5f * (float)hit.HitDirection, -2.5f);
        }

        if (!Main.dedServ) {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "DeerSkullGore1".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "DeerSkullGore2".GetGoreType(), Scale: NPC.scale);
        }
    }

    public override void PostAI() {
        if (NPC.alpha > 0) {
            NPC.alpha -= 30;
            if (NPC.alpha < 0)
                NPC.alpha = 0;
        }
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0f;

        float dist = Math.Abs(Main.player[NPC.target].Center.X - NPC.Center.X);
        float value = Utils.Remap(dist, 250f, 1000f, 1f, 0.85f, true);
        NPC.velocity.X *= value;

        float num1376 = 120f;
        if (NPC.localAI[0] < num1376) {
            if (NPC.localAI[0] == 0f) {
                //SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                NPC.TargetClosest();
                if (NPC.direction > 0)
                    NPC.velocity.X += 2f;
                else
                    NPC.velocity.X -= 2f;
            }

            NPC.localAI[0] += 1f;
        }

        if (_aiTimer1 == 0f) {
            NPC.TargetClosest();
            _aiTimer1 = 1f;
            _aiTimer2 = NPC.direction;
        }
        else if (_aiTimer1 == 1f) {
            NPC.TargetClosest();
            float num1380 = 0.2f;
            float num1381 = 5f;
            float num1382 = 4f;
            float num1383 = 300f;
            float num1384 = 30f;

            NPC.velocity.X += _aiTimer2 * num1380;
            if (NPC.velocity.X > num1381)
                NPC.velocity.X = num1381;

            if (NPC.velocity.X < 0f - num1381)
                NPC.velocity.X = 0f - num1381;

            float num1385 = Main.player[NPC.target].Center.Y - NPC.Center.Y;
            if (Math.Abs(num1385) > num1382)
                num1384 = 25f;

            if (num1385 > num1382)
                num1385 = num1382;
            else if (num1385 < 0f - num1382)
                num1385 = 0f - num1382;

            NPC.velocity.Y = (NPC.velocity.Y * (num1384 - 1f) + num1385) / num1384;
            if ((_aiTimer2 > 0f && Main.player[NPC.target].Center.X - NPC.Center.X < 0f - num1383) || (_aiTimer2 < 0f && Main.player[NPC.target].Center.X - NPC.Center.X > num1383)) {
                _aiTimer1 = 2f;
                _aiTimer2 = 0f;
                if (NPC.Center.Y + 20f > Main.player[NPC.target].Center.Y)
                    _aiTimer2 = -1f;
                else
                    _aiTimer2 = 1f;
            }
        }
        else if (_aiTimer1 == 2f) {
            float num1386 = 0.2f;
            float num1387 = 0.9f;
            float num1388 = 5f;

            NPC.velocity.Y += _aiTimer2 * num1386;
            NPC.velocity.X *= 0.95f;
            if (NPC.velocity.Length() > num1388)
                NPC.velocity *= num1387;

            if (NPC.velocity.X > -1f && NPC.velocity.X < 1f) {
                NPC.TargetClosest();
                _aiTimer1 = 3f;
                _aiTimer2 = NPC.direction;
            }
        }
        else if (_aiTimer1 == 3f) {
            float num1389 = 0.3f;
            float num1390 = 0.1f;
            float num1391 = 4f;
            float num1392 = 0.95f;

            NPC.velocity.X += _aiTimer2 * num1389;
            if (NPC.Center.Y > Main.player[NPC.target].Center.Y)
                NPC.velocity.Y -= num1390;
            else
                NPC.velocity.Y += num1390;

            if (NPC.velocity.Length() > num1391)
                NPC.velocity *= num1392;

            if (NPC.velocity.Y > -1f && NPC.velocity.Y < 1f) {
                NPC.TargetClosest();
                _aiTimer1 = 0f;
                _aiTimer2 = NPC.direction;
            }
        }

        Player player = Main.player[NPC.target];
        if (!player.InModBiome<BackwoodsBiome>() || !BackwoodsFogHandler.IsFogActive) {
            _aiTimer1 = 0f;
            NPC.velocity.Y += 0.1f;
            NPC.velocity.Y = Math.Min(10f, NPC.velocity.Y);
            NPC.velocity.X -= (player.Center - NPC.Center).X.GetDirection() * 0.1f;
            NPC.velocity.X = Math.Clamp(NPC.velocity.X, -5f, -5f);
        }

        NPC.direction = NPC.velocity.X.GetDirection();
        NPC.rotation = Utils.AngleLerp(NPC.rotation, Helper.VelocityAngle(NPC.velocity), 0.1f);
    }
}

sealed class DeerSkullBody : BaseBody {
    public override void SetStaticDefaults() {
        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Hide = true
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
    }

    public override void SetDefaults() {
        NPC.CloneDefaults(NPCID.DiggerBody);
        NPC.aiStyle = -1;
        NPC.behindTiles = false;
        NPC.dontTakeDamage = true;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if (NPC.life > 0) {
            for (int num681 = 0; (double)num681 < hit.Damage / (double)NPC.lifeMax * 50.0; num681++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, hit.HitDirection, -1f);
            }

            return;
        }

        for (int num682 = 0; num682 < 5; num682++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, 2.5f * (float)hit.HitDirection, -2.5f);
        }

        if (!Main.dedServ) {
            int num = (int)NPC.ai[2];
            bool rib = num >= 5 && num <= 7;
            string gore = rib ? "DeerSkullGore32" : "DeerSkullGore3";
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, gore.GetGoreType(), Scale: NPC.scale);
        }
    }
}

sealed class DeerSkullTail : BaseTail {
    public override void SetStaticDefaults() {
        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Hide = true
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
    }

    public override void SetDefaults() {
        NPC.CloneDefaults(NPCID.DiggerTail);
        NPC.aiStyle = -1;
        NPC.behindTiles = false;
        NPC.dontTakeDamage = true;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if (NPC.life > 0) {
            for (int num681 = 0; (double)num681 < hit.Damage / (double)NPC.lifeMax * 50.0; num681++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, hit.HitDirection, -1f);
            }

            return;
        }

        for (int num682 = 0; num682 < 5; num682++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, 2.5f * (float)hit.HitDirection, -2.5f);
        }

        if (!Main.dedServ) {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "DeerSkullGore4".GetGoreType(), Scale: NPC.scale);
        }
    }
}

#region BASE

public enum WormSegmentType {
    Head,
    Body,
    Tail
}

public abstract class Worm : ModNPC {
    public abstract WormSegmentType SegmentType { get; }

    public NPC HeadSegment => Main.npc[NPC.realLife];
    public NPC FollowingNPC => SegmentType == WormSegmentType.Head ? null : Main.npc[(int)NPC.ai[1]];
    public NPC FollowerNPC => SegmentType == WormSegmentType.Tail ? null : Main.npc[(int)NPC.ai[0]];

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => SegmentType == WormSegmentType.Head ? null : false;

    private bool _startDespawning;

    public sealed override bool PreAI() {
        NPC.aiStyle = -1;

        if (NPC.localAI[1] == 0) {
            NPC.localAI[1] = 1f;
            Init();
        }

        if (SegmentType == WormSegmentType.Head) {
            HeadAI();

            if (!NPC.HasValidTarget) {
                NPC.TargetClosest(true);
            }
        }
        else {
            BodyTailAI();
        }

        NPC.aiStyle = 6;

        return false;
    }

    public override void PostAI() {
        //NPC.aiStyle = 6;
    }

    internal virtual void HeadAI() { }

    internal virtual void BodyTailAI() { }

    internal virtual void Init() { }
}

public abstract class BaseHead : Worm {
    public sealed override WormSegmentType SegmentType => WormSegmentType.Head;

    public abstract int BodyType { get; }

    public abstract int TailType { get; }

    public int MinSegmentLength { get; set; }

    public int MaxSegmentLength { get; set; }

    public Vector2? ForcedTargetPosition { get; set; }

    public virtual int SpawnBodySegments(int segmentCount) => NPC.whoAmI;

    protected int SpawnSegment(IEntitySource source, int type, int latestNPC, int distance) {
        int oldLatest = latestNPC;
        latestNPC = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI, 0, latestNPC, distance);

        Main.npc[oldLatest].ai[0] = latestNPC;

        NPC latest = Main.npc[latestNPC];
        latest.realLife = NPC.whoAmI;

        return latestNPC;
    }

    internal sealed override void HeadAI() {
        HeadAI_SpawnSegments();

        HeadAI_Movement(false);
    }

    private void HeadAI_SpawnSegments() {
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            bool hasFollower = NPC.ai[0] > 0;
            if (!hasFollower) {
                NPC.realLife = NPC.whoAmI;
                int latestNPC = NPC.whoAmI;

                int randomWormLength = Main.rand.Next(MinSegmentLength, MaxSegmentLength + 1);

                int distance = randomWormLength - 2;

                IEntitySource source = NPC.GetSource_FromAI();

                // Spawn the body segments like usual
                while (distance > 0) {
                    latestNPC = SpawnSegment(source, BodyType, latestNPC, distance);
                    distance--;
                }

                SpawnSegment(source, TailType, latestNPC, distance);

                NPC.netUpdate = true;

                int count = 0;
                foreach (var n in Main.ActiveNPCs) {
                    if ((n.type == Type || n.type == BodyType || n.type == TailType) && n.realLife == NPC.whoAmI)
                        count++;
                }

                if (count != randomWormLength) {
                    // Unable to spawn all of the segments... kill the worm
                    foreach (var n in Main.ActiveNPCs) {
                        if ((n.type == Type || n.type == BodyType || n.type == TailType) && n.realLife == NPC.whoAmI) {
                            n.active = false;
                            n.netUpdate = true;
                        }
                    }
                }

                NPC.TargetClosest(true);
            }
        }
    }

    private void HeadAI_Movement(bool collision) {

    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Vector2 drawPosition = NPC.Center - screenPos;
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Rectangle sourceRectangle = new(0, 0, 54, 30);
        SpriteEffects effects = (SpriteEffects)(NPC.velocity.X < 0f).ToInt();
        Vector2 origin = new Vector2(NPC.direction != 1 ? 20 : 34, 22);
        Main.EntitySpriteDraw(texture, drawPosition + new Vector2(5f * NPC.direction, 0f).RotatedBy(NPC.rotation), sourceRectangle,
            NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, origin, NPC.scale, effects);

        return false;
    }
}

public abstract class BaseBody : Worm {
    public sealed override WormSegmentType SegmentType => WormSegmentType.Body;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Vector2 drawPosition = NPC.Center - screenPos;
        Texture2D texture = TextureAssets.Npc[Type].Value;
        SpriteEffects effects = (SpriteEffects)(NPC.direction == -1).ToInt();
        Vector2 origin = new Vector2(54, 48) / 2f;
        int num = (int)NPC.ai[2];
        bool rib = num >= 5 && num <= 7;
        Rectangle sourceRectangle = new(rib ? 54 : 0, 0, 54, 48);
        if (rib && NPC.direction == -1) {
            effects = SpriteEffects.FlipVertically;
        }
        Main.EntitySpriteDraw(texture, drawPosition, sourceRectangle, NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation - MathHelper.PiOver2 * NPC.direction + (rib ? MathHelper.PiOver2 : 0f),
            origin, NPC.scale * (num <= 5 ? 0.85f : 1f),
            effects);

        return false;
    }

    internal override void BodyTailAI() {
        CommonAI_BodyTail(this);
    }

    internal static void CommonAI_BodyTail(Worm worm) {
        if (!worm.NPC.HasValidTarget)
            worm.NPC.TargetClosest(true);

        if (Main.player[worm.NPC.target].dead && worm.NPC.timeLeft > 30000)
            worm.NPC.timeLeft = 10;

        NPC following = worm.NPC.ai[1] >= Main.maxNPCs ? null : worm.FollowingNPC;
        if (following is null || !following.active || following.friendly || following.townNPC || following.lifeMax <= 5) {
            worm.NPC.life = 0;
            worm.NPC.HitEffect(0, 10);
            worm.NPC.active = false;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, worm.NPC.whoAmI, 0f, 0f, 0f, 0, 0, 0);
            }
        }

        if (following is not null) {
            float dirX = following.Center.X - worm.NPC.Center.X;
            float dirY = following.Center.Y - worm.NPC.Center.Y;
            worm.NPC.rotation = (float)Math.Atan2(dirY, dirX) + MathHelper.PiOver2;
            float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
            int num = (int)worm.NPC.ai[2];
            bool rib = num >= 5 && num <= 7;
            float dist = (length - worm.NPC.width * (worm is DeerSkullTail ? 0.875f : rib ? 0.85f : (num == 8 ? 0.6f : num > 5 ? 0.75f : 0.715f))) / length;
            float posX = dirX * dist;
            float posY = dirY * dist;

            worm.NPC.direction = dirX.GetDirection();

            worm.NPC.velocity = Vector2.Zero;
            worm.NPC.position.X += posX;
            worm.NPC.position.Y += posY;
        }
    }
}

public abstract class BaseTail : Worm {
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Vector2 drawPosition = NPC.Center - screenPos;
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Rectangle sourceRectangle = new(0, 0, 54, 48);
        SpriteEffects effects = (SpriteEffects)(NPC.direction == -1).ToInt();
        Vector2 origin = texture.Size() / 2f;
        Main.EntitySpriteDraw(texture, drawPosition, sourceRectangle, NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, origin, NPC.scale, effects);

        return false;
    }

    public sealed override WormSegmentType SegmentType => WormSegmentType.Tail;

    internal override void BodyTailAI() {
        BaseBody.CommonAI_BodyTail(this);
    }
}
#endregion