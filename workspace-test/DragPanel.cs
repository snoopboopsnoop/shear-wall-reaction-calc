using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspace_test
{
    internal class DragPanel : BasePanel
    {
        // right click menu
        private ContextMenuStrip cm;

        public DragPanel() : base()
        {
            cm = new ContextMenuStrip();
            base.SetCM(cm);

            // NOTE: IF YOU CHANGE THE CM CHANGE INDEXES IN CONTEXTMENU_OPENING FUNC
            cm.Items.Add("Set as SLRS");
        }

        // deal with right click menu selections
        private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;
            System.Console.WriteLine(item.Text);

            if (item.Text == "Set as SLRS")
            {
                //main.ToggleDragPage();
            }
        }
        private void contextMenu_Opening(object sender, EventArgs e)
        {
            if (SelectedLines().Count != 0)
            {
                cm.Items[0].Enabled = true;
            }
            else cm.Items[0].Enabled = false;
        }

    }
}
