﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.ObjectModel;

namespace Lasagna
{
    public static class CollisionDetection
    {
        private const float Half = 0.5f;
        private const int Zero = 0;

        //By the interfaces, we know IPlayer, IEnemy, ITile, and IItem are all IColliders as well.
        public static void Update(ReadOnlyCollection<IPlayer> players, ReadOnlyCollection<IEnemy> enemies, ReadOnlyCollection<ITile> tiles, ReadOnlyCollection<IItem> items, ReadOnlyCollection<IProjectile> projectiles)
        {
            //Players vs. Players, Tiles, Enemies, Items, projectiles.
            foreach (IPlayer player in players)
            {
                //If player is dead, exit.
                if (player.IsDead)
                    continue;

                CheckAllCollisions<ITile>(player, player.Bounds, tiles);

                //If this player is mario, and he's transitioning/blinking, we only check collisions for tiles
                if (player is Mario && ((Mario)player).IsBlinking)
                    continue;

                CheckAllCollisions<IPlayer>(player, player.Bounds, players);
                CheckAllCollisions<IEnemy>(player, player.Bounds, enemies);
                CheckAllCollisions<IItem>(player, player.Bounds, items);
                CheckAllCollisions<IProjectile>(player, player.Bounds, projectiles);
            }

            //Tiles are static, so don't need to check against themselves.
            //Tiles vs. Enemies, Items, projectiles.
            foreach (ITile tile in tiles)
            {
                //Invisible block don't check if invisible
                if (tile is InvisibleItemBlockTile && !((InvisibleItemBlockTile)tile).IsVisible)
                    continue;

                CheckAllCollisions<IEnemy>(tile, tile.Bounds, enemies);
                CheckAllCollisions<IItem>(tile, tile.Bounds, items);
                CheckAllCollisions<IProjectile>(tile, tile.Bounds, projectiles);
            }

            //Enemies vs enemies, items, projectiles
            foreach (IEnemy enemy in enemies)
            {
                CheckAllCollisions<IEnemy>(enemy, enemy.Bounds, enemies);
                CheckAllCollisions<IItem>(enemy, enemy.Bounds, items);
                CheckAllCollisions<IProjectile>(enemy, enemy.Bounds, projectiles);
            }

            //Projectiles vs projectiles
            foreach (IProjectile proj in projectiles)
                CheckAllCollisions<IProjectile>(proj, proj.Bounds, projectiles);
        }

        public static bool CheckRectForCollisions(ICollider sourceCollider, Rectangle rect, ReadOnlyCollection<IEnemy> enemies, ReadOnlyCollection<ITile> tiles)
        {
            bool collided = CheckAllCollisions<IEnemy>(sourceCollider, rect, enemies);
            collided = CheckAllCollisions<ITile>(sourceCollider, rect, tiles) || collided;

            return collided;
        }

        private static bool CheckAllCollisions<T>(ICollider sourceCollider, Rectangle sourceRect, ReadOnlyCollection<T> targetColliders)
        {
            if (!typeof(ICollider).IsAssignableFrom(typeof(T)) || sourceCollider == null
                || sourceRect == Rectangle.Empty || targetColliders == null)
                return false;

            bool collided = false;
            float biggestCollisionLength = Zero;
            ICollider targetCollider = null;
            CollisionSide biggestSourceSide = CollisionSide.None, biggestTargetSide = CollisionSide.None;

            //Find biggest collision intersection
            foreach (ICollider col in targetColliders)
            {
                CollisionSide sourceSide, targetSide;
                if (sourceCollider != col && CheckCollision(sourceRect, col.Bounds, out sourceSide, out targetSide))
                {
                    collided = true;

                    Rectangle overlap = Rectangle.Intersect(sourceRect, col.Bounds);
                    float colSize = overlap.Width * overlap.Height;
                    if (colSize > biggestCollisionLength)
                    {
                        biggestCollisionLength = colSize;
                        targetCollider = col;
                        biggestSourceSide = sourceSide;
                        biggestTargetSide = targetSide;
                    }
                }
            }

            //Only apply collision response for the collider which we collided most with this frame
            if (collided && targetCollider != null 
                && biggestSourceSide != CollisionSide.None && biggestTargetSide != CollisionSide.None)
            {
                sourceCollider.OnCollisionResponse(targetCollider, biggestSourceSide);
                targetCollider.OnCollisionResponse(sourceCollider, biggestTargetSide);
            }

            return collided;
        }
        
        private static bool CheckCollision(Rectangle r1, Rectangle r2, out CollisionSide r1CollisionSide, out CollisionSide r2CollisionSide)
        {
            r1CollisionSide = CollisionSide.None;
            r2CollisionSide = CollisionSide.None;

            if (r1.IsEmpty || r2.IsEmpty || !r1.Intersects(r2))
                return false;

            r1CollisionSide = GetSideOfCollision(r1, r2);
            r2CollisionSide = GetSideOfCollision(r2, r1);

            return r1CollisionSide != CollisionSide.None && r2CollisionSide != CollisionSide.None;
        }

        private static CollisionSide GetSideOfCollision(Rectangle sourceRect, Rectangle targetRect)
        {
            CollisionSide side;

            float avgWidth = Half * (sourceRect.Width + targetRect.Width);
            float avgHeight = Half * (sourceRect.Height + targetRect.Height);
            float xDirection = sourceRect.Center.X - targetRect.Center.X;
            float yDirection = sourceRect.Center.Y - targetRect.Center.Y;

            if (targetRect.IsEmpty || sourceRect.IsEmpty || Math.Abs(xDirection) > avgWidth || Math.Abs(yDirection) > avgHeight)
                side = CollisionSide.None;
            else
            {
                float yWidth = avgWidth * yDirection;
                float xHeight = avgHeight * xDirection;

                if (yWidth > xHeight)
                    side = (yWidth > -xHeight) ? CollisionSide.Top : CollisionSide.Right;
                else
                    side = (yWidth > -xHeight) ? CollisionSide.Left : CollisionSide.Bottom;
            }

            return side;
        }
    }
}
