using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.entities.Inventory;

// Inventory is handled as a list, it can only take max 5 items
public class Inventory
{
    private const int MaxSlots = 5;
    private readonly List<ItemBase> _items = new();
    public IReadOnlyList<ItemBase> Items => _items;

    public bool AddItem(ItemBase item)
    {
        if (!item.InventoryItem)
            return false;
        if (_items.Count >= MaxSlots)
            return false;

        _items.Add(item);
        return true;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        int slotSize = 40;
        int slotGap = 8;

        int screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
        
        // total width of all slots combined
        int totalWidth = MaxSlots * slotSize + (MaxSlots - 1) * slotGap;
        
        // position inventory row in the top-right corner of the screen
        Vector2 startPos = new Vector2(screenWidth - totalWidth - 20, 40);
        
        // draws the background of the inventory
        Rectangle backgroundRect = new Rectangle(
            (int)startPos.X - 10,
            (int)startPos.Y - 10,
            totalWidth + 20,
            slotSize + 20
        );

        spriteBatch.Draw(pixel, backgroundRect, Color.Orange);

        // draw each slot
        for (int i = 0; i < MaxSlots; i++)
        {
            int x = (int)(startPos.X + i * (slotSize + slotGap));
            int y = (int)startPos.Y;
            
            Rectangle slotRect = new Rectangle(x, y, slotSize, slotSize);
            spriteBatch.Draw(pixel, slotRect, Color.White);
            
            if (i >= _items.Count)
                continue;

            var item = _items[i];
            if (item == null)
                continue;
            
            Rectangle iconRect = new Rectangle(
                x + 4,
                y + 4,
                slotSize - 8,
                slotSize - 8
            );

            spriteBatch.Draw(item.CurrentTex, iconRect, Color.White);
        }
    }
}

