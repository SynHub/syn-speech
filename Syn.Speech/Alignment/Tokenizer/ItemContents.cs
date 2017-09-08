//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    public class ItemContents 
    {
        private readonly FeatureSet _relations;

        public ItemContents()
        {
            Features = new FeatureSet();
            _relations = new FeatureSet();
        }

        public virtual void AddItemRelation(string relationName, Item item)
        {
            _relations.SetObject(relationName, item);
        }

        public virtual void RemoveItemRelation(string relationName)
        {
            _relations.Remove(relationName);
        }

        public virtual Item GetItemRelation(string relationName)
        {
            return (Item)_relations.GetObject(relationName);
        }

        public virtual FeatureSet Features { get; private set; }
    }
}
