using Microsoft.Xna.Framework;

namespace Airman
{
    public class Enemy : ModifiedPlayer
    {

        public void Activate(Vector2 pos, int health)
        {
            Health = health;
            Activate(pos);
        }

        public new void DeActivate()
        {
            timeForTermination = false;
            base.DeActivate();
        }

        public int checkProjectileCollision(Projectile[] projs)
        {
            int damage = 0 ;
            for(int i = 0; i < projs.Length; i++)
            {
                if (projs[i].IsActive())
                {
                    if (DrawRectangle.Intersects(projs[i].DrawRectangle))
                    {
                        projs[i].DeActivate();
                        damage = 40;
                        break;
                    }
                }
            }

            return damage;
        }

        public bool checkPlayerCollision(Rectangle[] rect)
        {
            /*if()
            {
                if(timeForTermination == false)
                    player.Health -= 50;

                return true;
            }

            return false;*/

            bool returnValue = false;
            foreach(Rectangle r in rect)
            {
                if (DrawRectangle.Intersects(r))
                    returnValue = true;
            }

            return returnValue;
        }

        //All drawing...
        public void enemyUpdate(float deltaTime, Vector2 angle, Rectangle[] playerRect, Projectile[] projs)
        {
            //Add code here
            //Movement...
            if (IsActive())
            {
                //Should we De-Activate?
                if (DrawRectangle.X < 0)
                    DeActivate();

                if (DrawRectangle.Y > GameInformation.screenHeight || DrawRectangle.Y < 0)
                    DeActivate();

                /*if (checkPlayerCollision(playerRect))
                    Health = 0; //Die...*/

                //If Health is 0...
                if (Health <= 0)
                {
                    if(timeForTermination == false)
                        Controller.explodeEffect.Play();

                    Health = totalHealth; //Make sure Health can't be lesser than Zero...
                                          //Play explosion sound...
                    isExploding = true;
                    timeForTermination = true;
                }

                //If terminating...
                if (timeForTermination)
                {
                    //play end animation...
                    //if Current frame is last of that animation... deactivate...
                    if (explosionSpr.CurrentFrame < explosionSpr.TotalFrames)
                    {
                        explosionSpr.UpdateAnimation(deltaTime, 0, explosionSpr.TotalFrames, false, false);
                    }
                    else
                    {
                        explosionSpr.CurrentFrame = 0;
                        isExploding = false;
                        timeForTermination = false;
                        DeActivate();     //Won't draw helicopter, but should drawa explosion
                    }
                }
                else
                {
                    //Active...
                    UpdateAnimation(deltaTime, 0, 7, true, true);

                    int attackHealth = checkProjectileCollision(projs);
                    if(attackHealth > 0)
                    {
                        Health -= attackHealth;
                        Controller.score += 10;
                    }
                    //Health = Health - checkProjectileCollision(projs);

                    float xSpeed, ySpeed;

                    //Move to the end of the screen...
                    xSpeed = MoveSpeed * angle.X;
                    ySpeed = MoveSpeed * angle.Y;

                    Update(deltaTime);

                    //Movement presumably...
                    Move(DrawRectangle.X + xSpeed, DrawRectangle.Y + ySpeed);
                }
            }
        }

        /*private void DrawHealth(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
        {
            batch.Draw(Game1.collisionTexture, totalHealthRect, Color.Black);
            batch.Draw(Game1.collisionTexture, healthRect, Color.Yellow);
        }*/

        public void drawEnemy(Microsoft.Xna.Framework.Graphics.SpriteBatch batch, int i)
        {
            //sprite.DrawSource(batch);
            if (IsActive())
            {
                if (timeForTermination == false)
                {
                    DrawSourceFlippedColor(batch, Color.Red);
                    DrawHealth(batch, Color.Red);
                }
            }

            if(timeForTermination == true)
            {
                explosionSpr.DrawSource(batch);
            }

            /*batch.DrawString(GameInformation.infoFont, "Health = " + Health + ", isActive = " + IsActive(),
                new Vector2(150, 40 * i), Color.Black);*/
            /*batch.DrawString(GameInformation.infoFont, "IsActive() = " + IsActive() +
                "timeForDeletion = " + timeForDeletion,
                new Vector2(150, (40 * i) + 20), Color.Red);*/
        }
    }
}
