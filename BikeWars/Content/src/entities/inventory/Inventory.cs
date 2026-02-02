using System.Collections.Generic;
using BikeWars.Content.components;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.entities.items;
using BikeWars.Content.entities.MapObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.entities.Inventory;

// Inventory is handled as an array, it can only take max 5 items
public class Inventory
{
    private const int MaxSlots = 5;
    private readonly ItemBase[] _items = new ItemBase[MaxSlots];
    public IReadOnlyList<ItemBase> Items => _items;
    private int _invalidSlotIndex = -1;
    private float _invalidSlotTimer = 0f;

    public bool AddItem(ItemBase item)
    {
        if (!item.InventoryItem)
            return false;
        if (item is Beer beer)
        {
            if (beer._isDestroyed)
            {
                return false;
            }
        }
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

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, int selectedInventoryIndex, bool showSelection, int playerIndex)
    {
        int slotSize = 40;
        int slotGap = 8;

        int screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
        int screenHeight = spriteBatch.GraphicsDevice.Viewport.Height;

        // total width of all slots combined
        int totalWidth = MaxSlots * slotSize + (MaxSlots - 1) * slotGap;

        // position inventory row in the top-right corner of the screen if player1 is owner
        Vector2 startPos;
        if (playerIndex == 1)
        {
            startPos = new Vector2(screenWidth - totalWidth - 20, 40);
        }
        else
        {
            startPos = new Vector2(20, screenHeight - slotSize - 40);
        }

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


            var item = _items[i];
            if (item != null)
            {
                Rectangle iconRect = new Rectangle(x + 4, y + 4, slotSize - 8, slotSize - 8);
                spriteBatch.Draw(item.CurrentTex, iconRect, Color.White);
            }
            if (item is Beer && !Beer.Ready)
            {
                int timeLeft = Beer.RemainingTotalSeconds;
                DrawTimer(spriteBatch, timeLeft, slotRect);
            }
            if (item is DogFood && !DogBowl.Ready)
            {
                int timeLeft = DogBowl.RemainingTotalSeconds;
                DrawTimer(spriteBatch, timeLeft, slotRect);
            }
            if (showSelection && i == selectedInventoryIndex)
            {
                spriteBatch.Draw(pixel, slotRect, Color.Green * 0.4f);
            }
            if (i == _invalidSlotIndex)
            {
                spriteBatch.Draw(pixel, slotRect, Color.Red * 0.7f);
            }
        }

    }
    public void Update(GameTime gameTime)
    {
        if (_invalidSlotIndex == -1) return;

        _invalidSlotTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_invalidSlotTimer <= 0f)
        {
            _invalidSlotIndex = -1;
            _invalidSlotTimer = 0f;
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

    public void RedSlot(int slotindex)
    {
        _invalidSlotIndex = slotindex;
        _invalidSlotTimer = 0.5f;
    }
    private void DrawTimer(SpriteBatch spriteBatch, int timeLeft, Rectangle slotRect)
    {
        string text = $"{timeLeft}";
        Vector2 size = UIAssets.DefaultFont.MeasureString(text);

        Vector2 pos = new Vector2(
            slotRect.Center.X - size.X / 2f,
            slotRect.Top - size.Y - 2f - 6
        );

        spriteBatch.DrawString(UIAssets.DefaultFont, text, pos, Color.Red);
    }
}

