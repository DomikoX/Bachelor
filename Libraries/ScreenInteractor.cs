using System;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace ClientPCService
{


    public partial class NativeMethods
    {
        /// Return Type: BOOL->int
        ///fBlockIt: BOOL->int
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "BlockInput")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool BlockInput([System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)] bool fBlockIt);

    }

    public class ScreenInteractor
    {
        private CustomMessage _cm;


        /// <summary>
        /// Show mesage in new WPF windows
        /// </summary>
        /// <param name="title"> tittle of the message</param>
        /// <param name="text">text of the message</param>
        public void ShowMessage(string title, string text)
        {
            if (title.Equals("speak:"))
            {
                Speak(text);
                return;
            }

            var message = new CustomMessage(title,text);
            message.Show();
        }

        /// <summary>
        /// Block this PC by Setting full screen WPF windows  with warnign message and block any user inputs
        /// </summary>
        public void Block()
        {
            _cm = new CustomMessage("System is blocked", "NOPE!!!");
            _cm.WindowState = WindowState.Maximized;
            _cm.WindowStyle = WindowStyle.None;
            _cm.Topmost = true;
            _cm.CanClose = false;
            _cm.Show();

            NativeMethods.BlockInput(true);
            DisableTaskManager();
            //Thread.Sleep(10000);//for debugging
           // Unblock();

        }

        /// <summary>
        /// Ublocks computer after it was blocked by Block()
        /// </summary>
        public void Unblock()
        {
            EnableTaskManager();
            NativeMethods.BlockInput(false);
            if (_cm == null) return;
            _cm.Topmost = false;
            _cm.CanClose = true;
            _cm.Close();
            _cm = null;
        }

        /// <summary>
        /// Set registry to user can not run Task manager
        /// </summary>
        private void DisableTaskManager()
        {
            RegistryKey regkey = default(RegistryKey);
            string keyValueInt = "1";
            string subKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
            try
            {
                regkey = Registry.CurrentUser.CreateSubKey(subKey);
                regkey.SetValue("DisableTaskMgr", keyValueInt);
                regkey.SetValue("NoLogoff", keyValueInt);
                regkey.SetValue("DisableLockWorkstation", keyValueInt);
                regkey.SetValue("DisableChangePassword", keyValueInt);
                regkey.Close();
            }
            catch (Exception ex)
            {
                Interaction.MsgBox(ex.Message, MsgBoxStyle.Critical, "Registry Error!");
            }

        }


        /// <summary>
        /// Enable user to Run Task manager
        /// </summary>
        private void EnableTaskManager()
        {
            RegistryKey regkey = default(RegistryKey);
            string keyValueInt = "0";
            //0x00000000 (0)
            string subKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
            try
            {
                regkey = Registry.CurrentUser.CreateSubKey(subKey);
                regkey.SetValue("DisableTaskMgr", keyValueInt);
                regkey.SetValue("NoLogoff", keyValueInt);
                regkey.SetValue("DisableLockWorkstation", keyValueInt);
                regkey.SetValue("DisableChangePassword", keyValueInt);
                regkey.Close();
            }
            catch (Exception ex)
            {
                Interaction.MsgBox(ex.Message, MsgBoxStyle.Critical, "Registry Error!");
            }

        }
















        private void Speak(string text )
        {
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {

                // Configure the audio output. 
                synth.SetOutputToDefaultAudioDevice();

                // Speak a string synchronously.
                synth.Speak(text);
            }
        }


       


    }
}