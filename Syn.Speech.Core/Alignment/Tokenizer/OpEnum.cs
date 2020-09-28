using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    internal class OpEnum
    {
        private static readonly HashMap<string, OpEnum> Map = new HashMap<string, OpEnum>();
        public static OpEnum Next = new OpEnum("n");
        public static OpEnum Prev = new OpEnum("p");
        public static OpEnum NextNext = new OpEnum("nn");
        public static OpEnum PrevPrev = new OpEnum("pp");
        public static OpEnum Parent = new OpEnum("parent");
        public static OpEnum Daughter = new OpEnum("daughter");
        public static OpEnum LastDaughter = new OpEnum("daughtern");
        public static OpEnum Relation = new OpEnum("R");
        private readonly string _name;


        static OpEnum() { }

        private OpEnum(string name)
        {
            _name = name;
            Map.Put(_name, this);
        }

        public static OpEnum GetInstance(string name)
        {
            return Map.Get(name);
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
