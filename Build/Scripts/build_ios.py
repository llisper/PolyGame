# -*- coding: utf-8 -*-
import os
import shutil
import platform
import subprocess

build_path = None
product_path = None

def build(env, args):
    global build_path
    global product_path
    build_path = env['build_path'] + '/iOS'
    product_path = env['product_path'] + '/iOS'

    if args.clean:
        clean(env, args)

    generate_project(env, args)

def clean(env, args):
    if ('Bundle' in args.buildFlags) and os.path.exists(build_path):
        shutil.rmtree(build_path)
    if ('Resource' in args.buildFlags) and os.path.exists(product_path):
        shutil.rmtree(product_path)

def generate_project(env, args):
    if not os.path.exists(build_path):
        os.makedirs(build_path)
    log_file = os.path.normpath(build_path + '/Editor.log')

    os.chdir(env['unity_path'])
    invoke_args = [
        './Unity',
        '-projectPath', env['project_path'],
        '-quit',
        '-batchmode',
        '-logFile', log_file,
        '-buildTarget', 'iOS',
        '-executeMethod', 'BuildAPIs.BuildiOS',
        args.appIdentifier,
        args.version,
        args.cdn,
        '|'.join(args.buildFlags),
        str(not args.release).lower(),
        args.branch,
        args.rev
    ]

    print("Generating iOS Project")
    print(' '.join(invoke_args))

    p = subprocess.Popen(invoke_args, cwd=env['unity_path'])
    p.wait()
    retcode = p.returncode
    if retcode != 0:
        raise RuntimeError("Generate iOS Project FAILED, retcode: " + str(retcode))
    print("iOS Project Generated")
