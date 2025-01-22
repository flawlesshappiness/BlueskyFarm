using Godot;
using Godot.Collections;
using System.Collections;

public partial class UnlockableGroup : Node3D
{
    [Export]
    public SoundInfo HideSound;

    [Export]
    public SoundInfo ShowSound;

    [Export]
    public ParticleInfo HideParticle;

    [Export]
    public ParticleInfo ShowParticle;

    [Export]
    public Array<Node3D> NotUnlocked;

    [Export]
    public Array<Node3D> Unlocked;

    public void RegisterDebugActions(string category)
    {
        Debug.RegisterAction(new DebugAction
        {
            Id = category,
            Category = category,
            Text = "Animate unlock",
            Action = v => AnimateUnlock()
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = category,
            Category = category,
            Text = "Set locked",
            Action = v => SetNotUnlocked()
        });
    }

    public void SetUnlocked(bool unlocked)
    {
        if (unlocked)
        {
            SetUnlocked();
        }
        else
        {
            SetNotUnlocked();
        }
    }

    public void SetUnlocked()
    {
        NotUnlocked.ForEach(x => x.Disable());
        Unlocked.ForEach(x => x.Enable());
    }

    public void SetNotUnlocked()
    {
        NotUnlocked.ForEach(x => x.Enable());
        Unlocked.ForEach(x => x.Disable());
    }

    public Coroutine AnimateUnlock()
    {
        return Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            if (NotUnlocked != null && NotUnlocked.Count > 0)
            {
                NotUnlocked.ForEach(x => AnimateHide(x));
                SoundController.Instance.Play(HideSound, GlobalPosition);

                yield return new WaitForSeconds(0.25f);
            }

            foreach (var node in Unlocked)
            {
                AnimateShow(node);
                yield return new WaitForSeconds(0.1f);
            }
        }

        Coroutine AnimateHide(Node3D node)
        {
            return Coroutine.Start(HideCr);
            IEnumerator HideCr()
            {
                var start = node.Scale;
                var end = Vector3.One * 0.001f;
                var curve = Curves.EaseInBack;
                yield return LerpEnumerator.Lerp01(0.25f, f =>
                {
                    var t = curve.Evaluate(f);
                    node.Scale = start.Lerp(end, t);
                });

                Particle.PlayOneShot(HideParticle, node.GlobalPosition);
                node.Disable();
                node.Scale = start;
            }
        }

        Coroutine AnimateShow(Node3D node)
        {
            return Coroutine.Start(ShowCr);
            IEnumerator ShowCr()
            {
                node.Scale = Vector3.One * 0.001f;
                node.Enable();
                var start = node.Scale;
                var end = Vector3.One;
                var curve = Curves.EaseOutBack;

                Particle.PlayOneShot(ShowParticle, node.GlobalPosition);
                SoundController.Instance.Play(ShowSound, node.GlobalPosition);

                yield return LerpEnumerator.Lerp01(0.25f, f =>
                {
                    var t = curve.Evaluate(f);
                    node.Scale = start.Lerp(end, t);
                });
            }
        }
    }
}
