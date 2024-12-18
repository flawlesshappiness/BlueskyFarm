using Godot;
using System.Collections;

public partial class SporeMushroomRoots : Node3DScript
{
    [NodeType]
    public AnimationPlayer Animator;

    [NodeName]
    public Sound3D SfxRoots;

    public override void _Ready()
    {
        base._Ready();
        AnimateIdle();
    }

    public void AnimateIdle()
    {
        Animator.Play("idle");
        SfxRoots.Stop();
    }

    public void AnimateAppear()
    {
        Animator.Play("appear");
        SfxRoots.Play();

        this.StartCoroutine(Cr, nameof(AnimateAppear));
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(Animator.CurrentAnimation.Length);

            var start = SfxRoots.VolumeDb;
            var end = -80f;
            yield return LerpEnumerator.Lerp01(2f, f =>
            {
                SfxRoots.VolumeDb = Mathf.Lerp(start, end, f);
            });

            SfxRoots.Stop();
        }
    }
}
