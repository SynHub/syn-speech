using System.IO;
//CUSTOM
namespace Syn.Speech.Helper
{
    public class URL
    {
        private string _path;

        public URLType Type { get; set; }

        public URL()
        {
            Type = URLType.None;
        }

        public URL(string path) : this(URLType.Path, path) { }

        public URL(URLType type, string pathOrContent)
        {
            Type = type;
            if (type == URLType.Path)
            {
                Path = pathOrContent;
            }
            else if (type == URLType.Resource)
            {
                Content = pathOrContent;
            }
        }

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                if (!string.IsNullOrEmpty(_path))
                {
                    var pathString = System.IO.Path.GetFullPath(_path);
                    if (System.IO.File.Exists(pathString))
                    {
                        File = new FileInfo(pathString);
                    }
                }
            }
        }

        public Stream OpenStream()
        {
            return File.OpenRead();
           
        }

        public FileInfo File { get; set; }

        public string Content { get; set; }

        public override string ToString()
        {
            return Type == URLType.Path ? Path : "Resource Content...";
        }
    }
}
