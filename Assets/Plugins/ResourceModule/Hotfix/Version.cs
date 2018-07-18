using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ResourceModule.Hotfix
{
    public class Version
    {
        [Serializable]
        public class Config
        {
            public string name;
            public string cdn;

            public Config() { }
            public Config(string name, string cdn)
            {
                this.name = name;
                this.cdn = cdn;
            }
        }

        public int Major { get; }
        public int Minor { get; }
        public string Name { get; }
        public string Cdn { get; }

        Version(string name, string cdn)
        {
            var match = Regex.Match(name, @"(\d+)\.(\d+)");
            Major = int.Parse(match.Groups[1].Value);
            Minor = int.Parse(match.Groups[2].Value);
            Name = name;
            if (cdn[cdn.Length - 1] != '/')
                cdn += '/';
            Cdn = cdn;
        }

        public static Version Create()
        {
            return Create(FileLoader.Load(PathRouter.Version));
        }

        public static Version Create(byte[] data)
        {
            data = FileLoader.RemoveBOM(data);
            string json = Encoding.UTF8.GetString(data);
            var config = JsonUtility.FromJson<Config>(json);
            return new Version(config.name, config.cdn);
        }

        public static Version Create(string name, string cdn)
        {
            return new Version(name, cdn);
        }

        public void WriteToFile()
        {
            string path = PathRouter.SandboxPath + PathRouter.Version;
            string text = JsonUtility.ToJson(new Config(ToString(), Cdn), true);
            File.WriteAllText(path, text, Encoding.UTF8);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (!(obj is Version))
                return false;

            var other = (Version)obj;
            return Major == other.Major && Minor == other.Minor;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", Major, Minor);
        }

        public static bool operator <(Version v1, Version v2)
        {
            return v1.Major < v2.Major || (v1.Major == v2.Major && v1.Minor < v2.Minor);
        }

        public static bool operator <=(Version v1, Version v2)
        {
            return !(v2 < v1);
        }

        public static bool operator >(Version v1, Version v2)
        {
            return v1 != v2 && !(v1 < v2);
        }

        public static bool operator >=(Version v1, Version v2)
        {
            return !(v1 < v2);
        }

        public static bool operator ==(Version v1, Version v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(Version v1, Version v2)
        {
            return !v1.Equals(v2);
        }
    }
}
