public partial class EnemyController : ResourceController<EnemyInfoCollection, EnemyInfo>
{
    public static EnemyController Instance => Singleton.Get<EnemyController>();
    public override string Directory => $"Enemy";

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
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

    public void SpawnEnemies()
    {
        foreach (var info in Collection.Resources)
        {
            if (!info.Enabled) continue;
            var enemy = CreateEnemy(info);
        }
    }

    private void DebugSpawnEnemy(EnemyInfo info)
    {
        var enemy = CreateEnemy(info);
        enemy.DebugSpawn();
    }

    private void RegisterDebugActions()
    {
        var category = "Enemy";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Spawn enemy",
            Action = SpawnEnemy
        });

        void SpawnEnemy(DebugView view)
        {
            view.HideContent();
            view.Content.Show();
            view.ContentSearch.Show();
            view.ContentSearch.ClearItems();

            foreach (var enemy in Collection.Resources)
            {
                var filename = System.IO.Path.GetFileNameWithoutExtension(enemy.Path);
                view.ContentSearch.AddItem(filename, () => DebugSpawnEnemy(enemy));
            }

            view.ContentSearch.UpdateButtons();
        }
    }
}
