using System.Linq;

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
        var enemy = info.Scene.Instantiate<Enemy>();
        enemy.SetParent(Scene.Current);
        return enemy;
    }

    public void SpawnEnemies()
    {
        Debug.TraceMethod();
        Debug.Indent++;

        var areas = BasementController.Instance.CurrentBasement.Settings.Areas
            .Select(x => x.AreaName);

        foreach (var area in areas)
        {
            var safe_enemies = Collection.Resources
            .Where(x => x.Enabled && !x.IsDangerous)
            .TakeRandom(2);

            var dangerous_enemies = Collection.Resources
                .Where(x => x.Enabled && x.IsDangerous)
                .TakeRandom(1);

            var enemies = safe_enemies.Concat(dangerous_enemies);

            foreach (var info in enemies)
            {
                var enemy = CreateEnemy(info);
                enemy.TargetArea = area;
                Debug.Trace($"Spawned {enemy.Name} in {area}");
            }
        }

        Debug.Indent--;
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

            foreach (var info in Collection.Resources)
            {
                var filename = System.IO.Path.GetFileNameWithoutExtension(info.Scene.ResourcePath);
                view.ContentSearch.AddItem(filename, () => DebugSpawnEnemy(info));
            }

            view.ContentSearch.UpdateButtons();
        }
    }
}
