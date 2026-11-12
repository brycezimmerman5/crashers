# Crashers

Sprite-based 2.5D arena for Unity **6000.0.32f1**. Built-in render pipeline, new Input System, single player with an `IInputSource` seam for later local co-op.

## Open and play

1. Install Unity **6000.0.32f1** and open this folder.
2. Let the editor import packages. That is the only required setup.
3. Open `Assets/Crashers/Scenes/Arena.unity` if it is not already open (an empty editor window opens it automatically) and press Play.

`Crashers/Export Runtime Definitions` is optional. It writes the same definitions the arena already creates at Play into `Assets/Crashers/Data/`.

## Controls

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Move | WASD (W is farther / up the screen, S is nearer) | Left stick and d-pad |
| Jump | Space | South (A / Cross) |
| Light attack | J | West (X / Square) |
| Heavy attack | K | North (Y / Triangle) |
| Ability 1, Fireball | U | Right trigger |
| Ability 2, Shockwave | I | Left trigger |
| Ability 3, Bolt | O | Right shoulder |
| Pick up weapon | E | East (B / Circle) |

Light attacks chain three hits. Heavy attacks chain two. In the air, either attack button uses the aerial hit. Bolt is locked until level 2. Killing the red grunt drops a broadsword; stand on it and press interact. The sword equips immediately because the weapon slot starts empty. Progress (level, XP, gold, unlocks) is saved to `Application.persistentDataPath/crashers-progress.json`. The knight respawns a couple of seconds after falling.

## Depth

Characters move on X and Z. Z is arena depth, not camera depth. Screen height is `jump height + depth * 0.65`, so walking up the screen walks farther from the camera. Sorting uses ground Z only: higher Z draws behind.
