﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lasagna
{
    interface IItem
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
    }
}
