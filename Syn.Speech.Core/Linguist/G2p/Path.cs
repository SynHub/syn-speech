using System;
using System.Collections.Generic;
using System.Text;
//REFACTORED
using Syn.Speech.Fsts.Semirings;

namespace Syn.Speech.Linguist.G2p
{
    /// <summary>
    /// @author John Salatas "jsalatas@users.sourceforge.net"
    /// </summary>
    public class Path
    {
        // the path
        private List<String> _path;

        // the path's cost

        // the paths' semiring
        private Semiring _semiring;

        /**
        /// Create a Path instance with specified path and semiring elements
         */
        public Path(List<String> path, Semiring semiring) 
        {
            _path = path;
            _semiring = semiring;
            Cost = _semiring.Zero;
        }

        /**
        /// Create a Path instance with specified semiring element
         */
        public Path(Semiring semiring) 
            :this(new List<String>(), semiring)
        {
            
        }

        /**
        /// Get the path
         */
        public List<String> GetPath() 
        {
            return _path;
        }

        /**
        /// Get the paths' cost
         */

        public float Cost { get; set; }

        /**
        /// Set the paths' cost
         */

        /**
        /// Get the paths
         */
        public void SetPath(List<String> path) 
        {
            _path = path;
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see java.lang.Object#toString()
         */
        override
        public string ToString() 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Cost + "\t");
            foreach (String s in _path) 
            {
                sb.Append(s);
                sb.Append(' ');
            }
            return sb.ToString().Trim();
        }

    }
}
