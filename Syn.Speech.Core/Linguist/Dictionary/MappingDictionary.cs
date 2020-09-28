using System;
using System.Collections.Generic;
using System.IO;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Dictionary
{
    /// <summary>
    /// Maps the phones from one phoneset to another to use dictionary from the one
    /// acoustic mode with another one. The mapping file is specified with a mapList
    /// property. The contents should look like
    /// <para>
    /// AX AH
    /// IX IH
    /// </para>
    /// </summary>
    public class MappingDictionary : TextDictionary
    {

        [S4String(Mandatory = true, DefaultValue = "")]
        public const String PropMapFile = "mapFile";

        private URL _mappingFile;
        private readonly HashMap<String, String> _mapping = new HashMap<String, String>();

        public MappingDictionary(URL mappingFile, URL wordDictionaryFile, URL fillerDictionaryFile, List<URL> addendaUrlList,
                String wordReplacement, UnitManager unitManager)
            : base(wordDictionaryFile, fillerDictionaryFile, addendaUrlList, wordReplacement, unitManager)
        {

            _mappingFile = mappingFile;
        }

        public MappingDictionary()
        {

        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);

            _mappingFile = ConfigurationManagerUtils.GetResource(PropMapFile, ps);
        }


        public override void Allocate()
        {
            base.Allocate();
            if (!_mappingFile.Path.Equals(""))
                LoadMapping(_mappingFile.OpenStream());
        }


        /// <summary>
        /// Gets a context independent unit. There should only be one instance of any CI unit.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <param name="isFiller">if true, the unit is a filler unit</param>
        /// <returns>The unit.</returns>
        protected override Unit GetCIUnit(String name, bool isFiller)
        {
            if (_mapping.ContainsKey(name))
            {
                name = _mapping.Get(name);
            }
            return unitManager.GetUnit(name, isFiller, Context.EmptyContext);
        }

        protected void LoadMapping(Stream inputStream)
        {
            var br = new StreamReader(inputStream);
            String line;
            while ((line = br.ReadLine()) != null)
            {
                var st = new StringTokenizer(line);
                if (st.countTokens() != 2)
                {
                    throw new IOException("Wrong file format");
                }
                _mapping.Put(st.nextToken(), st.nextToken());
            }
            br.Close();
            inputStream.Close();
        }
    }
}
