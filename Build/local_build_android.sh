#!/bin/sh

root=`dirname $0`
cd $root/Scripts
python -B build.py -clean -buildTarget Android -buildFlags Resource Bundle -cdn "skip-update"

cd $root
cp Output/Android/PolyGame/build/outputs/apk/*.apk Output/Android
