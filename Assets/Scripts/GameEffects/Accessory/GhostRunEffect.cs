﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Adds the GhostFeat to the origin(if it is a Characther), towards
 * the direction pointed by the targetTile, rrespecting the GhostRange.
 * The targetTile must be empty or be Enterable.
 * */
public class GhostRunEffect : GameEffect {

	[SerializeField]
	private float _GhostSpeed;

	[SerializeField]
	private int _GhostRange;

	protected override void DoEffect (Interactive origin, Interactive target) {
		// Do nothing
	}

	protected override void DoEffect (Interactive origin, Tile targetTile) {
		if (origin.MyTile != targetTile && 
		    (targetTile.OnTop == null || (targetTile.OnTop!=null && !targetTile.OnTop.Blockable)) ) {

			if (MapController.Instance.GetNeighbours(origin.MyTile, _GhostRange).Contains(targetTile)) {
				if (origin is Characther) 
					((Characther)origin).AddMovementFeat(new GhostRunFeat(targetTile, _GhostSpeed));
				if (origin is Personage) ((Personage)origin).InterruptActions();
			}
		}
	}

}
