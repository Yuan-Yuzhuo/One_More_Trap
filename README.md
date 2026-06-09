# One More Trap

**One More Trap** is a 2D puzzle platformer built in Unity. The player moves through trap-heavy side-view levels, activates mechanisms, avoids hazards, and reaches the exit while learning how each level changes the environment.

Repository: <https://github.com/Yuan-Yuzhuo/One_More_Trap>

## Overview

| Item | Description |
| --- | --- |
| Genre | 2D Puzzle Platformer |
| Engine | Unity 2022.3 LTS |
| Project Version | Unity 2022.3.62f3 |
| Core Loop | Move, jump, dash, activate mechanisms, avoid traps, reach the exit |
| Main Goal | Clear all 9 challenge levels |
| Main Risk | Timing-sensitive collision and platform behavior can create edge-case movement bugs |

## One-Sentence Idea

A player solves trap-based platform puzzles by activating mechanisms that change the environment.

## Gameplay

The game is built around compact platforming challenges where the level itself is the puzzle. Players move through side-view stages, read the layout, trigger buttons or coins, and react to hazards such as spikes, falling traps, moving platforms, retractable ground, chasing spikes, and deadly red lines.

Failure is part of the design. The player is expected to observe what changed, retry quickly, and use movement tools more precisely on each attempt.

## Core Gameplay Loop

1. Move through the level.
2. Jump or dash across platforms.
3. Trigger buttons, coins, moving ground, or doors.
4. Avoid spikes, falling traps, collapsing floors, and moving hazards.
5. Reach the exit door and transition to the next challenge.

## Controls

| Action | Default Key |
| --- | --- |
| Move Left | `A` |
| Move Right | `D` |
| Jump | `W` |
| Dash | `Space` |

The main menu includes a personalized configuration screen where players can rebind movement, jump, and dash keys.

## Current Features

- 9 playable challenge levels.
- Visitor Mode for playing without logging in.
- Formal Mode with local login/register flow.
- Local ranking records for formal challenge clears.
- Visitor completion dialog that does not save rankings.
- Player movement, fixed-height jump, dash, wall-slide behavior, and death reload handling.
- Scene hints with fade-in/fade-out presentation.
- Current-level HUD badge and run statistics.
- Main menu BGM, mute toggle, click feedback, and demo-video screen.
- Audio feedback for jump, run, dash, death, and scene transition.
- Black fade transitions for scene changes and death reloads.
- Frozen end-of-level moment before door transitions.

## Vertical Slice

The smallest playable version of this project is a single level where the player:

- moves and jumps,
- activates a floor button,
- triggers a retractable platform,
- opens a door,
- avoids spikes,
- and reaches the exit.

## Level Structure

| Level | Scene | Focus |
| --- | --- | --- |
| 1 | `1.Beginning` | Falling traps and unstable ground |
| 2 | `2.MovingSpike` / `Hurdle2` | Spike avoidance |
| 3 | `3.3MovingSpkies` | Moving spike timing |
| 4 | `4.2MovingSpikes` | Platform timing and spike pressure |
| 5 | `5.ChasingSpikes` | Dash usage against chasing hazards |
| 6 | `6.Coin` | Coin-triggered level interaction |
| 7 | `7.TrapCoin` | Greed-based trap design |
| 8 | `8.maze` | Space-station themed moving and retractable ground |
| 9 | `9.FreeFalling` | Inertia, free fall, and lethal red-line hazards |

## Core Systems

| System | Main Script(s) | Purpose |
| --- | --- | --- |
| Player controller | `PlayerController.cs` | Movement, jump, dash, audio, collision, death handling |
| Input configuration | `PlayerInputConfig.cs` | Custom key bindings saved with `PlayerPrefs` |
| Camera follow | `CameraFollow.cs` | Keeps the camera tracking the player |
| Scene transition | `SceneTransitionController.cs` | Fade transitions, transition audio, frozen door transition |
| Level progression | `NextLevelDoor.cs` | Moves the player to the next challenge scene |
| Stats and ranking flow | `GameStatsTracker.cs` | HUD, deaths, time, double jumps, visitor clear dialog, formal save dialog |
| Local account database | `LocalGameDatabase.cs` | Local login, registration, and challenge records |
| Scene hints | `SceneHintController.cs` | Level-entry hint text with fade timing |
| Main menu | `MainMenuController.cs` | Home menu, visitor/formal mode, ranking, settings, demo, BGM mute |
| Hazards and mechanisms | Scene-specific scripts | Spikes, falling traps, retractable ground, moving objects, coins, buttons |

## Project Structure

```text
Assets/
  Resources/                 Runtime-loaded audio and demo video
  Scenes/                    Main menu and challenge scenes
  Script/                    Gameplay, UI, transition, account, and stats scripts
    scene_1/                 Level 1 trap scripts
    scene_2/                 Level 2 trigger scripts
    scene_5/                 Level 5 hazard/door scripts
    scene_6/                 Level 6 coin/ground scripts
    scene_7/                 Level 7 trap coin scripts
    scene_8/                 Level 8 button/retractable ground scripts
    scene_9/                 Level 9 space and death-line scripts
  Simple 2D Platformer BE2/  Imported character/platform asset pack
  2DRPK/                     Imported coin asset pack
ProjectSettings/
Packages/
```

## How to Run

1. Install **Unity 2022.3 LTS**.
2. Clone this repository.
3. Open the project folder in Unity Hub.
4. Open `Assets/Scenes/MainMenu.unity`.
5. Press Play.

For a full challenge run, start from the main menu and choose either:

- **Visitor Mode**: play immediately without ranking submission.
- **Formal Mode**: log in or register, then submit clear records to the local ranking table.

## Development Notes

- Runtime audio is loaded from `Assets/Resources`.
- The menu demo video is loaded from `Assets/Resources/Video Project.mp4` and is played muted.
- Formal ranking data is local to the player machine.
- The project uses IMGUI-based menu and HUD screens rather than Unity UI Toolkit.
- Some scene logic is level-specific because each challenge introduces different traps and mechanisms.

## Biggest Risk

The main technical risk is collision behavior around moving, retractable, and shrinking platforms. Timing-sensitive platformers can produce edge cases such as wall sticking, side clipping, or player trapping between colliders. This project addresses several of those issues in player collision logic, but platform tuning and collider review remain important when adding new levels.

## Asset Credits

This section lists third-party assets and generated/custom assets used by the project. If this game is distributed publicly, every third-party asset should be checked against its original license terms.

### Sprites and Visual Assets

| Asset | Usage | Source |
| --- | --- | --- |
| Simple 2D Platformer Assets Pack | Character sprite and platformer art base | <https://assetstore.unity.com/packages/2d/characters/simple-2d-platformer-assets-pack-188518> |
| 2D Animated Coin - 2D RPK | Coin sprite | <https://assetstore.unity.com/packages/2d/environments/2d-animated-coin-2d-rpk-22009> |
| Platform tiles and environment sprites | Level platforms and environment decoration | Unity Asset Store: <https://assetstore.unity.com/> |
| Menu background, space background, doors, space station, spacecraft images, and other level-specific images | Game scenes and menu presentation | Original/generated project assets |
| Runtime-generated UI textures | Main menu panel, wooden buttons, icon buttons, particles, leaves, dialog panels | Generated in project code |

### Audio

| Asset File | Usage | Credit / Source |
| --- | --- | --- |
| `click.mp3` | Main menu click feedback | Mouse Click, Pixabay: <https://pixabay.com/sound-effects/film-special-effects-computer-mouse-click-352734/> |
| `run.mp3` | Player run loop | Run, Pixabay: <https://pixabay.com/sound-effects/nature-run-142540/> |
| `jump.mp3` | Player jump sound | Funny Spring Jump, Pixabay: <https://pixabay.com/sound-effects/film-special-effects-funny-spring-jump-140378/> |
| `whoosh.mp3` | Player dash sound | Whoosh Cinematic, Pixabay: <https://pixabay.com/sound-effects/film-special-effects-whoosh-cinematic-376875/> |
| `death.mp3` | Player death sound | Death, Pixabay: <https://pixabay.com/sound-effects/film-special-effects-death-408455/> |
| `esclate.mp3` | Scene transition sound | Escalator, Pixabay: <https://pixabay.com/sound-effects/city-escalator-71671/> |
| `cadidate_1.mp3` | Main menu background music | Pixabay BGM candidate; original source link should be added before public release |

### Video

| Asset File | Usage | Source |
| --- | --- | --- |
| `Video Project.mp4` | Muted looping demo video in the main menu | Project demo video |

### Fonts

| Font | Usage |
| --- | --- |
| Georgia | Decorative fantasy-style title fallback |
| Garamond | Decorative fantasy-style title fallback |
| Times New Roman | Decorative fantasy-style title fallback |
| Verdana / Arial | Readable UI text fallback |

### Engine and Tools

| Tool | Usage |
| --- | --- |
| Unity 2022.3.62f3 LTS | Game engine and editor |
| C# | Gameplay, UI, audio, transition, and data logic |
| Unity IMGUI | Runtime menu, HUD, ranking, and dialog UI |

## Status

The repository contains the Unity project setup, gameplay scripts, main menu systems, 9 challenge scenes, local account/ranking support, audio integration, demo-video integration, and current UI polish work.
