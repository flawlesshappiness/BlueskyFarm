public partial class EnemyController : ResourceController<EnemyInfoCollection, EnemyInfo>
{
    public static EnemyController Instance => Singleton.Get<EnemyController>();
    public override string Directory => $"Enemy";

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

    public void SpawnEnemies()
    {
        foreach (var info in Collection.Resources)
        {
            var enemy = CreateEnemy(info);
        }
    }
}
