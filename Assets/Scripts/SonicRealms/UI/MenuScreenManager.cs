using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SonicRealms.UI
{
    /// <summary>
    /// Simple manager than ensures only one of its child Menu Screens is open at any time.
    /// </summary>
    [DisallowMultipleComponent]
    public class MenuScreenManager : UIBehaviour
    {
        /// <summary>
        /// All menu screens that are immediate children of the manager.
        /// </summary>
        protected List<MenuScreen> MenuScreens;

        /// <summary>
        /// The menu screen to open first
        /// </summary>
        public MenuScreen FirstMenuScreen;

        /// <summary>
        /// The currently open menu screen.
        /// </summary>
        public MenuScreen OpenMenuScreen;

        protected override void Awake()
        {
            MenuScreens = new List<MenuScreen>();
        }

        /// <summary>
        /// Updates the manager's list of menu screens.
        /// </summary>
        public void UpdateList()
        {
            if (MenuScreens == null) MenuScreens = new List<MenuScreen>();
            GetComponentsInChildren(MenuScreens);
            MenuScreens.RemoveAll(screen => !screen.transform.parent == transform);
        }

        protected override void Start()
        {
            if (FirstMenuScreen != null) Open(FirstMenuScreen);
        }

        protected override void OnTransformParentChanged()
        {
            UpdateList();
        }

        /// <summary>
        /// Opens the given menu screen.
        /// </summary>
        /// <param name="menuScreen"></param>
        public void Open(MenuScreen menuScreen)
        {
            StartCoroutine(Open_Coroutine(menuScreen, () => { }));
        }

        /// <summary>
        /// Opens the menu screen with the given name. This will only open menu screens that are immediate
        /// children of the manager.
        /// </summary>
        /// <param name="menuScreenName"></param>
        public void Open(string menuScreenName)
        {
            var menuScreen = MenuScreens.FirstOrDefault(screen => screen.name == menuScreenName);
            if (menuScreen != null) Open(menuScreen);
        }

        /// <summary>
        /// Closes the currently open menu screen.
        /// </summary>
        public void CloseCurrent()
        {
            StartCoroutine(CloseCurrent_Coroutine(() => { }));
        }

        protected IEnumerator Open_Coroutine(MenuScreen menuScreen, Action callback)
        {
            if (menuScreen == OpenMenuScreen)
            {
                callback();
                yield break;
            }

            if (OpenMenuScreen != null)
            {
                var done = false;
                StartCoroutine(CloseCurrent_Coroutine(() => done = true));
                yield return new WaitUntil(() => done);
            }

            menuScreen.Open();
            yield return new WaitWhile(() => menuScreen.IsOpening);
        }

        protected IEnumerator CloseCurrent_Coroutine(Action callback)
        {
            if (OpenMenuScreen == null)
            {
                callback();
                yield break;
            }

            OpenMenuScreen.Close();
            yield return new WaitWhile(() => OpenMenuScreen.IsClosing);
            callback();
        }
    }
}
