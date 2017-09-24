﻿using System.Collections.Generic;

namespace Lasagna
{
    public class KoopaEnemy : MovingEnemy
    {
        private Dictionary<EnemyState, ISprite> koopaStates = new Dictionary<EnemyState, ISprite>()
        {
            { EnemyState.Idle, EnemySpriteFactory.Instance.CreateSprite_Koopa_Walk() },
            { EnemyState.Dead, EnemySpriteFactory.Instance.CreateSprite_Koopa_Die() },
        };

        public KoopaEnemy(int spawnPosX, int spawnPosY)
            : base(spawnPosX, spawnPosY)
        {
            //WalkLeft and WalkRight use same sprite as idle, set that here.
            if (koopaStates.ContainsKey(EnemyState.Idle))
            {
                koopaStates.Add(EnemyState.WalkLeft, koopaStates[EnemyState.Idle]);
                koopaStates.Add(EnemyState.WalkRight, koopaStates[EnemyState.Idle]);
            }
        }

        public override void ChangeState(EnemyState newState)
        {
            if (koopaStates != null && koopaStates.ContainsKey(currentState) && koopaStates[currentState] != null)
            {
                currentState = newState;
                currentSprite = koopaStates[currentState];
            }
        }

        public override void Damage()
        {
            ///TODO: Turn into shell here instead of calling base method
            base.Damage();
        }
    }
}