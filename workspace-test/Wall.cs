using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspace_test
{

    public class Wall : Line
    {
        public Wall()
        {

        }

        public Wall(Point p1, Point p2) : base(p1, p2)
        {

        }

        public Wall(Point p1, Point p2, Color color, int opacity = 100) : base(p1, p2, color, opacity)
        {

        }

        public void DrawLine(PaintEventArgs e, bool drawText = true)
        {
            base.DrawLine(e);
        }
    }
}
