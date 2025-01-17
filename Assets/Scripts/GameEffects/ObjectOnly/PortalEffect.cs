﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * OnAwake the effect creates a clone of the gameobject it is on,
 * and places it at a random position in the map.
 * When the effect is triggered i sends the target to the next tile, following the 
 * given target->origin direction, of its clone. To make the transition it adds
 * a PortalFeat to the target(Characther).
 * */
public class PortalEffect : GameEffect {

	protected Interactive _Clone;

	void Start () {
		if (_Clone==null) {
			Interactive clone = GeneralFabric.CreateObject<Interactive>(gameObject, transform.parent);
			_Clone = clone;
			clone.GetComponent<PortalEffect>().SetClone(GetComponent<Interactive>());
			MapController.Instance.PlaceIt(MapController.Instance.GetMapTiles(), clone);
		}
	}

	protected override void DoEffect (Interactive origin, Interactive target) {
		MapController mapController = MapController.Instance;

		Tile tile = mapController.GetNextTile(
			_Clone.MyTile, mapController.GetDirection(target.MyTile, origin.MyTile));

		if (tile==null || (tile.OnTop!=null && tile.OnTop.Blockable)) {
			tile = null;
			List<Tile> neighbours = mapController.GetNeighbours(_Clone.MyTile);
			for (int i=0; i<neighbours.Count; i++) {
				if (neighbours[i].OnTop==null || (neighbours[i].OnTop!=null && !neighbours[i].OnTop.Blockable)) {
					tile = neighbours[i];
					i = neighbours.Count;
				}
			}
		}

		if (tile.OnTop==null || !tile.OnTop.Blockable) {
			if (target is Characther) 
				((Characther)target).AddMovementFeat(new PortalFeat(_Clone.MyTile, tile));
			if (target is Personage) ((Personage)target).InterruptActions();
		}

	}

	protected void SetClone (Interactive clone) {
		_Clone = clone;
	}

}
