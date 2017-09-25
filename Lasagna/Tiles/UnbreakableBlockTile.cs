﻿namespace Lasagna
{
    public class UnbreakableBlockTile : BaseTile
    {
        public UnbreakableBlockTile(int spawnXPos, int spawnYPos)
            : base(spawnXPos, spawnYPos)
        {
            CurrentSprite = TileSpriteFactory.Instance.CreateSprite_UnbreakableBlock();
        }

        //Not needed currently
        public override void ChangeState() {}
    }
}
