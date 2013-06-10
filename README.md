DotNetNuke-FileWatcher
======================

Monitors the files in your portals folder and instantly adds or removes them from the database. This is useful if you FTP or use explorer to copy files to your website.

## How it works

Utilizes .NET FileSystemWatcher in order to monitor the portals folder for any changes in order to keep the database in sync.

## What it solves

- If you add a file to your portals directory through any means except though the DotNetNuke interface that file won't be picked up by DotNetNuke until the scheduler system runs. *Assuming you have the auto sync enabled*
- You no longer have the performance hit of running the fully recursive auto sync on every file in your portal.
- Instantly have access to any file that is in your portal no matter how it got there.

## Fun Facts

- Utilizes the <code>IServiceRouteMapper</code> interface in order to have DotNetNuke enable the file system watcher code at application start.
- Is a DotNetNuke library project.

## Requirements

- DotNetNuke 7.0.6
- Full Trust Environment. *Will not work on shared hosting like GoDaddy*

