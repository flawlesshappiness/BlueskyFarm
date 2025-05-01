using GodotSteam;
using System;
using System.Text.Json;

public partial class SteamController : SingletonController
{
    public override string Directory => "Steam";
    public static SteamController Instance => Singleton.Get<SteamController>();
    public bool SteamRunning { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        SteamRunning = Steam.IsSteamRunning();
        Debug.Log($"Steam running: {SteamRunning}");
    }

    public bool TryReadData<T>(string name, out T data)
        where T : class
    {
        Debug.TraceMethod(name);

        data = null;

        try
        {
            if (!SteamRunning) return false;
            if (!Steam.FileExists(name)) return false;

            var size = Steam.GetFileSize(name);
            var result = Steam.FileRead(name, size);

            var success = result["ret"].AsBool();
            var buffer = result["buf"].AsByteArray();

            if (!success) return false;

            var json = System.Text.Encoding.Default.GetString(buffer);
            data = JsonSerializer.Deserialize<T>(json);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Debug.LogError(e.StackTrace);
            return false;
        }
    }

    public bool TryWriteData<T>(string name, T data)
        where T : class
    {
        Debug.TraceMethod(name);

        try
        {
            if (!SteamRunning) return false;

            var json = JsonSerializer.Serialize(data);
            return TryWriteData(name, json);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Debug.LogError(e.StackTrace);
            return false;
        }
    }

    public bool TryWriteData(string name, string data)
    {
        Debug.TraceMethod(name);

        try
        {
            if (!SteamRunning) return false;

            var buffer = System.Text.Encoding.Default.GetBytes(data);
            var success = Steam.FileWrite(name, buffer);

            return success;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Debug.LogError(e.StackTrace);
            return false;
        }
    }
}
