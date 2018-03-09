# Simple utility to keep views in CouchDb updated #

I was looking for a similar functionality as in Lotus Domino, where one can keep the views in the 
database up to date.  This utility can be run as command line daemon and on interval forces refresh of views in 
databases on a CouchDb server.  It is also possible to run this in a docker, which is how i use this.

The code is written in C#, using .NET Core 2.0

## Options ##
| name | type| value 
|---|---|---|
|server|string|url for the CouchDb server, mandatory, must be prefixed with http(s)|
|user|string | optional, couchdb user |
|password|string | optional, couchdb password |
|daemon|bool | optional, run in daemon mode |
|daemonDelaySec| int |delay between runs in daemon mode, default 600 seconds|
|server|url for the CouchDb server | mandatory, must be prefixed with http(s)|
|databases|string list| optional, comma seperated list of databases, if not specified all databases, but ones with name prefixed with '_' will be updated|


## Build ##

## Build ##
```
C:\work\CouchDbViewUpdate>dotnet build
Microsoft (R) Build Engine version 15.6.82.30579 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  Restore completed in 35,89 ms for C:\work\CouchDbViewUpdate\CouchDbViewUpdate.csproj.
  CouchDbViewUpdate -> C:\work\CouchDbViewUpdate\bin\Debug\netcoreapp2.0\CouchDbViewUpdate.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:00.89
```

## Command line usage ##
```
C:\work\CouchDbViewUpdate>dotnet run server=http://localhost:5984
[15:53:49 INF] CouchDbViewUpdate 0.9
[15:53:49 INF] Copyright (C) 2018 Kristján Þór Finnsson <kristfin@gmail.com>
[15:53:49 INF] Using configuration
        server=http://localhost:5984
        user=null
        password=null
        daemon=False
        daemonDelaySec=600
        databases=[]

[15:53:49 INF] Getting databases from http://localhost:5984
[15:53:50 INF] Will update views in databases: log
[15:53:52 INF] Getting views from log
[15:53:53 INF] updating view log.alles
[15:53:54 INF] updating view log.errors
[15:53:55 INF] updating view log.countLevels
```

## Build docker ##
```
docker build -t kristfin/couchdb-view-update:0.9 .
docker tag kristfin/couchdb-view-update:0.9  kristfin/couchdb-view-update:latest
```

## Run docker ##
```
docker run couchdb-view-update server=http://localhost:5984 daemon=true
```

## docker-compose.yml ##
```
version: '3.1'
services:

  couchdb-view-update:
    image: couchdb-view-update:latest
    container_name: couchdb-view-update
    environment:
       - server=http://couchdb:5984
       - user=admin
       - password=jabadabadu
       - daemon=true
       - daemonDelaySec=789
```

## Feedback, Issues, Contributing

**Please use Github issues for any questions, bugs, feature requests.**
