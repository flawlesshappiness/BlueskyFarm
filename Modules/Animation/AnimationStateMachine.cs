using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class AnimationStateMachine : BaseStateMachine
{
    public AnimationPlayer Animator { get; private set; }

    public Dictionary<string, AnimationState> Animations = new();

    public void Initialize(AnimationPlayer animator)
    {
        Animator = animator;
        Animator.AnimationFinished += AnimationFinished;
    }

    /// <summary>
    /// Creates a StateNode, and an AnimationState using the given animation name
    /// </summary>
    /// <param name="animation">The animation name</param>
    /// <param name="looping">Should the animation loop?</param>
    public AnimationState CreateAnimation(string animation, bool looping)
    {
        var state = new AnimationState(animation)
        {
            Looping = looping,
            Node = CreateNode(animation)
        };

        Animations.Add(animation, state);

        return state;
    }

    protected virtual void AnimationFinished(StringName animName)
    {
        TryProcessCurrentState(true);
    }

    public override void SetCurrentState(StateNode node)
    {
        base.SetCurrentState(node);

        if (Animations.TryGetValue(node.Name, out var state))
        {
            var animation = Animator.GetAnimation(node.Name);
            animation.LoopMode = state.Looping ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;
        }

        Animator.Play(node.Name);
    }
}
