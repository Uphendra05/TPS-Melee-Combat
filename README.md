# TPS Melee Combat System — Unity

A third person melee combat system built in Unity. Fully script-driven with no animator parameter dependencies. Includes a custom animation event tool inspired by Unreal Engine's Notify system.

---

## Overview

This project is a modular third person melee combat framework built around a single animator state approach. Combos, weapon switching, hit detection, and animation events are all controlled entirely through code — no transition webs, no animator parameters.

---

## Systems

### Combat / Combo System

Script-driven combo chain using a list of `WeaponDataSO` entries. Each entry in the list is one hit in the combo sequence. The system tracks the current combo index, handles input windows, lunge movement toward the closest enemy during an attack, and resets the chain automatically after an idle timeout.

- Single animator state for all attacks via `AnimatorOverrideController`
- Combo window based on `normalizedTime` thresholds
- Lunge toward target during active attack frames
- Auto reset after configurable idle time

---

### Animation Event Tool

A custom data-driven animation event system inspired by Unreal Engine's Notify system. Events are stored in a `ScriptableObject` and are fully decoupled from the animation clip asset — the clip stays untouched.

**Two event types:**

- `Notify` — fires once at a single point in the animation timeline
- `NotifyState` — active across a duration with `OnNotifyStart`, `OnNotifyTick`, and `OnNotifyEnd` callbacks

**Custom Unity Editor:**

- Visual timeline with color coded notify and state tracks
- Scrubber that syncs with a live animation preview window
- Playback controls inside the inspector
- Add, remove, and recolor events directly in the inspector

**Runtime:**

- `AnimationEventPlayer` lives on the player and is shared across all systems
- Each system calls `Play(SO)` and `Stop()` — one central `Tick` in `Player.Update` handles the rest
- Safe interruption via `StopAllActiveStates` so NotifyState callbacks always close cleanly

---

### Weapon System

Weapons are managed through a `WeaponManager` component. Each `WeaponDataSO` carries its own clip, animation event timings, weapon type, damage, and any other combat data. Weapons are equipped and unequipped per combo hit.

- SO based weapon data
- Equip by weapon type
- Each combo hit can use a different weapon

---

### Camera System

Third person camera that follows and rotates around the player. Handles target locking and smooth transitions.

---

### Enemy AI / Target Finding

Closest enemy detection used by the combat system to determine lunge direction and target acquisition during an attack.

---

## Architecture

\```
Player
  ├── Animator + AnimatorOverrideController
  ├── AnimationEventPlayer         ← shared, ticked every frame
  ├── CombatSystem                 ← calls Play / Stop on eventPlayer
  ├── WeaponManager                ← equips per combo hit
  └── Camera / Targeting
\```

---

## How to Use

**Setting up an attack combo:**

1. Create a `WeaponDataSO` via `Create → Combat → WeaponData`
2. Assign an `AnimationClip` and set the `Animator State Name` to match your animator state
3. Add `Notify` or `NotifyState` events in the custom timeline inspector
4. Add entries to the `weaponCombos` list on your `CombatSystem` in order of the combo chain

**Setting up the event player:**

1. Add `AnimationEventPlayer` to your player GameObject
2. Pass the reference to `CombatSystem` via `Init(eventPlayer)`
3. Call `eventPlayer.Tick(normalizedTime)` in `Player.Update`

---

## Built With

- Unity
- C#
- ScriptableObject architecture
- Custom IMGUI editor tooling
