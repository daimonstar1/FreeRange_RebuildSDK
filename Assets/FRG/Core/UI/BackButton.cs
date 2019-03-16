using FRG.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LabyrinthUI {
    /// <summary>
    /// A singleton that listens for <see cref="LabyrinthInput.Back"/> and executes action that is put on top of the action stack to handle "going back" in game.
    /// </summary>
    public class BackButton : MonoBehaviour {
        public static BackButton instance { get { return ServiceLocator.ResolveRuntime<BackButton>(); } }

        public bool HasBackAction { get { return ActionStack.Count != 0 && ActionStack.Peek() != null; } }

        Stack<Action> ActionStack = new Stack<Action>();

        /// <summary>
        /// Register an action you want this button to do when it gets activated.
        /// Action is put on a stack so If you want to use the same button prefab for multiple cases you can do that, just register latest action and deregester after you're done with it.
        /// You can pass in null to do nothing (e.g. you want <see cref="LabyrinthInput.Back"/> handled directly)
        /// </summary>
        public void RegisterBackAction(Action action) {
            ActionStack.Push(action);
        }

        /// <summary>
        /// Deregester an action, removes it from top of the stack that will be executed on button press.
        /// </summary>
        /// <param name="action"></param>
        public void DeregisterBackAction(Action action) {
            if(ActionStack.Count > 0) {
                if(ActionStack.Peek() != action) {
                    Debug.LogWarning("You are trying to remove action from back button that is not set by you. See stack trace to trace the object and fix the order of calls.");
                    return;
                }
                ActionStack.Pop();
            }
        }

        public void ExecuteBackAction() {
            if(ActionStack.Count == 0) return;

            var action = ActionStack.Peek();
            if(action != null) {
                action();
            }
        }

        void Update() {
            if(HasBackAction) {
                ExecuteBackAction();
            }
        }
    }
}