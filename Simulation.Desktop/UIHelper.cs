using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Simulation.Desktop
{
    public class UIHelper
    {
        public static void UISafeInvoke(Action actionForUI, bool asyncAllowed = false)
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                if (object.ReferenceEquals(Application.Current.Dispatcher.Thread, Thread.CurrentThread))
                {
                    actionForUI();
                }
                else
                {
                    if (asyncAllowed)
                    {
                        Application.Current.Dispatcher.BeginInvoke(actionForUI);
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(actionForUI);
                    }
                }
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
