﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lasagna
{
    public abstract class BaseTile : ITile
    {
        private ISprite currentSprite;
        private int posX;
        private int posY;
        public virtual Rectangle Properties
        {
            get
            {
                if (CurrentSprite == null)
                {
                    return new Rectangle(0, 0, 0, 0);
                }
                return new Rectangle(posX, posY, CurrentSprite.Width, CurrentSprite.Height);
            }
        }
        protected ISprite CurrentSprite
        {
            get { return currentSprite; }
            set { currentSprite = value; }
        }
        protected int PosX { get { return posX; } }
        protected int PosY { get { return posY; } }

        protected BaseTile(int spawnPosX, int spawnPosY)
        {
            posX = spawnPosX;
            posY = spawnPosY;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (currentSprite != null)
                currentSprite.Update(gameTime, posX, posY);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (currentSprite != null)
                currentSprite.Draw(spriteBatch);
        }
        
        public virtual void OnCollisionResponse(IPlayer Mario, CollisionSide side)
        {
            return;
        }
        public virtual void OnCollisionResponse(IItem Item, CollisionSide side)
        {
            return;
        }
        public abstract void ChangeState();

        public void OnCollisionResponse(IEnemy enemy, CollisionSide side)
        {
            return;
        }
    }
}
