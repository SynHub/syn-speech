//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    /// <summary>
    /// Represents an ordered set of {@link Item}s and their associated children. 
    /// A relation has a name and a list of items, and is added to an {@link Utterance} via an {@link UsEnglishWordExpander}.
    /// </summary>
    public class Relation
    {
        /// <summary>
        /// Name of the relation that contains tokens from the original input text. 
        /// This is the first thing to be added to the utterance.
        /// </summary>
        public const string Token = "Token";

        /// <summary>
        /// Name of the relation that contains the normalized version of the original input text.
        /// </summary>
        public const string Word = "Word";

        /// <summary>
        /// Creates a relation.
        /// </summary>
        /// <param name="name">The name of the Relation.</param>
        /// <param name="owner">The utterance that contains this relation.</param>
        internal Relation( string name,  Utterance owner)
        {
            Name = name;
            Utterance = owner;
            Head = null;
            Tail = null;
        }

        /// <summary>
        /// Retrieves the name of this Relation.
        /// </summary>
        /// <value>The name of this Relation</value>
        public virtual string Name { get; private set; }

        /// <summary>
        /// Gets the head of the item list.
        /// </summary>
        /// <value>The head item</value>
        public virtual Item Head { get; set; }

        /// <summary>
        /// Gets the tail of the item list.
        /// </summary>
        /// <value>The tail item.</value>
        public virtual Item Tail { get; set; }


        /// <summary>
        /// Adds a new item to this relation. The item added shares its contents with the original item.
        /// </summary>
        /// <returns>The newly added item.</returns>
        public virtual Item AppendItem()
        {
            return AppendItem(null);
        }

        /// <summary>
        /// Adds a new item to this relation. The item added shares its contents with the original item.
        /// </summary>
        /// <param name="originalItem">The ItemContents that will be shared by the new item.</param>
        /// <returns>The newly added item.</returns>
        public virtual Item AppendItem(Item originalItem)
        {
            ItemContents contents;
            Item newItem;

            if (originalItem == null)
            {
                contents = null;
            }
            else
            {
                contents = originalItem.SharedContents;
            }
            newItem = new Item(this, contents);
            if (Head == null)
            {
                Head = newItem;
            }

            if (Tail != null)
            {
                Tail.Attach(newItem);
            }
            Tail = newItem;
            return newItem;
        }

        /// <summary>
        /// Returns the utterance that contains this relation.
        /// </summary>
        /// <value>The utterance that contains this relation.</value>
        public virtual Utterance Utterance { get; private set; }
    }
}
