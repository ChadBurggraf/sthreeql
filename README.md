# SThreeQL
#### A basic Microsoft SQL Server backup/restore utility for use with Amazon S3

This is a pretty naive backup and restore utility for Microsoft SQL Server utilizing
Amazon S3 as the storage medium.

While not suitable for more advanced backup/restore scenarios, it is useful when:

  - You have root access to your database server machines
  - You maintain a number of databases on each database server (i.e., you manage
    multiple clients' applications)
  - Your databases are relatively small
  - You would like a single point of configuration for all of your databases on a 
    machine

You will find that this solution works very well if, like me, you manage a number
of client applications, want to backup at least daily, and want to restore the latest
backups to a staging machine for up-to-date testing data.

## Building
  
To build:

    MSBuild.exe build.proj
    
The build script requires MSBuild v3.5 (on my machine, this is located at 
`C:\Windows\Microsoft.NET\Framework\v3.5\MSBuild.exe`).

This will create a release build. If you'd like to hack, you can use the Visual Studio
solution for building. To do this, first create `App.config` files for each executable project (`SThreeQL.Console`,
`SThreeQL.Service` and `SThreeQL.Test`). Copy the contents of `App.config.template` into
each file. Configure to taste.

## Configuring

All configuration happens within each executable's `.config` file. Basically,
you need to set up your Amazon S3 targets, your backup and/or restore data sources,
your backup and/or restore target definitions and any schedules you want to define.

Take a look at the `SThreeQL.Configuration` namespace for reference on what is required, what is
inferred and so forth.

Please note that **you need permission to drop and restore databases** for each data 
source you configure. Windows Authentication is not currently supported, so you'll
have to enable SQL Server Authentication on your databases.

## Running the Console

Once you've created the configuration for your backup or restore scenario, I recommend
that you run the console application (`s3ql.exe`) to test it out before moving on.

With the console you can run a single backup target, a single restore target, a single schedule,
all backup targets, all restore targets, or all schedules.

To qualify what runs:

  - **-a** will run all backup targets and then run all restore targets
  - **-b [-t backup-target-name]** will just run backup(s), optionally only running the target
    identified
  - **-r [-t restore-target-name]** will just run restore(s), optionally only running the target
    identified
  - **-s [-t schedule-name]** will run schedule(s) and continue executing until forcefully quit
    with *Ctl+C*, optionally only running the schedule identified

## Installing the Service

Now that you've verified your configuration is valid, you can install the service (`s3qlservice.exe`)
and start it. See Microsoft's `InstallUtil.exe` documentation if you're unsure how to install
a Windows service. 

Please make sure that you've copied the console application's configuration to the service's
configuration file (i.e., make sure `s3ql.exe.config` and `s3qlservice.exe.config` are in sync).

## Coming When Time Permits

  - A local backup file cache
  - A history log
  - Notifications (email/web hooks)
  - GUI-based configuration
  - An installer

You should pester me about implementing these things as often as possible.