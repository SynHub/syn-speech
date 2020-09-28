using System;
using System.Runtime.Serialization;
using Syn.Speech.Helper.Mathematics.Analysis;
using Syn.Speech.Util;
//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{
    
/**
 * This class implements the {@link RealVector} interface with a double array.
 * @since 2.0
 */
public class ArrayRealVector : RealVector , ISerializable {
    /** Serializable version identifier. */
   // private static  long serialVersionUID = -1097961340710804027L;
    /** Default format. */
    //private static final RealVectorFormat DEFAULT_FORMAT = RealVectorFormat.getInstance();

    /** Entries of the vector. */
    private double[] data;

    /**
     * Build a 0-length vector.
     * Zero-length vectors may be used to initialized construction of vectors
     * by data gathering. We start with zero-length and use either the {@link
     * #ArrayRealVector(ArrayRealVector, ArrayRealVector)} constructor
     * or one of the {@code append} method ({@link #append(double)},
     * {@link #append(ArrayRealVector)}) to gather data into this vector.
     */
    public ArrayRealVector() {
        data = new double[0];
    }

    /**
     * Construct a vector of zeroes.
     *
     * @param size Size of the vector.
     */
    public ArrayRealVector(int size) {
        data = new double[size];
    }

    /**
     * Construct a vector with preset values.
     *
     * @param size Size of the vector
     * @param preset All entries will be set with this value.
     */
    public ArrayRealVector(int size, double preset) {
        data = new double[size];
        Arrays.Fill(data, preset);
    }

    /**
     * Construct a vector from an array, copying the input array.
     *
     * @param d Array.
     */
    //public ArrayRealVector(double[] d) {
    //    data = d.Clone() as double[];
    //}

    /**
     * Create a new ArrayRealVector using the input array as the underlying
     * data array.
     * If an array is built specially in order to be embedded in a
     * ArrayRealVector and not used directly, the {@code copyArray} may be
     * set to {@code false}. This will prevent the copying and improve
     * performance as no new array will be built and no data will be copied.
     *
     * @param d Data for the new vector.
     * @param copyArray if {@code true}, the input array will be copied,
     * otherwise it will be referenced.
     * @throws NullArgumentException if {@code d} is {@code null}.
     * @see #ArrayRealVector(double[])
     */
    public ArrayRealVector(double[] d, bool copyArray) {
        if (d == null) {
            throw new ArgumentNullException();
        }
        data = (copyArray ? d.Clone() :  d) as double[];
    }

    /**
     * Construct a vector from part of a array.
     *
     * @param d Array.
     * @param pos Position of first entry.
     * @param size Number of entries to copy.
     * @throws NullArgumentException if {@code d} is {@code null}.
     * @throws NumberIsTooLargeException if the size of {@code d} is less
     * than {@code pos + size}.
     */
    //public ArrayRealVector(double[] d, int pos, int size){
    //    if (d == null) {
    //        throw new ArgumentNullException();
    //    }
    //    if (d.Length < pos + size) {
    //        throw new Exception("NumberIsTooLargeException");
    //    }
    //    data = new double[size];
    //    Array.Copy(d, pos, data, 0, size);
    //}

    /**
     * Construct a vector from an array.
     *
     * @param d Array of {@code Double}s.
     */
    public ArrayRealVector(Double[] d) {
        data = new double[d.Length];
        for (int i = 0; i < d.Length; i++)
        {
            data[i] = (double) d[i];
        }
    }

    /**
     * Construct a vector from part of an array.
     *
     * @param d Array.
     * @param pos Position of first entry.
     * @param size Number of entries to copy.
     * @throws NullArgumentException if {@code d} is {@code null}.
     * @throws NumberIsTooLargeException if the size of {@code d} is less
     * than {@code pos + size}.
     */
    public ArrayRealVector(Double[] d, int pos, int size) {
        if (d == null) {
            throw new ArgumentNullException();
        }
        if (d.Length < pos + size) {
            throw new Exception("NumberIsTooLargeException");
        }
        data = new double[size];
        for (int i = pos; i < pos + size; i++)
        {
            data[i - pos] = (double)d[i];
        }
    }

    /**
     * Construct a vector from another vector, using a deep copy.
     *
     * @param v vector to copy.
     * @throws NullArgumentException if {@code v} is {@code null}.
     */
    public ArrayRealVector(RealVector v) {
        if (v == null) {
            throw new ArgumentNullException();
        }
        data = new double[v.getDimension()];
        for (int i = 0; i < data.Length; ++i) {
            data[i] = v.getEntry(i);
        }
    }

    /**
     * Construct a vector from another vector, using a deep copy.
     *
     * @param v Vector to copy.
     * @throws NullArgumentException if {@code v} is {@code null}.
     */
    public ArrayRealVector(ArrayRealVector v)  :this( v, true) {

    }

    /**
     * Construct a vector from another vector.
     *
     * @param v Vector to copy.
     * @param deep If {@code true} perform a deep copy, otherwise perform a
     * shallow copy.
     */
    public ArrayRealVector(ArrayRealVector v, bool deep) {
        data = (deep ? v.data.Clone() : v.data) as double[];
    }

    /**
     * Construct a vector by appending one vector to another vector.
     * @param v1 First vector (will be put in front of the new vector).
     * @param v2 Second vector (will be put at back of the new vector).
     */
    public ArrayRealVector(ArrayRealVector v1, ArrayRealVector v2) {
        data = new double[v1.data.Length + v2.data.Length];
        Array.Copy(v1.data, 0, data, 0, v1.data.Length);
        Array.Copy(v2.data, 0, data, v1.data.Length, v2.data.Length);
    }

    /**
     * Construct a vector by appending one vector to another vector.
     * @param v1 First vector (will be put in front of the new vector).
     * @param v2 Second vector (will be put at back of the new vector).
     */
    public ArrayRealVector(ArrayRealVector v1, RealVector v2) {
         int l1 = v1.data.Length;
         int l2 = v2.getDimension();
        data = new double[l1 + l2];
        Array.Copy(v1.data, 0, data, 0, l1);
        for (int i = 0; i < l2; ++i) {
            data[l1 + i] = v2.getEntry(i);
        }
    }

    /**
     * Construct a vector by appending one vector to another vector.
     * @param v1 First vector (will be put in front of the new vector).
     * @param v2 Second vector (will be put at back of the new vector).
     */
    public ArrayRealVector(RealVector v1, ArrayRealVector v2) {
         int l1 = v1.getDimension();
         int l2 = v2.data.Length;
        data = new double[l1 + l2];
        for (int i = 0; i < l1; ++i) {
            data[i] = v1.getEntry(i);
        }
        Array.Copy(v2.data, 0, data, l1, l2);
    }

    /**
     * Construct a vector by appending one vector to another vector.
     * @param v1 First vector (will be put in front of the new vector).
     * @param v2 Second vector (will be put at back of the new vector).
     */
    public ArrayRealVector(ArrayRealVector v1, double[] v2) {
         int l1 = v1.getDimension();
         int l2 = v2.Length;
        data = new double[l1 + l2];
        Array.Copy(v1.data, 0, data, 0, l1);
        Array.Copy(v2, 0, data, l1, l2);
    }

    /**
     * Construct a vector by appending one vector to another vector.
     * @param v1 First vector (will be put in front of the new vector).
     * @param v2 Second vector (will be put at back of the new vector).
     */
    public ArrayRealVector(double[] v1, ArrayRealVector v2) {
         int l1 = v1.Length;
         int l2 = v2.getDimension();
        data = new double[l1 + l2];
        Array.Copy(v1, 0, data, 0, l1);
        Array.Copy(v2.data, 0, data, l1, l2);
    }

    /**
     * Construct a vector by appending one vector to another vector.
     * @param v1 first vector (will be put in front of the new vector)
     * @param v2 second vector (will be put at back of the new vector)
     */
    public ArrayRealVector(double[] v1, double[] v2) {
         int l1 = v1.Length;
         int l2 = v2.Length;
        data = new double[l1 + l2];
        Array.Copy(v1, 0, data, 0, l1);
        Array.Copy(v2, 0, data, l1, l2);
    }


    public override RealVector copy()  /*Todo: Supposed to be ArrayRealVector */{
        return new ArrayRealVector(this, true);
    }


    public override RealVector add(RealVector v)/*Todo: Supposed to be ArrayRealVector */{
        if (v is ArrayRealVector) {
             double[] vData = ((ArrayRealVector) v).data;
             int dim = vData.Length;
            checkVectorDimensions(dim);
            ArrayRealVector result = new ArrayRealVector(dim);
            double[] resultData = result.data;
            for (int i = 0; i < dim; i++) {
                resultData[i] = data[i] + vData[i];
            }
            return result;
        } else {
            checkVectorDimensions(v);
            double[] @out = data.Clone() as double[];
            var it = v.iterator();
            while (it.MoveNext()) {
                 Entry e = it.Current;
                @out[e.getIndex()] += e.getValue();
            }
            return new ArrayRealVector(@out, false);
        }
    }


    public override RealVector subtract(RealVector v) /*Todo: Supposed to return ArrayRealVector */ {
        if (v is ArrayRealVector) {
             double[] vData = ((ArrayRealVector) v).data;
             int dim = vData.Length;
            checkVectorDimensions(dim);
            ArrayRealVector result = new ArrayRealVector(dim);
            double[] resultData = result.data;
            for (int i = 0; i < dim; i++) {
                resultData[i] = data[i] - vData[i];
            }
            return result;
        } else {
            checkVectorDimensions(v);
            double[] @out = data.Clone() as double[];
            var it = v.iterator();
            while (it.MoveNext()) {
                 Entry e = it.Current;
                @out[e.getIndex()] -= e.getValue();
            }
            return new ArrayRealVector(@out, false);
        }
    }

   
    public new ArrayRealVector map(UnivariateFunction function) {
        return copy().mapToSelf(function) as ArrayRealVector;
    }

    /** {@inheritDoc} */

    public new ArrayRealVector mapToSelf(UnivariateFunction function) {
        for (int i = 0; i < data.Length; i++) {
            data[i] = function.value(data[i]);
        }
        return this;
    }

    public override RealVector mapAddToSelf(double d) {
        for (int i = 0; i < data.Length; i++) {
            data[i] += d;
        }
        return this;
    }

 
    public override RealVector mapSubtractToSelf(double d) {
        for (int i = 0; i < data.Length; i++) {
            data[i] -= d;
        }
        return this;
    }

    public override RealVector mapMultiplyToSelf(double d) {
        for (int i = 0; i < data.Length; i++) {
            data[i] *= d;
        }
        return this;
    }

    public override  RealVector mapDivideToSelf(double d) {
        for (int i = 0; i < data.Length; i++) {
            data[i] /= d;
        }
        return this;
    }


    public override RealVector ebeMultiply(RealVector v) /*Todo: Supposed to be ArrayRealVector */{
        if (v is ArrayRealVector) {
             double[] vData = ((ArrayRealVector) v).data;
             int dim = vData.Length;
            checkVectorDimensions(dim);
            ArrayRealVector result = new ArrayRealVector(dim);
            double[] resultData = result.data;
            for (int i = 0; i < dim; i++) {
                resultData[i] = data[i] * vData[i];
            }
            return result;
        } else {
            checkVectorDimensions(v);
            double[] @out = data.Clone() as double[];
            for (int i = 0; i < data.Length; i++) {
                @out[i] *= v.getEntry(i);
            }
            return new ArrayRealVector(@out, false);
        }
    }


    public  override RealVector ebeDivide(RealVector v) /*Todo: Supposed to be ArrayRealVector */{
        if (v is ArrayRealVector) {
             double[] vData = ((ArrayRealVector) v).data;
             int dim = vData.Length;
            checkVectorDimensions(dim);
            ArrayRealVector result = new ArrayRealVector(dim);
            double[] resultData = result.data;
            for (int i = 0; i < dim; i++) {
                resultData[i] = data[i] / vData[i];
            }
            return result;
        } else {
            checkVectorDimensions(v);
            double[] @out = data.Clone() as double[];
            for (int i = 0; i < data.Length; i++) {
                @out[i] /= v.getEntry(i);
            }
            return new ArrayRealVector(@out, false);
        }
    }

    /**
     * Get a reference to the underlying data array.
     * This method does not make a fresh copy of the underlying data.
     *
     * @return the array of entries.
     */
    public double[] getDataRef() {
        return data;
    }

    /** {@inheritDoc} */

    public override double dotProduct(RealVector v)  {
        if (v is ArrayRealVector) {
             double[] vData = ((ArrayRealVector) v).data;
            checkVectorDimensions(vData.Length);
            double dot = 0;
            for (int i = 0; i < data.Length; i++) {
                dot += data[i] * vData[i];
            }
            return dot;
        }
        return base.dotProduct(v);
    }


    public override double getNorm() {
        double sum = 0;
        foreach (double a in data) {
            sum += a * a;
        }
        return Math.Sqrt(sum);
    }


    public override double getL1Norm() {
        double sum = 0;
        foreach (double a in data) {
            sum += Math.Abs(a);
        }
        return sum;
    }


    public override double getLInfNorm() {
        double max = 0;
        foreach (double a in data) {
            max = Math.Max(max, Math.Abs(a));
        }
        return max;
    }

    
    public override double getDistance(RealVector v) {
        if (v is ArrayRealVector) {
             double[] vData = ((ArrayRealVector) v).data;
            checkVectorDimensions(vData.Length);
            double sum = 0;
            for (int i = 0; i < data.Length; ++i) {
                 double delta = data[i] - vData[i];
                sum += delta * delta;
            }
            return Math.Sqrt(sum);
        } else {
            checkVectorDimensions(v);
            double sum = 0;
            for (int i = 0; i < data.Length; ++i) {
                 double delta = data[i] - v.getEntry(i);
                sum += delta * delta;
            }
            return Math.Sqrt(sum);
        }
    }


    public override double getL1Distance(RealVector v) {
        if (v is ArrayRealVector) {
             double[] vData = ((ArrayRealVector) v).data;
            checkVectorDimensions(vData.Length);
            double sum = 0;
            for (int i = 0; i < data.Length; ++i) {
                 double delta = data[i] - vData[i];
                sum += Math.Abs(delta);
            }
            return sum;
        } else {
            checkVectorDimensions(v);
            double sum = 0;
            for (int i = 0; i < data.Length; ++i) {
                 double delta = data[i] - v.getEntry(i);
                sum += Math.Abs(delta);
            }
            return sum;
        }
    }


    public override double getLInfDistance(RealVector v) {
        if (v is ArrayRealVector) {
             double[] vData = ((ArrayRealVector) v).data;
            checkVectorDimensions(vData.Length);
            double max = 0;
            for (int i = 0; i < data.Length; ++i) {
                 double delta = data[i] - vData[i];
                max = Math.Max(max, Math.Abs(delta));
            }
            return max;
        } else {
            checkVectorDimensions(v);
            double max = 0;
            for (int i = 0; i < data.Length; ++i) {
                 double delta = data[i] - v.getEntry(i);
                max = Math.Max(max, Math.Abs(delta));
            }
            return max;
        }
    }


    public override RealMatrix outerProduct(RealVector v) {
        if (v is ArrayRealVector) {
             double[] vData = ((ArrayRealVector) v).data;
             int m = data.Length;
             int n = vData.Length;
             RealMatrix @out = MatrixUtils.CreateRealMatrix(m, n);
            for (int i = 0; i < m; i++) {
                for (int j = 0; j < n; j++) {
                    @out.setEntry(i, j, data[i] * vData[j]);
                }
            }
            return @out;
        } else {
             int m = data.Length;
             int n = v.getDimension();
             RealMatrix @out = MatrixUtils.CreateRealMatrix(m, n);
            for (int i = 0; i < m; i++) {
                for (int j = 0; j < n; j++) {
                    @out.setEntry(i, j, data[i] * v.getEntry(j));
                }
            }
            return @out;
        }
    }


    public override double getEntry(int index) {
        try {
            return data[index];
        } catch (IndexOutOfRangeException e) {
            throw new Exception("OutOfRangeException");
        }
    }

   
    public override int getDimension() {
        return data.Length;
    }

  
    public override RealVector append(RealVector v) {
        try {
            return new ArrayRealVector(this, (ArrayRealVector) v);
        } catch (InvalidCastException cce) {
            return new ArrayRealVector(this, v);
        }
    }

    /**
     * Construct a vector by appending a vector to this vector.
     *
     * @param v Vector to append to this one.
     * @return a new vector.
     */
    public ArrayRealVector append(ArrayRealVector v) {
        return new ArrayRealVector(this, v);
    }

   
    public override RealVector append(double @in) {
         double[] @out = new double[data.Length + 1];
        Array.Copy(data, 0, @out, 0, data.Length);
        @out[data.Length] = @in;
        return new ArrayRealVector(@out, false);
    }

    public override RealVector getSubVector(int index, int n) {
        if (n < 0) {
            throw new Exception("NotPositiveException");
        }
        ArrayRealVector @out = new ArrayRealVector(n);
        try {
            Array.Copy(data, index, @out.data, 0, n);
        } catch (IndexOutOfRangeException e) {
            checkIndex(index);
            checkIndex(index + n - 1);
        }
        return @out;
    }

 
    public override void setEntry(int index, double value)  {
        try {
            data[index] = value;
        } catch (IndexOutOfRangeException e) {
            checkIndex(index);
        }
    }


    public override void addToEntry(int index, double increment) {
        try {
        data[index] += increment;
        } catch(IndexOutOfRangeException e){
            throw new Exception("OutOfRangeException");
        }
    }

    
    public override void setSubVector(int index, RealVector v){
        if (v is ArrayRealVector) {
            setSubVector(index, ((ArrayRealVector) v).data);
        } else {
            try {
                for (int i = index; i < index + v.getDimension(); ++i) {
                    data[i] = v.getEntry(i - index);
                }
            } catch (IndexOutOfRangeException e) {
                checkIndex(index);
                checkIndex(index + v.getDimension() - 1);
            }
        }
    }

    /**
     * Set a set of consecutive elements.
     *
     * @param index Index of first element to be set.
     * @param v Vector containing the values to set.
     * @throws OutOfRangeException if the index is inconsistent with the vector
     * size.
     */
    public void setSubVector(int index, double[] v){
        try {
            Array.Copy(v, 0, data, index, v.Length);
        } catch (IndexOutOfRangeException e) {
            checkIndex(index);
            checkIndex(index + v.Length - 1);
        }
    }


    public override void set(double value) {
        Arrays.Fill(data, value);
    }


    public override double[] toArray(){
        return data.Clone() as double[];
    }

 
    public override String ToString()
    {
        return string.Empty; //DEFAULT_FORMAT.format(this);
    }

    /**
     * Check if instance and specified vectors have the same dimension.
     *
     * @param v Vector to compare instance with.
     * @throws DimensionMismatchException if the vectors do not
     * have the same dimension.
     */

    protected override void checkVectorDimensions(RealVector v) {
        checkVectorDimensions(v.getDimension());
    }

    /**
     * Check if instance dimension is equal to some expected value.
     *
     * @param n Expected dimension.
     * @throws DimensionMismatchException if the dimension is
     * inconsistent with vector size.
     */

    protected override void checkVectorDimensions(int n) {
        if (data.Length != n) {
            throw new Exception("DimensionMismatchException");
        }
    }

    /**
     * Check if any coordinate of this vector is {@code NaN}.
     *
     * @return {@code true} if any coordinate of this vector is {@code NaN},
     * {@code false} otherwise.
     */

    public override bool isNaN() {
        foreach (double v in data) {
            if (Double.IsNaN(v)) {
                return true;
            }
        }
        return false;
    }

    /**
     * Check whether any coordinate of this vector is infinite and none
     * are {@code NaN}.
     *
     * @return {@code true} if any coordinate of this vector is infinite and
     * none are {@code NaN}, {@code false} otherwise.
     */

    public override bool isInfinite() {
        if (isNaN()) {
            return false;
        }

        foreach (double v in data) {
            if (Double.IsInfinity(v)) {
                return true;
            }
        }

        return false;
    }

   
    public override bool Equals(Object other) {
        if (this == other) {
            return true;
        }

        if (!(other is RealVector)) {
            return false;
        }

        RealVector rhs = (RealVector) other;
        if (data.Length != rhs.getDimension()) {
            return false;
        }

        if (rhs.isNaN()) {
            return isNaN();
        }

        for (int i = 0; i < data.Length; ++i) {
            if (data[i] != rhs.getEntry(i)) {
                return false;
            }
        }
        return true;
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        throw new NotImplementedException();
    }

    /**
     * {@inheritDoc} All {@code NaN} values have the same hash code.
     */
    
    public override int GetHashCode() {
        if (isNaN()) {
            return 9;
        }
        return MathUtils.hash(data);
    }

    /** {@inheritDoc} */

    public new ArrayRealVector combine(double a, double b, RealVector y) {
        return copy().combineToSelf(a, b, y) as ArrayRealVector;
    }


    public new ArrayRealVector combineToSelf(double a, double b, RealVector y) {
        if (y is ArrayRealVector) {
             double[] yData = ((ArrayRealVector) y).data;
            checkVectorDimensions(yData.Length);
            for (int i = 0; i < data.Length; i++) {
                data[i] = a * data[i] + b * yData[i];
            }
        } else {
            checkVectorDimensions(y);
            for (int i = 0; i < data.Length; i++) {
                data[i] = a * data[i] + b * y.getEntry(i);
            }
        }
        return this;
    }


    public override double walkInDefaultOrder(RealVectorPreservingVisitor visitor) {
        visitor.start(data.Length, 0, data.Length - 1);
        for (int i = 0; i < data.Length; i++) {
            visitor.visit(i, data[i]);
        }
        return visitor.end();
    }


    public override double walkInDefaultOrder( RealVectorPreservingVisitor visitor,
         int start,  int end)  {
        checkIndices(start, end);
        visitor.start(data.Length, start, end);
        for (int i = start; i <= end; i++) {
            visitor.visit(i, data[i]);
        }
        return visitor.end();
    }

    /**
     * {@inheritDoc}
     *
     * In this implementation, the optimized order is the default order.
     */

    public override double walkInOptimizedOrder( RealVectorPreservingVisitor visitor) {
        return walkInDefaultOrder(visitor);
    }

    /**
     * {@inheritDoc}
     *
     * In this implementation, the optimized order is the default order.
     */

    public override double walkInOptimizedOrder( RealVectorPreservingVisitor visitor,
         int start,  int end)  {
        return walkInDefaultOrder(visitor, start, end);
    }

  
    public override double walkInDefaultOrder( RealVectorChangingVisitor visitor) {
        visitor.start(data.Length, 0, data.Length - 1);
        for (int i = 0; i < data.Length; i++) {
            data[i] = visitor.visit(i, data[i]);
        }
        return visitor.end();
    }


    public override double walkInDefaultOrder( RealVectorChangingVisitor visitor,
         int start,  int end)  {
        checkIndices(start, end);
        visitor.start(data.Length, start, end);
        for (int i = start; i <= end; i++) {
            data[i] = visitor.visit(i, data[i]);
        }
        return visitor.end();
    }

    /**
     * {@inheritDoc}
     *
     * In this implementation, the optimized order is the default order.
     */

    public override double walkInOptimizedOrder( RealVectorChangingVisitor visitor) {
        return walkInDefaultOrder(visitor);
    }

    /**
     * {@inheritDoc}
     *
     * In this implementation, the optimized order is the default order.
     */

    public override double walkInOptimizedOrder( RealVectorChangingVisitor visitor,
         int start,  int end)  {
        return walkInDefaultOrder(visitor, start, end);
    }
}
}
