using Godot;
using System.Collections;

public partial class BasementWellRoom : Node3DScript
{
    [NodeType]
    public BasementWell Well;

    [NodeName]
    public Node3D KeyOnRope;

    [NodeName]
    public Touchable KeyOnRopeTouchable;

    private bool _key_collected;

    public override void _Ready()
    {
        base._Ready();

        Well.AttachObjectToRope(KeyOnRope);
        Well.OnRaiseFinished += OnWellRaised;
        Well.OnRaise += OnWellAnimate;
        Well.OnLower += OnWellAnimate;

        KeyOnRopeTouchable.OnTouched += KeyOnRope_Touched;
    }

    private void OnWellAnimate()
    {
        KeyOnRopeTouchable.Disable();
    }

    private void OnWellRaised()
    {
        if (!_key_collected)
        {
            KeyOnRopeTouchable.Enable();
        }
    }

    private void KeyOnRope_Touched()
    {
        if (_key_collected) return;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return Player.Instance.WaitForProgress(2f, KeyOnRope);
            _key_collected = true;

            var item = ItemController.Instance.CreateItem("Key_Workshop");
            item.GlobalPosition = KeyOnRope.GlobalPosition;
            item.GlobalRotation = KeyOnRope.GlobalRotation;

            var dir = item.GlobalPosition.DirectionTo(Player.Instance.GlobalPosition);
            var vel = dir * 5f + Vector3.Up * 1.0f;
            item.RigidBody.LinearVelocity = vel;

            Particle.PlayOneShot("ps_smoke_puff_small", item.GlobalPosition);
            SoundController.Instance.Play("sfx_throw_light", item.GlobalPosition);

            KeyOnRope.Disable();
        }
    }
}
