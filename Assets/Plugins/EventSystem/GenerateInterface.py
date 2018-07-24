#-*- coding:utf-8 -*-
import sys
from UserString import MutableString

if len(sys.argv) < 2:
    exit()

n = int(sys.argv[1])
if n < 0:
    exit()

defines = '\n{0}const int paramLength = {1};\n'
tab = ' ' * 4

code = MutableString()
code += '{0}#region generated'.format(tab)
code += defines.format(tab, n)

for i in range(n + 1):
    code += '\n{0}public void Fire(int e'.format(tab)
    code += ''.join(list(', object p%d' % d for d in range(i)))
    code += ')\n{0}{{\n{1}object[] a = ParamArray({2});\n'.format(tab, tab * 2, i)
    if i > 0:
        code += tab * 2 + (tab * 2).join(list('a[{0}] = p{0};\n'.format(d) for d in range(i)))
    code += '%sFire(e, a);\n' % (tab * 2)
    code += '%sReleaseParamArray(a);\n%s}\n' % (tab * 2, tab)
code += '%s#endregion generated' % tab
print(code)
