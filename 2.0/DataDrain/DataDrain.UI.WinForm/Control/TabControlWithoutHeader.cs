using System;
using System.Windows.Forms;

namespace DataDrain.UI.WinForm.Control
{
    public partial class TabControlWithoutHeader : TabControl
    {
        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == 0x1328 && !DesignMode)
                    m.Result = (IntPtr)1;
                else
                    base.WndProc(ref m);
            }
            catch
            {

            }
        }
    }
}
