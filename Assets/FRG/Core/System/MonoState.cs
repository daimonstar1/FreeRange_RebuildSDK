using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FRG.Core {
    public class MonoState : PoolBehaviour, IState {

        #region SERIALIZED FIELDS

        [SerializeField] protected bool debug = false;
        [Tooltip("This state is isolated and not managed by FSM. Entered on spawn/enable and exited on despawn/disable.")]
        [SerializeField] protected bool selfManaged = true;
        [SerializeField] protected bool refreshWhenPaused = false;

        #endregion

        #region PROTECTED FIELDS

        protected bool ready = false;
        protected int choice = -1;
        protected bool poolSpawned = false;
        protected string debugChannel = null;

        protected virtual bool ShowDebugLogs {
            get {
                if(PreseedManager.IsPreseeding) return false;

                return debug;
            }
        }

        protected string DebugPrefixString {
            get {
                return "frame("+Time.frameCount+") time("+Time.time+") real("+Time.realtimeSinceStartup+") "+this.GetType().ToString() + "("+gameObject.name+") ";
            }
        }

        bool selfManagedOverride = false; //this let's us set data on a spawned state before it runs its Enter
        public bool SelfManaged {
            get {
                if(selfManagedOverride) return true;
                return selfManaged;
            }
            set {
                selfManagedOverride = value;
            }
        }
        static int lastFrameChecked;
        static bool lastShuttingDown;
        protected static bool IsShuttingDown {
            get {
                if ( lastFrameChecked != Time.frameCount ) {
                    lastFrameChecked = Time.frameCount;
                    lastShuttingDown = FocusHandler.IsShuttingDown;
                }
                return lastShuttingDown;
            }
        }

        #endregion

        #region PUBLIC FIELDS

        public bool Entered { get; protected set; }

        public bool HasRefreshedOnce { get; protected set; }

        public virtual bool Finished { get; protected set; }

        public virtual bool Cancelled { get; protected set; }

        public virtual int Choice {
            get {
                return choice;
            }
            protected set {
                choice = value;
            }
        }

        public float StateTime { get; protected set; }

        #endregion

        #region MONO CALLBACKS

        protected virtual void Awake() {
            Ready();
        }

        protected void OnEnable() {
            if(!poolSpawned && SelfManaged && !Entered) {
                Enter();
            }
        }

        protected virtual void Update() {
            if(IsShuttingDown) return;

            if(Entered && SelfManaged) {
                if(refreshWhenPaused || Time.timeScale > 0f) {
                    Refresh(Time.deltaTime);
                }
            }
        }

        protected virtual void LateUpdate() {
            if(IsShuttingDown) return;

            if(Entered && SelfManaged) LateRefresh(Time.deltaTime);
        }

        //protected virtual void FixedUpdate() {
        //    if(FocusHandler.IsShuttingDown) return;

        //    if(Entered && SelfManaged) FixedRefresh(Time.fixedDeltaTime);
        //}

        protected void OnDisable() {
#if UNITY_EDITOR
            if( IsShuttingDown ) return;
#endif
            if(!poolSpawned && SelfManaged && Entered) {
                Exit();
            }
        }

#endregion

#region POOLBEHAVIOUR METHODS

        public override void OnSpawn() {
            Ready();
            selfManagedOverride = false;

            Finished = false;
            Cancelled = false;
            Choice = -1;

            base.OnSpawn();

            if(IsShuttingDown) return;

            if(SelfManaged && !Entered) Enter();
        }

        public override void OnDespawn() {
            if(IsShuttingDown) return;

            if(Entered && SelfManaged) {
                Exit();
            }

            base.OnDespawn();
        }

        public override void DespawnAfterDelay(float delay) {
            base.DespawnAfterDelay(delay);
        }

#endregion

#region MISC PROTECTED METHODS

        protected virtual void Ready() {
            if(ready) return;

            //not sure how sure we are of this assumption
            poolSpawned = GetComponent<PoolObject>() != null;

            ready = true;
        }

#endregion

#region PUBLIC METHODS

        public virtual void Enter() {
            //if(ShowDebugLogs) SLC.Log( DebugPrefixString + "Enter", debugChannel, LogType.Log );
            if(ShowDebugLogs) Debug.Log( DebugPrefixString + "Enter", this );

            Ready();

            Entered = true;
            HasRefreshedOnce = false;

            Finished = false;
            Cancelled = false;
            choice = -1;
            StateTime = 0f;
        }

        public virtual void Refresh(float delta) {
            //if(debug && logger.IsInfoEnabled) logger.Info("frame("+Time.frameCount+") MonoState("+gameObject.name+") Refresh");
            StateTime += delta;
            HasRefreshedOnce = true;
        }

        public virtual void LateRefresh(float delta) {
            //if(debug && logger.IsInfoEnabled) logger.Info("frame("+Time.frameCount+") MonoState("+gameObject.name+") LateRefresh");
        }

        //public virtual void FixedRefresh(float delta) {
        //    //if(debug && logger.IsInfoEnabled) logger.Info("frame("+Time.frameCount+") MonoState("+gameObject.name+") Refresh");
        //}

        public virtual void Exit() {
            if(!Entered) return;

            //if(ShowDebugLogs) SLC.Log( DebugPrefixString + "Exit", debugChannel, LogType.Log );
            if(ShowDebugLogs) Debug.Log( DebugPrefixString + "Exit", this );

            Entered = false;
        }

        public virtual void Choose(int newChoice) {
            //if(ShowDebugLogs) SLC.Log( DebugPrefixString + "Choose("+newChoice+")", debugChannel, LogType.Log );
            if(ShowDebugLogs) Debug.Log( DebugPrefixString + "Choose("+newChoice+")", this );

            Choice = newChoice;
        }

        public virtual void Cancel() {
            //if(ShowDebugLogs) SLC.Log( DebugPrefixString + "Cancel()", debugChannel, LogType.Log );
            if(ShowDebugLogs) Debug.Log( DebugPrefixString + "Cancel()", this );

            Cancelled = true;
        }

        public virtual void Finish() {
            if(ShowDebugLogs) Debug.Log(DebugPrefixString + "Finish()", this);

            Finished = true;
        }

#endregion

    }

}