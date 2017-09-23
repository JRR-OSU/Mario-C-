﻿namespace Lasagna
{
    public class MissileProjectile : BaseProjectile
    {
        private ISprite missileSprite; //Reserved for missle sprite.

        public MissileProjectile(int spawnPosX, int spawnPosY)
            : base(spawnPosX, spawnPosY)
        {
            currentSprite = missileSprite;
        }

        public override void ChangeState()
        {
            //Reserved for later
        }
    }
}
