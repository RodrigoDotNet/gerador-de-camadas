using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DataDrain.ORM.Generator
{
    public static class EmErrorProvider
    {
        private static int _count;

        private static List<KeyValuePair<Control, string>> _controles = new List<KeyValuePair<Control, string>>();

        public static void SetErrorWithCount(this ErrorProvider ep, Control c, string message)
        {
            if (message == "")
            {
                if (ep.GetError(c) != "")
                {
                    _count--;

                    var ctrlRemove = new KeyValuePair<Control, string>();

                    foreach (var ctrl in _controles.Where(ctrl => ctrl.Key == c))
                    {
                        ctrlRemove = ctrl;
                    }

                    _controles.Remove(ctrlRemove);
                }
            }
            else
            {
                _count++;
                _controles.Add(new KeyValuePair<Control, string>(c, message));
            }

            ep.SetError(c, message);
        }

        public static bool HasErrors(this ErrorProvider ep, Form frm)
        {
            try
            {
                Clear(ep);
                frm.ValidateChildren();
                return _count != 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool HasErrors(this ErrorProvider ep, Control ctrl)
        {
            try
            {
                Clear(ep);

                foreach (Control c in ctrl.Controls)
                {

                    var mi = typeof(Control).GetMethod("NotifyValidating", BindingFlags.Instance |BindingFlags.NonPublic);


                    if (mi != null)
                    {
                        mi.Invoke(c, null);
                    }
                }

                return _count != 0;
            }
            catch
            {
                return false;
            }
        }

        public static int GetErrorCount(this ErrorProvider ep)
        {
            return _count;
        }

        public static void SetErrors(this ErrorProvider ep)
        {
            foreach (var ctrl in _controles)
            {
                ep.SetError(ctrl.Key, ctrl.Value);
            }
        }

        public static void Clear(this ErrorProvider ep)
        {
            _controles = new List<KeyValuePair<Control, string>>();
            _count = 0;
        }
    }
}
