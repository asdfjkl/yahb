# YAHB - Yet Another Hardlink-based Backup-tool

YAHB is a deduplicating file copy tool, intended for backup use. Deduplication works on the file-level with NTFS [hardlinks](https://en.wikipedia.org/wiki/Hard_link).

## Example 

Consider the following scenario: Your have a folder

    C:\MyFiles
    
for which you want to create backups. Assume for simplicity that the folder contains only two files:

    C:\MyFiles\movie.avi (huge 600 MB movie file)
    C:\MyFiles\todo.txt  (your todo-list, few kilobytes)

The large `movie.avi` doesn't change. Your `todo.txt` is changed almost daily, but it's only a very small file. Let's further assume, it's March 1st, 2019, the current time is 15:12, and you are creating backup with YAHB to `F:\Backup`. Then YAHB will simply copy `C:\MyFiles` as follows:

    F:\Backup\201903011512\C__\MyFiles\movie.avi
    F:\Backup\201903011512\C__\MyFiles\todo.txt
    
Suppose the next day (March 2nd, same time) you want to create another backup to the same location. The file `todo.txt` has changed inbetween, but the file `movie.avi` has not. YAHB will locate the last previous backup folder, and identify those files that changed, and those that didn't. Running YAHB again will result in the following backup:

    F:\Backup\201903021512\C__\MyFiles\movie.avi -> hardlink to F:\Backup\201903011512\C__\MyFiles\movie.avi
    F:\Backup\201903021512\C__\MyFiles\todo.txt
    
The folder `F:\Backup\201903021512` now only takes a few kilobytes, instead of 600 MB, since `movie.avi` is only stored once on the drive `F:`, but two NTFS hardlinks are pointing to it.

Moreover:

* If at some point, you decide to delete the folder `F:\Backup\201903011512` (but keep `F:\Backup\201903021512`), NTFS will detect that there is a hardlink pointing to `movie.avi`. It will delete the folder, but keep `movie.avi` on the disk. Same for the other way round.
* You always have a 1:1 copy of your current files at hand. In case of a desaster, there is no proprietary backup format to extract from, re-order your file structure etc. In the above example, just copy the latest version of `MyFiles` back, and all your data are there - maximum recoverability.
* If a file is currently locked (i.e. opened for read/write), YAHB supports to still create a copy of that file using [Windows Volume Shadow Copy Service](https://en.wikipedia.org/wiki/Shadow_Copy). This is useful, if you want to create a backup in the background while working with the computer, i.e. creating backups of documents while you have them still open in Word/[LibreOffice](https://www.libreoffice.org), or creating a backup of your Thunderbird or Firefox [Profile folder](https://www.howtogeek.com/255587/how-to-find-your-firefox-profile-folder-on-windows-mac-and-linux/), while still writing mails or browsing the web.

## Download

The latest release is available HERE.

## Installation

Just unzip to a folder, open a command-prompt and run `yahb`.

## Requirements

YAHB is currently 64 bit only, i.e. you need Windows 7 64bit, Windows 8.1 64bit or Windows 10 64bit.

The target drive MUST be NTFS-formatted. Otherwise hardlinks cannot be created.

YAHB requires [Microsoft NET Framework 4.5](https://dotnet.microsoft.com/download/dotnet-framework) or higher. The following versions of Windows ship with suitable versions of NET Framwework by default, i.e. you don't need to install anything if you run:
- Windows 8.1
- Windows 10 (any edition/version)

If you are stll running Windows 7, download and install the latest [Microsoft NET Framework](https://dotnet.microsoft.com/download/dotnet-framework)

Only if you want to make use of [Windows Volume Shadow Copy Service](https://en.wikipedia.org/wiki/Shadow_Copy) to copy files currently in use, you need to additionally install [Microsoft Visual Studio C++ 2019 Redistributable]( https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads). You need the version for 64bit systems, i.e. `vc_redist.x64.exe`. Note that it's very likely that this is already installed on your system by other programs.


## Usage

Note: To use the option `/vss` you MUST run yahb with elevated rights, i.e. from an elevated command prompt.



