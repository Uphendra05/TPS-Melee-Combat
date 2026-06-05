using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{

    [System.Serializable]
    public struct WeaponEntry
    {
        public WeaponType Type;
        public Weapon Weapon;
        public GameObject OffHandedWeapon;
    }


    [Section("Weapons")]
    [SerializeField] private List<WeaponEntry> _weapons;
    private WeaponEntry _currentEntry;
    private PlayerCombatSystem _combatSystem;


    private Dictionary<WeaponType, WeaponEntry> _weaponMap = new();

    private void Awake()
    {
        _combatSystem = GetComponent<PlayerCombatSystem>();

        foreach (var entry in _weapons)
            _weaponMap.Add(entry.Type, entry);
    }

    public void EquipByType(WeaponType type)
    {
        if (!_weaponMap.TryGetValue(type, out var entry)) return;

        UnequipCurrent();

        _currentEntry = entry;
        (entry.Weapon as MonoBehaviour)?.gameObject.SetActive(true);
        entry.OffHandedWeapon?.SetActive(true); 
        entry.Weapon.OnHit += _combatSystem.DamageEnemy;
    }

    public void UnequipCurrent()
    {
        if (_currentEntry.Weapon != null)
        {
            _currentEntry.Weapon.OnHit -= _combatSystem.DamageEnemy;
            (_currentEntry.Weapon as MonoBehaviour)?.gameObject.SetActive(false);
            _currentEntry.OffHandedWeapon?.SetActive(false); 
            _currentEntry = default;
        }
    }

    public IWeapon GetEquippedWeapon() => _currentEntry.Weapon;


}
