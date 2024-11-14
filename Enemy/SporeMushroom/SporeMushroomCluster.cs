using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class SporeMushroomCluster : Node3DScript
{
    [NodeType]
    public PsMushroomSmoke PsSmoke;

    [NodeName]
    public Area3D Trigger;

    [NodeName]
    public Sound3D SfxPuff;

    [NodeName]
    public Sound3D SfxAppear;

    [NodeName]
    public Sound3D SfxRoots;

    public event Action OnPlayerTrigger;

    private bool _triggered;

    private List<SporeMushroomModel> _models;
    private SporeMushroomRoots _roots;

    public override void _Ready()
    {
        base._Ready();
        Hide();

        _models = this.GetNodesInChildren<SporeMushroomModel>().ToList();
        _roots = this.GetNodeInChildren<SporeMushroomRoots>();

        RandomizeRotation();

        Trigger.BodyEntered += TriggerEntered;
    }

    private void RandomizeRotation()
    {
        var rng = new RandomNumberGenerator();
        GlobalRotation = new Vector3(0, rng.RandfRange(0, 360), 0);
    }

    private void TriggerEntered(Node3D body)
    {
        if (!Visible) return;
        if (_triggered) return;

        var fx_id = nameof(SporeMushroomCluster) + GetInstanceId();
        ScreenEffects.AnimateGaussianBlur(fx_id, 20, 0.2f, 0f, 15f);
        ScreenEffects.AnimateFog(fx_id, 1f, new Color(0.25f, 0.25f, 0.25f, 0.75f), 2f, 0f, 15f);
        AnimateMoveSpeed();

        SfxPuff.Play();
        PsSmoke.PlayPuff();
        PsSmoke.PlayIdle();
        _triggered = true;

        OnPlayerTrigger?.Invoke();
    }

    public void AnimateAppear()
    {
        var ordered_models = _models.OrderBy(x => x.Scale.Length()).ToList();
        StartCoroutine(Cr, nameof(AnimateAppear));
        IEnumerator Cr()
        {
            SfxAppear.Play();
            _roots.AnimateAppear();
            foreach (var model in ordered_models)
            {
                model.AnimateAppear();
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private void AnimateMoveSpeed()
    {
        var id = $"{nameof(SporeMushroomCluster)}.{nameof(AnimateMoveSpeed)}";
        Coroutine.Start(Cr, id);
        IEnumerator Cr()
        {
            FirstPersonController.Instance.MoveSpeedMultiplier = 0.25f;
            FirstPersonController.Instance.LookSpeedMultiplier = 0.25f;

            yield return new WaitForSeconds(2f);

            var move_start = FirstPersonController.Instance.MoveSpeedMultiplier;
            var look_start = FirstPersonController.Instance.LookSpeedMultiplier;

            yield return LerpEnumerator.Lerp01(2f, f =>
            {
                FirstPersonController.Instance.MoveSpeedMultiplier = Mathf.Lerp(move_start, 1f, f);
                FirstPersonController.Instance.LookSpeedMultiplier = Mathf.Lerp(look_start, 1f, f);
            });
        }
    }
}
