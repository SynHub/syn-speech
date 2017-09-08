using Syn.Speech.Helper;

//PATROLLED
namespace Syn.Speech.Alignment
{
  internal class OpEnum 
  {
    private static readonly HashMap<string, OpEnum> map =  new HashMap<string, OpEnum>();
    public static OpEnum NEXT = new OpEnum("n");
    public static OpEnum PREV = new OpEnum("p");
    public static OpEnum NEXT_NEXT = new OpEnum("nn");
    public static OpEnum PREV_PREV = new OpEnum("pp");
    public static OpEnum PARENT = new OpEnum("parent");
    public static OpEnum DAUGHTER = new OpEnum("daughter");
    public static OpEnum LAST_DAUGHTER = new OpEnum("daughtern");
    public static OpEnum RELATION = new OpEnum("R");
    private readonly string name;

   
    static OpEnum(){}

    private OpEnum(string name)
    {
        this.name = name;
        map.put(this.name, this);
    }

    public static OpEnum getInstance(string name)
    {
        return map.get(name);
    }

    public override string ToString()
    {
      return name;
    }
  }
}
