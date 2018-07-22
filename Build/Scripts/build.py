# -*- coding: utf-8 -*-
import sys
import env
import argparser
import build_android
import subprocess

builders = {
    'Android' : build_android.build,
    'iOS' : None,
}

env = env.init_env()
args = argparser.parse()
print(args)
builders[args.buildTarget](env, args)

