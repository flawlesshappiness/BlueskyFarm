public partial class EnemyController : ResourceController<EnemyInfoCollection, EnemyInfo>
{
    public override string Directory => $"Enemy";

    public override void _Ready()
    {
        base._Ready();
        BasementController.Instance.OnBasementGenerated += BasementGenerated;
    }

    private EnemyInfo GetRandomEnemyInfo()
    {
        return Collection.Resources.Random();
    }

    private Enemy CreateEnemy(EnemyInfo info)
    {
        var enemy = GDHelper.Instantiate<Enemy>(info.Path, Scene.Current);
        enemy.SetParent(Scene.Current);
        return enemy;
    }

    private void BasementGenerated(Basement basement)
    {
        foreach (var info in Collection.Resources)
        {
            var enemy = CreateEnemy(info);
        }
    }
}
