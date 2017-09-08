//PATROLLED
namespace Syn.Speech.Alignment
{

    /// <summary>
    /// Represents an ordered set of {@link Item}s and their associated children. 
    /// A relation has a name and a list of items, and is added to an {@link Utterance} via an {@link UsEnglishWordExpander}.
    /// </summary>
    public class Relation
    {
        private readonly string name;
        private readonly Utterance owner;
        private Item head;
        private Item tail;

        /// <summary>
        /// Name of the relation that contains tokens from the original input text. 
        /// This is the first thing to be added to the utterance.
        /// </summary>
        public const string TOKEN = "Token";

        /// <summary>
        /// Name of the relation that contains the normalized version of the original input text.
        /// </summary>
        public const string WORD = "Word";

        /// <summary>
        /// Creates a relation.
        /// </summary>
        /// <param name="name">The name of the Relation.</param>
        /// <param name="owner">The utterance that contains this relation.</param>
        internal Relation( string name,  Utterance owner)
        {
            this.name = name;
            this.owner = owner;
            head = null;
            tail = null;
        }

        /// <summary>
        /// Retrieves the name of this Relation.
        /// </summary>
        /// <returns>The name of this Relation</returns>
        public virtual string getName()
        {
            return name;
        }

        /// <summary>
        /// Gets the head of the item list.
        /// </summary>
        /// <returns>The head item</returns>
        public virtual Item getHead()
        {
            return head;
        }

        /// <summary>
        /// Sets the head of the item list.
        /// </summary>
        /// <param name="item">The new head item.</param>
        internal virtual void setHead(Item item)
        {
            head = item;
        }

        /// <summary>
        /// Gets the tail of the item list.
        /// </summary>
        /// <returns>The tail item.</returns>
        public virtual Item getTail()
        {
            return tail;
        }

        /// <summary>
        /// Sets the tail of the item list.
        /// </summary>
        /// <param name="item">The new tail item.</param>
        internal virtual void setTail(Item item)
        {
            tail = item;
        }

        /// <summary>
        /// Adds a new item to this relation. The item added shares its contents with the original item.
        /// </summary>
        /// <returns>The newly added item.</returns>
        public virtual Item appendItem()
        {
            return appendItem(null);
        }

        /// <summary>
        /// Adds a new item to this relation. The item added shares its contents with the original item.
        /// </summary>
        /// <param name="originalItem">The ItemContents that will be shared by the new item.</param>
        /// <returns>The newly added item.</returns>
        public virtual Item appendItem(Item originalItem)
        {
            ItemContents contents;
            Item newItem;

            if (originalItem == null)
            {
                contents = null;
            }
            else
            {
                contents = originalItem.getSharedContents();
            }
            newItem = new Item(this, contents);
            if (head == null)
            {
                head = newItem;
            }

            if (tail != null)
            {
                tail.attach(newItem);
            }
            tail = newItem;
            return newItem;
        }

        /// <summary>
        /// Returns the utterance that contains this relation.
        /// </summary>
        /// <returns>The utterance that contains this relation.</returns>
        public virtual Utterance getUtterance()
        {
            return owner;
        }
    }
}
