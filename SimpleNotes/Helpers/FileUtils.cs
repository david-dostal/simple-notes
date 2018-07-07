using System.IO;
using System.Linq;

namespace SimpleNotes.Helpers
{
    public static class FileUtils
    {
        public static bool IsInvalidFileName(string s)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return invalidChars.Any(c => s.Contains(c));
        }

        public static string AddDirectorySeparator(string path)
        {
            if (path == "")
                return path;
            else if (path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                return path;
            else if (path.Contains(Path.AltDirectorySeparatorChar))
                return path + Path.AltDirectorySeparatorChar;
            else
                return path + Path.DirectorySeparatorChar;
        }
    }
}
