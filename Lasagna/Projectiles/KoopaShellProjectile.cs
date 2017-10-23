﻿using Microsoft.Xna.Framework;

namespace Lasagna
{
    public class KoopaShellProjectile : BaseProjectile
    {
        private enum KoopaShellStates
        {
            Idle,
            Sliding,
            Gone
        }

        private int hitCount = 0;
        private int slidingTime = 0;
        private bool isMovingRight = true;
        private KoopaShellStates currentState = KoopaShellStates.Idle;
        private bool isShellKicked = false;

        public bool IsShellKicked {
            get
            {
                return isShellKicked;
            }
        }

        private ISprite shellDefault = EnemySpriteFactory.Instance.CreateSprite_Koopa_Shell();

        public KoopaShellProjectile(int spawnPosX, int spawnPosY, bool startMovingRight)
            : base(spawnPosX, spawnPosY, startMovingRight)
        {
            CurrentSprite = shellDefault;
        }

        public override void Update(GameTime gameTime)
        {
            if (slidingTime >= 100)
                CurrentSprite = null;
            if (currentState == KoopaShellStates.Sliding)
            {
                if (isMovingRight == true)
                    posX += (float)(gameTime.ElapsedGameTime.TotalSeconds * horizontalMoveSpeed) * (MovingRight ? 1 : -1);
                else
                    posX -= (float)(gameTime.ElapsedGameTime.TotalSeconds * horizontalMoveSpeed) * (MovingRight ? 1 : -1);

                slidingTime++;
            }

            base.Update(gameTime);
        }

        public override void DestroyShell()
        {
            currentState = KoopaShellStates.Gone;
            CurrentSprite = null;
        }

        protected override void OnCollisionResponse(IEnemy Enemy, CollisionSide side)
        {
            return;
        }

        protected override void OnCollisionResponse(ITile tile, CollisionSide side)
        {
            if (side.Equals(CollisionSide.Right))
                isMovingRight = false;
            else if (side.Equals(CollisionSide.Left))
                isMovingRight = true;
        }

        protected override void OnCollisionResponse(IItem Item, CollisionSide side)
        {
            return;
        }
        protected override void OnCollisionResponse(IPlayer player, CollisionSide side)
        {
            if (side.Equals(CollisionSide.Top))
            {
                hitCount++;
            }
            if (hitCount >= 2)
            {
                currentState = KoopaShellStates.Sliding;
                isShellKicked = true;
            }
        }
        protected override void OnCollisionResponse(IProjectile projectile, CollisionSide side)
        {
            if (projectile is KoopaShellProjectile)
                currentState = KoopaShellStates.Idle;
        }
    }
}