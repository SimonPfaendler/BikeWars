using System;
using System.Collections.Generic;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine;
public class CellData
{
    public HashSet<CollisionLayer> Layers = new();
    public int Count = 0;
    public List<ICollider>? Colliders = null;
}

public class SpatialHash
{
    private readonly int _cellSize;
    private readonly int _worldWidthInCells;
    private readonly int _xOffset;
    private readonly int _yOffset;

    public Dictionary<int, CellData> _cells = new();

    public SpatialHash(int cellSize, int worldWidthInCells, int xOffset = 0, int yOffset = 0)
    {
        _cellSize = cellSize;
        _worldWidthInCells = worldWidthInCells;
        _xOffset = xOffset;
        _yOffset = yOffset;
    }

    private (int, int) ToCellCoords(Vector2 pos)
    {
        return ((int)MathF.Floor(pos.X / _cellSize),
                (int)MathF.Floor(pos.Y / _cellSize));
    }

    private int To1DKey(int x, int y)
    {
        return (y + _yOffset) * _worldWidthInCells + (x + _xOffset);
    }

    public void Insert(ICollider c)
    {
        int minX = (int)MathF.Floor(c.Position.X / _cellSize);
        int maxX = (int)MathF.Floor(c.Position.X / _cellSize);
        int minY = (int)MathF.Floor(c.Position.Y / _cellSize);
        int maxY = (int)MathF.Floor(c.Position.Y / _cellSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int key = To1DKey(x, y);

                if (!_cells.TryGetValue(key, out var cell))
                {
                    cell = new CellData();
                    _cells[key] = cell;
                }

                cell.Count++;
                cell.Layers.Add(c.Layer); // Layer speichern
                cell.Colliders ??= new List<ICollider>();
                cell.Colliders.Add(c);
            }
        }
    }

    public void Remove(ICollider c)
    {
        int minX = (int)MathF.Floor(c.Position.X / _cellSize);
        int maxX = (int)MathF.Floor(c.Position.X / _cellSize);
        int minY = (int)MathF.Floor(c.Position.Y / _cellSize);
        int maxY = (int)MathF.Floor(c.Position.Y / _cellSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int key = To1DKey(x, y);

                if (_cells.TryGetValue(key, out var cell))
                {
                    cell.Colliders!.Remove(c);
                    cell.Count--;

                    if (cell.Count == 0)
                    {
                        _cells.Remove(key);
                        continue;
                    }

                    cell.Layers.Clear();
                    foreach (var col in cell.Colliders)
                        cell.Layers.Add(col.Layer);
                }
            }
        }
    }

    public List<ICollider> QueryNearby(Vector2 pos, int radius)
    {
        var (cellX, cellY) = ToCellCoords(pos);
        List<ICollider> results = new();

        for (int x = cellX - radius; x <= cellX + radius; x++)
        {
            for (int y = cellY - radius; y <= cellY + radius; y++)
            {
                int key = To1DKey(x, y);

                if (_cells.TryGetValue(key, out var cell) == false)
                    continue;
                results.AddRange(cell.Colliders!);
            }
        }
        return results;
    }

    public void Clear()
    {
        _cells.Clear();
    }
}