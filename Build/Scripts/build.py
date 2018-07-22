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

p = subprocess.Popen('git status -s'.split(), stdout=subprocess.PIPE)
changes = p.communicate()[0]
if len(changes) > 0:
    print('there are local changes that has not been commited, commit or cleanup the workspace first.')
    sys.exit(1)

env = env.init_env()
args = argparser.parse()
print(args)
builders[args.buildTarget](env, args)

subprocess.check_call('git reset --hard HEAD')
subprocess.check_call('git clean --fd')

