About
=====
    
When developing windows service you have to install, start, stop and uninstall it lots of times via **InstallUtil.exe**. This application helps you get rid of that routine - you are only pushing buttons.

You should only choose **.NET** version, find **.exe** for your service and set the **name** of the service itself. If you forgot it, you can look it up in your project.

Settings
========

You can set some splitting settings in the `.config` file.

#### netPath_default

If application fails to fint .NET path, you can specify it there.

#### consoleEncoding

Encoding for the command line output.

3rd party
=========

The application is written in C#, WPF with Visual Studio 2013 (if that matters) and .NET 4.5.1. Those nasty bastards did all the work, I just wrote a few lines of code.

Also I used [Ookii.Dialogs](http://www.ookii.org/Software/Dialogs/) for open file/folder dialogs, because for some reasons there is no such thing as OpenDirectoryDialog in WPF.

And I snatched some (all) icons from [Iconfinder](https://www.iconfinder.com/).