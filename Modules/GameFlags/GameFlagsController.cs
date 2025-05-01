using System.Collections.Generic;
using System.Linq;

public partial class GameFlagsController : SingletonController
{
    public override string Directory => $"{Paths.Modules}/GameFlags";
    public static GameFlagsController Instance => Singleton.Get<GameFlagsController>();

    private Dictionary<string, GameFlagData> _flags = new();

    protected override void Initialize()
    {
        base.Initialize();
        MainMenuView.Instance.OnGameStarted += OnGameStarted;
        MainMenuView.Instance.OnReturnToMainMenu += OnReturnToMainMenu;
    }

    private void OnGameStarted()
    {
        LoadData();
        Data.Game.OnBeforeSave += BeforeSave;
    }

    private void OnReturnToMainMenu()
    {
        Data.Game.OnBeforeSave -= BeforeSave;
    }

    private void LoadData()
    {
        _flags.Clear();
        foreach (var data in Data.Game.GameFlags)
        {
            _flags.Add(data.Id, data);
        }
    }

    private void BeforeSave()
    {
        Data.Game.GameFlags = _flags.Values.ToList();
    }

    public GameFlagData GetOrCreateFlag(string id)
    {
        if (!_flags.ContainsKey(id))
        {
            _flags.Add(id, new GameFlagData { Id = id });
        }

        return _flags[id];
    }

    public bool HasFlag(string id)
    {
        return !IsFlag(id, 0);
    }

    public bool IsFlag(string id, int value)
    {
        return GetOrCreateFlag(id).Value == value;
    }

    public void SetFlag(string id, int value)
    {
        GetOrCreateFlag(id).Value = value;
    }
}
