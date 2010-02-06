using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.Diagnostics;

namespace BreakOut
{
    public partial class Form1 : Form
    {
        private Paddle paddle1;
        private int borderWidth = 20; // the width of the black border at each edge of the screen
        private int lives = 3;
        private int score = 0;
        private const int brickColumns = 10; //columns in the brick matrix
        private const int brickRows = 7; //rows in the matrix of bricks on the screen
        private int brickCount; //keeps track of the number of active bricks
        private int level = 1;

        private SoundPlayer brickSound = new SoundPlayer(Resource1.brick);
        private SoundPlayer wallSound = new SoundPlayer(Resource1.wall);
        private SoundPlayer paddleSound = new SoundPlayer(Resource1.paddle);

        private Brick[,] brickArray = new Brick[brickColumns,brickRows];
        private List<Ball> ballList = new List<Ball>(1);
        private List<PowerUp> powerUpList = new List<PowerUp>();
        private Image[] backgrounds = { Resource1.desertBG, Resource1.CactusBG };
        
        public Form1()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            InitializeGame();
        }

        /// <summary>
        /// Sets up a new game. Called whenever the bricks/balls/powerups need to be reset
        /// </summary>
        private void InitializeGame()
        {
            brickCount = brickRows * brickColumns; //resets the brick count
            powerUpList.Clear();
            this.BackgroundImage = backgrounds[level % backgrounds.Length];
            //changes background with each level
            //rotates back to the beginning of the image array after the level number > number images in array
            paddle1 = new Paddle(Brushes.Black, 100, 100, 50, 10);
            InitializeBall(); //creates one ball
            int brickHeight = 20;
            int brickPadding = 5;
            int borderPadding = 50;
            InitializeBricks(brickArray, brickColumns, brickRows, brickHeight, brickPadding, borderPadding);
        }

        /// <summary>
        /// Resets the list of balls to just one ball in the centre of the screen with random downward velocity
        /// </summary>
        private void InitializeBall()
        {
            ballList.Clear();
            Random randy = new Random();
            int radius = 7;
            int dX = randy.Next(-radius, radius);
            int dY = randy.Next(radius);
            ballList.Add(new Ball(Brushes.White, radius, this.Width / 2, this.Height / 2,dX,dY)); //initialise ball
        }

        /// <summary>
        /// Initialises a two-dimensional array by filling it with bricks
        /// </summary>
        /// <param name="brickArray">The array to be filled</param>
        /// <param name="columns">Number of columns of bricks</param>
        /// <param name="rows">Number of rows of bricks</param>
        /// <param name="brickHeight">Height of each brick (px)</param>
        /// <param name="brickPadding">Padding between each brick (px)</param>
        /// <param name="borderPadding">Padding at the sides of the group of all the bricks (px)</param>
        private void InitializeBricks(Brick[,] brickArray, int columns, int rows, int brickHeight, int brickPadding, int borderPadding)
        {
            int brickWidth = ((this.Width - borderPadding * 2) / brickColumns) - brickPadding;
            int columnWidth = ((this.Width - borderPadding * 2) / brickColumns);
            int rowHeight = brickHeight + brickPadding;

            Random randy = new Random(); 
            for (int j = 0; j < brickRows; j++)
            {
                for (int i = 0; i < brickColumns; i++)
                {
                    int x = borderPadding + i * columnWidth;
                    int y = (j * rowHeight) + borderPadding;

                    brickArray[i, j] = new Brick(x, y, brickWidth, brickHeight, randy); //adds a random brick
                }
            }
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            //start the game on a double-click
            timer1.Enabled = true;
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            LoopThroughBalls(); //check the status of each ball and move each one
            LoopThroughPowerUps(); //check if a powerup collides with the paddle
            if (ballList.Count == 0) LostAllBalls();
            paddle1.StorePosition();
            if (brickCount == 0) NextLevel();
            this.Invalidate();  //repaint the screen
        }
        /// <summary>
        /// Check if each ball hits a wall, brick or paddle, then moves it
        /// If the ball falls off the bottom, remove it from memory.
        /// </summary>
        private void LoopThroughBalls()
        {
            for (int n = ballList.Count - 1; n >= 0; n--) //cycle through balls
            {
                if (ballList[n].Active == true) //ignore bricks if ball hasnt hit the paddle yet
                {
                    DetectWallCollision(ballList[n]);
                    CheckBricks(ballList[n], brickArray); //check if the ball collides with any bricks
                }
                DetectPaddleCollision(ballList[n], paddle1);
                ballList[n].Move();
                if (ballList[n].Top > this.Height) ballList.RemoveAt(n);
                //if it falls off the screen, remove it from the list
            }
        }

        /// <summary>
        /// Checks if the ball is about to hit the side or top walls, and reverses it's direction if necessary
        /// </summary>
        /// <param name="ball">The ball to check for collision</param>
        private void DetectWallCollision(Ball ball)
        {
            bool hit = false;
            if ((ball.Right + ball.XSpeed > this.Width - borderWidth) //right border
                || (ball.Left + ball.XSpeed < borderWidth)) //left border
            {
                hit = true;
                ball.ReverseX();
            }
            //is the ball about to hit the top border?
            if (ball.Top + ball.YSpeed < borderWidth)
            {
                hit = true;
                ball.ReverseY();
            }
            if (hit) { wallSound.Play(); }
        }

        /// <summary>
        /// Given a ball and brick, checks them for collision
        /// if they collide, checks if the brick had powerups inside it
        /// </summary>
        /// <param name="ball">Ball object to check for collision</param>
        /// <param name="bricks">Brick object to check for powerups</param>
        private void CheckBricks(Ball ball, Brick[,] bricks)
        {
            bool ultimatelyReverseX = false; //these variables exist so that the ball can only be reversed
            bool ultimatelyReverseY = false; //once per timer tick, otherwise sometimes the ball can be reversed twice
                                        //and plough right through bricks
            foreach (Brick brick in bricks)
            {
                bool hit, reverseX, reverseY;
                if (brick.Strength > 0) //only detect collision if the brick hasn't been hit
                {
                    //perform collision detection
                    DetectBrickCollision(ball, brick, out hit, out reverseX, out reverseY);
                    if (hit)
                    {
                        brick.Hit(); //subtract the strength
                        if (brick.Strength == 0)
                        {
                            CheckBrickForPowerUps(brick); //check if the brick had any powerups
                            brickCount--; //update the new brickcount
                            }
                        score += level * brick.CalculateScore();
                        brickSound.Play();
                        if (reverseX) ultimatelyReverseX = true;
                        if (reverseY) ultimatelyReverseY = true;
                    }
                }
            }
            if (ultimatelyReverseY) ball.ReverseY();
            if (ultimatelyReverseX) ball.ReverseX();
        }

        /// <summary>
        /// Given a ball and brick, checks if they collide, and whether the direction of the ball should be reversed
        /// </summary>
        /// <param name="ball">Ball object to check for collision</param>
        /// <param name="brick">Brick object to check for collision</param>
        /// <param name="hit">True if the ball collides with the brick</param>
        /// <param name="reverseX">True if the ball's x-direction should be reversed</param>
        /// <param name="reverseY">True if the ball's y-direction should be reversed</param>
        private void DetectBrickCollision(Ball ball, Brick brick, out bool hit, out bool reverseX, out bool reverseY)
        {
            reverseX = false; //each brick can only reverse the ball once
            reverseY = false;
            hit = false;
            //note the velocity of the ball cannot exceed its diameter - or this will not work
            //the ball could sail right through the brick

                //upwards collision
                if ((ball.YSpeed < 0) //is the ball travelling upwards?
                && (ball.Top + ball.YSpeed < brick.Bottom) //is the top of the ball inside the brick?
                && (ball.Bottom + ball.YSpeed > brick.Bottom)  //is the bottom of the ball under the brick?
                && (ball.Right + ball.XSpeed >= brick.Left) //is the right of the ball inside the brick
                && (ball.Left + ball.XSpeed <= brick.Right)) //is the left of the ball inside the brick
                {
                    hit = true;
                    reverseY = true;
                }

                //downwards collision
                if ((ball.YSpeed > 0) //is the ball travelling downwards?
                && (ball.Bottom + ball.YSpeed > brick.Top) //is the bottom of the ball inside the brick (y)
                && (ball.Top + ball.YSpeed < brick.Top) // is the top of the ball about the brick? (y)
                && (ball.Right + ball.XSpeed >= brick.Left)
                && (ball.Left + ball.XSpeed <= brick.Right))// do the x-coords match?
                {
                    hit = true;
                    reverseY = true;
                }

               //rightwards collision
               if ((ball.XSpeed > 0) //is the ball moving right
               && (ball.Left + ball.XSpeed < brick.Left) //is the left side of the ball to the left of the brick?
               && (ball.Right + ball.XSpeed > brick.Left) //is the right side of the ball inside the brick?
               && (ball.Bottom + ball.YSpeed >= brick.Top)
               && (ball.Top + ball.YSpeed <= brick.Bottom)) // do the y-coords match?
                {
                    hit = true;
                    reverseX = true;
                }

                //leftwards collision
                if ((ball.XSpeed < 0) //is the ball moving Left?
                    && (ball.Right + ball.XSpeed > brick.Right) //is the right side of the ball to the right of the brick?
                    && (ball.Left + ball.XSpeed < brick.Right) //is the left side of the ball inside the brick
                    && (ball.Bottom + ball.YSpeed >= brick.Top)
                    && (ball.Top + ball.YSpeed <= brick.Bottom))// do the y-coords match?
                {
                    hit = true;
                    reverseX = true;
                }
        }

        /// <summary>
        /// Called after a collision. Checks the brick to see if it had any powerups inside it
        /// If it does, the powerups are added to the list of powerups in play
        /// </summary>
        /// <param name="brick">The brick to check for powerups</param>
        private void CheckBrickForPowerUps(Brick brick)
        {
            if (brick.powerBall) //extra ball instantly takes effect
            {
                Random randy = new Random();
                int radius = randy.Next(3, 8); //new ball has a random radius
                ballList.Add(new Ball(Brushes.Black, radius, brick.Centre.X, brick.Centre.Y, false));
                //new ball is created centred at the centre of the old brick
            }

            //these powerups have to be caught by the paddle
            string powerUpLabel = null;
            Brush powerUpBrush = null;

            if (brick.powerShrink)
            {
                powerUpLabel = "Shrink";
                powerUpBrush = Brushes.LightBlue;
            }
            if (brick.powerStretch)
            {
                powerUpLabel = "Stretch";
                powerUpBrush = Brushes.LightGreen;
            }
            if (brick.powerMulti)
            {
                powerUpLabel = "Multi";
                powerUpBrush = Brushes.Pink;
            }

            if (powerUpLabel != null)
            {
                PowerUp powerup = new PowerUp(powerUpLabel, powerUpBrush, brick.Centre.X, brick.Centre.Y);
                powerUpList.Add(powerup);
            } //only add the powerup if a powerup is found in the brick
        }

        /// <summary>
        /// Checks if a ball is about to hit the paddle and calls the Collide() method of the ball if it is
        /// </summary>
        /// <param name="ball">Ball object to check for collision</param>
        /// <param name="paddle">Paddle object to check for collision</param>
        private void DetectPaddleCollision(Ball ball, Paddle paddle)
        {
            if ((ball.Right + ball.XSpeed > paddle.Left) &&
                (ball.Left + ball.XSpeed < paddle.Right) &&
                (ball.Bottom + ball.YSpeed > paddle.Top) &&
                (ball.Top + ball.YSpeed < paddle.Bottom) &&
                ball.YSpeed >= 0) //is the ball about to hit the paddle
            {
                ball.Collide(paddle);
                paddleSound.Play();
            }
        }

        /// <summary>
        /// Draws Background, bricks, paddle, powerups, and balls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawBackground(e.Graphics);
            foreach (Brick brick in brickArray) if (brick.Strength > 0) brick.Draw(e.Graphics);
            paddle1.Draw(e.Graphics);
            foreach (PowerUp powerup in powerUpList) powerup.Draw(e.Graphics);
            foreach(Ball ball in ballList) ball.Draw(e.Graphics);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            paddle1.X = e.X - paddle1.Width / 2;
            if (e.Y > 3 * this.Height / 4) paddle1.Y = e.Y;
            else paddle1.Y = 3 * this.Height / 4;
            //move the paddle so the centre is where the mouse is
            
            if (timer1.Enabled == false) this.Invalidate();
            //the timer tick handles the redraw, unless it isn't ticking
            //otherwise the game slows - too many mouse move events + timers
        }

        /// <summary>
        /// Moves each powerup, checking each powerup for intersection with paddle, 
        /// calling ApplyPowerUp() if an intersection occurs.
        /// Removes the powerup from play if it falls off the screen or is caught, 
        /// </summary>
        private void LoopThroughPowerUps()
        {
            for (int n = powerUpList.Count - 1; n >= 0; n--)
            {
                bool remove = false;
                if (powerUpList[n].Top > this.Height) remove = true;
                //if it falls off the screen, remove it from the list
                
                Rectangle powerRect = powerUpList[n].Rectangle;
                Rectangle paddleRect = paddle1.Rectangle;
                if (powerRect.IntersectsWith(paddleRect)) //check for intersection of powerup and paddle
                {
                    ApplyPowerUp(powerUpList[n].Type);
                    remove = true;
                }
                if (remove) powerUpList.RemoveAt(n); //remove the powerup at the end of the loop, not in the middle
                else powerUpList[n].Move();
            }
        }

        /// <summary>
        /// Called when the paddle hits a powerup.
        /// Applies the powerup to the game
        /// </summary>
        /// <param name="type">Name of the powerup</param>
        private void ApplyPowerUp(string type)
        {
            if (type == "Shrink")
            {
                if (paddle1.Width > 40) paddle1.Width = paddle1.Width - 20;
                //only if the paddle isn't too small apply this powerup
            }
            if (type == "Stretch") //stretch
            {
                paddle1.Width = paddle1.Width + 20;
            }
            if (type == "Multi") //split ball
            {
                Ball ball = ballList[0];
                double angle = Math.Atan2(-ball.YSpeed, ball.XSpeed);
                double speed = Math.Sqrt(ball.XSpeed * ball.XSpeed + ball.YSpeed * ball.YSpeed);
                double angle1 = angle + 2*Math.PI / 3; //add 120 degrees to initial angle
                double angle2 = angle - 2*Math.PI / 3; //subtract 120 degrees from initial angle
                //convert velocity from polar to cartesian
                float dX1 = (float)(Math.Cos(angle1) * speed);
                float dY1 = (float)(-Math.Sin(angle1) * speed);
                float dX2 = (float)(Math.Cos(angle2) * speed);
                float dY2 = (float)(-Math.Sin(angle2) * speed);
                //add two new balls to play in the position of the last ball, but with different velocities
                //gives the appearance of the ball splitting into 3
                ballList.Add(new Ball(ball.Brush, ball.Radius, ball.X, ball.Y, dX1, dY1));
                ballList.Add(new Ball(ball.Brush, ball.Radius, ball.X, ball.Y, dX2, dY2));
            }
        }

        /// <summary>
        /// Draws the borders and statistics onto the screen
        /// </summary>
        /// <param name="paper">Graphics object to draw the background on</param>
        private void DrawBackground(Graphics paper)
        {
            paper.FillRectangle(Brushes.Black,0,0,borderWidth,this.Height); //left border
            paper.FillRectangle(Brushes.Black,0,0,this.Width, borderWidth); //top border
            paper.FillRectangle(Brushes.Black,this.Width-borderWidth,0,borderWidth,this.Height); //right border
            Font statsFont = new Font("Fixedsys", 8);
            paper.DrawString(
                "Level: " + level + 
                " Score: " + score,
                statsFont, Brushes.White, borderWidth, 1);
            //draws score and level in the top left
            paper.DrawString("Lives: " + lives, statsFont, Brushes.White, this.Width - 45 - borderWidth, 1);
            //draw lives in the top right
        }

        /// <summary>
        /// Called if there are no more balls on screen.
        /// </summary>
        private void LostAllBalls()
        {
            lives -= 1;             //lose a life
            timer1.Enabled = false; //pause the game
            InitializeBall();       //new ball
            powerUpList.Clear();    //clear powerups
            if (lives < 0) GameOver();
        }

        /// <summary>
        /// Restarts game
        /// </summary>
        private void GameOver()
        {
            MessageBox.Show("Game Over. Your score: " + score + " points");
            lives = 3;
            score = 0;
            InitializeGame();
        }

        /// <summary>
        /// Increments the level by one, resetting the game and awarding another life
        /// </summary>
        private void NextLevel()
        {
            timer1.Enabled = false;
            lives++; //gives the player an extra life
            level++; //next level
            MessageBox.Show("Congratulations! New Level - Extra life");
            InitializeGame();
        }
    }
}