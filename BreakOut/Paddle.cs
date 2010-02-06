using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BreakOut
{
    class Paddle
    {
        private int width, height, xpos, ypos, oldX, oldY;
        private Brush paddleBrush;

        /// <summary>
        /// Creates a new paddle. Should only need one per game
        /// Gives option of having two player games
        /// </summary>
        /// <param name="padBrush">Brush used to draw the paddle</param>
        /// <param name="initialX">topleft X-coordinate</param>
        /// <param name="initialY">topleft Y-coordinate</param>
        /// <param name="initialWidth">Initial width of the paddle</param>
        /// <param name="initialHeight">Initial height of the paddle</param>
        public Paddle(Brush padBrush, int initialX, int initialY, int initialWidth, int initialHeight)
        {
            paddleBrush = padBrush;
            width = initialWidth;
            height = initialHeight;
            xpos = initialX;
            ypos = initialY;
        }

        /// <summary>
        /// Draw the paddle as a rectangle
        /// </summary>
        /// <param name="paper">The graphics object to draw the paddle on</param>
        public void Draw(Graphics paper)
        {
            paper.FillRectangle(paddleBrush, xpos, ypos, width, height);
        }


        /// <summary>
        /// Takes the current position of the ball and stores it in a variable.
        /// Used to calculate the velocity of the paddle from tick to tick,
        /// so it's speed can influence the ball's speed when they collide.
        /// </summary>
        public void StorePosition()
        {
            oldX = xpos;
            oldY = ypos;
        }

        public int X
        {
            get { return xpos; }
            set { xpos = value; }
        }
        public int Y
        {
            get { return ypos; }
            set { ypos = value; }
        }

        public int OldX { get { return oldX; } }
        public int OldY { get { return oldY; } }
        public int Width
        {
            get { return width; }
            set { width = value; }
        }
        public int Height
        {
            get { return height; }
            set { height = value; }
        }
        public int Left { get { return xpos; } }
        public int Right { get { return xpos + width; } }
        public int Top { get { return ypos; } }
        public int Bottom { get { return ypos + height; } }
        public Rectangle Rectangle { get { return new Rectangle(xpos, ypos, width, height); } }
    }

}
