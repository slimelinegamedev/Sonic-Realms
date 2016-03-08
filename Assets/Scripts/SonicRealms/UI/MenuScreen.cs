using System;
using System.Collections;
using System.Linq;
using SonicRealms.Core.Utils;
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
        /// An ID number for the screen, used in animations.
        /// </summary>
        [Tooltip("An ID number for the screen, used in animations.")]
        public int ScreenID;

        /// <summary>
        /// The transition this menu screen plays when going to another menu screen.
        /// </summary>
        [Space]
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

        [Foldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Animator int set to the ID of the destination screen when closing, or -1 if there is no destination.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Animator int set to the ID of the destination screen when closing, or -1 if there is no destination.")]
        public string DestinationInt;
        protected int DestinationIntHash;

        protected override void Reset()
        {
            ScreenID = FindObjectsOfType<MenuScreen>().Aggregate(-1, (i, screen) => ++i) + 1;

            Transition = GetComponent<Transition>();
            FirstItem = transform.childCount > 0 ? transform.GetChild(0).gameObject : null;
            Animator = GetComponent<Animator>();
        }

        protected override void Awake()
        {
            Transition = Transition ?? GetComponent<Transition>();
            DestinationIntHash = Animator.StringToHash(DestinationInt);
        }

        /// <summary>
        /// Opens the menu screen.
        /// </summary>
        public virtual void Open()
        {
            gameObject.SetActive(true);
            StartCoroutine(Enter_Coroutine(OnEnterComplete));
        }

        /// <summary>
        /// Closes the menu screen.
        /// </summary>
        public virtual void Close()
        {
            Close(null);
        }

        /// <summary>
        /// Closes the menu screen and lets it know of the destination screen.
        /// </summary>
        public virtual void Close(MenuScreen destination)
        {
            if (destination != null && Animator != null && DestinationIntHash != 0)
                Animator.SetInteger(DestinationIntHash, ScreenID);

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

            gameObject.SetActive(false);
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
