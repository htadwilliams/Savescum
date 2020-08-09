# Savescum

Documentation is for my failing memory, not necessarily for publication.

Windows tool for archiving / restoring sets of files. Totally not made to be used for quick save / load in games that aren't designed for it.

Written initially for Steam install of Ark Survival. Works with bare minimal functionality. Integrates well with Glue!

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

operation=<save|load>

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

### Examples

Recently used for Ark Survival:

#### load

Savescum operation=save gamePath=G:\games\steam\steamapps\common\ARK\ShooterGame\Saved\SavedArksLocal backupPath=G:\games\steam\steamapps\common\ARK\ShooterGame\Saved\BackupArksLocal backupPrefix=BackupArk

#### save

Savescum operation=load gamePath=G:\games\steam\steamapps\common\ARK\ShooterGame\Saved\SavedArksLocal backupPath=G:\games\steam\steamapps\common\ARK\ShooterGame\Saved\BackupArksLocal backupPrefix=BackupArk protectPrefix=OverwrittenArk
