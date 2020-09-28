using System.Collections.Generic;

namespace Syn.Speech.Helper
{
    public class Attributes
    {

        private readonly Dictionary<string, string> _mainTable = new Dictionary<string, string>();

        public Attributes(int capacity)
        {
            _mainTable = new Dictionary<string, string>(capacity);
        }
 
        public void Add(string name, string value)
        {
            _mainTable.Add(name,value);
        }

        public string getValue(string name)
        {
            string toReturn;
            return _mainTable.TryGetValue(name, out toReturn) ? toReturn : null;
        }

        public int getLength()
        {
            return _mainTable.Count;
        }
    }
}
