using System;
using Android.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Platform;
using Android.App;
using Android.Util;


namespace Microsoft.Xna.Framework
{
    internal class ScreenReceiver : BroadcastReceiver
    {
        private AndroidGameWindow _gameWindow;
        private AndroidGameActivity _activity;
        private bool _isScreenLocked;


        public bool IsScreenLocked
        {
            get { return _isScreenLocked; }
            internal set { _isScreenLocked = value; }
        }

        public ScreenReceiver(AndroidGameWindow gameWindow, AndroidGameActivity activity)
        {
            this._gameWindow = gameWindow;
            this._activity = activity;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            Android.Util.Log.Info("Kni", intent.Action.ToString());
            if(intent.Action == Intent.ActionScreenOff)
            {
                OnLocked();
            }
            else if(intent.Action == Intent.ActionScreenOn)
            {
                // If the user turns the screen on just after it has automatically turned off, 
                // the keyguard will not have had time to activate and the ActionUserPreset intent
                // will not be broadcast. We need to check if the lock is currently active
                // and if not re-enable the game related functions.
                // http://stackoverflow.com/questions/4260794/how-to-tell-if-device-is-sleeping
                KeyguardManager keyguard = (KeyguardManager)context.GetSystemService(Context.KeyguardService);
                if (!keyguard.InKeyguardRestrictedInputMode())
                    OnUnlocked();
            }
            else if(intent.Action == Intent.ActionUserPresent)
            {
                // This intent is broadcast when the user unlocks the phone
                OnUnlocked();
            }

            if (intent.Action == Android.Telephony.TelephonyManager.ActionPhoneStateChanged)
            {
                if (intent.Extras != null)
                {
                    string state = intent.GetStringExtra(Android.Telephony.TelephonyManager.ExtraState);
                    if (state == Android.Telephony.TelephonyManager.ExtraStateRinging)
                    {
                        // TODO: Find a way to set Game.IsActive = false during a call.
                        // View.ClearFocus() doesn't have any affect. 
                        // The best we can do currently is to sent the game to foreground.
                        _activity.MoveTaskToBack(true);
                    }
                }
            }
        }

        private void OnLocked()
        {
            _isScreenLocked = true;
            MediaPlayer.IsMuted = true;
        }

        private void OnUnlocked()
        {
            _isScreenLocked = false;
            MediaPlayer.IsMuted = false;
            _gameWindow.GameView._appState = AndroidGameWindow.AppState.Resumed;
            _gameWindow._runner.RequestFrame();
            try
            {
                if (!_gameWindow.GameView.IsFocused)
                    _gameWindow.GameView.RequestFocus();
            }
            catch (Exception ex)
            {
                Log.Verbose("RequestFocus()", ex.ToString());
            }
        }
    }
}

