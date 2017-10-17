﻿using Microsoft.Xna.Framework;

namespace Lasagna
{
    public class FireProjectile : BaseProjectile
    {
        private enum FireballStates
        {
            Idle,
            Exploding,
            Gone
        }

        //How long fireball goes up after bouncing
        private const float fireballBounceTime = 0.2f;

        private float explodeTimeLeft;
        private float movingUpTimeLeft;
        private FireballStates currentState = FireballStates.Idle;
        private ISprite fireballDefault = ProjectileSpriteFactory.Instance.CreateSprite_Fireball_Default();
        private ISprite fireballExplode = ProjectileSpriteFactory.Instance.CreateSprite_Fireball_Explode();

        public FireProjectile(int spawnPosX, int spawnPosY, bool startMovingRight)
            : base(spawnPosX, spawnPosY, startMovingRight)
        {
            CurrentSprite = fireballDefault;
        }

        public override void Update(GameTime gameTime)
        {
            if (currentState == FireballStates.Idle)
            {
                posX += (float)(gameTime.ElapsedGameTime.TotalSeconds * horizontalMoveSpeed) * (MovingRight ? 1 : -1);

                if (movingUpTimeLeft > 0)
                {
                    posY -= (float)(gameTime.ElapsedGameTime.TotalSeconds * verticalMoveSpeed);
                    movingUpTimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else
                    posY += (float)(gameTime.ElapsedGameTime.TotalSeconds * verticalMoveSpeed);
            }
            else if (currentState == FireballStates.Exploding)
            {
                explodeTimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (explodeTimeLeft < 0)
                    ChangeState();
            }

            base.Update(gameTime);
        }

        public override void ChangeState()
        {
            if (currentState == FireballStates.Idle)
            {
                explodeTimeLeft = fireballExplode.ClipLength;
                currentState = FireballStates.Exploding;
                CurrentSprite = fireballExplode;
            }
            else if (currentState == FireballStates.Exploding)
            {
                currentState = FireballStates.Gone;
                CurrentSprite = null;
            }
        }

        protected override void OnCollisionResponse(IEnemy Enemy, CollisionSide side)
        {
            if (currentState == FireballStates.Idle)
                this.ChangeState();
        }

        protected override void OnCollisionResponse(ITile tile, CollisionSide side)
        {
            //Fireballs bound when they hit tiles
            if (side == CollisionSide.Bottom)
                movingUpTimeLeft = fireballBounceTime;
            else if (currentState == FireballStates.Idle)
                ChangeState();
        }

        protected override void OnCollisionResponse(IItem Item, CollisionSide side)
        {
            if (currentState == FireballStates.Idle)
                this.ChangeState();
        }
    }
}
