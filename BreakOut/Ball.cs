using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BreakOut
{
    class Ball
        {
        private bool active;
        private float x, y, dX, dY, radius;
        private Brush ballBrush;
        
        /// <summary>
        /// The Brush object used to draw the ball with
        /// </summary>
        public Brush Brush
        {
            get { return ballBrush; }
            set { ballBrush = value; }
        }

        public float X { get { return x; } }
        public float Y { get { return y; } }
        public float Radius { get { return radius; } }
        public float Left { get { return x - radius; } }
        public float Right { get { return x + radius; } }
        public float Top { get { return y - radius; } }
        public float Bottom { get { return y + radius; } }
        public float XSpeed { get { return dX; } }
        public float YSpeed { get { return dY; } }

        /// <summary>
        /// Returns true if the ball is active, i.e. it can interact with bricks
        /// True unless the ball is a powerup that hasn't touched the paddle yet
        /// </summary>
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        /// <summary>
        /// Makes a new, active ball
        /// </summary>
        /// <param name="ball">Brush used to draw the ball</param>
        /// <param name="rad">Radius of the ball</param>
        /// <param name="centreX">Initial x-position of the centre of the ball</param>
        /// <param name="centreY">Initial y-position of the centre of the ball</param>
        /// <param name="Xspeed">Initial delta-x speed of the ball. Positive is right.</param>
        /// <param name="Yspeed">Initial delta-y speed of the ball. Positive is down.</param>
        public Ball(Brush ball, float rad, float centreX, float centreY, float Xspeed, float Yspeed)
        {
            radius = rad;
            x = centreX;
            y = centreY;
            dX = Xspeed;
            dY = Yspeed;
            ballBrush = ball;
            this.Active = true;
        }

        /// <summary>
        /// Makes a new ball, specifying whether the ball is active
        /// </summary>
        /// <param name="ball">Brush used to draw the ball</param>
        /// <param name="rad">Radius of the ball</param>
        /// <param name="centreX">Initial x-position of the centre of the ball</param>
        /// <param name="centreY">Initial y-position of the centre of the ball</param>
        /// <param name="Xspeed">Initial delta-x speed of the ball. Positive is right.</param>
        /// <param name="Yspeed">Initial delta-y speed of the ball. Positive is down.</param>
        /// <param name="activeBall">Will the ball interact with bricks yet?</param>
        public Ball(Brush ball, float rad, float centreX, float centreY, float Xspeed, float Yspeed, bool activeBall)
        {
            radius = rad;
            x = centreX;
            y = centreY;
            dX = Xspeed;
            dY = Yspeed;
            ballBrush = ball;
            active = activeBall;
        }

        /// <summary>
        /// Initialises a ball with initial dY = radius and dX = 0
        /// i.e. used for powerup balls
        /// </summary>
        /// <param name="ball">Brush used to draw the ball</param>
        /// <param name="rad">Radius of the ball</param>
        /// <param name="centreX">Centre of the ball (x)</param>
        /// <param name="centreY">Centre of the ball (y)</param>
        /// <param name="activeBall">Will the ball interact with bricks?</param>
        public Ball(Brush ball, float rad, float centreX, float centreY, bool activeBall)
        {
            radius = rad;
            x = centreX;
            y = centreY;
            dX = 0; 
            dY = radius; 
            ballBrush = ball;
            active = activeBall;
        }
         
        
        /// <summary>
        /// Draw the ball centred on x,y
        /// </summary>
        /// <param name="paper">Graphics object to draw the ball on</param>
        public void Draw(Graphics paper)
        {
            paper.FillEllipse(ballBrush, x - radius, y - radius, 2 * radius, 2 * radius);
        }

        /// <summary>
        /// Increments the position of the ball
        /// </summary>
        public void Move()
        {
            x += dX;
            y += dY;
        }

        /// <summary>
        /// Advanced Collision Model between the ball and a paddle
        /// If the ball hits a corner of the paddle, it is reflected back
        /// If the ball hits the top of the paddle, it's y-speed is inverted
        /// If the paddle is moving in any direction, it will nudge the ball in that direction
        /// Also ensures the ball moves no further than it's radius per timer tick
        /// </summary>
        /// <param name="paddle">The paddle to detect collisions with</param>
        public void Collide(Paddle paddle)
        {
            ReverseY(); //no matter what, the y-speed is always reversed

            //if it will impact on any corner, reverse the X
            if ((paddle.Top > this.Top +dY  && paddle.Top < this.Bottom + dY)
                //does the ball intersect the top of the paddle AND
                && (paddle.Left > this.Left + dX && paddle.Left < this.Right + dX && dX > 0)
                //is the ball approaching from the left and intersecting the left?
                || (paddle.Right > this.Left + dX && paddle.Right < this.Right + dX && dX < 0))
                //is the ball approaching from the right and intersecting the right?
                ReverseX();

            int ratio = 4; //ratio of paddle speed : ball speed change

            dX += (paddle.X - paddle.OldX)/ratio;
            dY += (paddle.Y - paddle.OldY)/ratio;
            //increment the speed by the speed of the paddle

            if (dX >  radius) dX = radius;
            if (dX < -radius) dX = -radius;
            if (dY > radius) dY = radius;
            if (dY < -radius) dY = -radius;
            //catch if dX or dY is more than the radius
            //the collision detection doesn't work if the ball travels too fast
            //the ball could go right through a brick or wall or paddle without being detected
            //also the game is more fun if the ball can't get too fast
            
            this.Active = true;
            //if the ball wasn't active before, i.e. it is a powerup from a brick, it is active now
        }

        /// <summary>
        /// Converts the ball to a string
        /// </summary>
        /// <returns>Returns e.g "Ball { x, y, X-velocity, Y-velocity }"</returns>
        public override string ToString()
        {
            string s = "Ball {X = " + x + ", Y = " + y + ", dX = " + dX + ", dY = " + dY + "}";
            return s;
        }

        /// <summary>
        /// Invert the x-direction of the ball
        /// </summary>
        public void ReverseX() { dX = -dX; }
        /// <summary>
        /// Invert the y-Direction of the ball
        /// </summary>
        public void ReverseY() { dY = -dY; }

    }
}
