﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceWars
{
    public class GameScreen : Screen
    {

        // TODO: Share all ScreenStates in one class
        private enum ScreenState { NORMAL, FADE_IN, COUNTDOWN, GAMEOVER }
        private ScreenState currentState;

        public static SpriteFont fontUI;
        private SpriteFont fontCountdown;

        private Texture2D blackTex, iconMissileGemini, iconMissilePORT, iconMissileCrusader;
        private int blackTexAlpha;
        private float totalElapsed;
        private string currentCount;


        private float countDownScale;
        private float timer;
        private float timerDelay = 1;
        private float spawnTimer;
        private SoundEffect sfxCountdown, sfxReady;
        private Random random;
        private KeyboardState prevState;

        private Dictionary<string, SoundEffect> gameSFXs;

        Game1 _main;

        // Data (assets needed for certain tasks)
        Texture2D texCommandCenter, texGeminiMissile, texAsteroid;

        // Entities (anything on the screen)
        GameObject background;
        public static CommandCenter player1, player2;
        public static List<Asteroid> asteroids;
        public static Queue<Asteroid> deadAsteroids;

        // Settings
        private const uint NUM_ASTEROIDS = 20;
        public static int currentNumAsteroids;

        public GameScreen(Game1 main) : base (main)
        {
            background = new GameObject(
                content.Load<Texture2D>("Sprites/space"),
                Vector2.Zero,
                2.5f,
                0.0f,
                false,
                SpriteEffects.None);
            _main = main;

            currentState = ScreenState.FADE_IN;
            blackTexAlpha = 255;
            currentCount = "3";
            countDownScale = 10.0f;
            timer = 3;
            prevState = Keyboard.GetState ();
  
        }//public Screen ()

        public override void Initialize(){
            asteroids = new List<Asteroid>();
            deadAsteroids = new Queue<Asteroid> ();
            player1 = new CommandCenter(this, texCommandCenter, texGeminiMissile, new Vector2(100, 100));
            player2 = new CommandCenter(this, texCommandCenter, texGeminiMissile, new Vector2(1000, 200));

            currentNumAsteroids = 0;
            spawnTimer = 0.0f;

            random = new Random();

            for (int i = 0; i < NUM_ASTEROIDS; i++)
            {
                Asteroid tmpAsteroid = new Asteroid(texAsteroid, Vector2.Zero);
                asteroids.Add(tmpAsteroid);
                deadAsteroids.Enqueue ( tmpAsteroid );
            }

        }//public override void Initialize(){

        public override void LoadContent(){
            // Textures
            texCommandCenter = content.Load<Texture2D>("Sprites/command_center");
            texGeminiMissile = content.Load<Texture2D>("Sprites/missile");
            texAsteroid = content.Load<Texture2D>("Sprites/asteroid");
            blackTex = content.Load<Texture2D> ( "Sprites/black" );

            // Fonts
            fontCountdown = content.Load<SpriteFont> ( "Fonts/Times" );
            fontUI = content.Load<SpriteFont> ( "Fonts/agencyFBUI" );
            // Sound Effects
            sfxCountdown = content.Load<SoundEffect>("Audio/countdownvoice");
            //sfxReady = content.Load<SoundEffect> ( "Audio/areyouready" );

            // Initialize gameSFXs dictionary
            gameSFXs = new Dictionary<string, SoundEffect> ();
            gameSFXs.Add ( "launch", content.Load<SoundEffect> ( "Audio/launch" ) );
            gameSFXs.Add ( "explode", content.Load<SoundEffect> ( "Audio/explosion" ) );

            // Initialize UI Textures
            iconMissileGemini = content.Load<Texture2D> ( "Sprites/UI/GeminiMissileIcon" );
            iconMissilePORT = content.Load<Texture2D> ( "Sprites/UI/PORTMissileIcon" );
            iconMissileCrusader = content.Load<Texture2D> ( "Sprites/UI/CrusaderMissileIcon" );


        }//public override void LoadContent()

        public override void Update(GameTime gameTime)
        {}

        public override void Update(GameTime gameTime, KeyboardState keyState) {
            float elapsed = ( (float)gameTime.ElapsedGameTime.Milliseconds ) / 1000.0f;
            totalElapsed += elapsed;

            switch ( currentState ) {
                case ScreenState.NORMAL:
                    // TODO: Make a list for all GameObjects
                    //       and do all collision checks there

                    // Player Updates
                    player1.Update ( gameTime );
                    player2.Update ( gameTime );
                    SpawnAsteroids(elapsed);
                    // Asteroid Updates
                    foreach ( Asteroid asteroid in asteroids ) {
                        asteroid.Update ( gameTime, graphics );
                    }

                    UpdateInput ( keyState );
                    
                    break;
                case ScreenState.COUNTDOWN:
                    timer -= elapsed;
                    timerDelay -= elapsed;

                    if ( timer <= -1  ) {
                        currentState = ScreenState.NORMAL;
                    }
                    else if ( timer <= 0  ) {
                        currentCount = "  Match Start"; // Sweep KING!!!
                    }
                    else if ( timer <= 1 ) {
                        currentCount = "1";
                    }
                    else if ( timer <= 2 ) {
                        currentCount = "2";
                    }
                    else if ( timer >= 3 ) {
                        currentCount = "3";
                    }

                    if ( timerDelay <= 0 ) {
                        countDownScale = 10;
                        timerDelay = 1.0f;
                    }
                    countDownScale = ( 30 * timerDelay );
                    if ( countDownScale < 10 )
                        countDownScale = 10;
                    break;
                case ScreenState.FADE_IN:
                    blackTexAlpha-= 2;
                    if ( blackTexAlpha <= 0 ) {
                        totalElapsed = 0;
                        currentState = ScreenState.COUNTDOWN;
                        sfxCountdown.Play ();
                        //sfxReady.Play ();
                    }
                    break;
                default:
                    break;
            }


        }//public override void Update(GameTime gameTime, KeyboardState keyState) {

        public override void UpdateInput(KeyboardState keyState)
        {
            handlePlayerInput(player1, keyState, Keys.A, Keys.D, Keys.W);
            handlePlayerInput(player2, keyState, Keys.NumPad4, Keys.NumPad6, Keys.NumPad8);
        }//public override void UpdateInput(KeyboardState keyState)

        private void handlePlayerInput (CommandCenter player, KeyboardState keyState
                , Keys left, Keys right, Keys primary)
        {
            if (player._currentActive == null)
            {
                if (keyState.IsKeyDown(left)) {
                    player.AimLeft();}
                else if (keyState.IsKeyDown(right)) {
                    player.AimRight();}

                if (keyState.IsKeyDown(primary)) {
                    player.Launch();}
            } else {
                if (keyState.IsKeyDown(left)) {
                    player._currentActive.TurnLeft();}
                else if (keyState.IsKeyDown(right)) {
                    player._currentActive.TurnRight();}

                if (keyState.IsKeyDown(primary)) {
                    player._currentActive.ActivateSpecial();
                }
            }//else

            KeyboardState newState = Keyboard.GetState ();
            bool readyToCycleLeft = !prevState.IsKeyDown(Keys.Q);
            bool readyToCycleRight = !prevState.IsKeyDown ( Keys.E );

            if ( keyState.IsKeyDown ( Keys.Q ) && readyToCycleLeft ) {
                    player1.cycleWeaponsLeft ();
            }
            else if ( keyState.IsKeyDown( Keys.E ) && readyToCycleRight) {
                    player1.cycleWeaponsRight ();
            }

            prevState = newState;

        }// private void function handlePlayerInput (CommandCenter player, KeyState keyState

        public void SpawnAsteroids(float elapsed)
        {
            spawnTimer -= elapsed;
            if (currentNumAsteroids < NUM_ASTEROIDS)
            {
                if (spawnTimer <= 0)
                {
                    Vector2 spawnPoint = new Vector2(graphics.Viewport.Width / 2, graphics.Viewport.Height + 50);
                    float speed = random.Next(100, 100);
                    float rot = -random.Next(150, 210);
                    float mass = random.Next(1, 5);

                    Asteroid tmpAsteroid = deadAsteroids.Dequeue ();
                    tmpAsteroid.setProperty(spawnPoint, rot, speed);
                    tmpAsteroid.Mass = mass;
                    currentNumAsteroids++;
                    spawnTimer = 1f;
                   
                }
            }
        }

        public void playSFX ( string sfxName ) {
            gameSFXs[sfxName].Play ();
        }

        public void drawPlayerUI (SpriteBatch spriteBatch) {
            int counter = 0;
            
            foreach ( CommandCenter.WeaponsList weaponType in  Enum.GetValues(typeof(CommandCenter.WeaponsList) ) ) {
                int x = 25 + 35 * ( counter % 7 );
                int y = 25 + 10 * ( counter / 7 );
                float scale = 0.3f;
                Vector2 tmpPos = new Vector2 ( x, y );
                Color color = new Color ( 255, 255, 255, 200 );
                if ( player1.currentWeapon == weaponType )
                    color = new Color ( 30, 220, 30, 100 );
                switch ( weaponType ) {
                    case CommandCenter.WeaponsList.GEMINI_MISSILE:
                        spriteBatch.Draw ( iconMissileGemini, tmpPos, null, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0 );
                        break;
                    case CommandCenter.WeaponsList.PORT_MISSILE:
                        spriteBatch.Draw ( iconMissilePORT, tmpPos, null, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0 );
                        break;
                    case CommandCenter.WeaponsList.CRUSADER_MISSILE:
                        spriteBatch.Draw ( iconMissileCrusader, tmpPos, null, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0 );
                        break;
                    default:
                        break;
                }

                counter++;
            }
        }

        public override void Draw ()
        {
            background.Draw(spriteBatch);
            for (int i = 0; i < NUM_ASTEROIDS; i++)
            {
                asteroids[i].Draw(spriteBatch);
            }
            drawPlayerUI ( spriteBatch );
            player1.Draw ( spriteBatch );
            player2.Draw ( spriteBatch );
            if ( currentState == ScreenState.FADE_IN )
                spriteBatch.Draw ( blackTex,
                    new Rectangle ( 0, 0, graphics.Viewport.Width, graphics.Viewport.Height),
                    new Color ( 0, 0, 0, blackTexAlpha ) );

            // Countdown TODO: Clean Up, separate function maybe
            Vector2 stringSize = fontCountdown.MeasureString ( currentCount );
            Vector2 tmpVect = new Vector2 ( (graphics.Viewport.Width - stringSize.X) / 2,
                                        (graphics.Viewport.Height - stringSize.Y) / 2 );

            if ( currentState == ScreenState.COUNTDOWN ) {
                spriteBatch.DrawString ( fontCountdown,
                    currentCount,
                    tmpVect,
                    Color.Red,
                    0.0f,
                    new Vector2 (stringSize.X / 2, stringSize.Y / 2),
                    countDownScale,
                    SpriteEffects.None,
                    0 );
            }

            string output = "Gemini Missile: ";
            switch (player1.currentWeapon) {
                case CommandCenter.WeaponsList.GEMINI_MISSILE:
                    output = "Gemini Missile: ";
                    break;
                case CommandCenter.WeaponsList.PORT_MISSILE:
                    output = "PORT Missile: ";
                    break;
                case CommandCenter.WeaponsList.CRUSADER_MISSILE:
                    output = "Crusader Missile: ";
                    break;
                default:
                    break;
            };
            stringSize = fontCountdown.MeasureString ( output );
            tmpVect = new Vector2 ( 25, 60 );
            output += player1.Weapons[player1.currentWeapon];

            spriteBatch.DrawString ( fontUI,
                output,
                tmpVect,
                Color.LimeGreen,
                0.0f,
                Vector2.Zero,
                1,
                SpriteEffects.None,
                0 );
        }//public override void Draw()


    }//public class GameScreen : Screen
}//namespace SpaceWars
