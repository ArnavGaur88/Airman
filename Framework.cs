using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Airman
{
    class Framework
    {
    }

    struct GameInformation
    {
        //contains static info about stuff like screen width, screen height...
        //Also contains a spritefont...
        public static int screenWidth, screenHeight;
        public static SpriteFont infoFont;

        public static void Initialize(int scWidth, int scHeight, SpriteFont font)
        {
            screenWidth = scWidth;
            screenHeight = scHeight;
            infoFont = font;
        }
    }

    public class Sprite
    {
        //animation also...
        protected Texture2D sprite;

        //animation...
        protected int currentFrame, totalFrames, frameRate;
        float animationTiming;
        protected int frameWidth, frameHeight;
        protected int rows, cols;
        protected bool isAnimating = true;

        //activation...
        protected bool isActive;

        //Animation Controls...
        public int CurrentFrame
        {
            get { return currentFrame; }
            set
            {
                if (value > totalFrames)
                    currentFrame = totalFrames;
                else
                    currentFrame = value;
            }
        }

        public int TotalFrames
        {
            get { return totalFrames; }
        }

        public Rectangle drawRectangle;
        protected Rectangle sourceRect;

        public void Initialize(Texture2D spr, Vector2 pos, int width, int height)
        {
            sprite = spr;

            isActive = false;

            isAnimating = false;
            drawRectangle = new Rectangle((int)pos.X, (int)pos.Y, width, height);
        }

        public void Activate(Vector2 pos)
        {
            if (isActive == false)
            {
                drawRectangle.X = (int)pos.X;
                drawRectangle.Y = (int)pos.Y;

                isActive = true;
            }
        }

        public void DeActivate()
        {
            if (isActive == true)
                isActive = false;
        }

        public bool IsActive()
        {
            return isActive;
        }

        public void InitializeAnimation(int totalFrames, int frameRate, int rows, int cols)
        {
            currentFrame = 0;
            this.totalFrames = totalFrames;
            this.frameRate = frameRate;

            this.rows = rows;
            this.cols = cols;

            frameWidth = sprite.Width / rows;
            frameHeight = sprite.Height / cols;

            sourceRect = new Rectangle(0, 0, frameWidth, frameHeight);
        }

        public void UpdateAnimation(float deltaTime, int first, int lastFrame, bool loop, bool topDown)
        {
            if (animationTiming > frameRate)
            {
                animationTiming = 0;
                if (currentFrame < lastFrame)
                {
                    currentFrame += 1;
                }
                else if (currentFrame == lastFrame)
                {
                    //Should stay at the current frame, in theory...
                    if (loop)
                        currentFrame = first;
                }
            }
            else
                animationTiming += deltaTime;

            //Make sure source rectangle fits
            UpdateSourceRect(topDown);
        }

        public void Draw(SpriteBatch batch)
        {
            if (isActive == true)
                batch.Draw(sprite, drawRectangle, Color.White);
        }

        public void DrawColoured(SpriteBatch batch, Color color)
        {
            if (isActive == true)
                batch.Draw(sprite, drawRectangle, color);
        }

        public void DrawFlipped(SpriteBatch batch)
        {
            if (isActive == true)
                batch.Draw(sprite, drawRectangle, null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
        }

        public void DrawSource(SpriteBatch batch)
        {
            if (isActive == true)
                batch.Draw(sprite, drawRectangle, sourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }

        public void DrawSourceFlipped(SpriteBatch batch)
        {
            if (isActive == true)
                batch.Draw(sprite, drawRectangle, sourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
        }

        public void DrawSourceFlippedColor(SpriteBatch batch, Color col)
        {
            if (isActive == true)
                batch.Draw(sprite, drawRectangle, sourceRect, col, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
        }

        public void DrawSourceValues(SpriteBatch batch, SpriteFont font)
        {
            batch.DrawString(font, "Source = (" + sourceRect.X + ", " + sourceRect.Y + ")",
                new Vector2(150, 150), Color.Black);
        }

        public void UpdateSourceRect(bool topDown)
        {
            if (isActive == true)
            {
                if (topDown == false)
                {
                    sourceRect.X = currentFrame % rows;
                    sourceRect.Y = currentFrame / rows;
                }
                else
                {
                    sourceRect.X = currentFrame / cols;
                    sourceRect.Y = currentFrame % cols;
                }

                sourceRect.X *= frameWidth;
                sourceRect.Y *= frameHeight;

                //batch.Draw(sprite, sourceRect, drawRectangle, Color.White);
                //batch.Draw(sprite, drawRectangle, sourceRect, Color.White);
            }
        }

        public void Move(float x, float y)
        {
            drawRectangle.X = (int)x;
            drawRectangle.Y = (int)y;
        }
    }


    /*
     * Steps to use:
     * Call Initialize
     * Activate
     */
    public class Player : Sprite
    {
        /*
         * Sprite - this structure contains the following:
         *      Texture
         *      Position                -- REDUNDANCY, REMOVED
         *      DrawRectangle
         *      Basic Drawing Method
         */

        public int Health;
        protected int totalHealth;

        //protected Sprite sprite;  //private...

        //for collision detection...
        protected Rectangle futureRect;

        //States...
        //Removed some states, not as important in a framework
        public bool timeForDeletion;         //Ending state has passed...
        public bool timeForTermination;      //Ending state starts...

        //Touch input...
        //private TouchCollection touches;

        //Movement...
        protected float MoveSpeed = 1f;
        protected float xSpeed = 0f;
        protected float ySpeed = 0f;

        //Draw Rectangle
        public Rectangle DrawRectangle
        {
            get { return drawRectangle; }
        }

        //Projectiles are a collection of sprites...

        //Initialization...
        public void Initialize(Texture2D spr, Vector2 pos, int width, int height, int health)
        {
            base.Initialize(spr, pos, width, height);

            futureRect = new Rectangle((int)pos.X, (int)pos.Y, width, height);

            timeForTermination = false;
            timeForDeletion = false;

            Health = health;
            totalHealth = health;
        }

        //collision detection...
        public void checkCollisionAndMove3(Vector2 destVec, List<Rectangle> rects)
        {
            futureRect.X = DrawRectangle.X;
            futureRect.Y = DrawRectangle.Y;

            //which positions are free...


            //This needs to be implemented because
            //checking is done on a list or rectangles
            //all rects should have checking done on basis of
            //original position
            float oldX = futureRect.X;
            float oldY = futureRect.Y;

            float xMove = destVec.X, yMove = destVec.Y;

            foreach (Rectangle rect in rects)
            {
                //check xFree and yFree...


                futureRect.X = (int)destVec.X;
                if (futureRect.Intersects(rect))
                {
                    //Move futureRect x to correct position...
                    if (futureRect.X < rect.X)
                        xMove = rect.X - futureRect.Width;
                    else
                        xMove = rect.X + rect.Width;
                }

                futureRect.X = (int)oldX;

                futureRect.Y = (int)destVec.Y;
                if (futureRect.Intersects(rect))
                {
                    if (futureRect.Y < rect.Y)
                        yMove = rect.Y - futureRect.Height;
                    else
                        yMove = rect.Y + rect.Height;
                }

                futureRect.Y = (int)oldY;
            }

            Move(xMove, yMove);
        }

        //Speed and collision checking
        protected void Update(float deltaTime)
        {
            //future rect positioning...
            futureRect.X = DrawRectangle.X;
            futureRect.Y = DrawRectangle.Y;

            //Make speed frame-independent...
            xSpeed *= deltaTime;
            ySpeed *= deltaTime;
        }

        protected Player checkEntityCollisions(Player[] players)
        {
            if (players.Length < 0)
                return null;
            else
            {
                foreach (Player pl in players)
                {
                    if (pl.IsActive())
                    {
                        if (drawRectangle.Intersects(pl.DrawRectangle))
                            return pl;
                    }
                }
            }

            return null;
        }
    }

    struct Circle
    {
        //radius and position of center...
        Vector2 Origin;

        public int X
        {
            get { return (int)Origin.X; }
        }

        public int Y
        {
            get { return (int)Origin.Y; }
        }

        public float Radius;

        public Rectangle getDrawRect()
        {
            Rectangle rect = new Rectangle((int)(X - Radius),
                (int)(Y - Radius), (int)Radius * 2, (int)Radius * 2);

            return rect;
        }

        public Circle(Vector2 Origin, float Radius)
        {
            this.Origin = Origin;
            this.Radius = Radius;
        }

        public bool Intersects(Rectangle rect)
        {
            //check whether inside rectangle...
            return false;
        }

        public bool Intersects(Circle circ)
        {
            Vector2 Position = new Vector2(X, Y);
            Vector2 othPosition = new Vector2(circ.X, circ.Y);
            Vector2 dist = othPosition - Position;

            if (dist.Length() < (Radius + circ.Radius))
                return true;

            return false;
        }

        public void Move(float xMove, float yMove)
        {
            Origin.X += xMove;
            Origin.Y += yMove;
        }
    }

    struct Background2D
    {
        //tiles and collision positions...
        public static List<Rectangle> collisionPositions;
        public static List<Sprite> tiles;
        public static List<Texture2D> parallaxLayers;
        private static Rectangle[] parallaxPositions;

        //Testing...
        private static Color testingColor;

        public static void Initialize()
        {
            collisionPositions = new List<Rectangle>();
            tiles = new List<Sprite>();
            parallaxLayers = new List<Texture2D>();
        }

        public static void InitializeParallax()
        {
            parallaxPositions = new Rectangle[parallaxLayers.Count];

            for (int i = 0; i < parallaxLayers.Count; i++)
                parallaxPositions[i] = new Rectangle(0, 0, GameInformation.screenWidth,
                    GameInformation.screenHeight);
        }

        public static void AddTesting()
        {
            //Color
            testingColor = new Color(Color.Black, 0.5f);

            /*collisionPositions.Add(new Rectangle(63, 21, 111, 37));
            collisionPositions.Add(new Rectangle(72, 0, 28, 36));
            collisionPositions.Add(new Rectangle(10, 15, 92, 84));
            collisionPositions.Add(new Rectangle(150, 321, 19, 62));*/
            collisionPositions.Add(new Rectangle(0, 300, 300, 150));
        }

        public static void TestingParallaxInfo(SpriteBatch batch)
        {
            for (int i = 0; i < parallaxLayers.Count; i++)
                batch.DrawString(GameInformation.infoFont, "(" + parallaxPositions[i].X + ", " +
                    parallaxPositions[i].Y + "}", new Vector2(0, (i * 50)), Color.Red);
        }

        public static void Draw(SpriteBatch batch)
        {
            foreach (Sprite tile in tiles)
                tile.Draw(batch);
        }

        public static void DrawTesting(SpriteBatch batch, Texture2D spr)
        {
            foreach (Rectangle rect in collisionPositions)
                batch.Draw(spr, rect, testingColor);
        }

        public static void DrawLayer(SpriteBatch batch, Texture2D layer, Rectangle drawPos)
        {
            batch.Draw(layer, drawPos, Color.White);
        }

        public static void DrawParallaxing(SpriteBatch batch)
        {
            for (int i = 0; i < parallaxLayers.Count; i++)
            {
                if (parallaxPositions[i].X < 0)
                    DrawLayer(batch, parallaxLayers[i],
                        new Rectangle(parallaxPositions[i].X + parallaxPositions[i].Width,
                        parallaxPositions[i].Y, parallaxPositions[i].Width, parallaxPositions[i].Height));

                DrawLayer(batch, parallaxLayers[i], parallaxPositions[i]);
            }
        }

        public static void DrawDynamicParallaxing(SpriteBatch batch)
        {
            /*for(int i = 0; i < parallaxLayers.Count; i++)
            {
                if(parallaxPositions[i].X < 0)
                    DrawLayer(batch, parallaxLayers[i], 
                        new Rectangle(parallaxPositions[i].X + parallaxPositions[i].Width,
                        parallaxPositions[i].Y, ))
            }*/
        }

        public static void UpdateParallaxing(double deltaTime, double speed)
        {
            //double deltaTime = gameTime.ElapsedGameTime.TotalMilliseconds / 2;
            //deltaTime /= 2f;

            for (int i = 0; i < parallaxLayers.Count; i++)
            {
                //first move...
                parallaxPositions[i].X -= (int)(i * (speed) * (deltaTime));

                //then check
                if (parallaxPositions[i].X + parallaxPositions[i].Width < 0)
                    parallaxPositions[i].X += parallaxPositions[i].Width;
            }
        }

        public static void UpdateDynamicParallax(double deltaTime, Vector2 normalizedSpeedVector)
        {
            //Move in direction of speed...
            //Assume speedVector is normalized
            deltaTime /= 2f;

            for (int i = 0; i < parallaxLayers.Count; i++)
            {
                parallaxPositions[i].X += (int)(normalizedSpeedVector.X * (i * (deltaTime / 2)));
                parallaxPositions[i].Y += (int)(normalizedSpeedVector.Y * (i * (deltaTime / 2)));

                //If out of bounds...
                if (parallaxPositions[i].X + parallaxPositions[i].Width < 0)
                    parallaxPositions[i].X += parallaxPositions[i].Width;

                if (parallaxPositions[i].Y + parallaxPositions[i].Height < 0)
                    parallaxPositions[i].Y += parallaxPositions[i].Height;

                if (parallaxPositions[i].X > parallaxPositions[i].Width)
                    parallaxPositions[i].X -= parallaxPositions[i].Width;

                if (parallaxPositions[i].Y > parallaxPositions[i].Height)
                    parallaxPositions[i].Y -= parallaxPositions[i].Height;
            }
        }
    }
}
