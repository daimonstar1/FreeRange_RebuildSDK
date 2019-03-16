using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FRG.Core {

    public interface INetFSM {
        bool IsEntered();
        bool ShowDebugState();
        int GetStateIndex();
        string GetStateName(int index);
        void SetPuppetMode(bool puppet);
        void ForcePuppetState(int index);
        void SubscribeStateChangeDelegate(Action<INetFSM, int> StateChangeAction);
        void UnsubscribeStateChangeDelegate(Action<INetFSM, int> StateChangeAction);
    }

    /// <summary>
    /// Simple Hierarchical FSM
    /// MonoState that can manage sub-states with encapsulated transition rules
    /// States defined by generic enumeration type
    /// 
    /// Uses reflection to find state specific methods and spawners
    ///      
    ///     spawner_STATENAME poolobject spawner used to spawn sub state's prefab
    ///     Init_STATENAME called right after spawning (useful for handing params to sub state before it Enters
    ///     Enter_STATENAME called after spawn and init
    ///     Refresh_STATENAME called framewise to refresh sub state, accepts delta time parameter
    ///     (removed) FixedRefresh_STATENAME called at fixed framerate to refresh sub state, accepts delta time parameter
    ///     Exit_STATENAME called at end of sub state's lifespan, BEFORE it has been despawned (good time to retrieve data from sub-state)
    ///     
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoMachine<T> : MonoState, INetFSM where T : struct, IComparable, IConvertible, IFormattable {

        #region NESTED DEFINITIONS

        public class StateReflectionInfo {
            public MethodInfo initMethod = null;
            public MethodInfo enterMethod = null;
            public MethodInfo refreshMethod = null;
            public MethodInfo lateRefreshMethod = null;
            //public MethodInfo fixedRefreshMethod = null;
            public MethodInfo exitMethod = null;
            public FieldInfo stateField = null;
            public FieldInfo spawnerField = null;

            public StateReflectionInfo(object machine, T state) {
                Type machineType = machine.GetType();
                string initMethodName = METHOD_PREFIX_INIT + state.ToString();
                string enterMethodName = METHOD_PREFIX_ENTER + state.ToString();
                string refreshMethodName = METHOD_PREFIX_REFRESH + state.ToString();
                string lateRefreshMethodName = METHOD_PREFIX_LATE_REFRESH + state.ToString();
                //string fixedRefreshMethodName = METHOD_PREFIX_FIXED_REFRESH + state.ToString();
                string exitMethodName = METHOD_PREFIX_EXIT + state.ToString();
                string monoStateName = STATE_PREFIX + state.ToString();
                string spawnerName = SPAWNER_PREFIX + state.ToString();

                initMethod = machineType.GetMethod(initMethodName, bindingFlags);
                enterMethod = machineType.GetMethod(enterMethodName, bindingFlags);
                refreshMethod = machineType.GetMethod(refreshMethodName, bindingFlags);
                lateRefreshMethod = machineType.GetMethod(lateRefreshMethodName, bindingFlags);
                //fixedRefreshMethod = machineType.GetMethod(fixedRefreshMethodName, bindingFlags);
                exitMethod = machineType.GetMethod(exitMethodName, bindingFlags);
                stateField = machineType.GetField(monoStateName, bindingFlags);
                spawnerField = machineType.GetField(spawnerName, bindingFlags);
            }
        }

        #endregion

        #region SERIALIZED FIELDS

        [SerializeField] bool transitionInLateRefresh = false;

        #endregion

        #region PROTECTED FIELDS

        protected Dictionary<T, int> StateIndices = new Dictionary<T, int>(EnumEqualityComparer<T>.Default);
        protected Dictionary<int, T> IndexToState = new Dictionary<int, T>(EnumEqualityComparer<int>.Default);
        protected Dictionary<T, Action> InitMethods = new Dictionary<T, Action>(EnumEqualityComparer<T>.Default);
        protected Dictionary<T, Action> EnterMethods = new Dictionary<T, Action>(EnumEqualityComparer<T>.Default);
        protected Dictionary<T, Action<float>> RefreshMethods = new Dictionary<T, Action<float>>(EnumEqualityComparer<T>.Default);
        protected Dictionary<T, Action<float>> LateRefreshMethods = new Dictionary<T, Action<float>>(EnumEqualityComparer<T>.Default);
        //protected Dictionary<T, Action<float>> FixedRefreshMethods = new Dictionary<T, Action<float>>(EnumEqualityComparer<T>.Default);
        protected Dictionary<T, Action> ExitMethods = new Dictionary<T, Action>(EnumEqualityComparer<T>.Default);
        protected Dictionary<T, MonoState> MonoStates = new Dictionary<T, MonoState>(EnumEqualityComparer<T>.Default);
        protected Dictionary<T, PoolObjectSpawner> StateSpawners = new Dictionary<T, PoolObjectSpawner>(EnumEqualityComparer<T>.Default);

        protected MonoState monoState = null;

        protected static BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy;

        protected const string METHOD_PREFIX_INIT = "Init_";
        protected const string METHOD_PREFIX_ENTER = "Enter_";
        protected const string METHOD_PREFIX_REFRESH = "Refresh_";
        protected const string METHOD_PREFIX_LATE_REFRESH = "LateRefresh_";
        //protected const string METHOD_PREFIX_FIXED_REFRESH = "FixedRefresh_";
        protected const string METHOD_PREFIX_EXIT = "Exit_";
        protected const string STATE_PREFIX = "state_";
        protected const string SPAWNER_PREFIX = "spawner_";

        //this is a double dictionary to handle cases where multiple machines use the same state enumeration type
        protected static Dictionary<Type, Dictionary<T, StateReflectionInfo>> cachedStateReflectionInfo = new Dictionary<Type, Dictionary<T, StateReflectionInfo>>();

        //so useful, I want it here
        protected bool stateDebounce;

        protected T puppetState = default(T);

        #endregion

        #region PUBLIC FIELDS

        public T State { get; private set; }
        public T LastState { get; private set; }
        public T LastFrameState { get; private set; }
        public T NextState { get; private set; }

        public bool Puppet { get; protected set; }

        public float TimeInState { get; private set; }

        public virtual int StateIndex {
            get {

                //this should not happen unless called before Ready
                if(!StateIndices.ContainsKey(State)) {

                    Enum test = Enum.Parse(typeof(T), State.ToString()) as Enum;

                    int index = Convert.ToInt32(test);

                    StateIndices[State] = index;
                    IndexToState[index] = State;
                }

                return StateIndices[State];
            }
        }

        public bool StateCancelled {
            get {
                return monoState != null && monoState.Cancelled;
            }
        }

        public bool StateFinished {
            get {
                return monoState != null && monoState.Finished;
            }
        }

        public int StateChoice {
            get {
                return monoState != null ? monoState.Choice : -1;
            }
        }

        public event Action<INetFSM, int> SendStateChangeToNetwork = null;

        public event Action<T> StateChanged = null;

        #endregion

        #region LIFE SPAN

        #endregion

        #region STATE METHODS

        public override void Enter() {
            base.Enter();

            State = Puppet ? puppetState : GetStartState();
            LastState = State;
            NextState = State;
            LastFrameState = State;

            if(ShowDebugLogs) {
                Debug.Log(DebugPrefixString + "Enter StartState(" + State + ")", this);
            }

            EnterState();

            if(StateChanged != null) StateChanged(State);
            if(SendStateChangeToNetwork != null) SendStateChangeToNetwork(this, StateIndex);

            ClearFramewiseInputs();
            ClearStatewiseInputs();
            RefreshMachine(0f);
        }

        public override void Refresh(float delta) {
            if(!Entered) return;

            base.Refresh(delta);

            using(ProfileUtil.PushSample("MonoMachine.Refresh")) {
                RefreshMachine(delta);
            }
        }

        public override void LateRefresh(float delta) {
            if(!Entered) return;

            base.LateRefresh(delta);

            using(ProfileUtil.PushSample("MonoMachine.LateRefreshMachine")) {
                LateRefreshMachine(delta);
            }
        }

        //public override void FixedRefresh(float delta) {
        //    if(!Entered) return;

        //    base.FixedRefresh(delta);

        //    using(ProfileUtil.PushSample("MonoMachine.FixedRefreshMachine")) {
        //        FixedRefreshMachine(delta);
        //    }
        //}

        public override void Exit() {
            if(!Entered) return;

            ExitState();

            base.Exit();
        }

        #endregion

        #region MACHINE METHODS

        protected virtual T GetStartState() {
            return default(T);
        }

        protected virtual void RefreshMachine(float delta) {

            bool transition = false;

            if(!transitionInLateRefresh) {

                RefreshInputs(delta);

                LastFrameState = State;

                if(Puppet) {
                    transition = PuppetStateTransition();
                }
                else {
                    transition = StateTransition();
                }

                //clear inputs here so states can set them if they like
                ClearFramewiseInputs();
            }

            if(!transition) {
                RefreshState(delta);
            }
        }

        protected virtual void LateRefreshMachine(float delta) {

            bool transition = false;

            if(transitionInLateRefresh) {

                RefreshInputs(delta);

                LastFrameState = State;

                if(Puppet) {
                    transition = PuppetStateTransition();
                }
                else {
                    transition = StateTransition();
                }

                //clear inputs here so states can set them if they like
                ClearFramewiseInputs();
            }

            if(!transition) {
                LateRefreshState(delta);
            }
        }

        //protected virtual void FixedRefreshMachine(float delta) {
        //    FixedRefreshState(delta);
        //}

        protected virtual void RefreshInputs(float delta) { }

        protected virtual bool StateTransition() {
            return ChangeState(State);
        }

        protected virtual bool PuppetStateTransition() {
            return ChangeState(puppetState);
        }

        protected virtual bool ChangeState(T newState, bool allowReentry = false) {
            if(!allowReentry && EnumEqualityComparer<T>.Default.Equals(State, newState)) return false;

            //if ( ShowDebugLogs && !string.IsNullOrEmpty( debugChannel ) ) {
            //    SLC.Log( DebugPrefixString + "ChangeState(" + State + "->" + newState + ") TimeInState("+TimeInState+")", debugChannel, LogType.Log );
            //}
            if(ShowDebugLogs) {
                Debug.Log(DebugPrefixString + "ChangeState(" + State + "->" + newState + ") TimeInState(" + TimeInState + ")", this);
            }

            NextState = newState;

            ExitState();

            LastState = State;
            State = NextState;

            EnterState();

            if(StateChanged != null) StateChanged(State);
            if(SendStateChangeToNetwork != null) SendStateChangeToNetwork(this, StateIndex);

            return true;
        }

        protected virtual void ClearFramewiseInputs() {
        }

        protected virtual void ClearStatewiseInputs() {
        }

        protected virtual void EnterState() {
            ClearStatewiseInputs();

            TimeInState = 0f;

            monoState = null;

            //for now only allowing either a direct state ref, or a spawned one
            if(MonoStates.ContainsKey(State)) {
                monoState = MonoStates[State];
                //Debug.Log("MonoMachine("+gameObject.name+") monoState = MonoStates[" + State.ToString() + "]");
                
            }
            else if(StateSpawners.ContainsKey(State)) {
                if(StateSpawners[State].ElementPrefab != null) {

                    PoolObject pObject = StateSpawners[State].Spawn();
                    if(pObject != null) {
                        monoState = pObject.gameObject.GetComponent<MonoState>();
                    }
                    else {
                        Debug.LogWarning("StateSpawners[" + State.ToString() + "] failed spawn attempt!");
                    }

                }
                else {
                    Debug.LogWarning("StateSpawners[" + State.ToString() + "] Spawner has no prefab!");
                }
            }

            //init invokes after substate spawn, but before Enter
            if(InitMethods.ContainsKey(State) && InitMethods[State] != null) {
                //Debug.LogWarning("InitMethods[" + State.ToString() + "] invoked!");
                InitMethods[State]();
            }

            if(monoState != null) {
                monoState.Enter();
            }

            if(EnterMethods.ContainsKey(State) && EnterMethods[State] != null) {
                EnterMethods[State]();
            }

            stateDebounce = false;
        }

        protected virtual void RefreshState(float delta) {
            TimeInState += delta;

            if(monoState != null) monoState.Refresh(delta);

            Action<float> action = null;
            if(RefreshMethods.TryGetValue(State, out action)) {
                if(action != null) {
                    action(delta);
                }
            }
        }

        protected virtual void LateRefreshState(float delta) {
            if(monoState != null) monoState.LateRefresh(delta);

            Action<float> action = null;
            if(LateRefreshMethods.TryGetValue(State, out action)) {
                if(action != null) {
                    action(delta);
                }
            }
        }

        //protected virtual void FixedRefreshState(float delta) {
        //    if(monoState != null) monoState.FixedRefresh(delta);

        //    Action<float> action = null;
        //    if(FixedRefreshMethods.TryGetValue(State, out action)) {
        //        if(action != null) {
        //            action(delta);
        //        }
        //    }
        //}

        protected virtual void ExitState() {

            if(ExitMethods.ContainsKey(State) && ExitMethods[State] != null) {
                ExitMethods[State]();
            }

            if(monoState != null) {
                monoState.Exit();
            }

            if(StateSpawners.ContainsKey(State)) {
                StateSpawners[State].Despawn();
            }

            //Debug.Log("MonoMachine("+gameObject.name+") ExitState monoState = null");
            monoState = null;
        }

        #endregion

        #region MISC PROTECTED METHODS

        protected override void Ready() {
            if(ready) return;

            MapStateToIndex();

            base.Ready();

            // use reflection once to acquire relevant sub state methods and spawners
            //#if UNITY_EDITOR
            //            if(ShowDebugLogs) Debug.Log("frame("+Time.frameCount+") MonoMachine("+gameObject.name+") Ready");
            //#endif

            Type myType = this.GetType();
            if(!cachedStateReflectionInfo.ContainsKey(myType)) {
                cachedStateReflectionInfo[myType] = new Dictionary<T, StateReflectionInfo>();
            }

            Dictionary<T, StateReflectionInfo> myReflectionCache = cachedStateReflectionInfo[myType];

            //use reflection to get all my methods
            Type stateType = typeof(T);

            foreach(T state in Enum.GetValues(stateType)) {

                if(!myReflectionCache.ContainsKey(state)) {
                    //only concat these strings and reflection look ups once for the machine class
                    myReflectionCache[state] = new StateReflectionInfo(this, state);
                }

                if(myReflectionCache[state].initMethod != null) {
#if UNITY_EDITOR
                    if(debug) Debug.Log("frame(" + Time.frameCount + ") MonoState(" + gameObject.name + ") Ready located InitMethods for state(" + state + "), creating delegate...");
#endif

                    InitMethods[state] = (Action)Delegate.CreateDelegate(typeof(Action), this, myReflectionCache[state].initMethod);
                }

                if(myReflectionCache[state].enterMethod != null) {
#if UNITY_EDITOR
                    if(debug) Debug.Log("frame(" + Time.frameCount + ") MonoState(" + gameObject.name + ") Ready located EnterMethod for state(" + state + "), creating delegate...");
#endif
                    EnterMethods[state] = (Action)Delegate.CreateDelegate(typeof(Action), this, myReflectionCache[state].enterMethod);
                }

                if(myReflectionCache[state].refreshMethod != null) {
                    ParameterInfo[] parameters = myReflectionCache[state].refreshMethod.GetParameters();
                    if(parameters != null && parameters.Length == 1 && parameters[0].ParameterType == typeof(float)) {
#if UNITY_EDITOR
                        if(debug) Debug.Log("frame(" + Time.frameCount + ") MonoMachine(" + gameObject.name + ") Ready located RefreshMethod for state(" + state + "), creating delegate...");
#endif

                        RefreshMethods[state] = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), this, myReflectionCache[state].refreshMethod);
                    }
#if UNITY_EDITOR
                    else {
                        Debug.LogError("frame(" + Time.frameCount + ") MonoMachine(" + gameObject.name + ") Ready located Refresh for state(" + state + "), but its parameters are incorrect!", this);
                    }
#endif
                }

                if(myReflectionCache[state].lateRefreshMethod != null) {
                    ParameterInfo[] parameters = myReflectionCache[state].lateRefreshMethod.GetParameters();
                    if(parameters != null && parameters.Length == 1 && parameters[0].ParameterType == typeof(float)) {
#if UNITY_EDITOR
                        if(debug) Debug.Log("frame(" + Time.frameCount + ") MonoMachine(" + gameObject.name + ") Ready located UpdateMethod for state(" + state + "), creating delegate...");
#endif

                        LateRefreshMethods[state] = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), this, myReflectionCache[state].lateRefreshMethod);
                    }
#if UNITY_EDITOR
                    else {
                        Debug.LogError("frame(" + Time.frameCount + ") MonoMachine(" + gameObject.name + ") Ready located LateRefresh for state(" + state + "), but its parameters are incorrect!", this);
                    }
#endif
                }

                //                if(myReflectionCache[state].fixedRefreshMethod != null) {
                //                    ParameterInfo[] parameters = myReflectionCache[state].fixedRefreshMethod.GetParameters();
                //                    if(parameters != null && parameters.Length == 1 && parameters[0].ParameterType == typeof(float)) {
                //#if UNITY_EDITOR
                //                        if(debug) Debug.Log("frame(" + Time.frameCount + ") MonoMachine(" + gameObject.name + ") Ready located FixedRefresh method for state(" + state + "), creating delegate...");
                //#endif

                //                        FixedRefreshMethods[state] = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), this, myReflectionCache[state].fixedRefreshMethod);
                //                    }
                //#if UNITY_EDITOR
                //                    else {
                //                        Debug.LogError("frame(" + Time.frameCount + ") MonoMachine(" + gameObject.name + ") Ready located FixedRefresh for state(" + state + "), but its parameters are incorrect!", this);
                //                    }
                //#endif
                //                }

                if(myReflectionCache[state].exitMethod != null) {
#if UNITY_EDITOR
                    if(debug) Debug.Log("frame(" + Time.frameCount + ") MonoState(" + gameObject.name + ") Ready located ExitMethod for state(" + state + "), creating delegate...");
#endif

                    ExitMethods[state] = (Action)Delegate.CreateDelegate(typeof(Action), this, myReflectionCache[state].exitMethod);
                }

                if(myReflectionCache[state].stateField != null) {
                    if(myReflectionCache[state].stateField.FieldType == typeof(MonoState)) {
                        MonoState mState = myReflectionCache[state].stateField.GetValue(this) as MonoState;
                        if(mState != null) {

                            if(mState.gameObject.scene.rootCount == 0) {
                                mState = GameObject.Instantiate(mState.gameObject, transform).GetComponent<MonoState>();
                                myReflectionCache[state].stateField.SetValue(this, mState);
                                //Debug.Log("frame(" + Time.frameCount + ") MonoState(" + gameObject.name + ") Ready spawned MonoState prefab for state(" + state + ")");
                            }

                            MonoStates[state] = mState;
#if UNITY_EDITOR
                            if(debug) Debug.Log("frame(" + Time.frameCount + ") MonoState(" + gameObject.name + ") Ready located MonoState for state(" + state + ")");
#endif
                        }
                    }
                }

                if(myReflectionCache[state].spawnerField != null) {
                    if(myReflectionCache[state].spawnerField.FieldType == typeof(PoolObjectSpawner)) {
                        PoolObjectSpawner spawner = myReflectionCache[state].spawnerField.GetValue(this) as PoolObjectSpawner;
                        if(spawner != null) {
                            StateSpawners[state] = spawner;
#if UNITY_EDITOR
                            if(debug) Debug.Log("frame(" + Time.frameCount + ") MonoState(" + gameObject.name + ") Ready located PoolObjectSpawner for state(" + state + ")");
#endif
                        }
                    }
                }

            }
        }

        protected virtual void MapStateToIndex() {
            foreach(T state in (T[])Enum.GetValues(typeof(T))) {
                Enum test = Enum.Parse(typeof(T), state.ToString()) as Enum;
                int index = Convert.ToInt32(test);

                //Debug.Log("frame(" + Time.frameCount + ") MonoState(" + gameObject.name + ") MapStateToIndex state(" + state + ") index("+index+")");

                StateIndices[state] = index;
                IndexToState[index] = state;
            }
        }

        #endregion

        #region INetFSM interface methods

        public int GetStateIndex() {
            return StateIndex;
        }

        public string GetStateName(int index) {
            return IndexToState[index].ToString();
        }

        public void SetPuppetMode(bool puppet) {
            Puppet = puppet;
            puppetState = GetStartState();
        }

        public void ForcePuppetState(int index) {
            //Debug.Log("frame("+Time.frameCount+") MonoMachine("+gameObject.name+") ForcePuppetState state("+index+")");

            if(!IndexToState.ContainsKey(index)) return;

            puppetState = IndexToState[index];
            //Debug.Log("frame("+Time.frameCount+") MonoMachine("+gameObject.name+") ForcePuppetState PuppetState("+PuppetState+")");

            //dmd2bu i believe this is safe and best, but let's be aware of this when looking for syncing bugs
            if(Entered) Refresh(0f);
        }

        public void SubscribeStateChangeDelegate(Action<INetFSM, int> StateChangeAction) {
            SendStateChangeToNetwork += StateChangeAction;
        }

        public void UnsubscribeStateChangeDelegate(Action<INetFSM, int> StateChangeAction) {
            SendStateChangeToNetwork -= StateChangeAction;
        }

        public virtual bool IsEntered() {
            return Entered;
        }

        public virtual bool ShowDebugState() {
            return Entered;
        }

        #endregion

    }
}