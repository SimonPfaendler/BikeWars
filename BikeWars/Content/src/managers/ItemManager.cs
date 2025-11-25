using System.Collections.Generic;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.entities.items;
using BikeWars.Entities.Characters;
namespace BikeWars.Content.managers;
public class ItemManager
{
    private readonly List<ItemBase> _items = new();
    public IReadOnlyList<ItemBase> Items => _items;
    public void AddItem(ItemBase item)
    {
        _items.Add(item);
    }

    public void LoadContent(ContentManager content)
    {
        foreach (var item in _items)
        {
            item.LoadContent(content);
        }
    }

    public void Update(GameTime gameTime, Player player)
    // if a Player collides with a none Inventory, pickable Item it gets collect.
    // if it is a pickable item and player is pressing q it gets collected otherwise not
    {
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            var item = _items[i];
            bool collision = player.Intersects(item.Collider);

            // if (!item.InventoryItem && item is IPickable)
            // {
            //     if (collision)
            //     {
            //         _items.RemoveAt(i);
            //         continue;
            //     }
            // }

            if (item.InventoryItem)
            {
                if (collision && InputHandler.IsPressed(GameAction.INTERACT))
                {
                    if (player.Inventory.AddItem(item))
                    {
                        _items.RemoveAt(i);
                        continue;
                    }
                }
            }

            item.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var item in _items)
        {
            item.Draw(spriteBatch);
        }
    }
}