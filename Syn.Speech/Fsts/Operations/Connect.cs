using System.Collections.Generic;
using System.Linq;
using Syn.Logging;

//REFACTORED
using Syn.Speech.Fsts.Semirings;

namespace Syn.Speech.Fsts.Operations
{
    /// <summary>
    /// Connect operation.
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class Connect
    {
        /**
    /// Calculates the coaccessible states of an fst
     */
        private static void CalcCoAccessible(Fst fst, State state, List<List<State>> paths, HashSet<State> coaccessible) 
        {
            // hold the coaccessible added in this loop
            List<State> newCoAccessibles = new List<State>();
            foreach (List<State> path in paths) 
            {
                int index = path.LastIndexOf(state);
                if (index != -1) {
                    if (state.FinalWeight != fst.Semiring.Zero
                            || coaccessible.Contains(state)) {
                        for (int j = index; j > -1; j--) {
                            if (!coaccessible.Contains(path[j])) {
                                newCoAccessibles.Add(path[j]);
                                coaccessible.Add(path[j]);
                            }
                        }
                    }
                }
            }

            // run again for the new coaccessibles
            foreach (State s in newCoAccessibles) 
            {
                CalcCoAccessible(fst, s, paths, coaccessible);
            }
        }

        /**
        /// Copies a path
         */
        private static void DuplicatePath(int lastPathIndex, State fromState, State toState, List<List<State>> paths)
        {
            List<State> lastPath = paths[lastPathIndex];
            // copy the last path to a new one, from start to current state
            int fromIndex = lastPath.IndexOf(fromState);
            int toIndex = lastPath.IndexOf(toState);
            if (toIndex == -1)
            {
                toIndex = lastPath.Count - 1;
            }
            List<State> newPath = new List<State>(lastPath.Skip(fromIndex).Take(toIndex-fromIndex));
            paths.Add(newPath);
        }

        /**
        /// The depth first search recursion
         */
        private static State DepthFirstSearchNext(Fst fst, State start,
                List<List<State>> paths, List<Arc>[] exploredArcs, HashSet<State> accessible)
        {
            int lastPathIndex = paths.Count - 1;

            List<Arc> currentExploredArcs = exploredArcs[start.GetId()];
            paths[lastPathIndex].Add(start);
            if (start.GetNumArcs() != 0)
            {
                int arcCount = 0;
                int numArcs = start.GetNumArcs();
                for (int j = 0; j < numArcs; j++)
                {
                    Arc arc = start.GetArc(j);
                    if ((currentExploredArcs == null)
                            || !currentExploredArcs.Contains(arc))
                    {
                        lastPathIndex = paths.Count - 1;
                        if (arcCount++ > 0)
                        {
                            DuplicatePath(lastPathIndex, fst.Start, start,
                                    paths);
                            lastPathIndex = paths.Count - 1;
                            paths[lastPathIndex].Add(start);
                        }
                        State next = arc.NextState;
                        AddExploredArc(start.GetId(), arc, exploredArcs);
                        // detect self loops
                        if (next.GetId() != start.GetId())
                        {
                            DepthFirstSearchNext(fst, next, paths, exploredArcs, accessible);
                        }
                    }
                }
            }
            lastPathIndex = paths.Count - 1;
            accessible.Add(start);

            return start;
        }

        /**
        /// Adds an arc top the explored arcs list
         */
        private static void AddExploredArc(int stateId, Arc arc,List<Arc>[] exploredArcs)
        {
            if (exploredArcs[stateId] == null)
            {
                exploredArcs[stateId] = new List<Arc>();
            }
            exploredArcs[stateId].Add(arc);

        }

        /**
        /// Initialization of a depth first search recursion
         */
        private static void DepthFirstSearch(Fst fst, HashSet<State> accessible, List<List<State>> paths, List<Arc>[] exploredArcs, HashSet<State> coaccessible)
        {
            State currentState = fst.Start;
            State nextState = currentState;
            do
            {
                if (!accessible.Contains(currentState))
                {
                    nextState = DepthFirstSearchNext(fst, currentState, paths, exploredArcs,
                            accessible);
                }
            } while (currentState.GetId() != nextState.GetId());
            int numStates = fst.GetNumStates();
            for (int i = 0; i < numStates; i++)
            {
                State s = fst.GetState(i);
                if (s.FinalWeight != fst.Semiring.Zero)
                {
                    CalcCoAccessible(fst, s, paths, coaccessible);
                }
            }
        }

        /**
        /// Trims an Fst, removing states and arcs that are not on successful paths.
        /// 
        /// @param fst the fst to trim
         */
        public static void Apply(Fst fst) 
        {
            Semiring semiring = fst.Semiring;
            if (semiring == null) 
            {
                Logger.LogInfo<Connect>("Fst has no semiring.");
                return;
            }

            HashSet<State> accessible = new HashSet<State>();
            HashSet<State> coaccessible = new HashSet<State>();
            
            List<Arc>[] exploredArcs = new List<Arc>[fst.GetNumStates()];

            List<List<State>> paths = new List<List<State>>();
            paths.Add(new List<State>());

            DepthFirstSearch(fst, accessible, paths, exploredArcs, coaccessible);

            HashSet<State> toDelete = new HashSet<State>();

            for (int i = 0; i < fst.GetNumStates(); i++) 
            {
                State s = fst.GetState(i);
                if (!(accessible.Contains(s) || coaccessible.Contains(s))) {
                    toDelete.Add(s);
                }
            }

            fst.DeleteStates(toDelete);
        }
    }
}
