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
    build_path = env['build_path'] + '/Android'
    product_path = env['product_path'] + '/Android'

    if args.clean:
        clean(env, args)

    generate_project(env, args)
    build_apk(env)

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
        '-buildTarget', 'Android',
        '-executeMethod', 'BuildAPIs.BuildAndroid',
        args.appIdentifier,
        args.version,
        args.cdn,
        '|'.join(args.buildFlags),
        str(not args.release).lower(),
        args.branch,
        args.rev
    ]

    print("Generating Android Project")
    print(' '.join(invoke_args))

    p = subprocess.Popen(invoke_args, cwd=env['unity_path'])
    p.wait()
    retcode = p.returncode
    if retcode != 0:
        raise RuntimeError("Generate Android Project FAILED, retcode: " + str(retcode))
    print("Android Project Generated")

def build_apk(env):
    gradle_project_path = build_path + '/' + os.path.basename(env['project_path'])
    gradle_project_path = os.path.normpath(gradle_project_path)

    invoke_args = [
        '--project-dir=' + gradle_project_path,
        '-Dorg.gradle.java.home=' + env['java_sdk_home'],
        'assembleDebug'
    ]

    if platform.system() == 'Windows':
        invoke_args = ['cmd', '/c', 'gradle.bat'] + invoke_args
    else:
        invoke_args.insert(0, './gradle')

    print("Assembling APK")
    print(' '.join(invoke_args))

    p = subprocess.Popen(invoke_args, cwd=env['gradle_home'])
    p.wait()
    retcode = p.returncode
    if retcode != 0:
        raise RuntimeError("Assemble APK FAILED, retcode: " + str(retcode))
    print("APK Assembled")
