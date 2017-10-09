﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Lasagna
{
    public class InvisibleItemBlockTile : BaseTile
    {
        private enum BlockState
        {
            Invisible,
            Visible
        }

        private BlockState currentState;
        private ISprite visibleSprite = TileSpriteFactory.Instance.CreateSprite_ItemBlockUsed();
        private bool CollidedWithThreeSides = false;

        /*public override Rectangle Bounds
        {
            get
            {
                Rectangle properties = new Rectangle();
                if (currentState == BlockState.Invisible)
                {
                    properties = new Rectangle(base.PosX, base.PosY, CurrentSprite.Width, CurrentSprite.Height - 10);
                }
                else
                {
                    properties = new Rectangle(base.PosX, base.PosY, CurrentSprite.Width, CurrentSprite.Height);
                }
                return properties;
            }
        }
        */

        public override bool MarioCollidedWithThreeSides() { return this.CollidedWithThreeSides; }

        public bool IsVisible
        {
            get { return currentState == BlockState.Visible; }
        }

        public InvisibleItemBlockTile(int spawnXPos, int spawnYPos)
            : base(spawnXPos, spawnYPos)
        {
            CurrentSprite = visibleSprite;
            currentState = BlockState.Invisible;
            MarioEvents.OnReset += ChangeToInvisible;
        }

        public override void Update(GameTime gameTime)
        {
            //Only call base function if we're visible. Else draw nothing.
            if (currentState != BlockState.Invisible)
                base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //Only call base function if we're visible. Else draw nothing.
            if (currentState != BlockState.Invisible)
                base.Draw(spriteBatch);
        }

        public override void ChangeState()
        {
            //Toggles us between visible and invisible
            if (currentState == BlockState.Invisible)
            {
                CurrentSprite = visibleSprite;
                currentState = BlockState.Visible;
            }
            else
            {
                currentState = BlockState.Invisible;
            }

        }

        protected override void OnCollisionResponse(IPlayer Mario, CollisionSide side)
        {
            if (this.currentState.Equals(BlockState.Invisible) && (side.Equals(CollisionSide.Top) ||
                side.Equals(CollisionSide.Left) || side.Equals(CollisionSide.Right)))
            {
                this.CollidedWithThreeSides = true;
            }
            if (this.currentState.Equals(BlockState.Invisible) &&
                (Mario.Bounds.Y > base.PosY + CurrentSprite.Height))
            {
                Debug.WriteLine(this.CollidedWithThreeSides);
                this.CollidedWithThreeSides = false;
            }
            if (this.currentState.Equals(BlockState.Invisible) && side.Equals(CollisionSide.Bottom) && 
                this.CollidedWithThreeSides == false)
            {
                this.ChangeState();
            }
        }

        ///TODO: Temp methods for sprint3
        public void ChangeToInvisible(object sender, EventArgs e)
        {
            if (currentState == BlockState.Visible)
                ChangeState();
        }
    }
}
