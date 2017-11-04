﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

 
    namespace Lasagna
    {
        public class HUD
        {
            public const int startTime = 400;
            public const int coinValue = 200;
            public const int brokenBrickValue = 50;
            public const int mushroomValue = 1000;
            public const int fireflowerValue = 1000;
            public const int starValue = 1000;
            public const int oneUpValue = 1000;
            public const int initialLives = 3;

            private int Time;



            public HUD()
            {

                Score.Lives = initialLives;
                Score.marioScore = 0;
                Score.Coins = 0;

                Time = startTime;
                Score.enemyKilledPoints = new int[10] { 100, 200, 400, 500, 800, 1000, 2000, 4000, 8000, 10000 };
               Score.marioEnemyKilledCount = 0;

            }

            public void Update(double time, double previous)
            {
                Time = startTime - (int)(time - previous);
            }

            public void Draw(SpriteBatch batch, SpriteFont font, bool twoPlayer)
            {

            batch.Begin();
                
            batch.DrawString(font, "MARIO" + addSpaces(4) + "COINS" + addSpaces(4) + "LIVES" + addSpaces(5) + "TIME" + addSpaces(4) + "WORLD", new Vector2(10, 10), Color.White);
            batch.DrawString(font, formattedScore(Score.marioScore) + addSpaces(3) + formattedCoins(Score.Coins) + addSpaces(7) + formattedLives(Score.Lives) + addSpaces(8) + Time.ToString() + addSpaces(5) + "1-1", new Vector2(10, 25), Color.White);
                
            batch.End();
            }

            private String addSpaces(int spaces)
            {
                String text = "";
                for (int i = 0; i < spaces; i++) text = text + " ";
                return text;
            }

            private String formattedScore(int score)
            {
                int gameScore = score;
                String text = "";
                for (int i = 0; i < 6; i++)
                {
                    text = (gameScore % 10) + text;
                    gameScore /= 10;
                }
                return text;
            }

            private String formattedCoins(int coins)
            {
                String text = "";
                if (coins < 10)
                {
                    text = "0" + coins.ToString();
                }
                else
                {
                    text = coins.ToString();
                }
                return text;
            }

            private String formattedLives(int lives)
            {
                String text = "";
                if (lives < 10)
                {
                    text = "0" + lives.ToString();
                }
                else if (lives > 99)
                {
                    text = "99";
                    lives = 99;
                }
                else
                {
                    text = lives.ToString();
                }
                return text;
            }
        }
    }