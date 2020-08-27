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

The latest release is available [HERE](https://github.com/asdfjkl/yahb/releases).

## Installation

Just unzip to a folder, open a command-prompt and run `yahb`.

## Requirements

YAHB is currently 64 bit only, i.e. you need Windows 7 64bit, Windows 8.1 64bit or Windows 10 64bit.

* When copying to a *locally attached drive*, the target drive MUST be NTFS-formatted. Otherwise hardlinks cannot be created.
* When copying to a *network share*, things are more complicated. Basically the underlying file system must support hardlinks, and must expose hardlink creation in such a way, that Windows API commands can be used to create hardlinks. This is supported with i.e. SAMBA when [Unix Extensions](https://www.samba.org/samba/docs/current/man-html/smb.conf.5.html#UNIXEXTENSIONS) are enabled. Fortunately, most typical NAS solutions like Synology or QNAP suport this and work out-of-the-box.

YAHB requires [Microsoft NET Framework 4.5](https://dotnet.microsoft.com/download/dotnet-framework) or higher. The following versions of Windows ship with suitable versions of NET Framwework by default, i.e. you don't need to install anything if you run:
- Windows 8.1
- Windows 10 (any edition/version)

If you are stll running Windows 7, download and install the latest [Microsoft NET Framework](https://dotnet.microsoft.com/download/dotnet-framework) [here](https://dotnet.microsoft.com/download/dotnet-framework).

Only if you want to make use of [Windows Volume Shadow Copy Service](https://en.wikipedia.org/wiki/Shadow_Copy) to copy files currently in use, you need to additionally install [Microsoft Visual Studio C++ 2019 Redistributable]( https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads). You need the version for 64bit systems, i.e. `vc_redist.x64.exe`. Note that it's very likely that this is already installed on your system by other programs.

## Restrictions

Windows has a `MAX_PATJ` restriction, i.e. [can't handle path names longer than 260 characters](https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file) - a relict from old MS-DOS times. 
Since keeps the original folder structure but in addition adds a timestamp and drive letter -- like e.g. `F:\Backup\201903021512\C__\MyFiles` it is possible to run into problems. 

Unfortunately there is no easy and hassle-free workaround for this `MAX_PATH` restriction I am aware of. It is recommended to keep the maximal path length in mind and possibly shortening folder names prior to creating a backup.

## Usage

Note: To use the option `/vss` you MUST run yahb with elevated rights, i.e. from an elevated command prompt.

```
YAHB (Yet Another Hardlink-based Backup-Tool)
Version 1.0.0.0
Copyright (c) 2019 Dominik Klein

     Syntax:: yahb.exe <source-dir> <target-dir> [<options>]

 source-dir:: source directory (i.e. C:\MyFiles)
 target-dir:: target directory (i.e. D:\Backups)

TYPICAL EXAMPLE:

 yahb c:\MyFiles d:\Backup /s /xf:*.tmp

will copy all files and the directory structure from c:\MyFiles
to d:\Backup\YYYYMMDDHHMM, including all subdirectories. Yahb will
also look for previous backups of c:\MyFiles in d:\Backup, and if
a file has not changed, it will create a hardlink to that location.
Moreover, all files with ending .tmp will be skipped.

OPTIONS

  /copyall                 :: copy ALL files. Otherwise the following directory
                              patterns and file types are excluded:

                              DIRECTORIES:
                              - 'System Volume Information'
                              - 'AppData\Local\Temp'
                              - 'AppData\Local\Microsoft\Windows\INetCache'
                              - 'C:\Windows'
                              - '$Recycle.Bin'

                              FILES AND PLACEHOLDERS:
                              - hiberfil.sys
                              - pagefile.sys
                              - swapfile.sys
                              - *.~
                              - *.temp

  /files:PAT1;PAT2;...     :: copy only files that match the supplied
                              file patterns (like *.exe)

  /help                    :: display this help screen

  /id:FILENAME             :: supply a list of Input Directories to copy which
                              are stored line by line in a textfile FILENAME.
                              If this options is used, <source-dir> can be
                              omitted. If both <source-dir> and /id:FILENAME
                              are present, all directories will be copied.
                              NOTE that if /s is provided, it will be 
                              applied to the list of input directories, and
                              will also be applied to <source-dir>.

  /list                    :: do not copy anything, just list all files

  /log:FILENAME            :: write all output (log) to a textfile FILNAME.
                              If FILENAME exists, it will be overwritten

  /+log:FILENAME           :: same as /log:FILENAME, but always append, i.e.
                              do not not overwrite FILENAME if it exists.

  /pause                   :: after finishing, wait for the user to press
                              ENTER before closing the program. This
                              prevents a command - prompt from vanishing
                              after finishing if run e.g.by Windows' RUNAS
                              command

  /s                       :: also copy all SUBDIRECTORIES of <source-dir>

  /tee                     :: even if /log:FILENAME or /+log:FILENAME is
                              chosen, still write everything additionally
                              to console output.

  /verbose                 :: by default, only the progress and errors 
                              are output to the console/log. In verbose
                              mode, all created files and directories
                              are listed - note that for large copy
                              operations, this frequent output to console
                              will slow down the overal operation

  /vss                     :: If a file is currently in use, and cannot be
                              accessed, try to still copy that file by using
                              Windows' Volume Shadow Copy Service.
                              YOU NEED TO RUN YAHB WITH ELEVATED (ADMIN)
                              RIGHTS FOR THIS TO WORK.

  /xd:DIR1;DIR2;...        :: eXclude directories dir1, dir2, and so forth.
                              I.e. if DIR is provided here, any (full)
                              directory path that contains DIR is skipped

  /xf:PAT1;PAT2;...        :: eXclude files with filename PAT1, PAT2 and so
                              forth. PAT can also be a file pattern like *.tmp

  /?                       :: display this help screen
