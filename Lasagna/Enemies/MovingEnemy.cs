﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Lasagna
{
    public abstract class MovingEnemy : IEnemy
    {
        private ISprite currentSprite;
        private EnemyState currentState;
        public enum EnemyHealth { Normal, Flipped, Stomped };
        public EnemyHealth currentHealth = EnemyHealth.Normal;
        public bool isLeft = true;
        public bool isMoving = true;
        public bool isSeen;
        private float[] orignalPos = new float[2];
        private float velocity = 1;
        private float fallingVelocity = (float)1.5;
        private float fallingVelocityDecayRate = (float).9;
        private Vector2 position;
        private float yDifference;
        private int deathTime = 0;

        private int maxVelocity = 37;
        private int increasingYDifference = 7;
        private int increasingVelocity = 2;
        private float adjustYDifference = (float)3.5;
        private int deathTimeMax = 20;
        

        protected ISprite CurrentSprite
        {
            get { return currentSprite; }
            set { currentSprite = value; }
        }
        protected EnemyState CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }
        protected int PosX { get { return (int)position.X; } }
        protected int PosY { get { return (int)position.Y; } }

        protected MovingEnemy(int spawnPosX, int spawnPosY)
        {
            position.X = spawnPosX;
            position.Y = spawnPosY;
            orignalPos[0] = position.X;
            orignalPos[1] = position.Y;
            MarioEvents.OnReset += Reset;
        }
        public Rectangle Bounds
        {
            get
            {
                Rectangle returnValue = new Rectangle();
                if (CurrentSprite == null){
                    returnValue = Rectangle.Empty;
                }
                else{
                    returnValue = new Rectangle((int)position.X, (int)position.Y, CurrentSprite.Width, CurrentSprite.Height);
                }
                return returnValue;
            }
        }
       
        public void Reset(object sender, EventArgs e)
        {
            currentHealth = EnemyHealth.Normal;
            position.X = orignalPos[0];
            position.Y = orignalPos[1];
            isLeft = true;
            isMoving = true;
            ChangeState(EnemyState.WalkRight);
            deathTime = 0;
            fallingVelocity = (float)1.5;
        }
        public virtual void Update(GameTime gameTime)
        {
            if (currentHealth != EnemyHealth.Flipped && deathTime >= deathTimeMax)
            {
                currentSprite = null;
            }
            if (currentSprite != null) {
                if(isSeen == true)
                {
                    isMoving = true;
                }
                else
                {
                    isMoving = false;
                }

                if (currentHealth == EnemyHealth.Flipped)
                {
                    DeathAnimation();
                    
                }
                if (currentHealth != EnemyHealth.Normal)
                {
                    deathTime++;
                }

                HandleHorizontalMovement();
                Fall(gameTime);
                currentSprite.Update(gameTime, (int)position.X, (int)position.Y);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (currentSprite != null)
                currentSprite.Draw(spriteBatch);
        }

        public abstract void ChangeState(EnemyState newState);

        public abstract void Damage();

        /*private void ChangeToDefault (object sender, EventArgs e)
        {
            //If we're not idle, change to idle.
            if (currentState != EnemyState.Idle)
                ChangeState(EnemyState.Idle);
        }
        */
        //Many sprites have different heights or widths, and Monogame has sprite pos in top left,
        //fix the positioning here so it doesn't flicker.
        protected void FixSpritePosition(ISprite oldSprite, ISprite newSprite)
        {
            int heightDifference = oldSprite.Height - newSprite.Height;
            if (heightDifference != 0)
                position.Y = position.Y + heightDifference;
            int widthDifference = oldSprite.Width - newSprite.Width;
            if (widthDifference != 0)
                position.X = position.X + widthDifference;
        }

        public void OnCollisionResponse(ICollider otherCollider, CollisionSide side)
        {
            if (otherCollider is IPlayer)
                OnCollisionResponse((IPlayer)otherCollider, side);
            else if (otherCollider is IEnemy)
                OnCollisionResponse((IEnemy)otherCollider, side);
            else if (otherCollider is IItem)
                OnCollisionResponse((IItem)otherCollider, side);
            else if (otherCollider is ITile)
                OnCollisionResponse((ITile)otherCollider, side);
            else if (otherCollider is IProjectile)
                OnCollisionResponse((IProjectile)otherCollider, side);
        }

        protected virtual void OnCollisionResponse(IPlayer mario, CollisionSide side)
        {
            return;
        }

        protected virtual void OnCollisionResponse(IItem item, CollisionSide side)
        {
            return;
        }

        protected virtual void OnCollisionResponse(IProjectile fireball, CollisionSide side)
        {
            return;
        }

        protected virtual void OnCollisionResponse(IEnemy enemy, CollisionSide side)
        {
            if (side.Equals(CollisionSide.Right))
            {
                ChangeState(EnemyState.WalkRight);
                isLeft = true;
            }
            else if (side.Equals(CollisionSide.Left))
            {
                ChangeState(EnemyState.WalkLeft);
                isLeft = false;
            }
        }

        protected virtual void OnCollisionResponse(ITile tile, CollisionSide side)
        {
            if (side.Equals(CollisionSide.Bottom) && currentHealth != EnemyHealth.Flipped)
            {
                if(tile is BreakableBrickTile && ((BreakableBrickTile)tile).IsChangingState)
                {
                    if (this is KoopaEnemy)
                        ChangeState(EnemyState.Dead);
                    else
                        ChangeState(EnemyState.Flipped);
                    currentHealth = EnemyHealth.Flipped;
                    Score.increaseScoreMario(200);
                }
                else if(tile is QuestionBlockTile && ((QuestionBlockTile)tile).IsChangingState)
                {
                    if (this is KoopaEnemy)
                        ChangeState(EnemyState.Dead);
                    else
                        ChangeState(EnemyState.Flipped);
                    currentHealth = EnemyHealth.Flipped;
                    Score.increaseScoreMario(200);
                }
                    position.Y -= yDifference;
                    velocity = 1;
            }
            if (side.Equals(CollisionSide.Right) && currentHealth != EnemyHealth.Flipped)
            {
                ChangeState(EnemyState.WalkRight);
                isLeft = true;
                    position.Y -= adjustYDifference;
            }
            else if (side.Equals(CollisionSide.Left) && currentHealth != EnemyHealth.Flipped)
            {
                ChangeState(EnemyState.WalkLeft);
                isLeft = false;
                    position.Y -= adjustYDifference;
            }
        }
        private void HandleHorizontalMovement()
        {
            if (currentHealth == EnemyHealth.Normal && isMoving == true)
            {
                if (isLeft == true)
                {
                    position.X--;
                }
                else
                {
                    position.X++;
                }
            }
        }
        private void Fall(GameTime gameTime)
        {
                yDifference = velocity * ((float)gameTime.ElapsedGameTime.Milliseconds / 50);
                position.Y += yDifference;
                velocity += fallingVelocity;
                velocity *= fallingVelocityDecayRate;
                if (velocity > maxVelocity)
                    velocity = maxVelocity;
        }
        private void DeathAnimation()
        {
            position.X ++;
            position.Y -= (float)increasingYDifference;
            velocity += increasingVelocity;
        }
    }
}
