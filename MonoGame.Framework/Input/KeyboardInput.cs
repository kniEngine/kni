// Copyright (C)2024 Nick Kastellanos

using System;
using System.Threading.Tasks;
using Microsoft.Xna.Platform.Input;

namespace Microsoft.Xna.Platform.Input
{
    public interface IKeyboardInput
    {
        bool IsVisible { get; }

        Task<string> Show(string title, string description, string defaultText = "", bool usePasswordMode = false);
        void Cancel(string result);
    }

    public interface IPlatformKeyboardInput
    {
        T GetStrategy<T>() where T : KeyboardInputStrategy;
    }
}

namespace Microsoft.Xna.Framework.Input
{
    public sealed class KeyboardInput : IKeyboardInput
        , IPlatformKeyboardInput
    {
        private static KeyboardInput _current;

        /// <summary>
        /// Returns the current Mouse instance.
        /// </summary> 
        public static KeyboardInput Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (typeof(KeyboardInput))
                {
                    if (_current == null)
                        _current = new KeyboardInput();

                    return _current;
                }
            }
        }


        public static bool IsVisible
        {
            get { return ((IKeyboardInput)KeyboardInput.Current).IsVisible; } 
        }

        /// <summary>
        /// Displays the keyboard input interface asynchronously.
        /// </summary>
        /// <param name="title">Title of the dialog box.</param>
        /// <param name="description">Description of the dialog box.</param>
        /// <param name="defaultText">Default text displayed in the input area.</param>
        /// <param name="usePasswordMode">If password mode is enabled, the characters entered are not displayed.</param>
        /// <returns>Text entered by the player. Null if back was used.</returns>
        /// <exception cref="System.Exception">Thrown when the message box is already visible</exception>
        /// <example>
        /// <code>
        /// var name = await KeyboardInput.Show("Name", "What's your name?", "Player");
        /// </code>
        /// </example>
        public static async Task<string> Show(string title, string description, string defaultText = "", bool usePasswordMode = false)
        {
            return await ((IKeyboardInput)KeyboardInput.Current).Show(title, description, defaultText, usePasswordMode);
        }

        /// <summary>
        /// Hides the keyboard input interface and returns the parameter as the result of <see cref="Show"/>
        /// </summary>
        /// <param name="result">Result to return</param>
        /// <exception cref="System.Exception">Thrown when the keyboard input is not visible</exception>
        /// <example>
        /// <code>
        /// var nameTask = KeyboardInput.Show("Name", "What's your name?", "Player");
        /// KeyboardInput.Cancel("John Doe");
        /// var name = await nameTask;
        /// </code>
        /// </example>
        public static void Cancel(string result)
        {
            ((IKeyboardInput)KeyboardInput.Current).Cancel(result);
        }

        private KeyboardInputStrategy _strategy;

        T IPlatformKeyboardInput.GetStrategy<T>()
        {
            return (T)_strategy;
        }

        private KeyboardInput()
        {
            _strategy = new ConcreteKeyboardInput();
        }


        #region IKeyboardInput

        private bool _isVisible;

        bool IKeyboardInput.IsVisible
        {
            get { return _isVisible; }
        }

        async Task<string> IKeyboardInput.Show(string title, string description, string defaultText = "", bool usePasswordMode = false)
        {
            if (IsVisible)
                throw new Exception("The function cannot be completed at this time: the KeyboardInput UI is already active. Wait until KeyboardInput.IsVisible is false before issuing this call.");

            _isVisible = true;

            string result = await _strategy.PlatformShow(title, description, defaultText, usePasswordMode);

            _isVisible = false;

            return result;
        }

        void IKeyboardInput.Cancel(string result)
        {
            if (!IsVisible)
                throw new Exception("The function cannot be completed at this time: the MessageBox UI is not active.");

            _strategy.PlatformCancel(result);
        }

        #endregion IKeyboardInput

    }
}
