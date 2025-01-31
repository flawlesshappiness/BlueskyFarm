using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class EnemyController : ResourceController<EnemyInfoCollection, EnemyInfo>
{
    public static EnemyController Instance => Singleton.Get<EnemyController>();
    public override string Directory => $"Enemy";

    private List<Enemy> _enemies = new();

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();

        BasementController.Instance.OnAreaEntered += AreaEntered;
        BasementController.Instance.OnBasementEntered += BasementEntered;
    }

    private EnemyInfo GetRandomEnemyInfo()
    {
        return Collection.Resources.Random();
    }

    private Enemy CreateEnemy(EnemyInfo info)
    {
        var enemy = info.Scene.Instantiate<Enemy>();
        enemy.SetParent(Scene.Current);
        _enemies.Add(enemy);
        return enemy;
    }

    public void SpawnEnemies()
    {
        Debug.TraceMethod();
        Debug.Indent++;

        var basement = BasementController.Instance.CurrentBasement;
        foreach (var area in basement.Settings.Areas)
        {
            SpawnAreaEnemies(area.AreaName);
        }

        Debug.Indent--;
    }

    public void SpawnAreaEnemies(string area)
    {
        Debug.TraceMethod(area);
        Debug.Indent++;

        var safe_enemies = Collection.Resources
            .Where(x => x.Enabled && !x.IsDangerous && x.Areas.Any(target_area => target_area.ToString() == area))
            .TakeRandom(2);

        var dangerous_enemies = Collection.Resources
            .Where(x => x.Enabled && x.IsDangerous && x.Areas.Any(target_area => target_area.ToString() == area))
            .TakeRandom(1);

        var enemies = safe_enemies.Concat(dangerous_enemies);

        foreach (var info in enemies)
        {
            var count = Mathf.Max(1, info.Count);
            for (int i = 0; i < count; i++)
            {
                var enemy = CreateEnemy(info);
                enemy.GlobalPosition = Vector3.Down * 100;
                enemy.TargetArea = area;
                enemy.InitializeEnemy();
                enemy.Despawn();
            }
        }

        Debug.Indent--;
    }

    private void DebugSpawnEnemy(EnemyInfo info)
    {
        var enemy = CreateEnemy(info);
        enemy.IsDebug = true;
    }

    private void RegisterDebugActions()
    {
        var category = "ENEMY";

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
                var filename = System.IO.Path.GetFileNameWithoutExtension(info.ResourcePath);
                view.ContentSearch.AddItem(filename, () => DebugSpawnEnemy(info));
            }

            view.ContentSearch.UpdateButtons();
        }
    }

    private void AreaEntered(string area)
    {
        foreach (var enemy in _enemies)
        {
            if (enemy.TargetArea == area)
            {
                enemy.Respawn();
            }
            else
            {
                enemy.Despawn();
            }
        }
    }

    private void BasementEntered()
    {
        AreaEntered(AreaNames.Basement);
    }
}
