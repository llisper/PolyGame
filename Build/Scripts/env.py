# -*- coding: utf-8 -*-
import os

def init_env():
    script_path = os.path.realpath(__file__)
    script_dir = os.path.dirname(script_path)
    os.chdir(script_dir)

    env = {}
    env['project_path'] = script_dir + '/../..'
    env['unity_path'] = env_var('UNITY_HOME')
    env['product_path'] = env['project_path'] + '/Product'
    env['build_path'] = env['project_path'] + '/Build/Output'

    env['android_sdk_home'] = env_var('ANDROID_SDK_HOME')
    env['android_ndk_home'] = env_var('ANDROID_NDK_HOME')
    env['java_sdk_home'] = env_var('JAVA_HOME')
    env['gradle_home'] = env_var('GRADLE_HOME')

    for k, v in env.iteritems():
        if k[-4:len(k)] == 'path':
            env[k] = os.path.normpath(v)
        print('env {0}={1}'.format(k, env[k]))

    return env

def env_var(name):
    if (not (name in os.environ)) or (len(os.environ[name]) == 0):
        raise RuntimeError(name + ' is not a environment variables')
    return os.environ[name]
