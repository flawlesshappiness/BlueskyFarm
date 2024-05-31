using Godot;
using System;
using System.Collections.Generic;

public abstract class Grid
{
    public static readonly Vector2I North = new Vector2I(0, -1);
    public static readonly Vector2I East = new Vector2I(1, 0);
    public static readonly Vector2I South = new Vector2I(0, 1);
    public static readonly Vector2I West = new Vector2I(-1, 0);
}

public class Grid<T> : Grid where T : class
{
    private readonly Dictionary<Vector2I, T> _coordinates = new Dictionary<Vector2I, T>();

    public Grid()
    {
    }

    public IEnumerable<T> Elements => _coordinates.Values;

    public T this[int x, int y]
    {
        get { TryGet(x, y, out var v); return v; }
        set { Set(x, y, value); }
    }

    public T this[Vector2I p]
    {
        get { TryGet(p, out var v); return v; }
        set { Set(p, value); }
    }

    public bool TryGet(int x, int y, out T v) => TryGet(new Vector2I(x, y), out v);
    public bool TryGet(Vector2I p, out T v) => _coordinates.TryGetValue(p, out v);

    public void Set(int x, int y, T v) => Set(new Vector2I(x, y), v);
    public void Set(Vector2I p, T v)
    {
        if (_coordinates.ContainsKey(p))
        {
            _coordinates[p] = v;
        }
        else
        {
            _coordinates.Add(p, v);
        }
    }

    public List<T> GetNeighbours(Vector2I p, Func<T, bool> predicate = null)
    {
        var neighbours = new List<T>();
        var offsets = new List<Vector2I> { North, East, South, West };

        foreach (var offset in offsets)
        {
            var po = p + offset;

            if (TryGet(po, out var v) && (predicate?.Invoke(v) ?? true))
            {
                neighbours.Add(v);
            }
        }

        return neighbours;
    }

    public IEnumerable<Vector2I> GetNeighbourCoordinates(Vector2I p, Func<Vector2I, bool> predicate = null)
    {
        var coordinates = new List<Vector2I>();
        var offsets = new List<Vector2I> { North, East, South, West };

        foreach (var offset in offsets)
        {
            var po = p + offset;

            if (predicate?.Invoke(po) ?? true)
            {
                coordinates.Add(po);
            }
        }

        return coordinates;
    }

    public IEnumerable<Vector2I> GetEmptyNeighbourCoordinates(Vector2I p) => GetNeighbourCoordinates(p, pn => !TryGet(pn, out _));
}
