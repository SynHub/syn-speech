using System;
using System.Collections.Generic;
using Syn.Speech.Decoders;
using Syn.Speech.Helper;
using Syn.Speech.Instrumentation;
using Syn.Speech.Results;
using Syn.Speech.Util.Props;
using IResultListener = Syn.Speech.Decoders.IResultListener;
//PATROLLED + REFACTORED
namespace Syn.Speech.Recognizers
{
    public class Recognizer : IResultProducer
    {
        /// <summary>
        /// The property for the decoder to be used by this recognizer.
        /// </summary>
        [S4Component(Type=typeof(Decoder))]
        public static string PropDecoder = "decoder";

        /// <summary>
        /// The property for the set of monitors for this recognizer 
        /// </summary>
        [S4ComponentList(Type = typeof(IMonitor))]
        public static string PropMonitors = "monitors";
        /// <summary>
        /// Defines the possible states of the recognizer.
        /// </summary>
        public enum State { Deallocated, Allocating, Allocated, Ready, Recognizing, Deallocating, Error };

        private string _name;
        private Decoder _decoder;
        private State _currentState = State.Deallocated;

        private readonly List<IStateListener> _stateListeners = new List<IStateListener>();
        private List<IMonitor> _monitors;

        public Recognizer(Decoder decoder, List<IMonitor> monitors)
        {
            _decoder = decoder;
            _monitors = monitors;
            _name = null;
        }

        public Recognizer()
        {

        }


        public void  NewProperties(PropertySheet propertySheet)
        {
            _decoder = (Decoder)propertySheet.GetComponent(PropDecoder);
            _monitors = propertySheet.GetComponentList<IMonitor>(PropMonitors);

            _name = propertySheet.InstanceName;
        }

        /// <summary>
        /// Performs recognition for the given number of input frames, or until a 'final' result is generated. This method
        /// should only be called when the recognizer is in the <code>allocated</code> state.
        /// </summary>
        /// <param name="referenceText">referenceText what was actually spoken</param>
        /// <returns>a recognition result</returns>
        public Result Recognize(String referenceText)
        {
            Result result;
            CheckState(State.Ready);
            try
            {
                SetState(State.Recognizing);
                result = _decoder.Decode(referenceText);
            }
            finally
            {
                SetState(State.Ready);
            }

            return result;
        }

        /// <summary>
        /// Performs recognition for the given number of input frames, or until a 'final' result is generated. This method
        /// should only be called when the recognizer is in the <code>allocated</code> state.
        /// </summary>
        /// <returns></returns>
        public Result Recognize()
        {
            return Recognize(null);
        }

        /// <summary>
        /// Checks to ensure that the recognizer is in the given state.
        /// </summary>
        /// <param name="desiredState">desiredState the state that the recognizer should be in</param>
        private void CheckState(State desiredState)
        {
            if (_currentState != desiredState)
            {
                throw new IllegalStateException("Expected state " + desiredState
                                                + " actual state " + _currentState);
            }
        }
        /// <summary>
        /// sets the current state
        /// </summary>
        /// <param name="newState"> newState the new state</param>
        private void SetState(State newState)
        {
            _currentState = newState;
            foreach (var item in _stateListeners)
            {
                item.StatusChanged(_currentState);
            }
        }

        /// <summary>
        /// Allocate the resources needed for the recognizer. Note this method make take some time to complete. This method
        /// should only be called when the recognizer is in the deallocated state.
        /// throws IllegalStateException if the recognizer is not in the DEALLOCATED state
        /// </summary>
        public void Allocate()
        {
            CheckState(State.Deallocated);
            SetState(State.Allocating);
            _decoder.Allocate();
            SetState(State.Allocated);
            SetState(State.Ready);
        }
        /// <summary>
        /// Deallocates the recognizer. This method should only be called if the recognizer is in the allocated
        /// state.
        /// throws IllegalStateException if the recognizer is not in the ALLOCATED state
        /// </summary>
        public void Deallocate()
        {
            CheckState(State.Ready);
            SetState(State.Deallocating);
            _decoder.Deallocate();
            SetState(State.Deallocated);
        }
        /// <summary>
        /// Retrieves the recognizer state. This method can be called in any state.
        /// </summary>
        /// <returns></returns>
        public State GetState()
        {
            return _currentState;
        }

        /// <summary>
        /// Resets the monitors monitoring this recognizer
        /// </summary>
        public void ResetMonitors()
        {
            foreach (IMonitor listener in _monitors)
            {
                if (listener is IResetable)
                    ((IResetable)listener).Reset();
            }
        }

        /// <summary>
        /// Adds a result listener to this recognizer. A result listener is called whenever a new result is generated by the
        /// recognizer. This method can be called in any state.
        /// </summary>
        /// <param name="resultListener">resultListener the listener to add</param>
        public void AddResultListener(IResultListener resultListener)
        {
            _decoder.AddResultListener(resultListener);
        }

        /// <summary>
        /// Adds a status listener to this recognizer. The status listener is called whenever the status of the recognizer
        /// changes. This method can be called in any state.
        /// </summary>
        /// <param name="stateListener">stateListener the listener to add</param>
        public void AddStateListener(IStateListener stateListener)
        {
            _stateListeners.Add(stateListener);
        }

        /// <summary>
        /// Removes a previously added result listener. This method can be called in any state.
        /// </summary>
        /// <param name="resultListener">resultListener the listener to remove</param>
        public void RemoveResultListener(IResultListener resultListener)
        {
            _decoder.RemoveResultListener(resultListener);
        }

        /// <summary>
        /// Removes a previously added state listener. This method can be called in any state.
        /// </summary>
        /// <param name="stateListener">stateListener the state listener to remove</param>
        public void RemoveStateListener(IStateListener stateListener)
        {
            _stateListeners.Remove(stateListener);
        }

        public override string ToString()
        {
            return "Recognizer: " + _name + " State: " + _currentState;
        }

    }
}
