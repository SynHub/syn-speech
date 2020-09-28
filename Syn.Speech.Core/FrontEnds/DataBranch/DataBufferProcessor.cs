using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.DataBranch
{
   
/**
 * A FIFO-buffer for <code>Data</code>-elements.
 * <p/>
 * <code>Data</code>s are inserted to the buffer using the <code>processDataFrame</code>-method.
 */
public class DataBufferProcessor : BaseDataProcessor , IDataListener , IConfigurable {

    /** The FIFO- data buffer. */
    private readonly List<IData> _featureBuffer = new List<IData>();

    /**
     * If this property is set <code>true</code> the buffer will wait for new data until it returns from a
     * <code>getData</code>-call. Enable this flag if the buffer should serve as starting point for a new
     * feature-pull-chain..
     */
    [S4Boolean(DefaultValue = false)]
    public const string PropWaitIfEmpty = "waitIfEmpty";
    private bool _waitIfEmpty;

    /**
     * The time in milliseconds which will be waited between two attempts to read a data element from the buffer when
     * being in <code>waitIfEmpty</code>-mode
     */
    [S4Integer(DefaultValue = 10)]
    public const string PropWaitTimeMs = "waitTimeMs";
    private long _waitTime;


    /** The maximal size of the buffer in frames. The oldest frames will be removed if the buffer grows out of bounds. */
    [S4Integer(DefaultValue = 50000)]
    public const string PropBufferSize = "maxBufferSize";
    private int _maxBufferSize;


    [S4ComponentList(Type = typeof(IConfigurable), BeTolerant = true)]
    public const string DataListeners = "dataListeners";
    private List<IDataListener> _dataListeners = new List<IDataListener>();

    /**
     * @param maxBufferSize The maximal size of the buffer in frames. The oldest frames will be removed if the buffer grows out of bounds.
     * @param waitIfEmpty If this property is set <code>true</code> the buffer will wait for new data until it returns from a
     * <code>getData</code>-call. Enable this flag if the buffer should serve as starting point for a new
     * feature-pull-chain.
     * @param waitTime The time in milliseconds which will be waited between two attempts to read a data element from the buffer when
     * being in <code>waitIfEmpty</code>-mode
     * @param listeners
     */
    public DataBufferProcessor(int maxBufferSize, bool waitIfEmpty, int waitTime, IEnumerable<IConfigurable> listeners) {

        _maxBufferSize = maxBufferSize;
        _waitIfEmpty = waitIfEmpty;

        if (waitIfEmpty) // if false we don't need the value
            _waitTime = waitTime;

        foreach (IConfigurable configurable in listeners) {
            Debug.Assert(configurable is IDataListener);
            AddDataListener((IDataListener) configurable);
        }
    }

    public DataBufferProcessor() {
    }


    public override void NewProperties(PropertySheet ps)  {
        base.NewProperties(ps);

        _maxBufferSize = ps.GetInt(PropBufferSize);
        _waitIfEmpty = ps.GetBoolean(PropWaitIfEmpty);

        if (_waitIfEmpty) // if false we don't need the value
            _waitTime = ps.GetInt(PropWaitTimeMs);

        _dataListeners = ps.GetComponentList<IDataListener>(DataListeners);
    }


    public void ProcessDataFrame(IData data) {
        _featureBuffer.Add(data);

        // inform data-listeners if necessary
        foreach (IDataListener dataListener in _dataListeners) {
            dataListener.ProcessDataFrame(data);
        }

        //reduce the buffer-size if necessary
        while (_featureBuffer.Count > _maxBufferSize) {
            _featureBuffer.Remove(0);
        }
    }


    /**
     * Returns the processed Data output.
     *
     * @return an Data object that has been processed by this DataProcessor
     * @throws edu.cmu.sphinx.frontend.DataProcessingException
     *          if a data processor error occurs
     */

    public override IData GetData() {
        IData data = null;

        while (_waitIfEmpty && _featureBuffer.IsEmpty()) {
            try {
                Thread.Sleep((int) _waitTime);
            } catch (ThreadInterruptedException e) {
                e.PrintStackTrace();
            }
        }

        if (!_featureBuffer.IsEmpty()) {
            data = _featureBuffer.Remove(0);
        } else {
            Debug.Assert(!_waitIfEmpty);
        }

        return data;
    }


    public int BufferSize
    {
        get { return _featureBuffer.Count; }
    }


    public void ClearBuffer() {
        _featureBuffer.Clear();
    }


    public List<IData> GetBuffer() {
        return _featureBuffer; //Todo Collections.unmodifiableList(featureBuffer);
    }


    /** Adds a new listener.
     * @param l*/
    public void AddDataListener(IDataListener l) {
        if (l == null)
            return;

        _dataListeners.Add(l);
    }


    /** Removes a listener.
     * @param l*/
    public void RemoveDataListener(IDataListener l) {
        if (l == null)
            return;

        _dataListeners.Remove(l);
    }

}
}
