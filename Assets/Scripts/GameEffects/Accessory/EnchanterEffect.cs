﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Changes the enemy TargetChar to the enemy that is closest to him.
 * If there is none, then trap the enemy for 1 turn.
 * */
public class EnchanterEffect : GameEffect {

	protected override void DoEffect (Interactive origin, Interactive target) {
		if (target != null && target is Enemy) {
			Enemy targetEnemy = target as Enemy;

			List<Enemy> enemies = FindObjectOfType<EnemiesController>().GetEnemies();

			if (enemies.Count > 0) {
				MapController mapControl = MapController.Instance;
				Enemy closerEnemy = null;
				int closerDist = 0;
				for (int i=0; i<enemies.Count; i++) {
					if (enemies[i] != targetEnemy) {
						int dist = mapControl.GetDistance(enemies[i].MyTile,target.MyTile);
						if (closerEnemy==null || closerDist > dist) {
							closerEnemy = enemies[i];
							closerDist = dist;
						}
					}
				}

				targetEnemy.TargetChar = closerEnemy;
				targetEnemy.RevealTargetCharacther();
			}
			else
				targetEnemy.BeTrapped(1);
		}
	}

}
