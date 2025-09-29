namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

//ref struct ExecuteArgs {
//    public byte CurrentState;
//    public double FrameCounter;
//    public bool ResetGryphonStats;
//}

//interface ILothorAnimation : ILoadable {
//    public Lothor.SpriteSheetColumn UsedColumn { get; }
//    public byte StartFrame { get; }

//    public void Execute(ref ExecuteArgs executeArgs) { }

//    void ILoadable.Load(Mod mod) => Lothor.Animations.TryAdd(GetType().Name, this);
//    void ILoadable.Unload() { }
//}

//sealed partial class Lothor : ModNPC {
//    internal static Dictionary<string, ILothorAnimation> Animations = [];

//    private ILothorAnimation _currentAnimation = null;
//    private bool _isAnimationPlaying = false;

//    partial void HandleAnimations() {
//        if (!_isAnimationPlaying || _currentAnimation == null) {
//            return;
//        }
//        ExecuteArgs executeArgs;
//        executeArgs.ResetGryphonStats = false;
//        executeArgs.FrameCounter = NPC.frameCounter;
//        executeArgs.CurrentState = CurrentState;
//        _currentAnimation.Execute(ref executeArgs);
//        NPC.frameCounter = executeArgs.FrameCounter;
//        if (executeArgs.ResetGryphonStats) {
//            _currentAnimation = null;
//            return;
//        }
//        CurrentState = executeArgs.CurrentState;
//    }

//    partial void UnloadAnimations() {
//        _currentAnimation = null;

//        Animations.Clear();
//        Animations = null;
//    }

//    private void PlayAnimation(string id) {
//        ILothorAnimation animation = Animations[id];
//        if (animation.Equals(_currentAnimation)) {
//            return;
//        }
//        _currentAnimation = animation;
//        _currentColumn = _currentAnimation.UsedColumn;
//        _isAnimationPlaying = true;
//        NPC.frameCounter = 0.0;
//        CurrentState = _currentAnimation.StartFrame;
//    }
//}

//sealed class FallingAnimation : ILothorAnimation {
//    public Lothor.SpriteSheetColumn UsedColumn => Lothor.SpriteSheetColumn.Stand;
//    public byte StartFrame => 18;
//}

//sealed class IdleAnimation : ILothorAnimation {
//    public Lothor.SpriteSheetColumn UsedColumn => Lothor.SpriteSheetColumn.Stand;
//    public byte StartFrame => 0;
//}

//sealed class ClawsAnimation : ILothorAnimation {
//    public Lothor.SpriteSheetColumn UsedColumn => Lothor.SpriteSheetColumn.Stand;
//    public byte StartFrame => 1;

//    public void Execute(ref ExecuteArgs executeArgs) {
//        byte lastFrame = 6;
//        double frameRate = 5.0;
//        ref byte currentFrame = ref executeArgs.CurrentState;
//        ref double frameCounter = ref executeArgs.FrameCounter;
//        ref bool reset = ref executeArgs.ResetGryphonStats;
//        if (++frameCounter > frameRate) {
//            frameCounter = 0.0;
//            currentFrame++;
//        }
//        if (currentFrame > lastFrame) {
//            reset = true;
//        }
//    }
//}

//sealed class SpittingAnimation : ILothorAnimation {
//    public Lothor.SpriteSheetColumn UsedColumn => Lothor.SpriteSheetColumn.Stand;
//    public byte StartFrame => 8;

//    public void Execute(ref ExecuteArgs executeArgs) {
//        byte lastFrame = 14;
//        double frameRate = 5.0;
//        ref byte currentFrame = ref executeArgs.CurrentState;
//        ref double frameCounter = ref executeArgs.FrameCounter;
//        ref bool reset = ref executeArgs.ResetGryphonStats;
//        if (++frameCounter > frameRate) {
//            frameCounter = 0.0;
//            currentFrame++;
//        }
//        if (currentFrame > lastFrame) {
//            reset = true;
//        }
//    }
//}

//sealed class BeforeJumpAnimation : ILothorAnimation {
//    public Lothor.SpriteSheetColumn UsedColumn => Lothor.SpriteSheetColumn.Stand;
//    public byte StartFrame => 16;

//    public void Execute(ref ExecuteArgs executeArgs) {
//        byte lastFrame = 18;
//        double frameRate = 5.0;
//        ref byte currentFrame = ref executeArgs.CurrentState;
//        ref double frameCounter = ref executeArgs.FrameCounter;
//        ref bool reset = ref executeArgs.ResetGryphonStats;
//        if (++frameCounter > frameRate) {
//            frameCounter = 0.0;
//            currentFrame++;
//        }
//        if (currentFrame > lastFrame) {
//            reset = true;
//        }
//    }
//}
