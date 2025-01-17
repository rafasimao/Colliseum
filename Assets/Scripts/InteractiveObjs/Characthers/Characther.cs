using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Facade class
public abstract class Characther : Attackable {

	// Attributes
	[SerializeField]
	protected int _MaxLife = 2, _Life = 2;
	[SerializeField]
	protected int _Strength = 1;
	[SerializeField]
	protected int _AtkRange = 1;
	
	// Status
	protected CharactherStatus _CharStatus;
	// Movement 
	protected CharactherMovement _CharMovement;

	public CharHUD _CharHUD;

	// Itens
	protected CharactherWeapons _CharWeapons; // Weapons
	public WeaponsHUD CharWeaponsHUD;
	// Accessories
	protected CharactherAccessories _CharAccessories;
	public AccessoriesHUD CharAccessoriesHUD;

	// Initiates
	virtual protected void Start () {
		if (_CharStatus == null)
			_CharStatus = new CharactherStatus();
		if (_CharMovement == null)
			_CharMovement = new CharactherMovement(this);
		if (_CharWeapons == null)
			_CharWeapons = new CharactherWeapons(CharWeaponsHUD);
		if (_CharAccessories == null)
			_CharAccessories = new CharactherAccessories(4,4,CharAccessoriesHUD);
	}

	/** Weapons/Accessories HUD's setters ************************/
	public void SetCharWeaponsHUD (WeaponsHUD weaponsHUD) {
		CharWeaponsHUD = weaponsHUD;
		if (_CharWeapons!=null)
			_CharWeapons.CharWeaponsHUD = weaponsHUD;
	}

	public void SetCharAccessoriesHUD (AccessoriesHUD accessoriesHUD) {
		CharAccessoriesHUD = accessoriesHUD;
		if (_CharAccessories!=null)
			_CharAccessories.CharAccessoriesHUD = accessoriesHUD;
	}

	/**
	 * Must be called to make the char perform the characther actions.
	 */
	protected virtual void Update () {
		// If not dead, performs usual actions
		if (!IsDead())
			_CharMovement.UpdateCharactherMovement();
	}

	/**
	 * Called to make the characther perform a action,
	 * must be overrided by the children class.
	 * */
	public bool MakeAction () {
		// If it is dead, do nothing
		bool result = (IsDead() || _CharStatus.IsTrapped());

		if (IsInAction()) return false;

		// Make microturn action
		if (!result) 
			result = MakeTurnAction();

		// Update components
		if (result) {
			_CharStatus.UpdateStatus();
			_CharWeapons.UpdateWeapons();
			_CharAccessories.UpdateAccessories();
		}

		return result; // Return if made action
	}

	abstract protected bool MakeTurnAction ();
	
	// Moving ------------------------------------------
	protected void AddMoveTo (Tile tile) {
		if (!_CharStatus.IsParalized())
			_CharMovement.AddMoveTo(tile);
	}

	public void AddMovementFeat (IFeat feat) {
		_CharMovement.AddMovementFeat(feat);
	}

	public bool IsMoving () {
		return _CharMovement.IsMoving();
	}

	public bool IsInAction () {
		return _CharMovement.IsInAction();
	}

	// Attacking ---------------------------------------
	protected void Attack (Interactive target) {
		_CharMovement.LookAtDirection(target.transform.position - transform.position);
		GetComponent<Animation>().Play("Attack");

		if (_CharWeapons.IsAnyFirstWeaponEquipped() && !target.Interactable)
			_CharWeapons.Attack(this, target);
		else
			target.BeAttacked(this, GetCharAtkDamage(target));

		_CharAccessories.OnAttack(this, target);

	}
	
	// Overrides ---------------------------------------
	//Be hitted by something
	public override void BeAttacked (Interactive iObj, int damage) {
		_Life -= damage;
		if (_Life<1 && Attackable) {
			GetComponent<Animation>().Play("Dead");
			Attackable = false;
			MyTile.TryGetOut(this);// To make it trespassable
			_CharMovement.InterruptMoves();
			GetComponent<Collider>().enabled = false;
			GetComponent<Rigidbody>().useGravity = false;
		} else if(Attackable)
			GetComponent<Animation>().Play("Damage");

		_CharAccessories.OnBeAttacked(this, iObj);
	}

	// Itens --------------------------------------------
	/* Weapon *********/
	public Weapon TryToEquip (Weapon weapon) {
		return _CharWeapons.EquipWeapon(weapon, CharactherWeapons.WeaponHand.FirstHand);
	}

	public void SwitchWeapons () {
		_CharWeapons.SwitchWeapons();
	}

	public Weapon GetFirstWeapon () {
		return _CharWeapons.GetFirstWeapon();
	}

	public Weapon GetSecondWeapon () {
		return _CharWeapons.GetSecondWeapon();
	}

	/* Accessories ********/
	public Accessory TryToEquip (Accessory accessory) {
		return _CharAccessories.EquipAccessory(accessory, _CharAccessories.GetFreeSpot());
	}

	public Accessory GetAccessory (int pos) {
		return _CharAccessories.GetAccessory(pos);
	}

	public void RemoveAccessory (int pos) {
		_CharAccessories.RemoveAccessory(pos);
	}

	// Accessory Action
	public void ActivateAccessory (int index, Tile tile) {
		_CharAccessories.Activate(index, this, tile);
	}

	// Getters ---------------------------------------------
	public bool IsDead () {
		return (_Life < 1);
	}

	public int GetMaxLife () {
		return _MaxLife;
	}

	public int GetLife () {
		return _Life;
	}

	public bool IsAttackReady () {
		return (_CharWeapons.IsAnyFirstWeaponEquipped()) ? 
			_CharWeapons.IsFirstWeaponReady() : true;
	}

	public void SetMyTile (Tile t) {
		MyTile = t;
	}

	// with modifiers
	public int UseStrengthModifiers (Interactive target) {
		return _CharStatus.UseStrengthModifiers(target) + _CharAccessories.GetStrengthModifier();
	}

	protected int GetCharAtkDamage (Interactive target) {
		return _Strength + UseStrengthModifiers(target);
	}

	protected int GetCurrentAttackRange () {
		if (_CharWeapons.IsAnyFirstWeaponEquipped())
			return _CharWeapons.GetFirstWeaponAttackRange() + _CharAccessories.GetRangeModifier();
		return _AtkRange + _CharAccessories.GetRangeModifier();
	}

	public int UseEnemyVisionModifier () {
		return _CharAccessories.GetEnemyVisionModifier();
	}

	// Player Status ---------------------------------------
	public void BeTrapped (int turns) {
		_CharStatus.BeTrapped(turns);
	}

	public void BeBuffered (int strBuff) {
		_CharStatus.BeBuffered(strBuff);
	}

	public void BeParallized (int turns) {
		_CharStatus.BeParalized(turns);
	} 

	public void BeUnparallized () {
		_CharStatus.BeParalized(0);
	}

	public bool IsTrapped () {
		return _CharStatus.IsTrapped();
	}
	
	public bool IsParalized () {
		return _CharStatus.IsParalized();
	}
	
	public bool IsBuffered () {
		return _CharStatus.IsBuffered();
	}

	// BeSaw by another char
	virtual public void BeSaw (Interactive target) {
		_CharAccessories.OnBeSaw(this, target);
		if (_CharHUD!=null) _CharHUD.ShowSurprised();
	}

}
