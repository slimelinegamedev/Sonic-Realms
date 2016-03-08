using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SonicRealms.UI
{
    /// <summary>
    /// A menu screen opened and switched between other menu screens by Menu Screen Managers. They're basically
    /// window frames that keep their Selectables away from other screens' Selectables, and one manager only
    /// has one menu screen at a time.
    /// </summary>
    [DisallowMultipleComponent]
    public class MenuScreen : UIBehaviour
    {
        /// <summary>
        /// The transition this menu screen plays when going to another menu screen.
        /// </summary>
        public Transition Transition;

        /// <summary>
        /// The first item to select when the menu screen is opened. This is only used the first time
        /// the screen is opened, after which the previously selected item is used.
        /// </summary>
        public GameObject FirstItem;

        /// <summary>
        /// Whether the menu screen is currently opening.
        /// </summary>
        public bool IsOpening
        {
            get { return Transition != null && Transition.State == TransitionState.Enter; }
        }

        /// <summary>
        /// Whether the menu screen is currently closing.
        /// </summary>
        public bool IsClosing
        {
            get { return Transition != null && Transition.State == TransitionState.Exit; }
        }

        /// <summary>
        /// The last item the menu screen had selected, which will be preserved if it is closed and reopened.
        /// </summary>
        protected GameObject PreviouslySelectedItem;

        protected override void Reset()
        {
            Transition = GetComponent<Transition>();
            FirstItem = transform.childCount > 0 ? transform.GetChild(0).gameObject : null;
        }

        protected override void Awake()
        {
            Transition = Transition ?? GetComponent<Transition>();
        }

        /// <summary>
        /// Opens the menu screen.
        /// </summary>
        public virtual void Open()
        {
            StartCoroutine(Enter_Coroutine(OnEnterComplete));
        }

        /// <summary>
        /// Closes the menu screen.
        /// </summary>
        public virtual void Close()
        {
            StartCoroutine(Exit_Coroutine(OnExitComplete));
        }

        protected virtual void OnEnterComplete()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem != null)
            {
                eventSystem.SetSelectedGameObject(FirstItem.gameObject);
            }
        }

        protected virtual void OnExitComplete()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem != null)
            {
                PreviouslySelectedItem = eventSystem.currentSelectedGameObject;
                eventSystem.SetSelectedGameObject(null);
            }
        }

        protected virtual IEnumerator Enter_Coroutine(Action callback)
        {
            if (Transition == null)
            {
                callback();
                yield break;
            }

            Transition.Enter();
            yield return new WaitWhile(() => Transition.State == TransitionState.Enter);
            callback();
        }

        protected virtual IEnumerator Exit_Coroutine(Action callback)
        {
            if (Transition == null)
            {
                callback();
                yield break;
            }

            Transition.Exit();
            yield return new WaitWhile(() => Transition.State == TransitionState.Exit);

            callback();
        }
    }
}
