using FlawLizArt.Animation.StateMachine;

public class AnimationState
{
    public string Animation { get; private set; } = string.Empty;
    public StateNode Node { get; set; }
    public bool Looping { get; set; }

    public AnimationState(string animation)
    {
        Animation = animation;
    }
}
