using Godot;
using System.Collections;

public partial class Dream_Falling : DreamScene
{
    [Export]
    public Node3D FarmGridTemplate;

    [Export]
    public AnimationPlayer AnimationPlayer;

    private string FxId => nameof(Dream_Falling);
    private bool _grid_spawned;

    public override void _Ready()
    {
        base._Ready();
        SpawnGrid();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        ScreenEffects.RemoveCameraShake(FxId + 1);
        ScreenEffects.RemoveCameraShake(FxId + 2);
    }

    public override IEnumerator WaitForReady()
    {
        while (!_grid_spawned)
        {
            yield return null;
        }
    }

    private void SpawnGrid()
    {
        this.StartCoroutine(Cr, nameof(SpawnGrid));
        IEnumerator Cr()
        {
            var count = 50;
            var size = 30f;
            var start = -size * count * 0.5f;
            var parent = FarmGridTemplate.GetParent();
            for (int z = 0; z < count; z++)
            {
                for (int x = 0; x < count; x++)
                {
                    var position = new Vector3(start, 0, start) + new Vector3(size * x, 0, size * z) - new Vector3(size * 0.5f, 0, size * 0.5f);

                    var grid = FarmGridTemplate.Duplicate() as Node3D;
                    grid.SetParent(parent);
                    grid.GlobalPosition = position;
                }

                yield return null;
            }

            FarmGridTemplate.Hide();
            _grid_spawned = true;
        }
    }

    public override void StartScene()
    {
        base.StartScene();
        StartAnimation();
    }

    public void CutToBlack()
    {
        CompleteDream(true);
    }

    private void StartAnimation()
    {
        ScreenEffects.SetCameraShake(FxId + 1, 1f);
        AnimationPlayer.Play("falling_over_farms");
    }

    public void StartIncreasingShake()
    {
        ScreenEffects.AnimateCameraShakeIn(FxId + 2, 2f, 2f);
    }
}
