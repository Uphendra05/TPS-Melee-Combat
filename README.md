

# TPS Melee Combat System — Unity

A third person melee combat framework built around one idea: everything is driven by code and data, not animator transition webs. Combos, hit detection, animation events and enemy AI are all modular and reusable.

---

## Demo

<!-- Replace the paths with your own GIFs, e.g. docs/combo.gif -->

**Combo and lunge**

https://github.com/user-attachments/assets/f7f4a90a-0f95-49fa-98d8-5da9828a58a4

**Animation event tool (timeline + live preview)**

https://github.com/user-attachments/assets/22366317-b600-4337-9d69-7f2a4ae29fa5

**Weapon tracer (scene view gizmos)**

https://github.com/user-attachments/assets/eee4188c-231a-493d-92fb-627750a1bf8a

**Enemy AI (chase and strafe)**

https://github.com/user-attachments/assets/4a3fe40f-0c64-41d5-9edc-3a0e1bbf8601

---

## Systems

### Combo System

A list of `WeaponDataSO` entries, where each entry is one hit in the chain. A single animator state plays every attack through an `AnimatorOverrideController`, and combo timing is based on `normalizedTime`. The chain resets after an idle timeout, and the player lunges toward the closest enemy during active frames.

### Animation Event Tool

A data-driven alternative to Unity's animation events, inspired by Unreal's Notify system. Events live in a ScriptableObject, so the clip stays untouched and the same clip can have different timings per character.

- `Notify` fires once at a point in time.
- `NotifyState` has `OnNotifyStart`, `OnNotifyTick` and `OnNotifyEnd`, and always closes cleanly if the animation is interrupted.
- A custom inspector shows a color coded timeline with a scrubber that syncs to a live animation preview.
- One `AnimationEventPlayer` on the player is shared by every system. Systems call `Play` and `Stop`, and the player ticks it once per frame.

### Weapon System

`WeaponManager` equips weapons by type. Each `WeaponDataSO` carries its clip, event timings and combat data, so every hit in a combo can use a different weapon.

### Weapon Tracer (Hit Detection)

Frame-rate independent melee hit detection. Empty socket transforms sit along the blade's edge, and while a swing is active the tracer casts a sphere from each socket's previous position to its current one every frame. Fast swings and low frame rates can't skip through an enemy.

- The hit window is driven by an `ANS_WeaponTracer` NotifyState, so damage, knockback and stun are tuned per attack on the timeline.
- Each target is hit once per swing.
- Hits pass a `DamageInfo` struct (damage, knockback, hit point, normal, stun, attacker) to `IDamageable.TakeHit`.
- The tracer also raises an event so the player can react to a landed hit, for example with camera shake.
- No allocations, and it does no work outside the hit window. Scene view gizmos show the sockets and sweep paths while tuning.

### Camera and Targeting

Third person camera with smooth follow. Closest enemy detection gives the combat system a lunge direction.

### Enemy AI: Behavior Tree

Every behavior is a reusable ScriptableObject recipe, and each enemy builds its own tree and its own data from it at runtime. One asset can drive any number of enemies with no shared state.

- `ExternalBehaviorTree` is the recipe asset. `BehaviorTree` is the component on each enemy that owns its blackboard and ticks the tree.
- Nodes: `Sequence`, `Selector` (re-checks higher priorities and aborts lower ones), decorators and action leaves. `OnExit` also runs on abort, so cleanup lives in one place.
- The blackboard holds typed shared variables, looked up once and read by key constants.
- `EnemyAI` is the only script that drives the `NavMeshAgent`. `EnemyAIAnimation` drives the locomotion blend tree from real movement. `EnemyCombatSystem` implements `IDamageable` and plays front or back hit reactions and will be used to drive actual combat.
- Current behaviors are Chase and Strafe. A `Selector` with strafe first and chase second makes enemies approach, then circle the target.

---

## Architecture

```
Player
  ├── AnimationEventPlayer      ← shared, ticked every frame
  ├── CombatSystem              ← plays combo data
  └── WeaponManager             ← equips weapon, owns its WeaponTracer

Enemy
  ├── EnemyAI                   ← only script that drives the NavMeshAgent
  ├── EnemyAIAnimation          ← Drives Animations
  ├── BehaviorTree              ← built from an ExternalBehaviorTree asset
  └── EnemyCombatSystem         ← IDamageable
```

---

## Quick Start

**Attack combo**

1. Create a `WeaponDataSO` and assign a clip and the animator state name.
2. Add Notify and NotifyState events on the timeline, including an `ANS_WeaponTracer` for the hit window.
3. Add the entries to `weaponCombos` in order.
4. Add `AnimationEventPlayer` to the player and call its `Tick` once per frame.

**Enemy**

1. Bake a NavMesh.
2. Add `NavMeshAgent`, `EnemyAI`, `EnemyAIAnimation`, `BehaviorTree` and `EnemyCombatSystem` to the prefab, and assign a behavior tree asset.
3. Place an `EnemySpawner` with the prefab, the player and spawn points.

**New behavior tree**

```csharp
[CreateAssetMenu(menuName = "Game/AI/BehaviorTrees/Standoff")]
public class StandoffTree : ExternalBehaviorTree
{
    public override void CreateData(BehaviorTreeData data) => data.Add<Transform>(BTKeys.Target, null);

    public override Node CreateBehaviorTree() =>
        new Selector()
            .AddChild(new Strafe(6f))
            .AddChild(new MoveToTarget(6f));
}
```

---

## Roadmap

- Hit stun, knockback and launch states for enemies
- Attack tokens so only a few enemies attack at once  
- More behavior tree nodes and a debug view of the active path
- Boss phases
- Proper Level Design and Arena for Boss
- Small Story-Driven Combat Experience
- Improved Shaders and VFX

---

## Credits & Acknowledgements

- Zeus GameDev Academy : www.youtube.com/@zeusgamedevacademy
- 용개발 : https://www.youtube.com/@user-jyj96
- The Game Dev Cave : www.youtube.com/@thegamedevcave

Thank you for sharing your knowledge and helping me build a stronger foundation in modular game development!
