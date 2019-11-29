using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace Airman
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static bool endGame = false;
        public static bool restartGame = false;

        private Controller controller;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            //Initialize game information...
            GameInformation.Initialize(GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                Content.Load<SpriteFont>("infoFont"));

            InitializeController();

            base.Initialize();
        }

        private void InitializeController()
        {
            controller = Controller.getInstance();
            controller.Initialize(Content);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) || 
                endGame == true)
                Exit();

            // TODO: Add your update logic here

            if(restartGame == true)
            {
                restartGame = false;
                controller = null;  //Unload previous controller...

                //New game is starting, advise Garbag Collector to pick up the slack
                System.GC.Collect();

                InitializeController(); //Load new one...
            }

            //get delta time...
            float deltaTime = (float)gameTime.ElapsedGameTime.Milliseconds;

            controller.UpdateLevel(deltaTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            controller.DrawLevel(spriteBatch);

            /*spriteBatch.DrawString(GameInformation.infoFont, "Screen Dimensions = " + GameInformation.screenWidth + "X" +
                GameInformation.screenHeight, new Vector2(150, 160), Color.Black);*/

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    class Controller
    {
        //Random...
        System.Random rand = new System.Random();

        //Game Information...
        Levels gameState, prevGameState;
        AirmanPlayer[] player;
        public static Texture2D collisionTexture;
        Texture2D logo;

        Input[] input;

        int random;
        Song backgroundSong;
        public static SoundEffect explodeEffect;
        public static SoundEffect hitSound;
        Color deathColor = new Color(Color.Black, 0.5f);

        private int enemyTimer = 0;

        private Microsoft.Xna.Framework.Content.ContentManager Content;

        //Enemies...
        static Enemy[] enemies;

        public static Enemy[] getEnemies()
        {
            return enemies;
        }

        //Score...
        public static int score = 0;

        private static Controller controller = null;

        public static Controller getInstance()
        {
            if (controller == null)
                controller = new Controller();

            return controller;
        }

        public static void Unload()
        {
            if (controller != null)
                controller = null;

        /*    private void Unload()
        {
            //player = null;
            //enemies = null;
            Controller.Unload();
            MediaPlayer.Stop();
            Background2D.parallaxLayers.Clear();
        }*/
    }

        public void Initialize(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            //Get content...
            this.Content = Content;

            //Get player inputs..

            //Check if gamepad is connected...
            if (GamePad.GetCapabilities(PlayerIndex.One).IsConnected &&
                GamePad.GetCapabilities(PlayerIndex.Two).IsConnected)
            {
                input = new Input[2];
                input[0].Initialize(Input.GameType.GAMEPAD, PlayerIndex.One);
                input[1].Initialize(Input.GameType.GAMEPAD, PlayerIndex.Two);
            }
            else if (GamePad.GetCapabilities(PlayerIndex.One).IsConnected)
            {
                input = new Input[1];
                input[0].Initialize(Input.GameType.GAMEPAD, PlayerIndex.One);
            }
            else
            {
                input = new Input[1];
                input[0].Initialize(Input.GameType.KEYBOARD, PlayerIndex.One);
            }

            player = new AirmanPlayer[input.Length];    //As many players as inputs

            gameState = Levels.MAIN_MENU;
            prevGameState = Levels.MAIN_MENU;

            collisionTexture = Content.Load<Texture2D>("Entities/collisionSprite");
            logo = Content.Load<Texture2D>("AirmanLogo");
            backgroundSong = Content.Load<Song>("Sounds/Music/Mercury");
            explodeEffect = Content.Load<SoundEffect>("Sounds/SFX/explosion3");
            hitSound = Content.Load<SoundEffect>("Sounds/SFX/Hit_03");
            //explodeEffect.

            //Background...
            Background2D.Initialize();

            InitializeLevel();
        }

        private void InitializePlayer(Texture2D tex)
        {
            for(int i = 0; i < player.Length; i++)
            {
                Texture2D explodeTexture = Content.Load<Texture2D>("ExplosionSprite");
                int explTotalFrames = 16, explRows = 4, explCols = 4, explWidth = 96, explHeight = 96;
                int explFrameRate = 33;

                int width = GameInformation.screenWidth / 10;

                player[i] = new AirmanPlayer();
                player[i].Initialize(tex, Vector2.Zero,
                    width, width/3, 100);
                player[i].InitializeHealth();
                player[i].InitializeAnimation(8, 33, 2, 4);
                player[i].InitializeProjectiles(Content.Load<Texture2D>("Projectiles/bullet"),
                    10, 99, 16, 8);

                player[i].InitializeExplosion(explodeTexture,
                    explWidth, explHeight);
                player[i].InitializeExplosionAnimation(explTotalFrames, explFrameRate, explRows, explCols);
                player[i].ChangeMoveSpeed(1f);
            }
        }

        private void ChangeGameState(Levels newState)
        {
            prevGameState = gameState;
            gameState = newState;

            //Initialize the new level...
            InitializeLevel();
        }

        public void InitializeLevel()
        {
            Texture2D tex;

            switch(gameState)
            {
                case Levels.MAIN_MENU:
                    //Parallax Background...
                    Background2D.parallaxLayers.Add(Content.Load<Texture2D>("Backgrounds/parallax-Layer1"));
                    Background2D.parallaxLayers.Add(Content.Load<Texture2D>("Backgrounds/parallax-Layer2"));
                    Background2D.parallaxLayers.Add(Content.Load<Texture2D>("Backgrounds/parallax-Layer3"));
                    Background2D.parallaxLayers.Add(Content.Load<Texture2D>("Backgrounds/parallax-Layer4"));
                    Background2D.parallaxLayers.Add(Content.Load<Texture2D>("Backgrounds/parallax-Layer5"));

                    Background2D.InitializeParallax();

                    //Player Texture...
                    tex = Content.Load<Texture2D>("Entities/helicopterSpritesheet");

                    //We need player...
                    InitializePlayer(tex);
                    break;

                case Levels.MAIN_SPLASH:
                    //Start playing song...
                    MediaPlayer.Play(backgroundSong);
                    MediaPlayer.IsRepeating = true; //Play the song on a loop...
                    break;

                case Levels.LEVEL_ONE:
                    //Loading resources needed...
                    tex = Content.Load<Texture2D>("Entities/helicopterSpritesheet");
                    Texture2D explodeTexture = Content.Load<Texture2D>("ExplosionSprite");
                    int explTotalFrames = 16, explRows = 4, explCols = 4, explWidth = 96, explHeight = 96;
                    int explFrameRate = 33;

                    enemies = new Enemy[8];
                    for(int i = 0; i < enemies.Length; i++)
                    {
                        int width = GameInformation.screenWidth / 5;

                        enemies[i] = new Enemy();
                        enemies[i].Initialize(tex, new Vector2(GameInformation.screenWidth, 0),
                             width, width / 3, 100);
                        enemies[i].InitializeHealth();
                        enemies[i].InitializeAnimation(8, 33, 2, 4);
                        enemies[i].InitializeExplosion(explodeTexture, 
                            explWidth, explHeight);
                        enemies[i].InitializeExplosionAnimation(explTotalFrames, explFrameRate, explRows, explCols);
                        enemies[i].ChangeMoveSpeed(2f);
                        //enemies[i].DeActivate();
                    }
                    for(int i = 0; i < player.Length; i++)
                    {
                        player[i].ChangeMoveSpeed(1f);
                        //player[i].InitializeCrosshairs();
                    }

                    break;

                case Levels.DYING_SPLASH:
                    MediaPlayer.Stop();
                    break;

                case Levels.TESTING:
                    break;

                default:
                    break;
            }
        }

        private bool timeForTermination()
        {
            bool quit = true;
            foreach(AirmanPlayer pl in player)
            {
                if (pl.timeForTermination == false)
                    quit = false;
            }

            return quit;
        }

        public void UpdateLevel(float deltaTime)
        {
            switch (gameState)
            {
                case Levels.MAIN_MENU:
                    Background2D.UpdateParallaxing(deltaTime, 0.02f);
                    KeyboardState ks = Keyboard.GetState();
                    GamePadState gs = GamePad.GetState(PlayerIndex.One);
                    if (gs.IsButtonDown(Buttons.Start) || ks.IsKeyDown(Keys.Space))
                        ChangeGameState(Levels.MAIN_SPLASH);
                    break;

                case Levels.MAIN_SPLASH:
                    Background2D.UpdateParallaxing(deltaTime, 0.1f);

                    if(prevGameState != Levels.MAIN_SPLASH)
                    {
                        for (int i = 0; i < player.Length; i++)
                        {
                            player[i].Activate(new Vector2(0, (GameInformation.screenHeight / 2) + (40 * i)));
                            player[i].ChangeMoveSpeed(0.2f);

                            if (player[i].DrawRectangle.X < GameInformation.screenWidth / 4)
                                player[i].updateSplash(deltaTime, Levels.MAIN_SPLASH);
                            else
                                ChangeGameState(Levels.LEVEL_ONE);
                        }
                    }
                    break;

                case Levels.LEVEL_ONE:

                    if (timeForTermination() == false)
                    {
                        generateEnemies(75);   //Enemies generated after 25 reached

                        Rectangle[] playerRects = new Rectangle[player.Length];

                        for(int i = 0; i < player.Length; i++)
                        {
                            input[i].Update(deltaTime);
                            player[i].Update(deltaTime, input[i].angle, input[i].firePressed);
                            playerRects[i] = player[i].DrawRectangle;
                        }

                        for (int i = 0; i < enemies.Length; i++)
                        {
                            if (enemies[i].IsActive())
                                enemies[i].enemyUpdate(deltaTime, new Vector2(-1, 0),
                                    playerRects, Lists.projectiles);
                        }

                        Background2D.UpdateParallaxing(deltaTime, 0.2f);
                    }
                    else
                    {
                        ChangeGameState(Levels.DYING_SPLASH);
                    }
                    break;

                case Levels.DYING_SPLASH:
                    if (Keyboard.GetState().GetPressedKeys().Length > 0 ||
                        GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back))
                        Game1.endGame = true;

                    if(Keyboard.GetState().IsKeyDown(Keys.Space) ||
                        GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
                    {
                        Game1.restartGame = true;
                        ChangeGameState(Levels.MAIN_MENU);
                    }

                    for(int i = 0; i < player.Length; i++)
                        player[i].Update(deltaTime, Vector2.Zero, false);

                    break;

                case Levels.TESTING:

                    for (int i = 0; i < player.Length; i++)
                    {
                        input[i].Update(deltaTime);
                        player[i].Update(deltaTime, input[i].angle, input[i].firePressed);
                    }

                    Background2D.UpdateParallaxing(deltaTime, 0.5f);
                    break;

                default:
                    //All cases do not need an initialization stage
                    break;
            }
        }

        private void generateEnemies(int generateRate)
        {
            if (enemyTimer < generateRate)
                enemyTimer += 1;
            else
            {
                random = rand.Next(0, GameInformation.screenHeight - enemies[0].DrawRectangle.Height);

                for (int i = 0; i < enemies.Length; i++)
                    if (enemies[i].IsActive() == false)
                    {
                        enemies[i].Activate(new Vector2(GameInformation.screenWidth,
                            random), 100);
                        break;
                    }

                enemyTimer = 0;
            }
        }

        public void DrawLevel(SpriteBatch batch)
        {
            switch (gameState)
            {
                case Levels.MAIN_MENU:
                    Background2D.DrawParallaxing(batch);

                    for(int i = 0; i < player.Length; i++)
                        player[i].Draw(batch);

                    /*drawPrompt(batch, "AIRMAN ! ! !",
                        new Vector2(GameInformation.screenWidth / 2 - 80, 10),
                        Color.DarkSalmon);
                    drawPrompt(batch, "Press any Space or Start...",
                        new Vector2(GameInformation.screenWidth / 4 + 80, GameInformation.screenHeight / 2), Color.Red);*/

                 batch.Draw(logo,
                     new Vector2((GameInformation.screenWidth / 2) - (logo.Width / 2),
                     0), Color.White);
                 //testingDraw(batch);
                 break;

             case Levels.MAIN_SPLASH:
                 Background2D.DrawParallaxing(batch);

                 for(int i = 0; i < player.Length; i++)
                    player[i].DrawSource(batch);

                 break;

             case Levels.LEVEL_ONE:
                 Background2D.DrawParallaxing(batch);

                    for (int i = 0; i < player.Length; i++)
                    {
                        player[i].DrawWithProjectiles(batch);
                        input[i].DrawTesting(batch);
                    }

                 drawEnemies(batch);     //Draw enemies
                 //input.DrawTesting(batch);
                 //testingDraw(batch);
                 break;

             case Levels.DYING_SPLASH:
                 Background2D.DrawParallaxing(batch);
                 batch.Draw(collisionTexture,
                     new Rectangle(0, 0, GameInformation.screenWidth, GameInformation.screenHeight),
                     deathColor);

                 for(int i = 0; i < player.Length; i++)
                    player[i].DrawWithProjectiles(batch);

                    drawPrompt(batch, "THANK YOU FOR PLAYING!", GameInformation.screenHeight + 80,
                        Color.HotPink);

                    drawPrompt(batch, "Press fire key to play again",
                        GameInformation.screenHeight - 120, Color.HotPink);

                    drawPrompt(batch, "Press any key to quit",
                        GameInformation.screenHeight - 80, Color.HotPink);

                    drawPrompt(batch, "Score" + score, GameInformation.screenHeight / 2,
                        Color.HotPink);
                    break;

             case Levels.TESTING:
                 Background2D.DrawParallaxing(batch);

                 for(int i = 0; i < player.Length; i++)
                    player[i].DrawWithProjectiles(batch);

                 testingDraw(batch);
                 break;
         }
     }

     private void drawEnemies(SpriteBatch batch)
     {
         for (int i = 0; i < enemies.Length; i++)
             enemies[i].drawEnemy(batch, i);
     }

     //public static void 

     private void testingDraw(SpriteBatch batch)
     {
         batch.DrawString(GameInformation.infoFont, "random = " + random,
             new Vector2(200, 200), Color.Red);
     }

     private void drawPrompt(SpriteBatch batch, string prompt, int y, Color col)
     {
            //batch.DrawString(GameInformation.infoFont, prompt,
            //  vec, col);

            //Draw at center of screen... y values change
            Vector2 stringDim = GameInformation.infoFont.MeasureString(prompt);
            //put at center of screen...
            batch.DrawString(GameInformation.infoFont, prompt,
                new Vector2((GameInformation.screenWidth / 2) - (stringDim.X / 2), y),
                col);
     }
    }

    struct Input
    {
        public Vector2 angle;
        //public Vector2 crossHairAngle;

        public bool firePressed;
        public bool keyPressed;

        KeyboardState keyState;
        GamePadState gs;
        PlayerIndex pIndex;

        public enum GameType
        {
            KEYBOARD,
            GAMEPAD,
            PHONE
        };

        GameType type;

        public GameType returnType()
        {
            return type;
        }

        public KeyboardState getKeyState()
        {
            return Keyboard.GetState();
        }

        public void Initialize(GameType type, PlayerIndex pIndex)
        {
            angle = Vector2.Zero;
            //crossHairAngle = Vector2.Zero;
            firePressed = false;
            keyPressed = false;

            this.type = type;
            InitializeType(pIndex);
        }

        private void InitializeType(PlayerIndex pIndex)
        {
            switch(type)
            {
                case GameType.KEYBOARD:
                    keyState = new KeyboardState();
                    break;
                case GameType.GAMEPAD:
                    gs = new GamePadState();
                    this.pIndex = pIndex;
                    break;
            }
        }

        public void Update(float deltaTime)
        {
            switch(type)
            {
                case GameType.PHONE:
                    break;

                case GameType.KEYBOARD:
                    keyState = Keyboard.GetState();
                    pcUpdate(deltaTime);
                    break;

                case GameType.GAMEPAD:
                    gs = GamePad.GetState(pIndex);
                    gamePadUpdate(deltaTime);
                    break;
            }
        }

        private void pcUpdate(float deltaTime)
        {
            //Default angle at 0f...
            angle.X = 0f;
            angle.Y = 0f;

            //crossHairAngle.X = 0;
            //crossHairAngle.Y = 0;

            firePressed = false;

            if (keyState.GetPressedKeys().Length > 0)
                keyPressed = true;

            if (keyState.IsKeyDown(Keys.D))
                angle.X = 1f;

            if (keyState.IsKeyDown(Keys.A))
                angle.X = -1f;

            if (keyState.IsKeyDown(Keys.W))
                angle.Y = -1f;

            if (keyState.IsKeyDown(Keys.S))
                angle.Y = 1f;

            //Crosshair control...
            /*if (keyState.IsKeyDown(Keys.Right))
                crossHairAngle.X = 1f;

            if (keyState.IsKeyDown(Keys.Left))
                crossHairAngle.X = -1f;

            if (keyState.IsKeyDown(Keys.Up))
                crossHairAngle.Y = -1f;

            if (keyState.IsKeyDown(Keys.Down))
                crossHairAngle.Y = 1f;
                */
            if (keyState.IsKeyUp(Keys.Space))
                firePressed = false;    //Player can immediately shoot...
            if(keyState.IsKeyDown(Keys.Space))
            {
                //fireUpdate(deltaTime / 8f);
                firePressed = true;
            }
        }

        private void gamePadUpdate(float deltaTime)
        {
            //Initially, fire is false...
            firePressed = false;

            if (gs.Triggers.Right > 0f)
                keyPressed = true;

            angle.X = (float)System.Math.Round(gs.ThumbSticks.Left.X);
            angle.Y = -((float)System.Math.Round(gs.ThumbSticks.Left.Y));

            //crossHairAngle.X = (float)System.Math.Round(gs.ThumbSticks.Right.X);
            //crossHairAngle.Y = -((float)System.Math.Round(gs.ThumbSticks.Right.Y));

            if (gs.Triggers.Right > 0f)
                firePressed = true;
            else if (gs.Triggers.Right == 0f)
                firePressed = false;
        }

        public void DrawTesting(SpriteBatch batch)
        {
            /*batch.DrawString(GameInformation.infoFont,
                "crossHairAngle = (" + crossHairAngle.X + ", " + crossHairAngle.Y + ")",
                new Vector2(150, 150), Color.Black);*/
        }
    }

    public struct Projectile
    {
        private Sprite sprite;
        private Vector2 angle;

        public Rectangle DrawRectangle
        {
            get { return sprite.drawRectangle; }
        }

        public void Initialize(Texture2D spr, Vector2 pos, int width, int height)
        {
            sprite = new Sprite();
            sprite.Initialize(spr, pos, width, height);
        }

        private void Move(float xMove, float yMove)
        {
            sprite.Move(DrawRectangle.X + xMove, DrawRectangle.Y + yMove);
        }

        public void Activate(Vector2 pos, Vector2 direction, float moveSpeed)
        {
            sprite.Activate(pos);
            angle.X = direction.X * moveSpeed;
            angle.Y = direction.Y * moveSpeed;
        }

        public void DeActivate()
        {
            sprite.DeActivate();
        }

        public bool IsActive()
        {
            return sprite.IsActive();
        }

        public void Update(float deltaTime)
        {
            if (IsActive())
            {
                float xSpeed, ySpeed;
                //If out of bounds...
                if (DrawRectangle.X > GameInformation.screenWidth || DrawRectangle.X < 0)
                    DeActivate();

                if (DrawRectangle.Y > GameInformation.screenHeight || DrawRectangle.Y < 0)
                    DeActivate();

                //Else, move...
                xSpeed = angle.X * deltaTime;
                ySpeed = angle.Y * deltaTime;

                Move(xSpeed, ySpeed);
            }
        }

        public void Draw(SpriteBatch batch)
        {
            sprite.Draw(batch);
        }
    }

    struct Lists
    {
        public static Projectile[] projectiles;
    }
}
