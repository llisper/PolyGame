#!/bin/sh

root=`dirname $0`
cd $root
changes=`git status -s`
if [[ ! -z "${changes// }" ]] 
then
    read -p "There are uncommited changes, continue will clean local changes. Continue?[y/n]" respond
    if [[ "$respond" = "Y" ]] || [[ "$respond" == "y" ]]
    then
        echo "CLEANUP WORKSPACE..."
        git reset --hard HEAD
        git clean -fd
    else
        exit 1
    fi
fi

cd $root/Scripts
echo "INVOKE BUILD SCRIPT..."
python -B build.py -clean -buildTarget Android -buildFlags Resource Bundle -cdn "skip-update"
status=$?
if [[ $status -ne 0 ]]
then
    echo "BUILD FAILED WTIH $status" >&2
fi

echo "CLEANUP WORKSPACE..."
sleep 1
git reset --hard HEAD
git clean -fd

if [[ $status -eq 0 ]]
then
    cd $root
    echo "COPY PACKAGE..."
    sleep 1
    pwd
    ls Output/Android/PolyGame/build/outputs/apk
    cp Output/Android/PolyGame/build/outputs/apk/*.apk Output/Android
fi
