#!/bin/sh

cd `dirname $0`/Scripts
python -B build.py -clean -buildTarget Android -buildFlags Resource Bundle
