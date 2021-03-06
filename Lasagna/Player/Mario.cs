﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


namespace Lasagna
{
    public class Mario : IPlayer, ICollider
    {
        internal MarioStateMachine stateMachine;
        private MarioCollisionHandler marioCollisionHandler;
        private MarioPhysics marioPhysics;

        private int[] orignalPos = new int[2];
        public Vector2 position;
        public bool isFalling = false;
        public int maxHeight;
        public bool RestrictMovement { get; set; }

        public bool isCollideGround { get; set; }

        private int marioWarpCount = 0;
        /// <summary>
        /// Constants
        /// </summary>
        private const int ZERO = 0;
        private const int ONE = 1;
        private const int TWO = 2;
        private const int NEGATIVE_ONE = -1;
        private const int TEN = 10;
        private const int ONE_HUNDRED = 100;
        private const int SEVENTY_FIVE = 75;
        private const int NEGATIVE_TWO_HUNDRED = -200;
        private const int TWO_SEVENTY_FIVE = 275;
        private const int NEGATIVE_FOUR_FORTY = -440;

        public bool marioIsDead = false;
        private bool keyOverlap = false;

        public bool IsDead { get { return marioIsDead; } }
        public bool StarPowered { get { return stateMachine != null && stateMachine.StarPowered; } }

        public Rectangle Bounds { get { return new Rectangle((int)position.X, -(int)position.Y, GetCurrentSprite().Width, GetCurrentSprite().Height); } }
        public bool IsBlinking { get { return stateMachine != null && (stateMachine.IsTransitioning || stateMachine.IsBlinking); } }
        public MarioStateMachine.MarioState CurrentState { get { return (stateMachine != null) ? stateMachine.CurrentState : MarioStateMachine.MarioState.Small; } }

        // If tag == 1, we have player 1, if tag is 2 we have player 2
        public int Tag { get; set; }
        public Mario(uint playerNumber, int x, int y)
        {
            Tag = (int)playerNumber;
            marioPhysics = new MarioPhysics(this);
            stateMachine = new MarioStateMachine(this, marioPhysics);
            marioCollisionHandler = new MarioCollisionHandler(this, stateMachine, marioPhysics);
            marioPhysics.GetStateMachineInstance();
            RestrictMovement = false;

            position.X = x;
            position.Y = -y;
            orignalPos[0] = (int)position.X;
            orignalPos[1] = -(int)position.Y;

            MarioEvents.OnReset += Reset;

            //Subscribe to keyboard events based on player number. Indexed from 0, so 0 = player 1, 1 = player 2, etc.
            if (playerNumber == 1)
            {
                MarioEvents.OnP2MoveLeft += MoveLeft;
                MarioEvents.OnP2MoveRight += MoveRight;
                MarioEvents.OnP2Jump += Jump;
                MarioEvents.OnP2Crouch += Crouch;
                MarioEvents.OnP2ShootFire += MarioFireProjectile;
            }
            else
            {
                MarioEvents.OnP1MoveLeft += MoveLeft;
                MarioEvents.OnP1MoveRight += MoveRight;
                MarioEvents.OnP1Jump += Jump;
                MarioEvents.OnP1Crouch += Crouch;
                MarioEvents.OnP1ShootFire += MarioFireProjectile;
            }
        }

        public void ForceMove(float x, float y)
        {
            position.X += x;
            position.Y -= y;
        }

        public void SetPosition(int x, int y)
        {
            position.X = x;
            position.Y = -y;
        }
        private ISprite GetCurrentSprite()
        {
            return stateMachine.CurrentSprite;
        }

        private void Reset(object sender, EventArgs e)
        {
            RestrictMovement = false;
            position.X = orignalPos[ZERO];
            position.Y = NEGATIVE_ONE * orignalPos[ONE];
            marioPhysics.velocity = Vector2.Zero;
            marioIsDead = false;
            marioPhysics.marioMovingLeft = false;
            marioPhysics.marioMovingRight = false;
            isCollideGround = false;
            BGMFactory.Instance.Play_MainTheme();
            stateMachine.Reset();
        }

        public void MarioFireProjectile(object sender, EventArgs e)
        {
            if (stateMachine != null && stateMachine.IsTransitioning || RestrictMovement)
                return;

            if (marioIsDead || stateMachine.CurrentState != MarioStateMachine.MarioState.Fire)
                return;

            bool facingRight = IsMarioMovingRight();

            int spawnX = Bounds.X + (facingRight ? Bounds.Width : 0);
            if(!facingRight)
                spawnX -= 30;
            MarioGame.Instance.RegisterProjectile(new FireProjectile(spawnX, Bounds.Y + Bounds.Height / 2, facingRight));
            SoundEffectFactory.Instance.PlayFireball();
        }

        private bool IsMarioMovingRight()
        {
            if (stateMachine == null)
                return false;

            MarioStateMachine.MarioMovement m = stateMachine.CurrentMovement;
            return m == MarioStateMachine.MarioMovement.CrouchRight
                || m == MarioStateMachine.MarioMovement.IdleRight
                || m == MarioStateMachine.MarioMovement.JumpRight
                || m == MarioStateMachine.MarioMovement.RunRight;
        }

        public void MoveLeft(object sender, EventArgs e)
        {
            if (stateMachine != null && stateMachine.IsTransitioning)
                return;
            marioPhysics.marioMovingLeft = true;
        }

        public void MoveRight(object sender, EventArgs e)
        {
            if (stateMachine != null && stateMachine.IsTransitioning)
                return;
            marioPhysics.marioMovingRight = true;
        }
        public void Crouch(object sender, EventArgs e)
        {
            if (stateMachine != null && stateMachine.IsTransitioning || marioPhysics.disableCrouch)
                return;

            if (!marioIsDead)
                stateMachine.Crouch();
        }

        public void Jump(object sender, EventArgs e)
        {
     
            if (stateMachine != null && stateMachine.IsTransitioning || isFalling)
                return;
            if (!marioPhysics.isJumping)
            {
                if (CurrentState.Equals(MarioStateMachine.MarioState.Small))
                {
                    SoundEffectFactory.Instance.PlayJumpMarioSmall();
                }
                else
                {
                    SoundEffectFactory.Instance.PlayJumpMarioBig();
                }
            }

            marioPhysics.Jump();
        }

        public void CheckFlagpoleHeight()
        {
            if (position.X > 6000 && position.Y >= -40)
                ForceMove(0, 1);                
        }

        public void Star()
        {
            stateMachine.Star();
        }

        public void Die(object sender, EventArgs e)
        {
            marioIsDead = true;
            stateMachine.KillMario();
        }

        // Overload of die which is used for Mario Collision
        public void Die()
        {
            RestrictMovement = true;
            marioIsDead = true;
            marioPhysics.velocity.X = ZERO;
            stateMachine.KillMario();
            if (Tag == 0)
                Score.LoseLifeMario();
            else
                Score.LoseLifeLuigi();
            BGMFactory.Instance.Play_YouAreDead();

        }

        public void OnCollisionResponse(ICollider otherCollider, CollisionSide side)
        {
            if (stateMachine != null && stateMachine.IsTransitioning)
                return;
            if (otherCollider is IPlayer)
                marioCollisionHandler.OnCollisionResponse((IPlayer)otherCollider, side);
            else if (otherCollider is IEnemy)
                marioCollisionHandler.OnCollisionResponse((IEnemy)otherCollider, side);
            else if (otherCollider is ITile)
                marioCollisionHandler.OnCollisionResponse((ITile)otherCollider, side);
            else if (otherCollider is IItem)
                marioCollisionHandler.OnCollisionResponse((IItem)otherCollider, side);
            else if (otherCollider is IProjectile)
                marioCollisionHandler.OnCollisionResponse((IProjectile)otherCollider, side);
        }



        public void Update(GameTime gameTime)
        {
            if (isCollideGround || marioPhysics.ignoreGravity == true)
                isFalling = false;
            if (marioIsDead || stateMachine.flagpoleSequence)
            {
                stateMachine.Update(gameTime, (int)position.X, -(int)position.Y);
                return;
            }
            if(!MarioIsInWarpZone() && !stateMachine.IsTransitioning && position.Y < NEGATIVE_FOUR_FORTY)
               Die();

            if (stateMachine.IsTransitioning)
            {
                marioPhysics.velocity = Vector2.Zero;
                marioPhysics.ignoreGravity = true;
            }
            else if (!stateMachine.IsTransitioning && marioPhysics.velocity != Vector2.Zero)
                marioPhysics.transitionVel = marioPhysics.velocity;

            if(!RestrictMovement && !keyOverlap)
            marioPhysics.Update(gameTime);
     
            if (isCollideGround)
                Score.ResetConsecutiveEnemiesKilled();

            stateMachine.Update(gameTime, (int)position.X, -(int)position.Y);
            isCollideGround = false;
            keyOverlap = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            stateMachine.Draw(spriteBatch);
        }


        public void BeginWarpAnimation(Direction moveDir, bool startWithMove)
        {           
            stateMachine.BeginWarpAnimation(moveDir, startWithMove);
        }

        public void MarioEnterWarpZone()
        {
            marioWarpCount++;
        }
        public bool MarioIsInWarpZone()
        {
            return marioWarpCount % TWO == ONE;
        }
    }
}
