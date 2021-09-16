using System;
using System.Windows.Forms;

namespace NeroClient
{
    public class IScreenLocker : Form
    {
        //Override form load to place center
        protected override void OnLoad(EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            base.OnLoad(e);
        }
    }
}