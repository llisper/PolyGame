# -*- coding: utf-8 -*-
import env
import argparser
import build_android

builders = {
    'Android' : build_android.build,
    'iOS' : None,
}

env = env.init_env()
args = argparser.parse()
print(args)
builders[args.buildTarget](env, args)