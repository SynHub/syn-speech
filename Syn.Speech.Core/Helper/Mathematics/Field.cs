//PATROLLED

using System;

namespace Syn.Speech.Helper.Mathematics
{

    public interface Field<T>
    {

        /** Get the additive identity of the field.
         * <p>
         * The additive identity is the element e<sub>0</sub> of the field such that
         * for all elements a of the field, the equalities a + e<sub>0</sub> =
         * e<sub>0</sub> + a = a hold.
         * </p>
         * @return additive identity of the field
         */
        T getZero();

        /** Get the multiplicative identity of the field.
         * <p>
         * The multiplicative identity is the element e<sub>1</sub> of the field such that
         * for all elements a of the field, the equalities a &times; e<sub>1</sub> =
         * e<sub>1</sub> &times; a = a hold.
         * </p>
         * @return multiplicative identity of the field
         */
        T getOne();

        /**
         * Returns the runtime class of the FieldElement.
         *
         * @return The {@code Class} object that represents the runtime
         *         class of this object.
         */
        Type getRuntimeClass();

    }
}
