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

branch = subprocess.check_output('git rev-parse --abbrev-ref HEAD'.split()).strip()
args.branch = branch
print('branch: ' + branch)
rev = subprocess.check_output('git rev-parse HEAD'.split()).strip()
args.rev = rev
print('rev:    ' + rev)

builders[args.buildTarget](env, args)


