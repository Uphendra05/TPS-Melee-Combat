using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{

    [System.Serializable]
    public struct WeaponEntry
    {
        public WeaponType Type;
        public GameObject OffHandedWeapon; // Do I need this ?
    }

    public WeaponTracer currentWeapon;


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
        currentWeapon.OnHitLanded += _combatSystem.OnWeaponHit;
    }

    public void UnequipCurrent()
    {
       currentWeapon.OnHitLanded -= _combatSystem.OnWeaponHit;
       _currentEntry = default;
    }



}
