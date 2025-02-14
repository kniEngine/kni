// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform.Input;

namespace Microsoft.Xna.Platform.Input
{
    public interface IMessageBox
    {
        bool IsVisible { get; }

#if !NET40
        Task<int?> Show(string title, string description, IEnumerable<string> buttons);
#endif
        void Cancel(int? result);


    }

    public interface IPlatformMessageBox
    {
        T GetStrategy<T>() where T : MessageBoxStrategy;
    }
}

namespace Microsoft.Xna.Framework.Input
{

    public sealed partial class MessageBox : IMessageBox
        , IPlatformMessageBox
    {
        private static MessageBox _current;

        /// <summary>
        /// Returns the current Mouse instance.
        /// </summary> 
        public static MessageBox Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (typeof(MessageBox))
                {
                    if (_current == null)
                        _current = new MessageBox();

                    return _current;
                }
            }
        }


        public static bool IsVisible
        {
            get { return ((IMessageBox)MessageBox.Current).IsVisible; }
        }

#if !NET40
        /// <summary>
        /// Displays the message box interface asynchronously.
        /// </summary>
        /// <param name="title">Title of the message box.</param>
        /// <param name="description">Description of the message box.</param>
        /// <param name="buttons">Captions of the message box buttons. Up to three supported.</param>
        /// <returns>Index of button selected by the player. Null if back was used.</returns>
        /// <exception cref="System.Exception">Thrown when the message box is already visible</exception>
        /// <example>
        /// <code>
        /// var color = await MessageBox.Show("Color", "What's your favorite color?", new[] { "Red", "Green", "Blue" });
        /// </code>
        /// </example>
        public static async Task<int?> Show(string title, string description, IEnumerable<string> buttons)
        {
            return await ((IMessageBox)MessageBox.Current).Show(title, description, buttons);
        }
#endif

        /// <summary>
        /// Hides the message box interface and returns the parameter as the result of <see cref="Show"/>
        /// </summary>
        /// <param name="result">Result to return</param>
        /// <exception cref="System.Exception">Thrown when the message box is not visible</exception>
        /// <example>
        /// <code>
        /// var colorTask = MessageBox.Show("Color", "What's your favorite color?", new[] { "Red", "Green", "Blue" });
        /// MessageBox.Cancel(0);
        /// var color = await colorTask;
        /// </code>
        /// </example>
        public static void Cancel(int? result)
        {
            ((IMessageBox)MessageBox.Current).Cancel(result);
        }


        private MessageBoxStrategy _strategy;

        T IPlatformMessageBox.GetStrategy<T>()
        {
            return (T)_strategy;
        }

        private MessageBox()
        {
            _strategy = InputFactory.Current.CreateMessageBoxStrategy();
        }

        #region IMessageBox

        private static bool _isVisible;

        bool IMessageBox.IsVisible
        {
            get { return _isVisible; }
        }

#if !NET40
        async Task<int?> IMessageBox.Show(string title, string description, IEnumerable<string> buttons)
        {
            if (IsVisible)
                throw new Exception("The function cannot be completed at this time: the MessageBox UI is already active. Wait until MessageBox.IsVisible is false before issuing this call.");

            _isVisible = true;

            List<string> buttonsList = buttons.ToList();
            if (buttonsList.Count > 3 || buttonsList.Count == 0)
                throw new ArgumentException("Invalid number of buttons: one to three required", "buttons");

            int? result = await _strategy.PlatformShow(title, description, buttonsList);

            _isVisible = false;

            return result;
        }
#endif

        void IMessageBox.Cancel(int? result)
        {
            if (!IsVisible)
                throw new Exception("The function cannot be completed at this time: the MessageBox UI is not active.");

            _strategy.PlatformCancel(result);
        }

        #endregion IMessageBox
    }
}