# Camera2.5

A fork of [Camera2](https://github.com/kinsi55/CS_BeatSaber_Camera2) with the focus on delivering more features, stability, performance improvements and much needed QoL than the original including support for Spout2.

#### You can always find the latest download in [the releases](https://github.com/UnskilledFreak/Camera2/releases).

## Index
- [General](#general) - A brief introduction.
- [Versions](#versions) - See which game versions are supports.
- [Camera Types](#camera-types) - A detailed list of all possible camera types.
- [Spout2 Feature](#spout2) - Yes, it's here!
- [General Settings](#general-settings) - General mod settings 
- [Scenes](#scenes) - scenes management
- [Backwards compatibility](#backwards-compatibility) - backwards compatibility list
- [Scene Tester](#scene-tester) - test the config of all possible, build in scenes
- [Movement Script Editor](#movement-script-editor) - manage your movement scripts in game!
- [Changelog](#changelog) - What was done?
- [Credits](#credits) - It's a must! 

## General
This fork heavily focuses on performance improvements, new much wanted features, QoL and stability. It exposes some features it already had and added new ones. The main additions are:
- exposed attached camera type
- Follower camera type
- Spout2 support with transparency even working under Linux (tested under Kubuntu)
- Movement Script Editor
- Scene Tester
- (for noodle maps) automatically switch between scenes with noodle track (player movement) and without one
- better in-game settings
- direct Movement Script setup
- much, much QoL and fixes
- other mods can check which Camera2 mod is used. This one exposes a `ImModded`-Value in the Plugin. If it exists, the user uses this variant. if it's absent then the user users the original version. 
- and many more!

**But why isn't it merged into the original?**  
This modded version started as a proof of concept to bring [Follower cameras](#follower-cameras) from CameraPlus back to Camera2. But what started as a single addition somehow ended up in a huge code cleaning and performance improving change. When I completed my first additions, the code base already drifted heavily apart from the original. It's almost impossible to merge it back.

## Versions
Camera2.5 release versions is a mix of the upstream of the original Camera2 mod by kinsi55 and my own additions. It is targeting **multiple game versions** based on what the community uses and the official, legacy branches. Currently, Camera2.5 is build for the following game versions:
- 1.29.1
- 1.34.2
- 1.37.1
- 1.40.8
- 1.44.1

## Camera types
Every camera can be configured individually.
- [General camera setup](#general-camera-setup)
- [First person](#first-person-cameras)
- [Third person](#third-person-cameras)
- [Attached](#attached-cameras)
- [Follower](#follower-cameras)
- [Drunk Follower](#drunk-follower-cameras)
- [FPFC](#fpfc-camera)

### General camera setup
All cameras are managed in game. Hit the `Camera 2.5` button in the mods panel of the main menu and you get a list of every camera you have on the left side.

- `Add` creates a new camera. You can start from scratch or pick one of the templates (side view, back view, static third person, face cam for avatars or a follower cam)
- `Duplicate` copies the selected camera with all of its settings
- `Delete` removes it, the last remaining camera can not be deleted
- `+` and `-` change the layer of the selected camera. Cameras on a higher layer are drawn on top of the others on your desktop. If two cameras share the same layer the number turns yellow.

Each entry in the list also tells you its type, in how many scenes it is used (red if it is in none, which means you will never see it) and if it is a Spout2 output.

The settings of the selected camera are split into tabs. Not every tab shows up for every camera type, the ones which make no sense for a type are hidden.

|Tab|What it does|
|--|--|
|General|name, type, field of view, render scale, anti aliasing and a lock for the position and size of the camera on your desktop|
|Visibility|show or hide walls, notes, avatar, debris, cut particles, UI, floor, sabers and everything else. Walls can also be transparent, custom notes can be forced even if they are set to HMD only and the avatar can be forced to be visible in first person. The render distance is set here too.|
|Position / Rotation|position and rotation of the camera or its offset, depends on the camera type|
|Target|the object an attached or follower camera sticks to or looks at|
|Fake Zoom|follower only, see [Follower cameras](#follower-cameras)|
|Smoothing|first person and follower cameras only, see [First person cameras](#first-person-cameras)|
|ModMaps|follow the player movement of noodle maps, make walls visible on modded maps and hide the HUD on them|
|360/90|follow the rotation of 360 and 90 degree maps, with its own smoothing|
|Scenes|pick the [scenes](#scenes) the camera is active in|
|Effects|transparency threshold and depth texture, needed if you want a transparent background|
|Misc|orthographic projection, attach to the spectator platform in multiplayer, near clip plane and a FPS limit (do not touch that one unless you really need it)|
|Display|position and size of the camera on your desktop from 0 to 1, where 0 / 0 is the bottom left corner|
|Movement Scripts|if scripts start from the origin or from the position of the camera and if they also run in the menu|
|Spout2|see [Spout2](#spout2)|

On the right side you can assign [movement scripts](#movement-script-editor) to the camera, the panel at the bottom shows a preview of what it sees.

You can also move and resize every camera directly on your desktop with the mouse. Drag it around or grab one of its corners. If you keep moving one by accident, lock it in the General tab. This does not work while in FPFC because the mouse is busy there.

Every camera is stored as its own file in `UserData/Camera2/Cameras`. If you edit them by hand while the game is running, press `Ctrl + Shift + F1` to reload everything.

### First person cameras
Shows what you see through your headset, just without all the shaking. It follows your head, and when you watch a replay it follows the head of the replay instead (can be turned off).
- Position and rotation smoothing are set separately in the Smoothing tab, lower values mean more smoothing
- The position and rotation settings work as an offset here. By default the offset pivots around your head, turn off "Pivoting Firstperson Offset" if you want it to be absolute like in CameraPlus. Both have their pros and cons.
- With an avatar you can force it to be visible in first person (Visibility tab), which is what the face cam template does
- You can limit how far it is allowed to move and rotate, but only in the config file for now (`Limits`)
- There is nothing to grab in game since it sticks to your head anyway
- Position smoothing ignores the rotation applied by mod maps so that [fast map rotations are actually watchable](https://www.youtube.com/watch?v=Nt7ZL50dYpI)
- "Force Upright" respects the map rotation, so if a map rotates you upside down your view will be upright in respect to the world rotation

### Third person cameras
A camera at a fixed spot in the world, the config calls it `Positionable`. You can type in its position and rotation, but it is way easier to grab it in game: point at the white cylinder, hold the trigger (or the mouse button in FPFC) and put it where you want it. The preview gets bigger while you hold it and the rotation snaps in when you get close to a 45 degree step. This also works in the pause menu.
- The grab cylinder and preview can either be always visible / hidden or hidden just while playing
- In-game preview size customizable
- A hidden camera still shows up as long as it is selected in the settings, otherwise you could not grab it
- You need to be within 6 meters to grab it, the pointer of the game does not reach any further
- Can follow the rotation of 360 / 90 maps and the player movement of noodle maps, both optional
- Can run [movement scripts](#movement-script-editor). If you assign more than one script it picks a random one each time.
- Orthographic projection if you need a flat view, the back view template uses it

### Attached cameras
An attached camera sticks to any object in the game and moves and rotates with it. This was always part of Camera2 but you had to set it up in the config file, now it is a normal type in the settings.

Put the name or the path of the object into "Target" or pick one of the templates:

|Template|Sticks to|
|--|--|
|Avatar Head|the head of your avatar|
|NalulunaAvatar Head|the head of your NalulunaAvatars avatar|
|NalulunaCat Head|the head of the cat. Yes, a cat cam.|
|Look at Pointer|the laser pointer|
|Left Panel / Right Panel|the HUD panels left and right of the track|
|Energy Bar|the energy bar|
|Score|the score display|

- Position and rotation are an offset to the target
- Position and rotation smoothing work like on first person cameras
- The target does not have to exist right away. The camera keeps looking for it and attaches itself as soon as it shows up, which is quite normal for things added by other mods. How often it looks can be changed in the [general settings](#general-settings).
- You can grab and move it in game, but only while it is selected in the settings. If the target runs away while you hold the camera, the offset is taken from where the target is when you let go.

### Follower cameras
The reason this fork exists. A follower camera sits at a fixed position like a third person one but always looks at its target. You may know it from CameraPlus.

It uses the same target system as the [attached cameras](#attached-cameras) including the templates. Without a target, or as long as the target is not found, it behaves like a normal third person camera.
- Rotation smoothing sets how fast it turns to its target, lower values mean more smoothing
- The rotation settings are an offset to the look direction. With "Use Rotation as Look-At Position Offset" they move the point the camera looks at instead, for example to look at the face and not at the body. This can also be relative to the target (up, down, left, right, forward, backward).
- "Use organic rotation" inverts the rotation offset which feels more natural, just try which one you like more
- Fake Zoom changes the field of view depending on the distance between camera and target. You set the nearest FOV, the farthest FOV and the distance at which the farthest one is reached. Hard to explain, try yourself.
- Can be grabbed and moved like a third person camera and keeps looking at its target while you do that
- Follows 360 / 90 maps and the player movement of noodle maps
- Works with [movement scripts](#movement-script-editor). A step can be set to only move the camera, so it keeps looking at its target while flying around.

### Drunk Follower cameras
This one was meant to be a joke but somehow ended up staying there. Basically the same as a [Follower](#follower-cameras) one but instead of just staring at its target, it... sways around and has a hard time to focus on what it should look it. Because its drunk.

### FPFC camera
Basically the same as a [First person](#first-person-cameras) camera but it follows the FPFC controller.

## Spout2
With [Spout2](https://github.com/leadedge/Spout2) a camera sends its picture directly to other programs like OBS, no window capture needed. So you can have as many cameras in your stream as you want, each with its own resolution which has nothing to do with the size of the game window.

Spout2 is supported with every cam including its channel feature. Transparency is also fully supported.

Every camera has a `Spout2` tab in its settings:

|Option|What it does|
|--|--|
|Enabled|turns the Spout2 output of this camera on|
|Width / Height|resolution of the output|
|Channel Name|name of the sender, this is what you select in the receiving program. Default is `Camera25`, so give every camera its own name.|
|Ignore Render Scale|if on, the render scale of the camera is not multiplied onto the resolution above|

Some things you should know:
- a camera with Spout2 turned on is not shown on your desktop anymore, it only renders into its output. The camera list marks it as "Spout2 output".
- for a transparent background set the "Transparency Threshold" in the Effects tab of the camera and make sure the receiving side does not throw the alpha channel away
- OBS can not receive Spout2 on its own, you need a plugin like [obs-spout2-plugin](https://github.com/Off-World-Live/obs-spout2-plugin). Then add a Spout2 source and select the channel name.
- the settings still call it experimental. It works fine for me and my testers but there might be some rough edges left.

## General settings
The mod itself only has three general options. There is no UI for them, they are set in two config files:

|Option|File|What it does|
|--|--|--|
|AutoSwitchFromCustom|`UserData/Camera2/Scenes.json`|allows the scene to switch back to a default one when a custom one is active|
|NoodleParentLookupDelay|`UserData/Camera2.json`|the delay (in seconds) on how fast the mod should look for a noodle-track (player movement)|
|TargetParentLookupDelay|`UserData/Camera2.json`|the delay (in seconds) on how fast the mod should look for a parent (attached and follower cams)|

## Scenes
Where CameraPlus has profiles, Camera2 has scenes. A scene is nothing more than a list of cameras which are active in a certain situation like being in the menu, playing a map or watching a replay. The mod switches between them on its own.

To put a camera into a scene, open its `Scenes` tab and tick every scene it should be active in.

|Scene|When it is used|
|--|--|
|Menu|in the menu|
|MultiplayerMenu|in the multiplayer lobby|
|Playing|while playing a map|
|Playing360|while playing a 360 or 90 degree map|
|PlayingModmap|while playing a modded map which moves the player (noodle track)|
|PlayingModmapNoMotion|while playing a modded map which does not move the player|
|PlayingMulti|while playing in multiplayer|
|SpectatingMulti|while spectating in multiplayer|
|Replay|while watching a replay|
|FPFC|while FPFC is active|

How the scene gets picked:
- the more specific scene wins. FPFC beats everything, then Replay, SpectatingMulti, PlayingMulti, the modmap or 360 scene and at last Playing
- a scene without any camera is skipped and the next one in line is used. So if you do not care about replays, leave Replay empty and you get what you have set up for Playing.
- if nothing fits, Menu is used. If Menu is empty as well, all cameras are shown
- on noodle maps the mod starts with PlayingModmapNoMotion and switches to PlayingModmap as soon as the map starts to move the player. Not every noodle map does that and some start with it somewhere in the middle of the song. Put at least one camera into PlayingModmap, this switch does not skip an empty scene but shows all cameras instead.

### Custom scenes
Besides the built in scenes you can create your own and switch to them whenever you want, for example one with only a face cam for the break between two songs. There is no UI to create them, they have to be added by hand in `UserData/Camera2/Scenes.json`:

```json
"CustomScenes": {
  "Facecam": ["FaceCam"],
  "Break": ["3PMainMenu", "Cat Cam"]
},
"CustomSceneBindings": {
  "F2": "Facecam",
  "F3": "Break"
}
```

`CustomScenes` holds the name of each scene and the names of the cameras it shows. There are three ways to switch to them:
- the `Camera 2.5` tab in the mods panel next to the song list, the first entry brings you back to the normal scene
- hotkeys, set in `CustomSceneBindings` with the Unity name of the key (like `F2`, `Alpha1` or `Keypad1`). `F1` always brings you back to the normal scene.
- other mods can switch scenes through the SDK

A custom scene stays active until you switch back, even when you start or leave a map. If the mod should go back to the normal scenes on its own, turn on `AutoSwitchFromCustom` (see [general settings](#general-settings)).

## Backwards compatibility
- CameraPlus cams
- CameraPlus Movement Scripts
- Camera2 settings - though there are some settings which won't get loaded correctly, but the basics like type, position, rotation, etc. works  

## Scene Tester
Setting up scenes like SpectatingMulti or Replay is annoying when you first have to get into that situation just to see if the right cameras show up. So the `Camera 2.5 Scene Tester` button in the main menu gives you one button for every built in scene. Hit one and the mod switches to that scene right away, no matter what the game is doing.

It stays there until the game changes its state the next time, for example when you start a map. An empty scene shows all cameras here.

## Movement Script Editor
A movement script moves a camera from step to step, like a small cutscene. Until now you had to write them by hand as JSON and reload to see what you did. Not anymore, just open `Camera 2.5 Movement Scripts` in the main menu.

The editor brings its own camera. It only exists while the editor is open, shows its picture at the bottom and can be grabbed like any other third person camera.

**Left side, the scripts**
- `Add`, `Duplicate` and `Delete` scripts
- `Name` renames the script, cameras which use it are updated as well
- `Loop` starts over when the last step is done, without it the camera stays at the last step
- `Sync to Song` runs along the song time, so it follows pausing and skipping in replays
- `Test Target` turns the editor camera into a follower which looks at that target. This way you can check how a script looks on a follower camera without jumping back and forth. It works like the target of a real camera including the templates and is not saved.

**Right side, the steps**
- `Add` puts a new step behind the selected one, it starts where the editor camera currently is
- `Delete`, `Move up` and `Move down` do what they say
- `Take pose from editor camera` writes the position and rotation of the editor camera into the selected step. The easiest way to build a script: grab the camera, move it, take pose, next step.
- `Play`, `Pause` and `Stop` to watch the result. Pause freezes the camera right where it is, pretty handy together with take pose.

**Middle, the selected step**

|Option|What it does|
|--|--|
|Pos X / Y / Z|where the camera should be|
|Rot X / Y / Z|where it should look|
|Field of View|0 keeps the field of view of the step before|
|Move Duration|seconds the camera takes to get here from the step before|
|Hold Time|seconds the camera stays here before it moves on|
|Eased movement|accelerates and slows down smoothly instead of moving at a constant speed|
|Follower: Pos only|follower cameras keep looking at their target and only take the position of this step|
|Follower: smooth Rot|follower cameras turn smoothly when the script switches between looking at the target and the rotation of a step, without it they snap|

While you edit, the flight path is drawn into the world. It goes from green (start) to red (end), every step has an arrow which shows where the camera looks and the selected step is the big yellow one. If the script loops, a grey line connects the last step with the first one.

A script needs at least two steps to be playable. Everything is saved on its own to `UserData/Camera2/MovementScripts`. Files you copy into that folder by hand show up after a restart or `Ctrl + Shift + F1`.

To use a script, select a third person or follower camera in the normal settings and enable the script in the list on the right. You can enable more than one, the camera then picks a random one each time. The `Movement Scripts` tab of the camera has two more options: if the script starts from the origin or from the position of the camera, and if it also runs in the menu.

## Changelog
The first number is the version of the original Camera2 it is based on, the second one is mine.
- 0.6.119 + 0.8.0
  - updated to Camera2 0.6.119
  - added MovementScript-Editor
  - added random seed on drunken cam only calculated on activating steps
  - added the possibility to grab parented and attached cams
  - added smooth rotation option to movement scripts to switch between manual positioned steps and follower
  - huge improved performance on parented cams (attached, follower, noodle) - new "find"-delay options in `UserData/Camera2.json`
  - changed floating point precision on slider settings to be 3 (.001 steps)
  - changed some of the null checks to keep my IDE happy
  - fixed some shader issues
  - fixed Spout2 texture is flipped upside down
  - fixed Spout2 cams start dead after game restart
  - fixed tabselector not selecting the last used tab on older versions
  - fixed tabselector break the config screen when it selects a tab index which was out of range
  - fixed FPFC controller is unable to grab cams
  - fixed transparent grab object on cameras
  - fixed alpha channel not getting send to Spout2 correctly - transparency now works
  - fixed memory leak - destroyed/deleted cams did not remove their render texture and the GC didnt care about it
  - fixed a rare crash condition when reloading cams while it was or getting deleted
  - fixed X icon when using linux
  - fixed follower cams not working correctly with movement scripts
- 0.6.118 + 0.5.1 (alpha, 1.40.8 only)
  - updated to Camera2 0.6.118
  - added Spout2 support
  - added "ImModded" flag on Plugin to check if the player runs the original Camera2 or my modded version
  - added cam duplication option
  - added more cam info in list panel
  - added better SiraUtil dependency checker, version depending
  - increased precision on view rect edit within VR
  - fixed FPS limit on separate cams
  - fixed FPFC (sort of)
  - fixed shaders distorting each other
  - reverted some FPFC related changes (while keeping the new update) to ensure compatibility with other mods
  - even more QoL
- 0.6.117 + 0.4.5 (alpha)
  - adds tons of QoL
  - supports multiple versions (just grab the one for your game version)
  - added follower cams
  - exposed attached cam
  - exposed many other settings and features
  - added noodle map without movement scene
  - moved menu and global config data handling to Zenject (SiraUtil is now a **dependency**)
  - cleaned ~~spaghetti~~ code
  - I honestly cant remember everything anymore, this took to long to do and yet it's still not finished 

## Credits
- [Kinsi](https://github.com/Kinsi55)
- [Eris](https://github.com/ErisApps)
- [qqrz997](https://github.com/qqrz997)
- [Auros](https://github.com/Auros)
- [Zephyr](https://github.com/Zeph-yr)
- [nicoco007](https://github.com/nicoco007)
- [Loloppe](https://github.com/Loloppe)
- [Fefeland](https://github.com/FEFELAND)