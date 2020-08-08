# Savescum
Windows tool for archiving / restoring sets of files. Totally not made to be used for quick save / load in games that aren't designed for it.

Written initially for Steam install of Ark Survival. Works with bare minimal functionality.

## Usage

Command-line interface.

Savescum operation=value setting=value ...

Spaces may only be used within quoted (TODO verify) values. 

### Required settings for all operations

#### gamePath

gamePath=<full path to game's save directory>
gamePath=d:\games\SomeGame\SavedGames

#### backupPath

backupPath=<full path to directory containing save backups>
gamePath=d:\games\SomeGame\SavedGamesBackup
 
Directory will be created by save operations if it doesn't exist.

#### operation

operation=<save|load|quickload|clean|clear> 

Some operations may have additional required settings.

##### save

Copies entire gamePath directory into backupPath directory, named with optionally specified prefixBackup and appended 00n suffix e.g. PrefixSavedGames001

##### load

* Backs up gamePath directory before restoring into it. Backup is named with date / time suffix.
* Deletes gamePath directory.
* Finds largest 00n suffix backup and uses it to replace directory that was just backed up and deleted.

### Optional settings

#### backupPrefix

backupPrefix=<valid filename string>
backupPrefix=SomegameBackupSaves

Must not contain characters that are invalid for file paths for the given platform, such as ? or *.

#### count

count=number
count=12
count=1

Used differently for different operations, namely load and clean. See operations for use.

### Examples

Recently used for Ark Survival:

#### load

Savescum operation=save gamePath=G:\games\steam\steamapps\common\ARK\ShooterGame\Saved\SavedArksLocal backupPath=G:\games\steam\steamapps\common\ARK\ShooterGame\Saved\BackupArksLocal backupPrefix=BackupArk

#### save

Savescum operation=load gamePath=G:\games\steam\steamapps\common\ARK\ShooterGame\Saved\SavedArksLocal backupPath=G:\games\steam\steamapps\common\ARK\ShooterGame\Saved\BackupArksLocal backupPrefix=BackupArk protectPrefix=OverwrittenArk

## TODO

Add Command-line options to:

 - [X] Set source file / directory for saves.
 - [ ] Load: Add command-line option to specify which save by ordinal.
 - [ ] Maintenance. Option to clean all but N saved entries, with flag for recycle vs. hard delete.
 - [ ] GUI, with presets for various games and installation platforms?
 - [ ] Robot to mix drinks and rub feet while games are loading. Stretch goal.
