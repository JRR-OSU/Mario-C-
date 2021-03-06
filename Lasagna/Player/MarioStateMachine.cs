﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Lasagna
{
    public class MarioStateMachine
    {
        private Mario mario;
        private MarioPhysics marioPhysics;
        public enum MarioState { Small, Big, Fire };
        public enum MarioMovement { CrouchRight, CrouchLeft, IdleLeft, IdleRight, RunLeft, RunRight, TurnLeft, TurnRight, JumpLeft, JumpRight, GrowLeft, GrowRight, ShrinkLeft, ShrinkRight, Flagpole, Die };
        private MarioState marioState = MarioState.Small;
        private MarioMovement marioMovement = MarioMovement.IdleRight;
        private ISprite currentSprite = MarioSpriteFactory.Instance.CreateSprite_MarioSmall_IdleRight();

        private const int ZERO = 0;
        private const int THREE = 3;
        private const int NINE = 9;
        private const int SIX = 6;
        private const int SEVEN = 7;
        private const int TWELVE = 12;

        //How long our death animation is
        private const float deathAnimLength = 4f;
        //How fast we move during death anim
        private const float deathAnimSpeed = 200f;
        //Used for fire powerup, which just switches colors
        private const float firePowerupTransitionLength = 2f;
        //Animation colors used for fireflower powerup anim
        private readonly Color[] firePowerupTransitionColors = new[]
        {
            Color.White,
            Color.Green,
            Color.PaleVioletRed,
            new Color(0.3f, 0.3f, 0.3f), //Black
            Color.White,
            Color.Green,
            Color.PaleVioletRed,
            new Color(0.3f, 0.3f, 0.3f),
            Color.White,
            Color.Green,
            Color.PaleVioletRed,
            new Color(0.3f, 0.3f, 0.3f),
            Color.White,
            Color.Green,
            Color.PaleVioletRed,
            new Color(0.3f, 0.3f, 0.3f)
        };
        //How long mario blinks for after beign damaged
        private const float blinkLength = 3f;
        private const float marioWarpMoveLength = 0.75f;
        private const float marioWarpStillLength = 0.75f;

        private bool canGrow = true;

        public bool isJumping = false;
        public bool isCollideFloor { get; set; }
        public bool isCollideUnder { get; set; }


        public bool isTouchingGround;

        private bool warpMoveFirst;
        private Direction warpDirection;
        private float warpTimeRemaining;
        //If this > 0 mario ignores collisions and can't be hurt. 
        private float blinkTimeRemaining;
        private bool blinkShow;
        //If this is > 0, Mario can't move or be hurt and ignores collisions. This is for when he's growing or shrinking.
        private float stateTransitionTimeRemaining;
        //Current state transition color
        private Color stateTransitionColor = Color.White;
        private float deathAnimTimeRemaining;

        private bool starPower = false;
        private int starDuration = 600;
        private int starCounter = 0;
        private int frameCount = 0;

        private int turnFrames = 0;

        public bool StarPowered { get { return starPower; } }
        public bool IsTransitioning { get { return stateTransitionTimeRemaining > 0 || warpTimeRemaining > 0; } }
        public bool IsBlinking { get { return blinkTimeRemaining > 0; } }

        public bool flagpoleSequence = false;
        public Vector2 flagpoleColPos = new Vector2();
        public Vector2 flagpoleSlide = new Vector2();
        private bool finishSequence = false;
        private int flagpoleCount = 0;

        internal Dictionary<MarioMovement, ISprite> smallStates = new Dictionary<MarioMovement, ISprite>();
        internal Dictionary<MarioMovement, ISprite> bigStates = new Dictionary<MarioMovement, ISprite>();
        internal Dictionary<MarioMovement, ISprite> fireStates = new Dictionary<MarioMovement, ISprite>();

        public MarioStateMachine(Mario player, MarioPhysics physics)
        {
            MarioSpriteHelper helper = new MarioSpriteHelper(this, player.Tag);
            marioPhysics = physics;
            isCollideFloor = false;
            isCollideUnder = false;

            mario = player;
        }

        private bool MarioFacingLeft()
        {
            return (marioMovement == MarioMovement.CrouchLeft || marioMovement == MarioMovement.IdleLeft
                    || marioMovement == MarioMovement.JumpLeft || marioMovement == MarioMovement.RunLeft);
        }

        public void Grow()
        {
            if (canGrow)
            {
                marioState = MarioState.Big;
                //If facing left, set movement to be transition left.
                marioMovement = (MarioFacingLeft()) ? MarioMovement.GrowLeft : MarioMovement.GrowRight;
                ISprite oldSprite = currentSprite;
                UpdateSprite();

                //Move us up since small sprite is smaller than big sprite
                if (oldSprite != null && currentSprite != null)
                    mario.ForceMove(0, -Math.Abs(currentSprite.Height - oldSprite.Height));

                //Play growing transition
                stateTransitionTimeRemaining = currentSprite.ClipLength;
            }
            canGrow = false;
        }

        public void Shrink()
        {
            canGrow = true;

            if (marioState == MarioState.Small)
                mario.Die();
            else
            {
                //If facing left, set movement to be transition left.
                marioMovement = (MarioFacingLeft()) ? MarioMovement.ShrinkLeft : MarioMovement.ShrinkRight;
                marioState = MarioState.Small;
                SoundEffectFactory.Instance.PlayPipeSound();
                UpdateSprite();
                blinkTimeRemaining = blinkLength;
                stateTransitionTimeRemaining = (marioState == MarioState.Big) ? firePowerupTransitionLength : currentSprite.ClipLength;
            }
        }

        public void DamageMario()
        {
            if (!starPower && stateTransitionTimeRemaining <= ZERO && blinkTimeRemaining <= ZERO)
                Shrink();
        }

        public void SetFireState()
        {
            canGrow = false;
            marioState = MarioState.Fire;
            currentSprite = fireStates[marioMovement];

            //Play transition
            stateTransitionTimeRemaining = firePowerupTransitionLength;
        }

        private void SwitchCurrentSprite(MarioMovement newMovement)
        {
            switch (marioState)
            {
                case MarioState.Big:
                    currentSprite = bigStates[newMovement];
                    break;
                case MarioState.Fire:
                    currentSprite = fireStates[newMovement];
                    break;
                case MarioState.Small:
                    currentSprite = smallStates[newMovement];
                    break;
            }
        }

        public void SetIdleState()
        {
            if (marioMovement == MarioMovement.RunRight || (marioMovement == MarioMovement.JumpRight && mario.isCollideGround))
            {
                marioMovement = MarioMovement.IdleRight;
            }
            else if (marioMovement == MarioMovement.RunLeft || (marioMovement == MarioMovement.JumpLeft && mario.isCollideGround))
            {
                marioMovement = MarioMovement.IdleLeft;
            }
            SwitchCurrentSprite(marioMovement);

        }

        public void SetGroundedState()
        {
            if (marioMovement == MarioMovement.JumpRight && !(marioMovement == MarioMovement.TurnRight))
            {
                if(marioPhysics.isRunning || marioPhysics.marioMovingRight)
                    marioMovement = MarioMovement.RunRight;
                else
                    marioMovement = MarioMovement.IdleRight;
            }
            else if (marioMovement == MarioMovement.JumpLeft && !(marioMovement == MarioMovement.TurnLeft))
            {
                if (marioPhysics.isRunning || marioPhysics.marioMovingLeft)
                    marioMovement = MarioMovement.RunLeft;
                else
                    marioMovement = MarioMovement.IdleLeft;
            }
            else if (!(marioMovement == MarioMovement.TurnRight) && !(marioMovement == MarioMovement.TurnLeft))
            {
                if (marioMovement == MarioMovement.RunRight && marioPhysics.marioMovingLeft)
                    marioMovement = MarioMovement.RunLeft;
               else if (marioMovement == MarioMovement.RunLeft && marioPhysics.marioMovingRight)
                    marioMovement = MarioMovement.RunRight;
            }
            SwitchCurrentSprite(marioMovement);

        }
        public void MoveLeft()
        {
            if ((marioMovement == MarioMovement.JumpLeft || marioMovement == MarioMovement.JumpRight))
                return;
            else if (marioMovement == MarioMovement.RunRight )
            {
                marioMovement = MarioMovement.TurnLeft;
                turnFrames++;
            }
            else if (turnFrames == 0)
            {
                marioMovement = MarioMovement.RunLeft;
            }
            SwitchCurrentSprite(marioMovement);
        }

        public void MoveRight()
        {
            if ((marioMovement == MarioMovement.JumpLeft || marioMovement == MarioMovement.JumpRight))
                return;
            else if (marioMovement == MarioMovement.RunLeft)
            {
                marioMovement = MarioMovement.TurnRight;
                turnFrames++;
            }
            else if (turnFrames == 0)
            {
                marioMovement = MarioMovement.RunRight;
            }
            SwitchCurrentSprite(marioMovement);
        }


        private void HandleTurnFrames()
        {
            if (marioMovement == MarioMovement.TurnLeft || marioMovement == MarioMovement.TurnRight)
            {
                turnFrames++; 
            }
            if (turnFrames > 10)
            {
                turnFrames=0;
                if (marioMovement == MarioMovement.TurnLeft)
                    marioMovement = MarioMovement.RunLeft;
                else if (marioMovement == MarioMovement.TurnRight)
                    marioMovement = MarioMovement.RunRight;
                SwitchCurrentSprite(marioMovement);
            }
        }

        public void HandleCrouch()
        {
            if ((marioMovement == MarioMovement.RunLeft || marioMovement == MarioMovement.IdleLeft) && marioState != MarioState.Small &&!(isJumping))
            {
                marioMovement = MarioMovement.CrouchLeft;
                return;
            }
            else if ((marioMovement == MarioMovement.RunRight || marioMovement == MarioMovement.IdleRight) && marioState != MarioState.Small && !(isJumping))
            {
                marioMovement = MarioMovement.CrouchRight;
                return;
            }

        }
        public void Crouch()
        {
            HandleCrouch();
            SwitchCurrentSprite(marioMovement);
        }
        public void HandleJump()
        {
            if (marioMovement == MarioMovement.RunLeft || marioMovement == MarioMovement.IdleLeft)
                marioMovement = MarioMovement.JumpLeft;
            else if (marioMovement == MarioMovement.RunRight || marioMovement == MarioMovement.IdleRight)
                marioMovement = MarioMovement.JumpRight;
            SwitchCurrentSprite(marioMovement);
        }

        public void Fall()
        {
            if (!isCollideFloor)
                mario.SetPosition(mario.Bounds.X, mario.Bounds.Y + SEVEN);
            else
            {
                isCollideUnder = false;
                EndJump();
            }
        }

        public void EndJump()
        {

            isJumping = false;
            //SwitchCurrentSprite(marioMovement);
        }
        public void Jump()
        {
            if (!isJumping)
                HandleJump();
            isJumping = true;
            SwitchCurrentSprite(marioMovement);
        }

        public void Star()
        {
            BGMFactory.Instance.Play_StarMan();
            starPower = true;
        }
        public bool isStar()
        {
            return starPower;
        }
        private void HandleStarPower()
        {
            if (starCounter < starDuration)
            {
                starCounter++;
            }
            else
            {
                starPower = false;
                starCounter = ZERO;
                BGMFactory.Instance.Play_MainTheme();
            }
        }
        private void DrawStarMario(SpriteBatch spriteBatch)
        {
            if (frameCount < THREE)
            {
                currentSprite.Draw(spriteBatch, Color.LightGreen);
            }
            else if (frameCount < SIX)
            {
                currentSprite.Draw(spriteBatch, Color.MediumVioletRed);
            }
            else if (frameCount < NINE)
            {
                currentSprite.Draw(spriteBatch, Color.Black);
            }
            else
            {
                currentSprite.Draw(spriteBatch);
            }
            frameCount++;
            if (frameCount > TWELVE)
                frameCount = ZERO;

        }
        public void KillMario()
        {
            marioMovement = MarioMovement.Die;
            deathAnimTimeRemaining = deathAnimLength;
            currentSprite = smallStates[marioMovement];
        }

        private void FinishStateTransition()
        {
            //Gain mushroom
            if (marioState == MarioState.Big
                && (marioMovement == MarioMovement.GrowLeft || marioMovement == MarioMovement.GrowRight))
            {
                marioMovement = (marioMovement == MarioMovement.GrowRight) ? MarioMovement.IdleRight : MarioMovement.IdleLeft;
                UpdateSprite();
            }
            //Damaged to small
            else if (marioState == MarioState.Small
                && (marioMovement == MarioMovement.ShrinkLeft || marioMovement == MarioMovement.ShrinkRight))
            {
                marioMovement = (marioMovement == MarioMovement.ShrinkRight) ? MarioMovement.IdleRight : MarioMovement.IdleLeft;
                UpdateSprite();
            }
            marioPhysics.ignoreGravity = false;
            marioPhysics.velocity = marioPhysics.transitionVel;
        }

        private void UpdateSprite()
        {
            if (marioState == MarioState.Small && smallStates.ContainsKey(marioMovement))
                currentSprite = smallStates[marioMovement];
            else if (marioState == MarioState.Big && bigStates.ContainsKey(marioMovement))
                currentSprite = bigStates[marioMovement];
            else if (marioState == MarioState.Fire && fireStates.ContainsKey(marioMovement))
                currentSprite = fireStates[marioMovement];
        }

        private void HandleDeathAnimation(GameTime gameTime)
        {
            if (marioMovement != MarioMovement.Die)
                return;

            deathAnimTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            float move = deathAnimSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Move up from 5%-15%
            if (deathAnimTimeRemaining < 0.9167f * deathAnimLength && deathAnimTimeRemaining >= 0.74999f * deathAnimLength)
                mario.ForceMove(0, -move);
            //Then move down from 30%-100%
            else if (deathAnimTimeRemaining < 0.74999f * deathAnimLength)
                mario.ForceMove(0, move);

            //When we finish animation, reset level.
            if (deathAnimTimeRemaining <= ZERO)
            {
                MarioEvents.Reset(this, EventArgs.Empty);
                MarioGame.Instance.TriggerDeathSequence();
            }
        }

        public void Update(GameTime gameTime, int spriteXPos, int spriteYPos)
        {
            if (marioMovement != MarioMovement.Die)
            {
                //If we're transitioning, handle transition timer.
                if (stateTransitionTimeRemaining > ZERO)
                {
                    stateTransitionTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    //To or from fire mario change colors
                    if (marioState == MarioState.Fire || (marioState == MarioState.Big && (marioMovement != MarioMovement.GrowLeft && marioMovement != MarioMovement.GrowRight)))
                    {
                        int fireFrame = (int)((stateTransitionTimeRemaining / firePowerupTransitionLength) * firePowerupTransitionColors.Length);
                        stateTransitionColor = firePowerupTransitionColors[fireFrame];
                    }
                    //Else just draw white
                    else
                        stateTransitionColor = Color.White;

                    if (stateTransitionTimeRemaining <= ZERO)
                        FinishStateTransition();
                }

                if (blinkTimeRemaining > ZERO)
                {
                    blinkTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    blinkShow = !blinkShow;

                    if (!blinkShow)
                        stateTransitionColor = Color.Transparent;
                    else if (stateTransitionTimeRemaining <= ZERO)
                        stateTransitionColor = Color.White;
                }

                if (warpTimeRemaining > 0)
                {
                    warpTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    bool vertical = warpDirection == Direction.Up || warpDirection == Direction.Down;
                    float speed = ((vertical) ? mario.Bounds.Height : mario.Bounds.Width) / marioWarpMoveLength;

                    float move = speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if ((warpMoveFirst && warpTimeRemaining > marioWarpStillLength - 0.1f) || (!warpMoveFirst && warpTimeRemaining <= marioWarpMoveLength + 0.01f))
                    {
                        if (vertical)
                            mario.ForceMove(ZERO, move * ((warpDirection == Direction.Up) ? -1 : 1));
                        else
                            mario.ForceMove(move * ((warpDirection == Direction.Left) ? -1 : 1), ZERO);
                    }

                    if (warpTimeRemaining <= ZERO)
                    {
                        //If we start with move, this is first phase, move to second
                        if (warpMoveFirst)
                            MarioGame.Instance.SecondWarpPhase();
                        //Otherwise finish warp
                        else
                        {
                            MarioGame.Instance.FinishWarp();
                            mario.MarioEnterWarpZone();
                        }
                        if(mario.MarioIsInWarpZone())
                            mario.Jump(null, null);
                        else
                        {

                            marioPhysics.ignoreGravity = false;
                            
                        }
                    }
                }

                if (starPower)
                {
                    HandleStarPower();
                }

                if (mario.isCollideGround)
                {
                    isJumping = false;
                    if (marioMovement == MarioMovement.JumpLeft)
                        marioMovement = MarioMovement.RunLeft;
                    else if (marioMovement == MarioMovement.JumpRight)
                        marioMovement = MarioMovement.RunRight;
                    SwitchCurrentSprite(marioMovement);
                }
            }
            else
                HandleDeathAnimation(gameTime);

            HandleTurnFrames();

            if (flagpoleSequence && !MarioGame.Instance.gameComplete)
            {
                HandleFlagPoleSequence();
            }


            currentSprite.Update(gameTime, spriteXPos, spriteYPos);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (MarioGame.Instance.gameComplete)
                return;
            if (starPower)
            {
                DrawStarMario(spriteBatch);
            }
            else if (stateTransitionTimeRemaining > ZERO || blinkTimeRemaining > ZERO)
                currentSprite.Draw(spriteBatch, stateTransitionColor);
            else
                currentSprite.Draw(spriteBatch);
        }

        public ISprite CurrentSprite
        {
            get { return currentSprite; }
        }
        public MarioState CurrentState
        {
            get { return marioState; }
        }
        public MarioMovement CurrentMovement
        {
            get { return marioMovement; }
        }

        public void Reset()
        {
            canGrow = true;
            starPower = false;
            isJumping = false;
            flagpoleSequence = false;
            flagpoleColPos = new Vector2();
            flagpoleSlide = new Vector2();
            finishSequence = false;
            flagpoleCount = 0;
            marioState = MarioState.Small;
            marioMovement = MarioMovement.IdleRight;
            currentSprite = smallStates[marioMovement];
            SwitchCurrentSprite(marioMovement);
            stateTransitionTimeRemaining = ZERO;
            warpTimeRemaining = ZERO;
            blinkTimeRemaining = ZERO;
        }

        public void BeginWarpAnimation(Direction moveDir, bool startWithMoving)
        {
            warpMoveFirst = startWithMoving;
            warpTimeRemaining = marioWarpMoveLength + marioWarpStillLength;
            warpDirection = moveDir;
        }

        private void HandleFlagPoleSequence()
        {
            if (flagpoleCount == ZERO)
            {
                BGMFactory.Instance.Play_LevelComplete();
                marioMovement = MarioMovement.Flagpole;
                SwitchCurrentSprite(marioMovement);
                mario.position = flagpoleColPos;
                flagpoleSlide = flagpoleColPos;
                flagpoleCount++;
            }
            else if (mario.position.Y > -345)
            {
                SlideDownFlagPoll();
                mario.position = flagpoleSlide;
            }
            else if (finishSequence)
            {
                if (mario.position.X < 6520)
                {
                    if(marioState == MarioState.Big || marioState == MarioState.Fire)
                        flagpoleSlide.Y = -355;
                    flagpoleSlide.X++;
                    mario.position = flagpoleSlide;
                }
                else
                {
                    MarioGame.Instance.gameComplete = true;
                    BGMFactory.Instance.DisableRepeatMode();
                    return;
                }
            }
            else if (mario.position.Y <= -345)
            {
                if (Score.flagAtBottom)
                {
                    marioMovement = MarioMovement.RunRight;
                    SwitchCurrentSprite(marioMovement);
                    mario.position = new Vector2(6360, -381);
                    flagpoleSlide = mario.position;
                    finishSequence = true;
                }
            }

            
        }

        private void SlideDownFlagPoll()
        {           
                flagpoleSlide.Y = flagpoleSlide.Y - 3f;
        }
    }
}