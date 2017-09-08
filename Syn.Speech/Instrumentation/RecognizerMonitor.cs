using System;
using System.Collections.Generic;
using Syn.Speech.Helper;
using Syn.Speech.Recognizers;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Instrumentation
{
    /// <summary>
    /// Monitor the state transitions of a given recognizer. This monitor maintains lists of components that should be 'run' when a recognizer state change is detected.
    /// </summary>
    public class RecognizerMonitor : IStateListener, IMonitor
    {

        /// <summary>
        /// The property for the recognizer to monitor
        /// </summary>
        [S4Component(Type = typeof(Recognizer))]
        public const String PropRecognizer = "recognizer";

        /// <summary>
        /// The property that defines all of the monitors to call when the recognizer is allocated
        /// </summary>
        [S4ComponentList(Type = typeof(IConfigurable))]
        public const String PropAllocatedMonitors = "allocatedMonitors";

        /// <summary>
        /// The property that defines all of the monitors to call when the recognizer is deallocated
        /// </summary>
        [S4ComponentList(Type = typeof(IConfigurable))]
        public const String PropDeallocatedMonitors = "deallocatedMonitors";

        // --------------------------
        // Configuration data
        // --------------------------
        Recognizer _recognizer;
        List<IRunnable> _allocatedMonitors;
        List<IRunnable> _deallocatedMonitors;
        String _name;

        public RecognizerMonitor(Recognizer recognizer, List<IRunnable> allocatedMonitors, List<IRunnable> deallocatedMonitors)
        {
            InitRecognizer(recognizer);
            this._allocatedMonitors = allocatedMonitors;
            this._deallocatedMonitors = deallocatedMonitors;
        }

        public RecognizerMonitor()
        {
        }


        public void NewProperties(PropertySheet ps)
        {
            InitRecognizer((Recognizer)ps.GetComponent(PropRecognizer));
            _allocatedMonitors = ps.GetComponentList<IRunnable>(PropAllocatedMonitors);
            _deallocatedMonitors = ps.GetComponentList<IRunnable>(PropDeallocatedMonitors);
        }

        private void InitRecognizer(Recognizer newRecognizer)
        {
            if (_recognizer == null)
            {
                _recognizer = newRecognizer;
                _recognizer.AddStateListener(this);
            }
            else if (_recognizer != newRecognizer)
            {
                _recognizer.RemoveStateListener(this);
                _recognizer = newRecognizer;
                _recognizer.AddStateListener(this);
            }
        }

        public void StatusChanged(Recognizer.State status)
        {
            List<IRunnable> runnableList = null;
            if (status == Recognizer.State.Allocated)
            {
                runnableList = _allocatedMonitors;
            }
            else if (status == Recognizer.State.Deallocated)
            {
                runnableList = _deallocatedMonitors;
            }

            if (runnableList != null)
            {
                foreach (IRunnable r in runnableList)
                {
                    r.Run();
                }
            }
        }
    }

}
