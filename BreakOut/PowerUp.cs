using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BreakOut
{
    class PowerUp
    {
        private string label;
        private int x, y;
        private int width = 40;
        private int height = 10;
        private int dY = 4;
        private Brush brush;

        /// <summary>
        /// The name of the PowerUp
        /// </summary>
        public string Type { get { return label; } }
        public int YSpeed { get { return dY; } }
        public int Left { get { return x; } }
        public int Right { get { return x + width; } }
        public int Bottom { get { return y + height; } }
        public int Top { get { return y; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public Rectangle Rectangle { get { return new Rectangle(x, y, width, height); } }

        /// <summary>
        /// Creates a new powerup
        /// </summary>
        /// <param name="type">The type of the powerup: Stretch, Shrink, Multi</param>
        /// <param name="powerUpBrush">Color of the powerup</param>
        /// <param name="xpos">Centre X position</param>
        /// <param name="ypos">Centre Y position</param>
        public PowerUp(string type, Brush powerUpBrush, int xpos, int ypos)
        {
            label = type;
            brush = powerUpBrush;
            x = xpos - width / 2; //transforms centre to left
            y = ypos - height / 2; //transforms centre to top
        }

        /// <summary>
        /// Draws the PowerUp on the screen
        /// </summary>
        /// <param name="paper">Graphics object to draw on</param>
        public void Draw(Graphics paper)
        {
            Font pFont = new Font(FontFamily.GenericSansSerif,6);
            paper.FillRectangle(brush, x, y, width, height);
            paper.DrawString(label, pFont, Brushes.Black, x, y);
        }

        /// <summary>
        /// Increments the position of the PowerUp.
        /// </summary>
        public void Move()
        {
            y += dY;
        }
    }
}
