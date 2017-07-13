using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForecourtSimulator_1
{
    public static class ThreadHelper
    {
        delegate void SetTextCallback(Form f, Control ctrl, string text);
        delegate void SetButtonCallback(Form f, Control ctrl, bool value);
        delegate void SetCheckboxCallback(Form f, Control ctrl, bool value);
        delegate void SetListViewCallback(Form f, Control ctrl, ListViewItem trans);
        //delegate void StartFuellingThreadCallback(Form f, Control ctrl, Thread thread);

        /// <summary>
        /// Set text property of various controls
        /// </summary>
        /// <param name="form">The calling form</param>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        public static void SetText(Form form, Control ctrl, string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (ctrl.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                form.Invoke(d, new object[] { form, ctrl, text });
            }
            else
            {
                ctrl.Text = text;
            }
        }

        public static void SetButton(Form form, Control ctrl, bool value)
        {
            if(ctrl.InvokeRequired)
            {
                SetButtonCallback d = new SetButtonCallback(SetButton);
                form.Invoke(d, new object[] { form, ctrl, value });
            }
            else
            {
                if (ctrl is Button)
                {
                    ((Button)ctrl).Enabled = value;
                }
            }
        }

        public static void SetCheckbox(Form form, Control ctrl, bool value)
        {
            if (ctrl.InvokeRequired)
            {
                SetCheckboxCallback d = new SetCheckboxCallback(SetCheckbox);
                form.Invoke(d, new object[] { form, ctrl, value });
            }
            else
            {
                if (ctrl is CheckBox)
                {
                    ((CheckBox)ctrl).Checked = value;
                }
            }
        }

        public static void AddToListView(Form form, Control ctrl, ListViewItem trans)
        {
            if (ctrl.InvokeRequired)
            {
                SetListViewCallback d = new SetListViewCallback(AddToListView);
                form.Invoke(d, new object[] { form, ctrl, trans });
            }
            else
            {
                if (ctrl is ListView)
                {
                    ((ListView)ctrl).Items.Add(trans);
                }
            }
        }

        //public static void StartFuellingThread(Form form, Control ctrl, Thread thread)
        //{
        //    if (ctrl.InvokeRequired)
        //    {
        //        StartFuellingThreadCallback d = new StartFuellingThreadCallback(StartFuellingThread);
        //        form.Invoke(d, new object[] { form, ctrl, thread });
        //    }
        //    else
        //    {
                
        //    }
        //}

    }
}
