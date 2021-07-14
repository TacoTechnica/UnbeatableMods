# UnbeatableMods
Mods for Unbeatable, including custom beatmap support and... right now that's it.


## Custom Beatmap Mod

Lets you easily import custom beatmaps into the game by placing them in a folder.

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

6) Copy the beatmap into `UNBEATABLE [white label]_Data/StreamingAssets/USER_BEATMAPS`

	If `USER_BEATMAPS` doesn't exist, you should create that directory yourself.

7) Convert your music file from Mp3 to wav/ogg and drag it into the same directory above

	Sadly Mp3s are not supported because Unity can't stream Mp3s (for stupid legal reasons).
	Mp3 support may come in the future, but will require extra programming tomfoolery

8) Edit the `AudioFilename` parameter in your OSU beatmap file to point to your music file, relative to StreamingAssets

	So basically the line should say `AudioFilename: USER_BEATMAPS/your_audio_file`

9) You're done! Your beatmap should be selectable in the Play menu, at the end of the song list.

	Different difficulties are selectable the same as any other song.

### Building/Developing

Start by installing the mod without building anything, and get that working.

Then, clone this repo. To make sure the imports line up correctly, clone the repository as a directory within the game's root directory.

So the repo directory should look something like this: `...steamapps/common/UNBEATABLE [white label]/UnbeatableMods`

Building will create a dll file somewhere in `UnbeatableMods/CustomBeatmaps/bin/...` called `CustomBeatmaps.dll`.

Copy this dll over to `UNBEATABLE [white label]/BepInEx/plugins/CustomBeatmaps` and replace the release dll.

Play the game and your changes should appear.

PROTIP: Enable the command line so you can easily view logs, edit `UNBEATABLE [white label]/BepInEx/config/BepInEx.cfg`



For issues/questions, [consult the Github Issues page](https://github.com/adrisj7/UnbeatableMods/issues)
