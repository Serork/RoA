using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Core.VisualEffects;

abstract class VisualEffect<T> : IPooledParticle, ILoadable where T : VisualEffect<T>, new() {
    public Vector2 Position;
    public Vector2 Velocity;
    public Color Color;
    public float Scale;
    public float Rotation;
    public Rectangle Frame;
    public Vector2 Origin;
    public bool DontEmitLight;
    public int TimeLeft;
    public int MaxTimeLeft;

    public virtual int InitalPoolSize => 1;

    public virtual T CreateBaseInstance() => new();

    public bool ShouldBeRemovedFromRenderer { get; protected set; }

    public bool IsRestingInPool => ShouldBeRemovedFromRenderer;

    public virtual Texture2D Texture => ModContent.Request<Texture2D>(ResourceManager.Textures + $"VisualEffects/{typeof(T).Name}").Value;

    protected void SetFramedTexture(int frames, int frameChoice = -1) {
        Frame = Texture.Frame(verticalFrames: frames, frameY: (frameChoice == -1 ? Main.rand.Next(frames) : frameChoice));
        Origin = Frame.Size() / 2f;
    }

    protected void SetHorizontalAndVerticallyFramedTexture(int horizontalFrames, int verticalFrames, int frameX = -1, int frameY = -1) {
        Frame = Texture.Frame(horizontalFrames, verticalFrames, frameX == -1 ? Main.rand.Next(horizontalFrames) : frameX, frameY == -1 ? Main.rand.Next(verticalFrames) : frameY);
        Origin = Frame.Size() / 2f;
    }

    public T Setup(Vector2 position, Vector2 velocity, Color color = default(Color), float scale = 1f, float rotation = 0f) {
        Position = position;
        Velocity = velocity;
        Color = color;
        Scale = scale;
        Rotation = rotation;

        SetDefaults();

        return (T)this;
    }

    protected virtual void SetDefaults() {
        TimeLeft = MaxTimeLeft;
    }

    public virtual Color GetParticleColor(ref ParticleRendererSettings settings) => Color;

    public virtual void Update(ref ParticleRendererSettings settings) {
        Velocity *= 0.9f;
        float length = Velocity.Length();
        Rotation += length * 0.0314f;
        Scale -= 0.05f - length / 1000f;

        if (Scale <= 0.1f || float.IsNaN(Scale) || --TimeLeft <= 0) {
            RestInPool();

            return;
        }

        if (!DontEmitLight) {
            Lighting.AddLight(Position, Color.ToVector3() * 0.5f);
        }
        Position += Velocity;
    }

    public virtual void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        spritebatch.Draw(Texture, Position - Main.screenPosition, Frame, GetParticleColor(ref settings), Rotation, Origin, Scale, SpriteEffects.None, 0f);
    }

    public virtual void RestInPool() {
        ShouldBeRemovedFromRenderer = true;
    }

    public virtual void FetchFromPool() {
        ShouldBeRemovedFromRenderer = false;
    }

    public void Load(Mod mod) {
        VisualEffectSystem.ParticlePools<T>.Pool = new ParticlePool<T>(InitalPoolSize, CreateBaseInstance);
        OnLoad(mod);
    }

    public virtual void OnLoad(Mod mod) { }

    public void Unload() {
        VisualEffectSystem.ParticlePools<T>.Pool = null;
        OnUnload();
    }

    public virtual void OnUnload() { }
}