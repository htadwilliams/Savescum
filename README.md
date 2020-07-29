# Savescum
Windows tool for archiving / restoring sets of files. Totally not made to be used for quick save / load in games that aren't designed for it.

Written initially for Steam install of Ark Survival. Works with bare minimal functionality.

## Usage

Command-line interface.

Savescum [load | save>]

### Save

* Copies entire save directory into archive directory with appended 00n suffix.

### Load

* Backs up game's current save directory before restoring into it. Backup is named with date / time suffix.
* Deletes game's current save directory.
* Finds largest 00n suffix archive and uses it to replace the save directory that was just backed up and deleted.

## TODO

Add Command-line options to:

[ ] Set source file / directory for saves.
[ ] Load: Add command-line option to specify which save by ordinal.
[ ] Maintenance. Option to clean all but N saved entries, with flag for recycle vs. hard delete.
[ ] GUI, with presets for various games and installation platforms?
[ ] Robot to mix drinks and rub feet while games are loading. Stretch goal.
