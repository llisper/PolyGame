@echo off
cd %~dp0scripts
python -B build.py -clean -buildTarget Android -debug -buildFlags Resource|Bundle
