# -*- coding: utf-8 -*-
import re
import argparse

class CheckIdentifier(argparse.Action):
    def __call__(self, parser, namespace, values, option_string=None):
        if re.match(r'^[a-zA-Z0-9_]+\.[a-zA-Z0-9_]+\.[a-zA-Z0-9_]+$', values):
            setattr(namespace, self.dest, values)
        else:
            raise RuntimeError(values + ' is not a valid app identifier')

class CheckVersionName(argparse.Action):
    def __call__(self, parser, namespace, values, option_string=None):
        if re.match(r'^[0-9]+\.[0-9]+$', values):
            setattr(namespace, self.dest, values)
        else:
            raise RuntimeError(values + ' is not a valid version')

def parse():
    parser = argparse.ArgumentParser(description='game package build')
    parser.add_argument('-clean', action='store_true', default=False)
    parser.add_argument('-buildTarget', choices=['Android', 'iOS'], default='Android')
    parser.add_argument('-appIdentifier', action=CheckIdentifier, default='com.polygame.test')
    parser.add_argument('-version', action=CheckVersionName, default='0.1')
    parser.add_argument('-cdn', default='http://localhost:8080/Product')
    parser.add_argument('-release', action='store_true', default=False, help='build release package')
    parser.add_argument('-buildFlags', nargs='*', choices=['None', 'Resource', 'Bundle'], default=['Resource', 'Bundle'])
    return parser.parse_args()
