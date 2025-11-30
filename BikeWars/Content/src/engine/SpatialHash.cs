using System;
using System.Collections.Generic;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

// public class SpatialHash2D<T>
namespace BikeWars.Content.engine;
public class SpatialHash
{
    private int _cellSize {get; set;}
    public int CellSize {get => _cellSize; set => _cellSize = value;}
    private readonly Dictionary<(int, int), List<ICollider>> _cells;
    public Dictionary<(int, int), List<ICollider>> Cells {get => _cells;}
    private readonly float _insertRadius;

    public SpatialHash(int cellSize, float insertRadius)
    {
        _cellSize = cellSize;
        _insertRadius = insertRadius;
        _cells = new Dictionary<(int, int), List<ICollider>>();
    }

    private (int, int) ToCellCoords(Vector2 pos)
    {
        return (
            (int)Math.Floor(pos.X / CellSize),
            (int)Math.Floor(pos.Y / CellSize)
        );
    }

    public void Insert(ICollider collider)
    {
        Vector2 pos = collider.Position;
        (int, int) center = ToCellCoords(pos);

        // Falls das Objekt einen Radius hat, trage es in mehrere Zellen ein
        int radius = (int)Math.Ceiling(_insertRadius / CellSize);

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                (int, int) key = (center.Item1 + x, center.Item2 + y);

                if (!_cells.TryGetValue(key, out List<ICollider> list))
                {
                    list = new List<ICollider>();
                    _cells[key] = list;
                }

                list.Add(collider);
            }
        }
    }

    public void Clear()
    {
        _cells.Clear();
    }

    public List<ICollider> QueryNearby(Vector2 pos)
    {
        (int, int) center = ToCellCoords(pos);
        List<ICollider> results = new List<ICollider>();

        // Suche in Nachbarzellen
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                (int, int) key = (center.Item1 + x, center.Item2 + y);
                if (_cells.TryGetValue(key, out var list))
                {
                    results.AddRange(list);
                }
            }
        }

        return results;
    }

    public List<ICollider> AllColliders()
    {
        List<ICollider> allColliders = new List<ICollider>();
        foreach (var cell in _cells)
        {
            {
                foreach(ICollider collider in cell.Value)
                {
                    allColliders.Add(collider);
                }
            }
        }
        return allColliders;
    }
}