using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Airman
{
    public class ModifiedPlayer : Player
    {
        protected Rectangle totalHealthRect;
        protected Rectangle healthRect;

        protected Sprite explosionSpr;
        protected bool isExploding = false;

        public void InitializeHealth()
        {
            totalHealthRect = new Rectangle(DrawRectangle.X, DrawRectangle.Y + DrawRectangle.Height,
                totalHealth, 10);
            healthRect = new Rectangle(totalHealthRect.X, totalHealthRect.Y,
                Health, 10);
        }

        public void InitializeExplosion(Texture2D explTex, int width, int height)
        {
            //explosionSound = explode;
            //explodSoundControl = explosionSound.CreateInstance();
            explosionSpr = new Airman.Sprite();
            explosionSpr.Initialize(explTex, new Vector2(DrawRectangle.X, DrawRectangle.Y), width, height);
            explosionSpr.Activate(new Vector2(explosionSpr.drawRectangle.X, explosionSpr.drawRectangle.Y));
        }

        public void InitializeExplosionAnimation(int totalFrames, int frameRate, int rows, int cols)
        {
            explosionSpr.InitializeAnimation(totalFrames, frameRate, rows, cols);
        }

        public bool checkPlayerCollision(Rectangle rect)
        {
            if (DrawRectangle.Intersects(rect))
                return true;

            return false;
        }

        protected void healthUpdate(float deltaTime)
        {
            totalHealthRect.X = DrawRectangle.X;
            totalHealthRect.Y = DrawRectangle.Y + (DrawRectangle.Height);
            healthRect.X = totalHealthRect.X;
            healthRect.Y = totalHealthRect.Y;

            healthRect.Width = Health;
        }

        public void ChangeMoveSpeed(float f)
        {
            MoveSpeed = f;
        }

        protected new void Update(float deltaTime)
        {
            base.Update(deltaTime);

            explosionSpr.drawRectangle.X = DrawRectangle.X;
            explosionSpr.drawRectangle.Y = DrawRectangle.Y;

            healthUpdate(deltaTime);
        }

        protected void DrawHealth(SpriteBatch batch, Color col)
        {
            batch.Draw(Controller.collisionTexture, totalHealthRect, Color.Black);
            batch.Draw(Controller.collisionTexture, healthRect, col);
        }
    }

    public class AirmanPlayer : ModifiedPlayer
    {
        //firing projectiles...
        private float fireTime;
        private int fireRate;
        bool fire;

        public AirmanPlayer()
        {
            Controller.score = 0;
            timeForTermination = false;
            //explosionSpr.CurrentFrame = 0;
        }

        public void InitializeProjectiles(Texture2D spr, int projectiles, int fireRate, int width, int height)
        {
            Lists.projectiles = new Projectile[projectiles];
            this.fireRate = fireRate;

            for (int i = 0; i < Lists.projectiles.Length; i++)
                Lists.projectiles[i].Initialize(spr, Vector2.Zero, width, height);

            //Crosshair position...
            /*if (crossHairSprite == null)
                crossHairSprite = crossSprite;*/
        }

        public void enemyHealthUpdate()
        {
            Enemy[] enemies = Controller.getEnemies();
            if (enemies != null)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i].IsActive() == true)
                    {
                        if (DrawRectangle.Intersects(enemies[i].DrawRectangle))
                        {
                            if (enemies[i].timeForTermination == false)
                            {
                                Health -= 50;
                                enemies[i].Health = 0;
                            }
                        }
                    }
                }
            }
        }

        public void Update(float deltaTime, Vector2 angle, bool firePressed)
        {
            //Update animation...
            UpdateAnimation(deltaTime, 0, 7, true, true);

            Dying(deltaTime);
            enemyHealthUpdate();

            //Initial speed...
            xSpeed = 0;
            ySpeed = 0;

            //Move accordig to angle vector...
            xSpeed = MoveSpeed * angle.X;
            ySpeed = MoveSpeed * angle.Y;

            //Move crosshair...
            //float crossXSpeed = MoveSpeed * crossAngle.X * deltaTime;
            //float crossYSpeed = MoveSpeed * crossAngle.Y * deltaTime;

            Update(deltaTime);

            float xMove = DrawRectangle.X + xSpeed;
            float yMove = DrawRectangle.Y + ySpeed;

            //Stay in bounds
            //-----------------------------------------------------------------
            if (xMove > GameInformation.screenWidth)
                xMove = GameInformation.screenWidth - DrawRectangle.Width;
            else if (xMove <= 0)
                xMove = 0f;

            if (yMove > GameInformation.screenHeight)
                yMove = GameInformation.screenHeight - DrawRectangle.Height;
            else if (yMove <= 0)
                yMove = 0f;
            //-----------------------------------------------------------------

            //Projectile checking...
            if (firePressed)
            {
                fireUpdate(deltaTime);
            }
            else
            {
                fireTime = 0f;
                fire = false;
            }

            if(fire == true)
            {
                Controller.hitSound.Play();
                for(int i = 0; i < Lists.projectiles.Length; i++)
                {
                    if (Lists.projectiles[i].IsActive() == false)
                    {
                        Lists.projectiles[i].Activate(new Vector2(DrawRectangle.X + DrawRectangle.Width, 
                            DrawRectangle.Y + DrawRectangle.Height / 2), Vector2.UnitX,
                        MoveSpeed + 0.5f);

                        break;
                    }
                }
            }

            UpdateProjectiles(deltaTime);

            Move(xMove, yMove);
        }

        private void fireUpdate(float deltaTime)
        {
            if (fireTime < fireRate)
            {
                fire = false;
                fireTime += deltaTime;
            }
            else
            {
                fire = true;
                fireTime = 0f;
            }
        }

        private void Dying(float deltaTime)
        {
            if(timeForTermination == true)
            {
                if (explosionSpr.CurrentFrame < explosionSpr.TotalFrames + 1)
                {
                    explosionSpr.UpdateAnimation(deltaTime, 0, explosionSpr.TotalFrames, false, false);
                }
                else
                    DeActivate();
            }
            else
            {
                if (Health <= 0)
                {
                    Controller.explodeEffect.Play();
                    timeForTermination = true;
                }
            }
        }

        private void UpdateProjectiles(float deltaTime)
        {
            for (int i = 0; i < Lists.projectiles.Length; i++)
                Lists.projectiles[i].Update(deltaTime);
        }

        public void updateSplash(float deltaTime, Levels gameState)
        {
            switch(gameState)
            {
                case Levels.MAIN_SPLASH:
                    Update(deltaTime, new Vector2(1, 0), false);
                    break;
            }
        }

        public void DrawWithProjectiles(SpriteBatch batch)
        {
            if (IsActive() && timeForTermination == false)
            {
                DrawSource(batch);
                DrawHealth(batch, Color.White);
                DrawProjectiles(batch);

                batch.DrawString(GameInformation.infoFont, "Score: " + Controller.score,
                    Vector2.Zero, Color.Red);

                //Crosshair...
                //DrawCrosshairs(batch, crossHairVector);
            }

            if (timeForTermination == true)
            {
                explosionSpr.DrawSource(batch);
            }
        }

        /*private void DrawCrosshairs(SpriteBatch batch, Vector2 pos)
        {
            //int sourceWidth = crossHairSprite.Width / 8;
            //int sourceHeight = crossHairSprite.Height / 8;

            batch.Draw(Controller.collisionTexture, new Rectangle((int)pos.X, (int)pos.Y, 
                crossHairWidth, crossHairHeight),
                null,
                Color.White, 0f, new Vector2(crossHairWidth / 2, crossHairHeight /2),
                SpriteEffects.None, 1f);
        }*/

        private void DrawProjectiles(SpriteBatch batch)
        {
            for (int i = 0; i < Lists.projectiles.Length; i++)
            {
                Lists.projectiles[i].Draw(batch);

                /*batch.DrawString(GameInformation.infoFont, i + " isActive = " + projectiles[i].IsActive(),
                    new Vector2(150, i * 100), Color.Black);*/
            }
        }
    }
}
