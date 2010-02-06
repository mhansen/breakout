using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BreakOut
{
    class Brick
    {
        private int x, y, width, height, strength, type;

        Image[] materials = { //an array containing all the pictures of the bricks
            Resource1.orangeBrick,
            Resource1.purpleBrick,
            Resource1.tanBrick,
            Resource1.stoneBrick };

        Brush[] strengthBrush = { //brush used to draw the strength indicator (like a traffic light)
            null, //if strength is 0 then the brick will not be drawn
            Brushes.Lime,
            Brushes.Orange,
            Brushes.Red };

        private bool multiBall = false;
        private bool shrinkPaddle = false;
        private bool stretchPaddle = false;
        private bool ballInside = false;

        /// <summary>
        /// How many more hits will it take to disable the brick
        /// </summary>
        public int Strength { get { return strength; } }

        public Point Centre { get { return new Point(Left + Width / 2, Top + Height / 2); } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public int Left { get { return x; } }
        public int Right { get { return x + width; } }
        public int Top { get { return y; } }
        public int Bottom { get { return y + height; } }

        //true if the brick does have the powerup
        public bool powerShrink { get { return shrinkPaddle; } }
        public bool powerStretch { get { return stretchPaddle; } }
        public bool powerMulti { get { return multiBall; } }
        public bool powerBall { get { return ballInside; } }

        /// <summary>
        /// Generates a brick with random strength, powerups and appearance
        /// </summary>
        /// <param name="xPos">X-coordinate of the topleft corner of the brick</param>
        /// <param name="yPos">Y-coordinate of the topleft corner of the brick</param>
        /// <param name="widthx">Width of the brick</param>
        /// <param name="heighty">Height of the brick</param>
        public Brick(int xPos, int yPos, int widthx, int heighty, Random randy)
        {
            x = xPos;
            y = yPos;
            width = widthx;
            height = heighty;

            strength = randy.Next(1, 4);
            type = randy.Next(materials.Length); //generate a random material of brick

            int powers = randy.Next(0, 101); 
            AssignPowerUp(powers); 
        }

        /// <summary>
        /// Draw the brick, with a small "traffic-light" style indicator in the centre showing the strength
        /// </summary>
        /// <param name="paper">The graphics object to draw the brick on</param>
        public void Draw(Graphics paper)
        {
            Rectangle brickRect = new Rectangle(x, y, width, height);
            Image brickImage = materials[type];
            paper.DrawImageUnscaledAndClipped(brickImage,brickRect); //draw the brick
            paper.DrawRectangle(Pens.Black, x, y, width, height); //draw the brick border

            int radius = height/6;
            Rectangle trafficLight = new Rectangle(Centre.X - radius, Centre.Y - radius, 2 * radius, 2 * radius);
            paper.FillEllipse(strengthBrush[strength], trafficLight); //fill traffic light display of strength
            paper.DrawEllipse(Pens.Black, trafficLight); //border
        }

        /// <summary>
        /// Decreases the strength of the brick by one
        /// </summary>
        public void Hit()
        {
            if (strength > 0) strength--;
        }

        /// <summary>
        /// Called after a brick is hit
        /// Returns a high score if the brick is destoryed, smaller score if it isn't destroyed
        /// </summary>
        /// <returns>Base score of the brick that was hit</returns>
        public int CalculateScore()
        {
            int score = 10 / (strength + 1); //strength can be zero: calculuate based on previous strength
            return score;
        }

        /// <summary>
        /// Takes a number and sets the powerups according to that number.
        /// </summary>
        /// <param name="powers">A number between 0 and 100</param>
        private void AssignPowerUp(int powers)
        {
            if (0 <= powers && powers <= 4) ballInside = true;
            if (5 <= powers && powers <= 9) multiBall = true;
            if (91 <= powers && powers <= 95) shrinkPaddle = true;
            if (96 <= powers && powers <= 100) stretchPaddle = true;
        }

        public override string ToString()
        {
            string s = "Brick at " + x + "," + y;
            return s;
        }
    }
}
