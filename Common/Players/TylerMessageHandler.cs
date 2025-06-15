using Microsoft.Xna.Framework;

using RoA.Core;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class TylerMessageHandler : ModPlayer {
    public enum MessageSource : byte {
        Idle,
        Death,
        AlmostDeath,
        Explosive,
        RedPotion,
        KilledBunny,
        OnSell,
        OnBuy,
        OnCall,
        Special
    }

    private byte _variation;
    private Vector2 _messageVelocity;
    private int[] _messageCooldownsByType;

    private static string GetCategoryName(MessageSource source) {
        string id;
        switch (source) {
            case MessageSource.Special:
                id = "Special";
                break;
            case MessageSource.Death:
                id = "Death";
                break;
            case MessageSource.AlmostDeath:
                id = "AlmostDeath";
                break;
            case MessageSource.Explosive:
                id = "Explosive";
                break;
            case MessageSource.RedPotion:
                id = "RedPotion";
                break;
            case MessageSource.KilledBunny:
                id = "KilledBunny";
                break;
            case MessageSource.OnSell:
                id = "OnSell";
                break;
            case MessageSource.OnBuy:
                id = "OnBuy";
                break;
            case MessageSource.OnCall:
                id = "OnCall";
                break;
            default:
                id = "Idle";
                break;
        }

        return Language.GetText($"Mods.RoA.TylerMessages.{id}").Value;
    }

    public override void OnEnterWorld() {
        _messageVelocity = new Vector2(0f, -8f);
        _messageCooldownsByType = new int[Enum.GetNames(typeof(MessageSource)).Length];

        GiveIdleMessageCooldown();
    }

    public void UpdateMessageCooldowns() {
        for (int index = 0; index < _messageCooldownsByType.Length; ++index) {
            if (_messageCooldownsByType[index] > 0) {
                --_messageCooldownsByType[index];
            }
        }
    }

    public void TryPlayingIdleMessage() {
        MessageSource source = MessageSource.Idle;
        if (_messageCooldownsByType[(int)source] > 0) {
            return;
        }

        Create(source, Player.Top, _messageVelocity);
    }

    private void GiveIdleMessageCooldown() => PutMessageTypeOnCooldown(MessageSource.Idle, Main.rand.Next(5800, 7200));

    public void PutMessageTypeOnCooldown(MessageSource source, int timeInFrames) => _messageCooldownsByType[(int)source] = timeInFrames;

    public void TryCreatingMessageWithCooldown(MessageSource messageSource, Vector2 position, Vector2 velocity, int cooldownTimeInTicks) {
        if (Main.netMode == NetmodeID.Server || _messageCooldownsByType[(int)messageSource] > 0) {
            return;
        }

        PutMessageTypeOnCooldown(messageSource, cooldownTimeInTicks);
        Create(messageSource, position, velocity);
    }

    public void Create(MessageSource source, Vector2 position, Vector2 velocity) {
        if (Main.netMode == NetmodeID.Server || !IsTylerSet()) {
            return;
        }

        GiveIdleMessageCooldown();
        SpawnPopupText(source, _variation, position, velocity);
        SpawnEmoteBubble();
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.MiscSounds + "hahahahahaha") with { Volume = 0.6f }, position);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendData(MessageID.RequestLucyPopup, number: (int)source, number2: _variation, number3: velocity.X, number4: velocity.Y, number5: (int)position.X, number6: (int)position.Y);
        }
        ++_variation;
    }

    private void SpawnEmoteBubble() {
        EmoteBubble.NewBubble(EmoteID.EmoteLaugh, new WorldUIAnchor(Player), 360);
        EmoteBubble.CheckForNPCsToReactToEmoteBubble(EmoteID.EmoteLaugh, Player);

        if (Main.netMode != NetmodeID.SinglePlayer) {
            NetMessage.SendData(MessageID.Emoji, -1, -1, null, Main.myPlayer, EmoteID.EmoteLaugh);
        }
    }

    public void CreateFromNet(MessageSource source, byte variation, Vector2 position, Vector2 velocity) => SpawnPopupText(source, variation, position, velocity);

    private void SpawnPopupText(MessageSource source, int variationUnwrapped, Vector2 position, Vector2 velocity) {
        string tylersQuote = GetCategoryName(source);
        PopupText.NewText(new AdvancedPopupRequest() {
            Text = tylersQuote,
            DurationInFrames = 300,
            Velocity = velocity,
            Color = new Color(255, 80, 200)
        }, position);
    }

    public bool IsTylerSet() {
        int top = ModContent.ItemType<Content.Items.Equipables.Vanity.SoapSellersShades>();
        int mid = ModContent.ItemType<Content.Items.Equipables.Vanity.SoapSellersJacket>();
        int last = ModContent.ItemType<Content.Items.Equipables.Vanity.SoapSellersJeans>();
        if ((Player.armor[10].type == top || Player.armor[0].type == top) &&
            (Player.armor[11].type == mid || Player.armor[1].type == mid) &&
            (Player.armor[12].type == last || Player.armor[2].type == last)) {
            return true;
        }

        return false;
    }
}