using System.IO;

namespace Codebot.Runtime
{
    public delegate string Mapper(string path);

    public static class PathMapper
    {
        public static Mapper Mapper { get; set; }

        public static string Map(string path)
        {
            return Mapper == null ? path : Mapper(path);
        }

        public static string Map(string path, params string [] args)
        {
            foreach (var arg in args)
                path = Path.Combine(path, arg);
            return Map(path);
        }
    }
}