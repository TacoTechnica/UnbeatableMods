# UnbeatableMods
Mods for Unbeatable, including **Custom Beatmap Support** and **Night Train Station Freeroam**

## Latest Releases/Downloads

**NOTE: Mods will NOT work with Unbeatable's latest opt-in beta. If that's currently installed, you must opt out of the beta for the mods to work.**

[Custom Beatmaps](https://github.com/adrisj7/UnbeatableMods/releases/tag/1.3.0)

[Train Station Freeroam](https://github.com/adrisj7/UnbeatableMods/releases/tag/1.1.0)


## Custom Beatmap Mod

Lets you easily import custom beatmaps into the game by placing them in a folder.

Also adds in a special "Edit" mode that lets you preview a beatmap and edit in OSU
at the same time, viewing OSU changes live in UNBEATABLE.
(This excellent idea is brought to you by "Jane from StateDog")

### Installation

1) [Download the latest release](https://github.com/adrisj7/UnbeatableMods/releases)

2) Locate Unbeatable's game directory, containing `UNBEATABLE [white label].exe`.

	In Steam you can go to Library, right click on the game -> Properties... -> Local Files -> Browse...

3) Drag the contents of the compressed archive into the game directory mentioned above.

	If there is another mod installed, this may replace some files. That is fine.

4) Run the game (through steam or the game executable directly). Check the Main Menu for the Blue Modded text.

	Next to the game's version number should be blue text that says "CUSTOM BEATMAPS".
	If you don't see this, the mod didn't work, either due to installation problem or it's my fault and you should [write a Github Issue.](https://github.com/adrisj7/UnbeatableMods/issues)

5) Create a custom beatmap in OSU

	Check out the included example file, containing one of each note. Open it up in OSU and play it in the game.
	Make sure you're editing in OSUMania mode (advanced->Allowed Modes), with Six Keys (Difficulty -> Key Count)

6) (Optional) Preview beatmap in Unbeatable while editing in OSU.

	In the main menu select "Play" and click the "Edit View" button in the bottom right corner.
	Click the name of the beatmap you want to preview.
   
	You may do this while OSU is open and edit the beatmap in OSU. Whenever you save in OSU, your
	changes will appear in Unbeatable.

7) Once you are done with your beatmap, copy the beatmap into `UNBEATABLE [white label]_Data/StreamingAssets/USER_BEATMAPS`

	If `USER_BEATMAPS` doesn't exist, you should create that directory yourself.

8) Edit the `AudioFilename` parameter in your OSU beatmap file to point to your music file, relative to StreamingAssets

	So basically the line should say `AudioFilename: USER_BEATMAPS/your_audio_file`

9) You're done! Your beatmap should be selectable in the Play menu, at the end of the song list.

	Different difficulties are selectable the same as any other song.

## Train Station Freeroam Mod

Lets you easily explore the night train station scene

- Enter the train station from the main menu
- Removed Big Collider that stops you from walking outside the station
- Custom camera follows Beat if she walks further that can be rotated/zoomed in with the arrow keys
- Beat can now "sprint" to walk faster with shift (only when exploring the train station scene)

### Installation

Just drag+drop into main game directory, same as CustomBeatmaps.


## Building/Developing

Start by installing the mod without building anything, and get that working.

Then, clone this repo. To make sure the imports line up correctly, clone the repository as a directory within the game's root directory.

So the repo directory should look something like this: `...steamapps/common/UNBEATABLE [white label]/UnbeatableMods`

Building will create a dll file somewhere in `UnbeatableMods/CustomBeatmaps/bin/...` called `CustomBeatmaps.dll`.

Copy this dll over to `UNBEATABLE [white label]/BepInEx/plugins/CustomBeatmaps` and replace the release dll.

Play the game and your changes should appear.

PROTIP: Enable the command line so you can easily view logs, edit `UNBEATABLE [white label]/BepInEx/config/BepInEx.cfg`




For issues/questions, [consult the Github Issues page](https://github.com/adrisj7/UnbeatableMods/issues)
