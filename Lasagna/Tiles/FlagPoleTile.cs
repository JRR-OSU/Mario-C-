﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Lasagna
{
    class FlagPoleTile : ITile
    {
        /// <summary>
        /// State = 0 : Not moving, State = 1 : Moving down
        /// </summary>
        private int State = 0;
        private int spriteXPos;
        private int spriteYPos;
		ISprite flag = TileSpriteFactory.Instance.CreateSprite_Flag();
		ISprite flagPole = TileSpriteFactory.Instance.CreateSprite_FlagPole();

        public FlagPoleTile(int spriteXPos, int spriteYPos)
        {
            this.spriteXPos = spriteXPos;
            this.spriteYPos = spriteYPos;
        }
        public void ChangeState()
        {
            this.State = 1;
        }
        public void Update(GameTime gameTime)
        {
            this.flagPole.Update(gameTime, this.spriteXPos, this.spriteYPos);
            this.flag.Update(gameTime, spriteXPos - 24, spriteYPos + 16);
            if (this.State == 1)
            {
                //Move flag down
            }
        }
        public void Draw(SpriteBatch spriteBatch) {
            this.flagPole.Draw(spriteBatch);
            this.flag.Draw(spriteBatch);
        }
    }
}
