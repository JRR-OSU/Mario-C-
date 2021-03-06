﻿using System.Collections.Generic;

namespace Lasagna
{
    public class GoombaEnemy : MovingEnemy
    {
        private Dictionary<EnemyState, ISprite> goombaStates = new Dictionary<EnemyState, ISprite>()
        {
            { EnemyState.Idle, EnemySpriteFactory.Instance.CreateSprite_Goomba_Walk() },
            { EnemyState.Dead, EnemySpriteFactory.Instance.CreateSprite_Goomba_Die() },
            { EnemyState.Flipped, EnemySpriteFactory.Instance.CreateSprite_Goomba_Flipped() },
        };

        public GoombaEnemy(int spawnPosX, int spawnPosY)
            : base(spawnPosX, spawnPosY)
        {
            //WalkLeft and WalkRight use same sprite as idle, set that here.
            if (goombaStates.ContainsKey(EnemyState.Idle))
            {
                goombaStates.Add(EnemyState.WalkLeft, goombaStates[EnemyState.Idle]);
                goombaStates.Add(EnemyState.WalkRight, goombaStates[EnemyState.Idle]);
                CurrentSprite = goombaStates[EnemyState.Idle];
            }
        }

        public override void Damage()
        {
            return;
        }

        public override void ChangeState(EnemyState newState)
        {
            if (goombaStates != null && goombaStates.ContainsKey(CurrentState) && goombaStates[CurrentState] != null)
            {
                FixSpritePosition(goombaStates[CurrentState], goombaStates[newState]);
                CurrentState = newState;
                CurrentSprite = goombaStates[CurrentState];
            }
        }

        protected override void OnCollisionResponse(IPlayer mario, CollisionSide side)
        {
            if (side.Equals(CollisionSide.Top) && currentHealth == EnemyHealth.Normal)
            {
                ChangeState(EnemyState.Dead);
                SoundEffectFactory.Instance.PlayStomp();
                currentHealth = EnemyHealth.Stomped;
            }
            else if((mario is Mario && ((Mario)mario).StarPowered) && currentHealth == EnemyHealth.Normal)
            {
                ChangeState(EnemyState.Flipped);
                currentHealth = EnemyHealth.Flipped;
                SoundEffectFactory.Instance.PlayStomp();
                Score.increaseScoreMario(200);
            }
        }

        protected override void OnCollisionResponse(IProjectile fireball, CollisionSide side)
        {
            if ((side.Equals(CollisionSide.Left) || side.Equals(CollisionSide.Right)) 
                && currentHealth == EnemyHealth.Normal)
            {
                ChangeState(EnemyState.Flipped);
                currentHealth = EnemyHealth.Flipped;
                SoundEffectFactory.Instance.PlayStomp();
                Score.increaseScoreMario(200);
            }
        }
    }
}
