@echo off
set version=0.9
docker build -t kristfin/couchdb-view-update:%version% . 
docker tag kristfin/couchdb-view-update:%version%  kristfin/couchdb-view-update:latest
