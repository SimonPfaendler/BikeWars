using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace BikeWars.Content.managers;
public class ItemManager
{
    private readonly List<ItemBase> _items = new();
    public List<ItemBase> Items => _items;
    public void AddItem(ItemBase item)
    {
        _items.Add(item);
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var item in _items)
        {
            item.Draw(spriteBatch);
        }
    }

    public void Remove(ItemBase item)
    {
        _items.Remove(item);
    }
}