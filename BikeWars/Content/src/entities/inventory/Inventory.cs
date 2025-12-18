using System.Collections.Generic;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.entities.Inventory;

// Inventory is handled as an array, it can only take max 5 items
public class Inventory
{
    private const int MaxSlots = 5;
    private readonly ItemBase[] _items = new ItemBase[MaxSlots];
    public IReadOnlyList<ItemBase> Items => _items;

    public bool AddItem(ItemBase item)
    {
        if (!item.InventoryItem)
            return false;
        for (int i = 0; i < MaxSlots; i++)
        {
            if (_items[i] == null)
            {
                _items[i] = item;
                return true;
            }
        }

        return false;
    }
    
    public ItemBase GetItemAt(int index)
    {
        if (index < 0 || index >= MaxSlots) return null;
        return _items[index];
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, int selectedIndex, bool showSelection)
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
            
            if (showSelection && i == selectedIndex)
            {
                spriteBatch.Draw(pixel, slotRect, Color.Green * 0.4f);
            }

            var item = _items[i];
            if (item != null)
            {
                Rectangle iconRect = new Rectangle(x + 4, y + 4, slotSize - 8, slotSize - 8);
                spriteBatch.Draw(item.CurrentTex, iconRect, Color.White);
            }
        }

    }
    
    public void RemoveItem(ItemBase item)
    {
        for(int i=0; i < MaxSlots; i++)
        {
            if(_items[i] == item)
            {
                _items[i] = null;
                return;
            }
        }
    }
    
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= MaxSlots)
            return;
            
        _items[index] = null;
    }
}

