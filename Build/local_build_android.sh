#!/bin/sh

cd `dirname $0`/Scripts
python -B build.py -clean -buildTarget Android -debug -buildFlags Resource|Bundle
