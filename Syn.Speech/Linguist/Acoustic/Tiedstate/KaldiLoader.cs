using System;
using Syn.Speech.Decoders.Adaptation;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    public class KaldiLoader:ILoader
    {
        [S4Component(Type = typeof(UnitManager))]
        public static string PROP_UNIT_MANAGER = "unitManager";
        private string location;

        [S4String(Mandatory = true)]
        public static string PROP_LOCATION = "location";
        private UnitManager unitManager;

        private Pool<ISenone> senonePool;
        private HMMManager hmmManager;
        private JProperties _modelJProperties;
        private LinkedHashMap<String, Unit> contextIndependentUnits;
        private float[][] transform;

        /**
        /// Constructs empty object.
         *
        /// Does nothing but is required for instantiation from the context object.
         */
        public KaldiLoader() {
        }

        public KaldiLoader(String location, UnitManager unitManager) {
            init(location, unitManager);
        }

        public void init(String location, UnitManager unitManager) {
            this.location = location;
            this.unitManager = unitManager;
        }

        public void NewProperties(PropertySheet ps)
        {
            init(ps.GetString(PROP_LOCATION), (UnitManager) ps.GetComponent(PROP_UNIT_MANAGER));
        }

        /**
        /// Loads the acoustic model.
         *
        /// @throws IOException if an error occurs while loading the model
         */
        public void Load()
        {
            //KaldiTextParser parser = new KaldiTextParser(location);
            //TransitionModel transitionModel = new TransitionModel(parser);
            //senonePool = new KaldiGmmPool(parser);

            //File file = new File(location, "phones.txt");
            //InputStream stream = new URL(file.getPath()).openStream();
            //Reader reader = new InputStreamReader(stream);
            //BufferedReader br = new BufferedReader(reader);
            //Map<String, Integer> symbolTable = new HashMap<String, Integer>();
            //String line;

            //while (null != (line = br.readLine())) {
            //    String[] fields = line.split(" ");
            //    if (Character.isLetter(fields[0].charAt(0)))
            //        symbolTable.put(fields[0], Integer.parseInt(fields[1]));
            //}

            //contextIndependentUnits = new HashMap<String, Unit>();
            //hmmManager = new LazyHmmManager(parser, transitionModel,
            //                                senonePool, symbolTable);

            //for (String phone : symbolTable.keySet()) {
            //    Unit unit = unitManager.getUnit(phone, "SIL".equals(phone));
            //    contextIndependentUnits.put(unit.getName(), unit);
            //    // Ensure monophone HMMs are created.
            //    hmmManager.get(HMMPosition.UNDEFINED, unit);
            //}

            //loadTransform();
            //loadProperties();
        }

        private void loadTransform()
        {
            //URL transformUrl = new URL(new File(location, "final.mat").getPath());
            //Reader reader = new InputStreamReader(transformUrl.openStream());
            //BufferedReader br = new BufferedReader(reader);
            //List<Float> values = new ArrayList<Float>();
            //int numRows = 0;
            //int numCols = 0;
            //String line;

            //while (null != (line = br.readLine())) {
            //    int colCount = 0;

            //    for (String word : line.split("\\s+")) {
            //        if (word.isEmpty() || "[".equals(word) || "]".equals(word))
            //            continue;

            //        values.add(Float.parseFloat(word));
            //        ++colCount;
            //    }

            //    if (colCount > 0)
            //        ++numRows;

            //    numCols = colCount;
            //}

            //transform = new float[numRows][numCols];
            //Iterator<Float> valueIterator = values.iterator();

            //for (int i = 0; i < numRows; ++i) {
            //    for (int j = 0; j < numCols; ++j)
            //        transform[i][j] = valueIterator.next();
            //}
        }

        private void loadProperties()
        {
            //File file = new File(location, "feat.params");
            //InputStream stream = new URL(file.getPath()).openStream();
            //Reader reader = new InputStreamReader(stream);
            //BufferedReader br = new BufferedReader(reader);
            //modelProperties = new Properties();
            //String line;

            //while ((line = br.readLine()) != null) {
            //    String[] tokens = line.split(" ");
            //    modelProperties.put(tokens[0], tokens[1]);
            //}
        }

        /**
        /// Gets the senone pool for this loader.
         *
        /// @return the pool
         */

        public Pool<ISenone> SenonePool
        {
            get { return senonePool; }
        }

        /**
        /// Returns the HMM Manager for this loader.
         *
        /// @return the HMM Manager
         */

        public HMMManager HMMManager
        {
            get { return hmmManager; }
        }

        /**
        /// Returns the map of context indepent units.
         *
        /// The map can be accessed by unit name.
         *
        /// @return the map of context independent units
         */

        public LinkedHashMap<string, Unit> ContextIndependentUnits
        {
            get { return contextIndependentUnits; }
        }

        /**
        /// Returns the size of the left context for context dependent units.
         *
        /// @return the left context size
         */

        public int LeftContextSize
        {
            get { return 1; }
        }

        /**
        /// Returns the size of the right context for context dependent units.
         *
        /// @return the right context size
         */

        public int RightContextSize
        {
            get { return 1; }
        }

        /**
        /// Returns the model properties
         */

        public JProperties Properties
        {
            get { return _modelJProperties; }
        }

        /**
        /// Logs information about this loader
         */
        public void LogInfo() {
        }

        /**
        /// Not implemented.
         */

        public Pool<float[]> MeansPool
        {
            get { return null; }
        }

        /**
        /// Not implemented.
         */

        public Pool<float[][]> MeansTransformationMatrixPool
        {
            get { return null; }
        }

        /**
        /// Not implemented.
         */

        public Pool<float[]> MeansTransformationVectorPool
        {
            get { return null; }
        }

        /**
        /// Not implemented.
         */

        public Pool<float[]> VariancePool
        {
            get { return null; }
        }

        /**
        /// Not implemented.
         */

        public Pool<float[][]> VarianceTransformationMatrixPool
        {
            get { return null; }
        }

        /**
        /// Not implemented.
         */

        public Pool<float[]> VarianceTransformationVectorPool
        {
            get { return null; }
        }


        /**
        /// Not implemented.
         */

        public GaussianWeights MixtureWeightsPool
        {
            get { return null; }
        }


        /**
        /// Not implemented.
         */

        public Pool<float[][]> TransitionMatrixPool
        {
            get { return null; }
        }

        /**
        /// Not implemented.
         */

        public float[][] TransformMatrix
        {
            get { return transform; }
        }

        public void Update(Transform transform, ClusteredDensityFileData clusters) {
            // TODO Not implemented yet
        }



    }
}
