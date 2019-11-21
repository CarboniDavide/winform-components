/// Winform Components for Visual Studio
/// Carboni Corporation 2019- All right reserved https://www.carboni.ch
/// Author: Carboni Davide
///
/// @copyright Copyright (c) 2019, Carboni Software, Inc.
/// @license AGPL-3.0
/// This code is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License, version 3,
/// as published by the Free Software Foundation.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
/// GNU Affero General Public License for more details.
///
/// You should have received a copy of the GNU Affero General Public License, version 3,
/// along with this program.  If not, see <http://www.gnu.org/licenses/>


using System;
using System.Drawing;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Threading;

namespace WinformComponents
{

    #region Window Shadow Aereo

    /// <summary>
    /// Make a windows aereo shadow for a form without form border style
    /// </summary>

    public class ShadowAereo : Form
    {
        static readonly Color BORDER_COLOR = Color.FromArgb(24, 131, 215);
        Boolean IsMaxSize = false;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
        int nLeftRect, // x-coordinate of upper-left corner
        int nTopRect, // y-coordinate of upper-left corner
        int nRightRect, // x-coordinate of lower-right corner
        int nBottomRect, // y-coordinate of lower-right corner
        int nWidthEllipse, // height of ellipse
        int nHeightEllipse // width of ellipse
         );

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        private bool m_aeroEnabled;                     // variables for box shadow
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        public struct MARGINS                           // struct for box shadow
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        private const int WM_NCHITTEST = 0x84;          // variables for dragging the form
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();

                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }

        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:                        // box shadow
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 1,
                            rightWidth = 1,
                            topHeight = 1
                        };
                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);

                    }
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)     // drag the form
                m.Result = (IntPtr)HTCAPTION;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!IsMaxSize)
                e.Graphics.DrawRectangle(new Pen(BORDER_COLOR, 1), 0, 0, this.Width - 1, this.Height - 1);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (this.WindowState == FormWindowState.Maximized)
                IsMaxSize = true;
            else
                IsMaxSize = false;
            this.Refresh();
        }

        public ShadowAereo()
        {
            m_aeroEnabled = false;
            this.Padding = new System.Windows.Forms.Padding(1);
            this.FormBorderStyle = FormBorderStyle.None;
        }
    }

    #endregion

    #region Window Shawdow Aereo Sizeable

    /// <summary>
    /// Make a sizeable and mouveable window with shadow for a form without form border style
    /// </summary>

    public class ShadowAereoSizeAble : Form
    {

        #region Variables

        static readonly Color BORDER_COLOR = Color.FromArgb(24, 131, 215);
        private Boolean Is_MaxSize = false;

        class ReSize
        {

            private bool Above, Right, Under, Left, Right_above, Right_under, Left_under, Left_above;


            int Thickness = 6;  //Thickness of border  u can cheang it
            int Area = 8;     //Thickness of Angle border 


            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="thickness">set thickness of form border</param>
            public ReSize(int thickness)
            {
                Thickness = thickness;
            }

            /// <summary>
            /// Constructor set thickness of form border=1
            /// </summary>
            public ReSize()
            {
                Thickness = 10;
            }

            //Get Mouse Position
            public string getMosuePosition(Point mouse, Form form)
            {
                bool above_underArea = mouse.X > Area && mouse.X < form.ClientRectangle.Width - Area; /* |\AngleArea(Left_Above)\(=======above_underArea========)/AngleArea(Right_Above)/| */ //Area===>(==)
                bool right_left_Area = mouse.Y > Area && mouse.Y < form.ClientRectangle.Height - Area;

                bool _Above = mouse.Y <= Thickness;  //Mouse in Above All Area
                bool _Right = mouse.X >= form.ClientRectangle.Width - Thickness;
                bool _Under = mouse.Y >= form.ClientRectangle.Height - Thickness;
                bool _Left = mouse.X <= Thickness;

                Above = _Above && (above_underArea); if (Above) return "a";   /*Mouse in Above All Area WithOut Angle Area */
                Right = _Right && (right_left_Area); if (Right) return "r";
                Under = _Under && (above_underArea); if (Under) return "u";
                Left = _Left && (right_left_Area); if (Left) return "l";


                Right_above =/*Right*/ (_Right && (!right_left_Area)) && /*Above*/ (_Above && (!above_underArea)); if (Right_above) return "ra";     /*if Mouse  Right_above */
                Right_under =/* Right*/((_Right) && (!right_left_Area)) && /*Under*/(_Under && (!above_underArea)); if (Right_under) return "ru";     //if Mouse  Right_under 
                Left_under = /*Left*/((_Left) && (!right_left_Area)) && /*Under*/ (_Under && (!above_underArea)); if (Left_under) return "lu";      //if Mouse  Left_under
                Left_above = /*Left*/((_Left) && (!right_left_Area)) && /*Above*/(_Above && (!above_underArea)); if (Left_above) return "la";      //if Mouse  Left_above

                return "";

            }


        }

        private const int cGrip = 16;      // Grip size
        private const int cCaption = 32;   // Caption bar height;
        ReSize resize = new ReSize();     // ReSize Class "/\" To Help Resize Form <None Style>

        public override Size MinimumSize
        {
            get { return base.MinimumSize; }
            set { base.MinimumSize = new Size(179, 51); }
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
        int nLeftRect, // x-coordinate of upper-left corner
        int nTopRect, // y-coordinate of upper-left corner
        int nRightRect, // x-coordinate of lower-right corner
        int nBottomRect, // y-coordinate of lower-right corner
        int nWidthEllipse, // height of ellipse
        int nHeightEllipse // width of ellipse
         );

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        private bool m_aeroEnabled;                     // variables for box shadow
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        public struct MARGINS                           // struct for box shadow
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        private const int WM_NCHITTEST = 0x84;          // variables for dragging the form
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        #endregion

        #region Fonctions

        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();

                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }

        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            int x = (int)(m.LParam.ToInt64() & 0xFFFF);               //get x mouse position
            int y = (int)((m.LParam.ToInt64() & 0xFFFF0000) >> 16);   //get y mouse position  you can gave (x,y) it from "MouseEventArgs" too
            Point pt = PointToClient(new Point(x, y));

            switch (m.Msg)
            {
                case WM_NCPAINT:                        // box shadow
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 1,
                            rightWidth = 1,
                            topHeight = 1
                        };
                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);

                    }
                    break;

                case WM_NCHITTEST:
                    {
                        if (!Is_MaxSize)
                            switch (resize.getMosuePosition(pt, this))
                            {
                                case "l": m.Result = (IntPtr)10; return;  // the Mouse on Left Form
                                case "r": m.Result = (IntPtr)11; return;  // the Mouse on Right Form
                                case "a": m.Result = (IntPtr)12; return;
                                case "la": m.Result = (IntPtr)13; return;
                                case "ra": m.Result = (IntPtr)14; return;
                                case "u": m.Result = (IntPtr)15; return;
                                case "lu": m.Result = (IntPtr)16; return;
                                case "ru": m.Result = (IntPtr)17; return; // the Mouse on Right_Under Form
                                                                          //case "": m.Result = pt.Y < 32 /*mouse on title Bar */ ? (IntPtr)2 : (IntPtr)1; return;
                            }
                    }
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)     // drag the form
                m.Result = (IntPtr)HTCAPTION;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (this.WindowState == FormWindowState.Maximized)
                Is_MaxSize = true;
            else
                Is_MaxSize = false;
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!Is_MaxSize)
                e.Graphics.DrawRectangle(new Pen(BORDER_COLOR, 1), 0, 0, this.Width - 1, this.Height - 1);
        }

        #endregion

        #region Init Class

        public ShadowAereoSizeAble()
        {
            m_aeroEnabled = false;
            this.Padding = new System.Windows.Forms.Padding(1);
            this.FormBorderStyle = FormBorderStyle.None;
        }

        #endregion
    }

    #endregion

    #region Windows Shadow 3D

    /// <summary>
    /// Make a 3d shadow for a form without border style
    /// </summary>

    public class Shadow3D : Form
    {
        private const int CS_DROPSHADOW = 0x00020000;

        // form mouve

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x1)
                        m.Result = (IntPtr)0x2;
                    return;
            }
            base.WndProc(ref m);
        }

        // 3D shadow

        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            get
            {
                CreateParams parameters = base.CreateParams;
                if (DropShadowSupported)
                    parameters.ClassStyle = (parameters.ClassStyle | CS_DROPSHADOW);
                return parameters;
            }
        }

        public static bool DropShadowSupported
        {
            get
            {
                OperatingSystem system = Environment.OSVersion;
                bool runningNT = system.Platform == PlatformID.Win32NT;

                return runningNT && system.Version.CompareTo(new Version(5, 1, 0, 0)) >= 0;
            }
        }
    }

    #endregion

    #region Border Trasparent

    /// <summary>
    /// Define a border trasparent with 4pixel
    /// </summary>

    public class TrasparentBorder : Form
    {
        [StructLayout(LayoutKind.Sequential)]

        public struct MARGINS
        {
            public int left;
            public int top;
            public int bot;
            public int rig;
        }

        [DllImport("DwmApi.dll")]

        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarinset);

        TrasparentBorder()
        {
            MARGINS margin = new MARGINS();
            margin.left = 4;
            margin.bot = 4;
            margin.top = 4;
            margin.rig = 4;
            int result = DwmExtendFrameIntoClientArea(this.Handle, ref margin);
        }

    }

    #endregion

    #region Windos 10 Form

    /// <summary>
    /// Make a window 10 form without form border style
    /// Use this when you want the same form style for each windows os
    /// </summary>

    public class Windows10 : Form
    {

        #region Template's Variables

        public System.Windows.Forms.Panel cmdClose;
        public System.Windows.Forms.Panel cmdMaximize;
        public System.Windows.Forms.Panel cmdMinimize;
        public System.Windows.Forms.Label lblTitre;
        public System.Windows.Forms.PictureBox picIcon;

        Boolean IS_Enter = false;
        Boolean Is_Leave = false;
        Boolean Is_Base = true;
        Boolean Is_Down = false;

        private System.ComponentModel.IContainer components;

        Color _titleBarColor = TITLEBAR_COLOR;   // Title Bar Color
        Color _containerColor = CONTAINER_COLOR; // Container Color
        Color _titleColor = TITLE_COLOR;         // Title Text Color
        string _titleText = "Titre";             // Title text
        Font _titleFont;                         // Title Font
        Image _titleImage;                       // Icon image
        ImageLayout _titleImageLayout;           // Icon layout

        Boolean _titleVisible = true;            // Title enabled/disbled
        Boolean _titleIconVisible = true;        // Icon enabled/disbled
        Boolean _buttonMinimizeVisible = true;   // Button Close  enabled/disbled
        Boolean _buttonMaximizeVisible = true;   // Button Maximize  enabled/disbled
        Boolean _buttonCloseVisible = true;      // Button Minimize  enabled/disbled

        public Color TitleBarColor
        {
            get { return _titleBarColor; }
            set { _titleBarColor = value; this.Refresh(); }
        }
        public Color ContainerColor
        {
            get { return _containerColor; }
            set { _containerColor = this.BackColor = value; this.Refresh(); }
        }
        public Color TitleColor
        {
            get { return _titleColor; }
            set { _titleColor = lblTitre.ForeColor = value; this.Refresh(); }
        }
        public Font TitleFont
        {
            get { return _titleFont; }
            set { _titleFont = value; lblTitre.Font = value; this.Refresh(); }
        }
        public string TitleText
        {
            get { return _titleText; }
            set { _titleText = lblTitre.Text = value; this.Refresh(); }
        }
        public Image TitleImage
        {
            get { return _titleImage; }
            set
            {
                _titleImage = picIcon.BackgroundImage = value;
                if (picIcon.BackgroundImage == null)
                    picIcon.BackColor = Color.Gray;
                else
                    picIcon.BackColor = Color.Transparent;

                this.Refresh();
            }
        }
        public ImageLayout TitleImageLayout
        {
            get { return _titleImageLayout; }
            set { _titleImageLayout = picIcon.BackgroundImageLayout = value; this.Refresh(); }
        }

        public Boolean TitleVisible
        {
            get { return _titleVisible; }
            set { _titleVisible = lblTitre.Visible = value; this.Refresh(); }
        }
        public Boolean TitleIconVisible
        {
            get { return _titleIconVisible; }
            set { _titleIconVisible = picIcon.Visible = value; this.Refresh(); }
        }
        public Boolean ButtonMinimizeVisible
        {
            get { return _buttonMinimizeVisible; }
            set { _buttonMinimizeVisible = cmdMinimize.Visible = value; Title_Button_Dispose(); this.Refresh(); }
        }
        public Boolean ButtonMaximizeVisible
        {
            get { return _buttonMaximizeVisible; }
            set { _buttonMaximizeVisible = cmdMaximize.Visible = value; Title_Button_Dispose(); this.Refresh(); }
        }
        public Boolean ButtonCloseVisible
        {
            get { return _buttonCloseVisible; }
            set { _buttonCloseVisible = cmdClose.Visible = value; Title_Button_Dispose(); this.Refresh(); }
        }

        // Bbuttons color

        static readonly Color TITLEBAR_COLOR = Color.White;
        static readonly Color TITLE_COLOR = Color.Black;
        static readonly Color CONTAINER_COLOR = System.Drawing.SystemColors.Control;

        static readonly Color CLOSE_COLOR_NORMAL = Color.Transparent;
        static readonly Color CLOSE_COLOR_ENTER = Color.Red;
        static readonly Color CLOSE_COLOR_LEAVE = Color.White;
        static readonly Color CLOSE_COLOR_DOWN = Color.Violet;

        static readonly Color MAXIMIZE_COLOR_NORMAL = Color.Transparent;
        static readonly Color MAXIMIZE_COLOR_ENTER = Color.LightGray;
        static readonly Color MAXIMIZE_COLOR_LEAVE = Color.White;
        static readonly Color MAXIMIZE_COLOR_DOWN = Color.Gray;

        static readonly Color MINIMIZE_COLOR_NORMAL = Color.Transparent;
        static readonly Color MINIMIZE_COLOR_ENTER = Color.LightGray;
        static readonly Color MINIMIZE_COLOR_LEAVE = Color.White;
        static readonly Color MINIMIZE_COLOR_DOWN = Color.Gray;

        // Animation timers

        System.Windows.Forms.Timer tmrCloseLeave = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer tmrMaximizeLeave = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer tmrMinimizeLeave = new System.Windows.Forms.Timer();

        int componentColorAlpha_Close = 0;
        int componentColorAlpha_Maximize = 0;
        int componentColorAlpha_Minimize = 0;

        #endregion

        #region Form's Variables

        static readonly Color BORDER_COLOR = Color.FromArgb(24, 131, 215);
        private Boolean Is_MaxSize = false;

        class ReSize
        {

            private bool Above, Right, Under, Left, Right_above, Right_under, Left_under, Left_above;


            int Thickness = 6;  // Thickness of border  u can cheang it
            int Area = 18;      // Thickness of Angle border 


            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="thickness">set thickness of form border</param>
            public ReSize(int thickness)
            {
                Thickness = thickness;
            }

            /// <summary>
            /// Constructor set thickness of form border=1
            /// </summary>
            public ReSize()
            {
                Thickness = 10;
            }

            // Get Mouse Position
            public string getMosuePosition(Point mouse, Form form)
            {
                bool above_underArea = mouse.X > Area && mouse.X < form.ClientRectangle.Width - Area; /* |\AngleArea(Left_Above)\(=======above_underArea========)/AngleArea(Right_Above)/| */ //Area===>(==)
                bool right_left_Area = mouse.Y > Area && mouse.Y < form.ClientRectangle.Height - Area;

                bool _Above = mouse.Y <= Thickness;  //Mouse in Above All Area
                bool _Right = mouse.X >= form.ClientRectangle.Width - Thickness;
                bool _Under = mouse.Y >= form.ClientRectangle.Height - Thickness;
                bool _Left = mouse.X <= Thickness;

                Above = _Above && (above_underArea); if (Above) return "a";   /*Mouse in Above All Area WithOut Angle Area */
                Right = _Right && (right_left_Area); if (Right) return "r";
                Under = _Under && (above_underArea); if (Under) return "u";
                Left = _Left && (right_left_Area); if (Left) return "l";


                Right_above =/*Right*/ (_Right && (!right_left_Area)) && /*Above*/ (_Above && (!above_underArea)); if (Right_above) return "ra";     /*if Mouse  Right_above */
                Right_under =/* Right*/((_Right) && (!right_left_Area)) && /*Under*/(_Under && (!above_underArea)); if (Right_under) return "ru";     //if Mouse  Right_under 
                Left_under = /*Left*/((_Left) && (!right_left_Area)) && /*Under*/ (_Under && (!above_underArea)); if (Left_under) return "lu";      //if Mouse  Left_under
                Left_above = /*Left*/((_Left) && (!right_left_Area)) && /*Above*/(_Above && (!above_underArea)); if (Left_above) return "la";      //if Mouse  Left_above

                return "";
            }
        }

        private const int cGrip = 16;      // Grip size
        private const int cCaption = 32;   // Caption bar height;
        ReSize resize = new ReSize();      // ReSize Class "/\" To Help Resize Form <None Style>
        /*
        public override Size MinimumSize
        {
            get { return base.MinimumSize; }
            set { base.MinimumSize = new Size(179, 51); }
        }*/

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
        int nLeftRect,      // x-coordinate of upper-left corner
        int nTopRect,       // y-coordinate of upper-left corner
        int nRightRect,     // x-coordinate of lower-right corner
        int nBottomRect,    // y-coordinate of lower-right corner
        int nWidthEllipse,  // height of ellipse
        int nHeightEllipse  // width of ellipse
         );

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        private bool m_aeroEnabled;                     //  box shadow
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        public struct MARGINS                           // box shadow struct
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        private const int WM_NCHITTEST = 0x84;          // variables to dragging the form
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;
        private const int WM_LBUTTONDBLCLK = 0x00A3;

        #endregion

        #region Button Init

        private void Mouse_Entry_Setting()
        {
            // Mouse Init Events Setting

            // Close
            cmdClose.MouseClick += new MouseEventHandler(cmdClose_MouseClick);
            cmdClose.MouseEnter += new EventHandler(cmdClose_MouseEnter);
            cmdClose.MouseDown += new MouseEventHandler(cmdClose_MouseDown);
            cmdClose.MouseLeave += new EventHandler(cmdClose_MouseLeave);
            cmdClose.Paint += new PaintEventHandler(cmdClose_Paint);

            // Maximize
            cmdMaximize.MouseClick += new MouseEventHandler(cmdMaximize_MouseClick);
            cmdMaximize.MouseEnter += new EventHandler(cmdMaximize_MouseEnter);
            cmdMaximize.MouseDown += new MouseEventHandler(cmdMaximize_MouseDown);
            cmdMaximize.MouseLeave += new EventHandler(cmdMaximize_MouseLeave);
            cmdMaximize.Paint += new PaintEventHandler(cmdMaximize_Paint);

            //Minimize
            cmdMinimize.MouseClick += new MouseEventHandler(cmdMinimize_MouseClick);
            cmdMinimize.MouseEnter += new EventHandler(cmdMinimize_MouseEnter);
            cmdMinimize.MouseDown += new MouseEventHandler(cmdMinimize_MouseDown);
            cmdMinimize.MouseLeave += new EventHandler(cmdMinimize_MouseLeave);
            cmdMinimize.Paint += new PaintEventHandler(cmdMinimize_Paint);
        }
        #endregion

        #region Timer Init

        private void Timer_Entry_Setting()
        {
            // Initit Timers Buttons Animation Color

            // Interval
            tmrCloseLeave.Interval = 50;
            tmrMaximizeLeave.Interval = 50;
            tmrMinimizeLeave.Interval = 50;

            // Events Timers
            tmrCloseLeave.Tick += new EventHandler(tmrCloseLeave_Tick);
            tmrMaximizeLeave.Tick += new EventHandler(tmrMaximizeLeave_Tick);
            tmrMinimizeLeave.Tick += new EventHandler(tmrMinimizeLeave_Tick);
        }
        #endregion

        #region Timers

        private void tmrCloseLeave_Tick(object sender, EventArgs e)
        {
            // Change button close color when mouse leave

            {
                componentColorAlpha_Close -= 85;
                cmdClose.BackColor = Color.FromArgb(componentColorAlpha_Close, CLOSE_COLOR_ENTER);
            }

            if (componentColorAlpha_Close <= 0) // Stop Animation
            {
                tmrCloseLeave.Enabled = false;
                Is_Base = true;
                Is_Leave = false;
                //cmdClose.Refresh();
            }
        }

        private void tmrMaximizeLeave_Tick(object sender, EventArgs e)
        {
            // Change button maximize color when mouse leave

            if (componentColorAlpha_Maximize <= 0) // Stop Animation
                tmrMaximizeLeave.Enabled = false;
            else
            {
                componentColorAlpha_Maximize -= 85;
                cmdMaximize.BackColor = Color.FromArgb(componentColorAlpha_Maximize, MAXIMIZE_COLOR_ENTER);
            }
        }

        private void tmrMinimizeLeave_Tick(object sender, EventArgs e)
        {
            // Change button minimize color when mouse leave

            if (componentColorAlpha_Minimize <= 0) // Stop Animation
                tmrMinimizeLeave.Enabled = false;
            else
            {
                componentColorAlpha_Minimize -= 85;
                cmdMinimize.BackColor = Color.FromArgb(componentColorAlpha_Minimize, MINIMIZE_COLOR_ENTER);
            }
        }

        #endregion

        #region Button's Evenement

        #region Button Close

        private void cmdClose_MouseEnter(object sender, EventArgs e)
        {
            IS_Enter = true;
            Is_Leave = false;
            Is_Base = false;
            Is_Down = false;
            //cmdClose.Refresh();
            cmdClose.BackColor = CLOSE_COLOR_ENTER;
            componentColorAlpha_Close = 255;
            tmrCloseLeave.Enabled = false;
        }

        private void cmdClose_MouseLeave(object sender, EventArgs e)
        {
            IS_Enter = false;
            Is_Leave = true;
            Is_Base = false;
            Is_Down = false; ;
            //cmdClose.Refresh();
            tmrCloseLeave.Enabled = true;
        }

        private void cmdClose_MouseDown(object sender, MouseEventArgs e)
        {
            IS_Enter = false;
            Is_Leave = false;
            Is_Base = false;
            Is_Down = true;
            cmdClose.BackColor = CLOSE_COLOR_DOWN;
        }

        private void cmdClose_MouseClick(object sender, EventArgs e)
        {
            Form.ActiveForm.Close(); // Close window
        }

        private void cmdClose_Paint(object sender, PaintEventArgs e)
        {
            if (IS_Enter)
            {
                //Draw the White X
                e.Graphics.DrawLine(new Pen(Brushes.White), 17, 9, 26, 18);
                e.Graphics.DrawLine(new Pen(Brushes.White), 17, 18, 26, 9);
            }

            if (Is_Leave)
            {
                //Draw the Black X
                e.Graphics.DrawLine(new Pen(Color.Black), 17, 9, 26, 18);
                e.Graphics.DrawLine(new Pen(Color.Black), 17, 18, 26, 9);
            }

            if (Is_Base)
            {
                //Draw the X shadows
                e.Graphics.DrawLine(new Pen(Brushes.LightGray, 2), 17, 9, 26, 18);
                e.Graphics.DrawLine(new Pen(Brushes.LightGray, 2), 17, 18, 26, 9);

                //Draw the Black X
                e.Graphics.DrawLine(new Pen(Brushes.Black, 1), 17, 9, 26, 18);
                e.Graphics.DrawLine(new Pen(Brushes.Black, 1), 17, 18, 26, 9);
            }

            if (Is_Down)
            {
                //Draw the White X
                e.Graphics.DrawLine(new Pen(Brushes.White, 1), 17, 9, 26, 18);
                e.Graphics.DrawLine(new Pen(Brushes.White, 1), 17, 18, 26, 9);
            }
        }
        #endregion

        #region Button Minimize

        private void cmdMinimize_MouseEnter(object sender, EventArgs e)
        {
            cmdMinimize.BackColor = MINIMIZE_COLOR_ENTER;
            componentColorAlpha_Minimize = 255;
            tmrMinimizeLeave.Enabled = false;
        }

        private void cmdMinimize_MouseLeave(object sender, EventArgs e)
        {
            tmrMinimizeLeave.Enabled = true;
        }

        private void cmdMinimize_MouseDown(object sender, MouseEventArgs e)
        {
            cmdMinimize.BackColor = MINIMIZE_COLOR_DOWN;
        }

        private void cmdMinimize_MouseClick(object sender, MouseEventArgs e)
        {
            // Change minimized windows state
            Form.ActiveForm.WindowState = FormWindowState.Minimized;
        }

        private void cmdMinimize_Paint(object sender, PaintEventArgs e)
        {
            // Draw the line
            e.Graphics.DrawLine(new Pen(Brushes.Black, 1), 17, 14, 27, 14);
        }

        #endregion

        #region Button Maximize

        private void cmdMaximize_MouseClick(object sender, EventArgs e)
        {
            // Windows state control

            if (Form.ActiveForm.WindowState == FormWindowState.Normal)
                Form.ActiveForm.WindowState = FormWindowState.Maximized;
            else
                Form.ActiveForm.WindowState = FormWindowState.Normal;
        }

        private void cmdMaximize_MouseDown(object sender, MouseEventArgs e)
        {
            cmdMaximize.BackColor = MAXIMIZE_COLOR_DOWN;
        }

        private void cmdMaximize_MouseEnter(object sender, EventArgs e)
        {
            cmdMaximize.BackColor = MAXIMIZE_COLOR_ENTER;
            componentColorAlpha_Maximize = 255;
            tmrMaximizeLeave.Enabled = false;
        }

        private void cmdMaximize_MouseLeave(object sender, EventArgs e)
        {
            tmrMaximizeLeave.Enabled = true;
        }

        private void cmdMaximize_Paint(object sender, PaintEventArgs e)
        {
            // Draw the square
            e.Graphics.DrawRectangle(new Pen(Brushes.Black, 1), 18, 9, 9, 9);
        }
        #endregion

        #endregion

        #region Override

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!Is_MaxSize)
                e.Graphics.DrawRectangle(new Pen(BORDER_COLOR, 1), 0, 0, this.Width - 1, this.Height - 1);
            // Title bar
            e.Graphics.FillRectangle((new SolidBrush(TitleBarColor)), 1, 1, this.Width - 2, 30);
            // Adjust Title Bar Title
            if (TitleIconVisible) // Draw the title when the icon is enabled 
                lblTitre.SetBounds(picIcon.Location.X + picIcon.Width + 3, 16 - lblTitre.Size.Height / 2, lblTitre.Width, lblTitre.Height);
            else // Draw the title when the icon is disabled
                lblTitre.SetBounds(5, 16 - lblTitre.Size.Height / 2, lblTitre.Width, lblTitre.Height);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message m)
        {
            int x = (int)(m.LParam.ToInt64() & 0xFFFF);               //get x mouse position
            int y = (int)((m.LParam.ToInt64() & 0xFFFF0000) >> 16);   //get y mouse position  you can gave (x,y) it from "MouseEventArgs" too
            Point pt = PointToClient(new Point(x, y));

            switch (m.Msg)
            {
                case WM_NCPAINT:         //box shadow
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 1,
                            rightWidth = 1,
                            topHeight = 1
                        };
                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);

                    }
                    break;

                case WM_LBUTTONDBLCLK: //double click
                    {
                        if (!cmdMaximize.Visible) return;
                    }
                    break;

                case WM_NCHITTEST:
                    {
                        if ((Is_MaxSize == false) && (cmdMaximize.Visible == true))
                            switch (resize.getMosuePosition(pt, this))
                            {
                                case "l": m.Result = (IntPtr)10; return;  // the Mouse on Left Form
                                case "r": m.Result = (IntPtr)11; return;  // the Mouse on Right Form
                                case "a": m.Result = (IntPtr)12; return;
                                case "la": m.Result = (IntPtr)13; return;
                                case "ra": m.Result = (IntPtr)14; return;
                                case "u": m.Result = (IntPtr)15; return;
                                case "lu": m.Result = (IntPtr)16; return;
                                case "ru": m.Result = (IntPtr)17; return; // the Mouse on Right_Under Form
                                                                          //case "": m.Result = pt.Y < 32 /*mouse on title Bar */ ? (IntPtr)2 : (IntPtr)1; return;
                            }

                        base.WndProc(ref m);       //drag the windoes
                        if ((int)m.Result == HTCLIENT)
                        {
                            if (pt.Y < cCaption)
                                m.Result = (IntPtr)HTCAPTION;
                            return;
                        }
                    }
                    break;

                default:
                    break;

            }
            base.WndProc(ref m);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title_Button_Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (this.WindowState == FormWindowState.Maximized)
                Is_MaxSize = true;
            else
                Is_MaxSize = false;

            Title_Button_Dispose();

            this.Refresh();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();

                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            // chanhe the window state

            if (cmdMaximize.Visible)
            {
                if (Form.ActiveForm.WindowState == FormWindowState.Maximized)
                    Form.ActiveForm.WindowState = FormWindowState.Normal;
                else
                    Form.ActiveForm.WindowState = FormWindowState.Maximized;
            }

            this.Refresh();
        }

        #endregion

        #region Core

        private void Title_Button_Dispose()
        {
            // Change the buttons position int the title bar according to the form state

            if ((cmdClose.Visible) && (cmdMaximize.Visible) && (cmdMinimize.Visible))
            {
                cmdClose.Location = new Point(this.Width - 46, 1);
                cmdMaximize.Location = new Point(this.Width - 92, 1);
                cmdMinimize.Location = new Point(this.Width - 138, 1);
            }

            if ((!cmdClose.Visible) && (cmdMaximize.Visible) && (cmdMinimize.Visible))
            {
                cmdMaximize.Location = new Point(this.Width - 46, 1);
                cmdMinimize.Location = new Point(this.Width - 92, 1);
            }

            if ((!cmdClose.Visible) && (!cmdMaximize.Visible) && (cmdMinimize.Visible))
            {
                cmdMinimize.Location = new Point(this.Width - 46, 1);
            }

            if ((cmdClose.Visible) && (!cmdMaximize.Visible) && (cmdMinimize.Visible))
            {
                cmdClose.Location = new Point(this.Width - 46, 1);
                cmdMinimize.Location = new Point(this.Width - 92, 1);
            }

            if ((cmdClose.Visible) && (cmdMaximize.Visible) && (!cmdMinimize.Visible))
            {
                cmdClose.Location = new Point(this.Width - 46, 1);
                cmdMaximize.Location = new Point(this.Width - 92, 1);
            }

            if ((!cmdClose.Visible) && (cmdMaximize.Visible) && (!cmdMinimize.Visible))
            {
                cmdMaximize.Location = new Point(this.Width - 46, 0);
            }
        }

        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }

        #endregion

        #region Init Class

        public Windows10()
        {
            components = null;

            m_aeroEnabled = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new System.Windows.Forms.Padding(1);
            this.cmdClose = new System.Windows.Forms.Panel();
            this.cmdMaximize = new System.Windows.Forms.Panel();
            this.cmdMinimize = new System.Windows.Forms.Panel();
            this.lblTitre = new System.Windows.Forms.Label();
            this.picIcon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdClose
            // 
            this.cmdClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdClose.BackColor = System.Drawing.Color.Transparent;
            //this.cmdClose.Location = new System.Drawing.Point(this.Width - 46, 1);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(45, 29);
            this.cmdClose.TabIndex = 12;
            // 
            // cmdMaximize
            // 
            this.cmdMaximize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdMaximize.BackColor = System.Drawing.Color.Transparent;
            //this.cmdMaximize.Location = new System.Drawing.Point(this.Width - 92, 1);
            this.cmdMaximize.Name = "cmdMaximize";
            this.cmdMaximize.Size = new System.Drawing.Size(45, 29);
            this.cmdMaximize.TabIndex = 11;
            // 
            // cmdMinimize
            // 
            this.cmdMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdMinimize.BackColor = System.Drawing.Color.Transparent;
            //this.cmdMinimize.Location = new System.Drawing.Point(this.Width - 138, 1);
            this.cmdMinimize.Name = "cmdMinimize";
            this.cmdMinimize.Size = new System.Drawing.Size(45, 29);
            this.cmdMinimize.TabIndex = 10;
            // 
            // lblTitre
            // 
            this.lblTitre.AutoSize = true;
            this.lblTitre.BackColor = System.Drawing.Color.Transparent;
            this.lblTitre.Location = new System.Drawing.Point(34, 10);
            this.lblTitre.Name = "lblTitre";
            this.lblTitre.Size = new System.Drawing.Size(28, 13);
            this.lblTitre.TabIndex = 9;
            this.lblTitre.Text = "Titre";
            this.lblTitre.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // picIcon
            // 
            this.picIcon.BackColor = System.Drawing.Color.Gray;
            this.picIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picIcon.Location = new System.Drawing.Point(1, 1);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(30, 30);
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picIcon.TabIndex = 8;
            this.picIcon.TabStop = false;
            // 
            // MyWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmdClose);
            this.Controls.Add(this.cmdMaximize);
            this.Controls.Add(this.cmdMinimize);
            this.Controls.Add(this.lblTitre);
            this.Controls.Add(this.picIcon);
            //this.MinimumSize = new System.Drawing.Size(600, 450);
            //this.Name = "MyWindow";
            //this.Size = new System.Drawing.Size(500, 300);            
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            Mouse_Entry_Setting();
            Timer_Entry_Setting();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }

    #endregion

    #region Windos 10 NoBorder Templeate
     
    /// <summary>
    /// Make a windows 10 form template using a user control
    /// </summary>

    public class Windows10_Templeate_NoBorder : UserControl
    {       
        #region Variables

        public System.Windows.Forms.Panel cmdClose;
        public System.Windows.Forms.Panel cmdMaximize;
        public System.Windows.Forms.Panel cmdMinimize;
        public System.Windows.Forms.Label lblTitre;
        public System.Windows.Forms.PictureBox picIcon;

        private const int WM_NCHITTEST = 0x84;          // variables to dragging form
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        Boolean Is_Enter = false;
        Boolean Is_Leave = false;
        Boolean Is_Base = true;
        Boolean Is_Down = false;
        public Boolean Is_MaxSize = false;

        private Point diff;

        private System.ComponentModel.IContainer components;

        Color _titleBarColor = TITLEBAR_COLOR;   // Title Bar Color
        Color _containerColor = CONTAINER_COLOR; // Container Color
        Color _titleColor = TITLE_COLOR;         // Title Text Color
        string _titleText = "Titre";             // Title text
        Font _titleFont;                         // Title font
        Image _titleImage;                       // Icon image
        ImageLayout _titleImageLayout;           // Icon layout

        Boolean _titleVisible = true;            // Title enabled/disbled
        Boolean _titleIconVisible = true;        // Icon enabled/disbled
        Boolean _buttonMinimizeVisible = true;   // Button Close  enabled/disbled
        Boolean _buttonMaximizeVisible = true;   // Button Maximize  enabled/disbled
        Boolean _buttonCloseVisible = true;      // Button Minimize  enabled/disbled

        public Color TitleBarColor
        {
            get { return _titleBarColor; }
            set { _titleBarColor = value; this.Refresh(); }
        }
        public Color ContainerColor
        {
            get { return _containerColor; }
            set { _containerColor = this.BackColor = value; this.Refresh(); }
        }
        public Color TitleColor
        {
            get { return _titleColor; }
            set { _titleColor = lblTitre.ForeColor = value; this.Refresh(); }
        }
        public Font TitleFont
        {
            get { return _titleFont; }
            set { _titleFont = value; lblTitre.Font = value; this.Refresh(); }
        }
        public string TitleText
        {
            get { return _titleText; }
            set { _titleText = lblTitre.Text = value; this.Refresh(); }
        }
        public Image TitleImage
        {
            get { return _titleImage; }
            set
            {
                _titleImage = picIcon.BackgroundImage = value;
                if (picIcon.BackgroundImage == null)
                    picIcon.BackColor = Color.Gray;
                else
                    picIcon.BackColor = Color.Transparent;

                this.Refresh();
            }
        }
        public ImageLayout TitleImageLayout
        {
            get { return _titleImageLayout; }
            set { _titleImageLayout = picIcon.BackgroundImageLayout = value; this.Refresh(); }
        }

        public Boolean TitleVisible
        {
            get { return _titleVisible; }
            set { _titleVisible = lblTitre.Visible = value; this.Refresh(); }
        }
        public Boolean TitleIconVisible
        {
            get { return _titleIconVisible; }
            set { _titleIconVisible = picIcon.Visible = value; this.Refresh(); }
        }
        public Boolean ButtonMinimizeVisible
        {
            get { return _buttonMinimizeVisible; }
            set { _buttonMinimizeVisible = cmdMinimize.Visible = value; Title_Button_Dispose(); this.Refresh(); }
        }
        public Boolean ButtonMaximizeVisible
        {
            get { return _buttonMaximizeVisible; }
            set { _buttonMaximizeVisible = cmdMaximize.Visible = value; Title_Button_Dispose(); this.Refresh(); }
        }
        public Boolean ButtonCloseVisible
        {
            get { return _buttonCloseVisible; }
            set { _buttonCloseVisible = cmdClose.Visible = value; Title_Button_Dispose(); this.Refresh(); }
        }

        // Color of the buttons

        static readonly Color TITLEBAR_COLOR = Color.White;
        static readonly Color TITLE_COLOR = Color.Black;
        static readonly Color CONTAINER_COLOR = System.Drawing.SystemColors.Control;

        static readonly Color CLOSE_COLOR_NORMAL = Color.Transparent;
        static readonly Color CLOSE_COLOR_ENTER = Color.Red;
        static readonly Color CLOSE_COLOR_LEAVE = Color.White;
        static readonly Color CLOSE_COLOR_DOWN = Color.Violet;

        static readonly Color MAXIMIZE_COLOR_NORMAL = Color.Transparent;
        static readonly Color MAXIMIZE_COLOR_ENTER = Color.LightGray;
        static readonly Color MAXIMIZE_COLOR_LEAVE = Color.White;
        static readonly Color MAXIMIZE_COLOR_DOWN = Color.Gray;

        static readonly Color MINIMIZE_COLOR_NORMAL = Color.Transparent;
        static readonly Color MINIMIZE_COLOR_ENTER = Color.LightGray;
        static readonly Color MINIMIZE_COLOR_LEAVE = Color.White;
        static readonly Color MINIMIZE_COLOR_DOWN = Color.Gray;

        // Animation Button timers

        System.Windows.Forms.Timer tmrCloseLeave = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer tmrMaximizeLeave = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer tmrMinimizeLeave = new System.Windows.Forms.Timer();

        int componentColorAlpha_Close = 0;
        int componentColorAlpha_Maximize = 0;
        int componentColorAlpha_Minimize = 0;

        #endregion

        #region Button Init

        private void Mouse_Entry_Setting()
        {
            // Mouse Init Events Setting

            // Close
            cmdClose.MouseClick += new MouseEventHandler(cmdClose_MouseClick);
            cmdClose.MouseEnter += new EventHandler(cmdClose_MouseEnter);
            cmdClose.MouseDown += new MouseEventHandler(cmdClose_MouseDown);
            cmdClose.MouseLeave += new EventHandler(cmdClose_MouseLeave);
            cmdClose.Paint += new PaintEventHandler(cmdClose_Paint);

            // Maximize
            cmdMaximize.MouseClick += new MouseEventHandler(cmdMaximize_MouseClick);
            cmdMaximize.MouseEnter += new EventHandler(cmdMaximize_MouseEnter);
            cmdMaximize.MouseDown += new MouseEventHandler(cmdMaximize_MouseDown);
            cmdMaximize.MouseLeave += new EventHandler(cmdMaximize_MouseLeave);
            cmdMaximize.Paint += new PaintEventHandler(cmdMaximize_Paint);

            //Minimize
            cmdMinimize.MouseClick += new MouseEventHandler(cmdMinimize_MouseClick);
            cmdMinimize.MouseEnter += new EventHandler(cmdMinimize_MouseEnter);
            cmdMinimize.MouseDown += new MouseEventHandler(cmdMinimize_MouseDown);
            cmdMinimize.MouseLeave += new EventHandler(cmdMinimize_MouseLeave);
            cmdMinimize.Paint += new PaintEventHandler(cmdMinimize_Paint);
        }
        #endregion

        #region Init Timers

        private void Timer_Entry_Setting()
        {
            // Init Button Timers for color animation

            // Interval
            tmrCloseLeave.Interval = 50;
            tmrMaximizeLeave.Interval = 50;
            tmrMinimizeLeave.Interval = 50;

            //Events Timers
            tmrCloseLeave.Tick += new EventHandler(tmrCloseLeave_Tick);
            tmrMaximizeLeave.Tick += new EventHandler(tmrMaximizeLeave_Tick);
            tmrMinimizeLeave.Tick += new EventHandler(tmrMinimizeLeave_Tick);
        }
        #endregion

        #region Timers

        private void tmrCloseLeave_Tick(object sender, EventArgs e)
        {
            // Change button close color when mouse leave

            {
                componentColorAlpha_Close -= 85;
                cmdClose.BackColor = Color.FromArgb(componentColorAlpha_Close, CLOSE_COLOR_ENTER);
            }

            if (componentColorAlpha_Close <= 0) // Stop Animation
            {
                tmrCloseLeave.Enabled = false;
                Is_Base = true;
                Is_Leave = false;
                cmdClose.Refresh();
            }
        }

        private void tmrMaximizeLeave_Tick(object sender, EventArgs e)
        {
            // Change button maximize color when mouse leave

            if (componentColorAlpha_Maximize <= 0) // Stop Animation
                tmrMaximizeLeave.Enabled = false;
            else
            {
                componentColorAlpha_Maximize -= 85;
                cmdMaximize.BackColor = Color.FromArgb(componentColorAlpha_Maximize, MAXIMIZE_COLOR_ENTER);
            }
        }

        private void tmrMinimizeLeave_Tick(object sender, EventArgs e)
        {
            // Change button minimize color when mouse leave

            if (componentColorAlpha_Minimize <= 0) // Stop Animation
                tmrMinimizeLeave.Enabled = false;
            else
            {
                componentColorAlpha_Minimize -= 85;
                cmdMinimize.BackColor = Color.FromArgb(componentColorAlpha_Minimize, MINIMIZE_COLOR_ENTER);
            }
        }

        #endregion

        #region Button's Evenement

        #region Button Close

        private void cmdClose_MouseEnter(object sender, EventArgs e)
        {
            Is_Enter = true;
            Is_Leave = false;
            Is_Base = false;
            Is_Down = false;
            cmdClose.Refresh();
            cmdClose.BackColor = CLOSE_COLOR_ENTER;
            componentColorAlpha_Close = 255;
            tmrCloseLeave.Enabled = false;
        }

        private void cmdClose_MouseLeave(object sender, EventArgs e)
        {
            Is_Enter = false;
            Is_Leave = true;
            Is_Base = false;
            Is_Down = false; ;
            cmdClose.Refresh();
            tmrCloseLeave.Enabled = true;
        }

        private void cmdClose_MouseDown(object sender, MouseEventArgs e)
        {
            Is_Enter = false;
            Is_Leave = false;
            Is_Base = false;
            Is_Down = true;
            cmdClose.BackColor = CLOSE_COLOR_DOWN;
        }

        private void cmdClose_MouseClick(object sender, EventArgs e)
        {
            Form.ActiveForm.Close(); // Close window
        }

        private void cmdClose_Paint(object sender, PaintEventArgs e)
        {
            if (Is_Enter)
            {
                //Draw the White X
                e.Graphics.DrawLine(new Pen(Brushes.White), 17, 9, 26, 18);
                e.Graphics.DrawLine(new Pen(Brushes.White), 17, 18, 26, 9);
            }

            if (Is_Leave)
            {
                //Draw the Black X
                e.Graphics.DrawLine(new Pen(Color.Black), 17, 9, 26, 18);
                e.Graphics.DrawLine(new Pen(Color.Black), 17, 18, 26, 9);
            }

            if (Is_Base)
            {
                //Draw the X shadows
                e.Graphics.DrawLine(new Pen(Brushes.LightGray, 2), 17, 9, 26, 18);
                e.Graphics.DrawLine(new Pen(Brushes.LightGray, 2), 17, 18, 26, 9);

                //Draw the Black X
                e.Graphics.DrawLine(new Pen(Brushes.Black, 1), 17, 9, 26, 18);
                e.Graphics.DrawLine(new Pen(Brushes.Black, 1), 17, 18, 26, 9);
            }

            if (Is_Down)
            {
                //Draw the White X
                e.Graphics.DrawLine(new Pen(Brushes.White, 1), 17, 9, 26, 18);
                e.Graphics.DrawLine(new Pen(Brushes.White, 1), 17, 18, 26, 9);
            }
        }
        #endregion

        #region Button Minimize

        private void cmdMinimize_MouseEnter(object sender, EventArgs e)
        {
            cmdMinimize.BackColor = MINIMIZE_COLOR_ENTER;
            componentColorAlpha_Minimize = 255;
            tmrMinimizeLeave.Enabled = false;
        }

        private void cmdMinimize_MouseLeave(object sender, EventArgs e)
        {
            tmrMinimizeLeave.Enabled = true;
        }

        private void cmdMinimize_MouseDown(object sender, MouseEventArgs e)
        {
            cmdMinimize.BackColor = MINIMIZE_COLOR_DOWN;
        }

        private void cmdMinimize_MouseClick(object sender, MouseEventArgs e)
        {
            // Change the windows state to minimized
            Form.ActiveForm.WindowState = FormWindowState.Minimized;
        }

        private void cmdMinimize_Paint(object sender, PaintEventArgs e)
        {
            // Draw the line
            e.Graphics.DrawLine(new Pen(Brushes.Black, 1), 17, 14, 27, 14);
        }

        #endregion

        #region Button Maximize

        private void cmdMaximize_MouseClick(object sender, EventArgs e)
        {
            if (Form.ActiveForm.WindowState == FormWindowState.Normal)
            {
                Form.ActiveForm.WindowState = FormWindowState.Maximized;
                Is_MaxSize = true;
            }
            else
            {
                Form.ActiveForm.WindowState = FormWindowState.Normal;
                Is_MaxSize = false;
            }

            this.Refresh();
        }

        private void cmdMaximize_MouseDown(object sender, MouseEventArgs e)
        {
            cmdMaximize.BackColor = MAXIMIZE_COLOR_DOWN;
        }

        private void cmdMaximize_MouseEnter(object sender, EventArgs e)
        {
            cmdMaximize.BackColor = MAXIMIZE_COLOR_ENTER;
            componentColorAlpha_Maximize = 255;
            tmrMaximizeLeave.Enabled = false;
        }

        private void cmdMaximize_MouseLeave(object sender, EventArgs e)
        {
            tmrMaximizeLeave.Enabled = true;
        }

        private void cmdMaximize_Paint(object sender, PaintEventArgs e)
        {
            // Draw the square
            e.Graphics.DrawRectangle(new Pen(Brushes.Black, 1), 18, 9, 9, 9);
        }
        #endregion

        #endregion

        #region Override

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Title bar
            e.Graphics.FillRectangle((new SolidBrush(TitleBarColor)), this.Location.X, this.Location.Y, this.Width, this.Location.Y + 28);
            // Adjust Title Bar Title
            if (TitleIconVisible) // Draw the title when the icon is enabled 
                lblTitre.SetBounds(picIcon.Location.X + picIcon.Width + 3, 14 - lblTitre.Size.Height / 2, lblTitre.Width, lblTitre.Height);
            else // Draw the title when the icon is disabled
                lblTitre.SetBounds(1, 14 - lblTitre.Size.Height / 2, lblTitre.Width, lblTitre.Height);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            // change the window position
            if (e.Button == MouseButtons.Left)
            {
                Point mouse_loc = Control.MousePosition;
                mouse_loc.Offset(diff.X, diff.Y);
                Form.ActiveForm.Location = mouse_loc;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            // change the mouse position
            diff = new Point(-e.X, -e.Y);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            // chanhe the window state

            if (cmdMaximize.Visible)
            {
                if (Form.ActiveForm.WindowState == FormWindowState.Maximized)
                    Form.ActiveForm.WindowState = FormWindowState.Normal;
                else
                    Form.ActiveForm.WindowState = FormWindowState.Maximized;
            }
            this.Refresh();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Core

        private void Title_Button_Dispose()
        {
            // Change the buttons position int the title bar

            if ((cmdClose.Visible) && (cmdMaximize.Visible) && (cmdMinimize.Visible))
            {
                cmdClose.Location = new Point(this.Width - 45, 0);
                cmdMaximize.Location = new Point(this.Width - 91);
                cmdMinimize.Location = new Point(this.Width - 137, 0);
            }

            if ((!cmdClose.Visible) && (cmdMaximize.Visible) && (cmdMinimize.Visible))
            {
                cmdMaximize.Location = new Point(this.Width - 45);
                cmdMinimize.Location = new Point(this.Width - 91, 0);
            }

            if ((!cmdClose.Visible) && (!cmdMaximize.Visible) && (cmdMinimize.Visible))
            {
                cmdMinimize.Location = new Point(this.Width - 45, 0);
            }

            if ((cmdClose.Visible) && (!cmdMaximize.Visible) && (cmdMinimize.Visible))
            {
                cmdClose.Location = new Point(this.Width - 45, 0);
                cmdMinimize.Location = new Point(this.Width - 91, 0);
            }

            if ((cmdClose.Visible) && (cmdMaximize.Visible) && (!cmdMinimize.Visible))
            {
                cmdClose.Location = new Point(this.Width - 45, 0);
                cmdMaximize.Location = new Point(this.Width - 91, 0);
            }

            if ((!cmdClose.Visible) && (cmdMaximize.Visible) && (!cmdMinimize.Visible))
            {
                cmdMaximize.Location = new Point(this.Width - 45, 0);
            }
        }

        #endregion

        public Windows10_Templeate_NoBorder()
        {
            components = null;

            this.cmdClose = new System.Windows.Forms.Panel();
            this.cmdMaximize = new System.Windows.Forms.Panel();
            this.cmdMinimize = new System.Windows.Forms.Panel();
            this.lblTitre = new System.Windows.Forms.Label();
            this.picIcon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdClose
            // 
            this.cmdClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdClose.BackColor = System.Drawing.Color.Transparent;
            this.cmdClose.Location = new System.Drawing.Point(468, 0);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(45, 29);
            this.cmdClose.TabIndex = 12;
            // 
            // cmdMaximize
            // 
            this.cmdMaximize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdMaximize.BackColor = System.Drawing.Color.Transparent;
            this.cmdMaximize.Location = new System.Drawing.Point(422, 0);
            this.cmdMaximize.Name = "cmdMaximize";
            this.cmdMaximize.Size = new System.Drawing.Size(45, 29);
            this.cmdMaximize.TabIndex = 11;
            // 
            // cmdMinimize
            // 
            this.cmdMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdMinimize.BackColor = System.Drawing.Color.Transparent;
            this.cmdMinimize.Location = new System.Drawing.Point(376, 0);
            this.cmdMinimize.Name = "cmdMinimize";
            this.cmdMinimize.Size = new System.Drawing.Size(45, 29);
            this.cmdMinimize.TabIndex = 10;
            // 
            // lblTitre
            // 
            this.lblTitre.AutoSize = true;
            this.lblTitre.BackColor = System.Drawing.Color.Transparent;
            this.lblTitre.Location = new System.Drawing.Point(33, 9);
            this.lblTitre.Name = "lblTitre";
            this.lblTitre.Size = new System.Drawing.Size(28, 13);
            this.lblTitre.TabIndex = 9;
            this.lblTitre.Text = "Titre";
            this.lblTitre.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // picIcon
            // 
            this.picIcon.BackColor = System.Drawing.Color.Gray;
            this.picIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picIcon.Location = new System.Drawing.Point(0, 0);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(30, 30);
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picIcon.TabIndex = 8;
            this.picIcon.TabStop = false;
            // 
            // MyWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.cmdClose);
            this.Controls.Add(this.cmdMaximize);
            this.Controls.Add(this.cmdMinimize);
            this.Controls.Add(this.lblTitre);
            this.Controls.Add(this.picIcon);
            this.MinimumSize = new System.Drawing.Size(300, 150);
            this.Name = "MyWindow";
            this.Size = new System.Drawing.Size(513, 292);
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
            Mouse_Entry_Setting();
            Timer_Entry_Setting();
            Dock = DockStyle.Fill;
        }
    }

    #endregion

    #region Windos 10 With Border Templeate

    /// <summary>
    /// Make a windows 10 form template using a user control without border
    /// </summary>

    public class Windows10_Templeate_WithBorder : Windows10_Templeate_NoBorder
    {
        static readonly Color BORDER_COLOR = Color.FromArgb(24, 131, 215);
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Title bar
            e.Graphics.FillRectangle((new SolidBrush(TitleBarColor)), this.Location.X, this.Location.Y, this.Width, this.Location.Y + 30);
            // Adjust Title Bar Title
            if (TitleIconVisible) // Draw the title when the icon is enabled 
                lblTitre.SetBounds(picIcon.Location.X + picIcon.Width + 3, 14 - lblTitre.Size.Height / 2, lblTitre.Width, lblTitre.Height);
            else // Draw the title when the icon is disabled
                lblTitre.SetBounds(1, 14 - lblTitre.Size.Height / 2, lblTitre.Width, lblTitre.Height);
            if (!Is_MaxSize)
                e.Graphics.DrawRectangle(new Pen(BORDER_COLOR, 1), 0, 0, this.Width - 1, this.Height - 1);
        }

        public Windows10_Templeate_WithBorder(): base()
        {}
    }

    #endregion

    #region Animation Components

    /// <summary>
    /// Compenent's animation
    /// This animation allow you to expand /reduce your componets (buttons, user controls etc..) in all direction (left, right, button, top)
    /// </summary>
    /// 
    public class AnimationComponents : Component
    {
        #region Variables

        // Animated Object
        private System.Windows.Forms.Control _object = null;

        public System.Windows.Forms.Control Object
        {
            get { return _object; }
            set { _object = value; if (_object != null) Init(); }
        }

        // Timers

        private System.Windows.Forms.Timer tmrOpen;
        private System.Windows.Forms.Timer tmrClose;

        public enum POSITION
        {
            In_The_Left,
            In_The_Right,
            In_The_Top,
            In_The_Bottom
        }

        // Define animation type
        public enum ANIMATION
        {
            Static, // animation dont change position and values when resize form
            Dinamic // change animations values when form is resized (magnetic option)
        }

        // Define animation when loading (necessary)
        public enum ON_LOAD
        {
            Is_Opened,
            Is_Closed
        }

        public struct OPTIONS
        {
            public ON_LOAD OnLoad;
            public POSITION Position;
            public ANIMATION Animation;
            public Boolean Magnetic;
            public int Size;
            public int OpenStep;
            public int CloseStep;
            public int OpenSpeed;
            public int CloseSpeed;
            public int OpenBearingSize;
            public int CloseBearingSize;
        }

        private ON_LOAD _onload;              // OnLoad options
        private POSITION _position;           // Animation side (left, right, top, down)
        private ANIMATION _animation;         // Define the animation type ( static, dinamic)
        private int _closeStep;               // Close step (pixel)
        private int _closeSpeed;              // Close speed
        private int _openStep;                // Close step (pixel)
        private int _openSpeed;               // Open speed
        private int _openBearingSize;         // Open bearing size (pixel) 
        private int _closeBearingSize;        // Close bearing size (pixel) 
        private int _size;                    // Open/Close size (pixel)
        private int _maxLimit;                // Animation max limit (automatically calculated)
        private int _minLimit;                // Animation min limit (automatically calculated) 
        private int _maxSize;                 // Animation max size (automatically calculated)  
        private int _openBearingPosition;     // Open bearing position
        private int _closeBearingPosition;    // Close bearing position
        private int _walkStep;                // Walk step.
        private int _stopPosition;            // Animation stop position (automatically calculated)
        private Boolean _magnetic;            // Define if the animation must be static in the area or must mouve when resize form
        private Boolean _isOperating;         // Check if the animation is on
        private Boolean _isOpened;            // Check if animation is in open or closed
        private AnchorStyles OldAnhor;        // Remember old anchor style when use magnetic option

        // Original object dimension and position
        public int LocationX;
        public int LocationY;
        public int Width;
        public int Height;

        // Return the max size of the animation [ only in closed mode]
        public int MaxSize { get { return _maxSize; } }
        
        // Return the max position where the open animation will be finished
        public int MaxLimit { get { return _maxLimit; } }
        
        // Return the min position where the close animatin will be finished
        public int MinLimit { get { return _minLimit; } }
        
        // Return the start entry poistion of the Bearing in the open animation
        public int OpenBearingPosition { get { return _openBearingPosition; } }
        
        // Return the start entry position of the Bearing in the close animation
        public int CloseBearingPosition { get { return _closeBearingPosition; } }
        
        // Return the step of the animation when open or close
        public int WalkStep { get { return _walkStep; } }
        
        // Return True if the Container is Opened
        public Boolean Is_Opened
        {
            get { return _isOpened; }
            set { _isOpened = value; }
        }
        public Boolean Is_Opening;
        public Boolean Is_Closing;

        // Return True if the Animation is working
        public Boolean Is_Operating
        {
            get
            {
                return _isOperating;
            }
            set
            {
                if ((Animation == ANIMATION.Dinamic) && (Object != null))
                    if (value == true)
                    {
                        Object.SizeChanged -= new System.EventHandler(this.DinamicSize);
                        Object.LocationChanged -= new System.EventHandler(this.DinamicLocation);
                    }
                    else
                    {
                        Object.SizeChanged += new System.EventHandler(this.DinamicSize);
                        Object.LocationChanged += new System.EventHandler(this.DinamicLocation);
                    }
                _isOperating = value;
            }
        }
        
        // Define where the animation will be developed[Left, Right, Top, Bottom]
        public POSITION Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                RefreshEl();
                Set_Timers();
            }
        }
        
        // Define if the animation is closed or opened at the first time
        public ON_LOAD OnLoad
        {
            get
            {
                return _onload;
            }
            set
            {
                _onload = value;
                Check_Status();
                RefreshEl();
            }
        }

        // Define if the animation is Static[the size and position not change with the control] or Dinamic[the size and position change with the control] 
        public ANIMATION Animation
        {
            get
            {
                return _animation;
            }
            set
            {
                _animation = value;
            }
        }
        
        // The size of the animation
        public int Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                RefreshEl();
            }
        }
        
        // Define the step of the animation when open
        public int OpenStep
        {
            get
            {
                return _openStep;
            }
            set
            {
                _openStep = value;
            }
        }
        
        // Define the step of the animation when close
        public int CloseStep
        {
            get
            {
                return _closeStep;
            }
            set
            {
                _closeStep = value;
            }
        }
        
        // Define the speed animation when open
        public int OpenSpeed
        {
            get
            {
                return _openSpeed;
            }
            set
            {
                if (value == 0) _openSpeed = 1;
                else _openSpeed = value;
            }
        }
        
        // Define the speed animation when close
        public int CloseSpeed
        {
            get
            {
                return _closeSpeed;
            }
            set
            {
                if (value == 0) _closeSpeed = 1;
                else _closeSpeed = value;
            }
        }
        
        // Define the size of the bearing when animation is opening
        public int OpenBearingSize
        {
            get
            {
                return _openBearingSize;
            }
            set
            {
                _openBearingSize = value;
                RefreshEl();
            }
        }
        
        // Define the size of the bearing when animation is closing
        public int CloseBearingSize
        {
            get
            {
                return _closeBearingSize;
            }
            set
            {
                _closeBearingSize = value;
                RefreshEl();
            }
        }
        
        // Define the position (X or Y) when using the function OpenPanel(position) where the animation will be finisched
        private int StopPosition
        {
            get
            {
                return _stopPosition;
            }
            set
            {
                _stopPosition = value;
                SetPosition();
            }
        }
        
        // Define if the anchor will be added at the end of the animation in the left,right,top or bottom
        public Boolean Magnetic
        {
            get
            {
                return _magnetic;
            }
            set
            {
                _magnetic = value;
            }
        }


        #endregion

        #region Base Animation's Functions

        private void SetDoubleBuffering(System.Windows.Forms.Control control, bool value)
        {
            //Enable double buffeing for ListView Img_List to prevent the flikering 
            System.Reflection.PropertyInfo controlProperty = typeof(System.Windows.Forms.Control)
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            controlProperty.SetValue(control, value, null);
        }
        private void Init()
        {
            // Take object size

            this.SetDoubleBuffering(this.Object, true);
            LocationX = _object.Location.X;
            LocationY = _object.Location.Y;
            Width = _object.Width;
            Height = _object.Height;
            OldAnhor = _object.Anchor;
            if ((Animation == ANIMATION.Dinamic) && (Object != null))
            {
                Object.SizeChanged += new System.EventHandler(this.DinamicSize);
                Object.LocationChanged += new System.EventHandler(this.DinamicLocation);
            }
        }

        // Init pair of timers depending on the animation area
        private void Set_Timers()
        {
            // Left Animation
            tmrOpen = new System.Windows.Forms.Timer();
            tmrClose = new System.Windows.Forms.Timer();

            if (Position == POSITION.In_The_Left)
            {
                tmrOpen.Tick += new EventHandler(tmrOpenLeft_Tick);
                tmrClose.Tick += new EventHandler(tmrCloseRight_Tick);
            }

            // Right Animation

            if (Position == POSITION.In_The_Right)
            {
                tmrOpen.Tick += new EventHandler(tmrOpenRight_Tick);
                tmrClose.Tick += new EventHandler(tmrCloseLeft_Tick);
            }

            // Top Animation

            if (Position == POSITION.In_The_Top)
            {
                tmrOpen.Tick += new EventHandler(tmrOpenUp_Tick);
                tmrClose.Tick += new EventHandler(tmrCloseDown_Tick);
            }

            // Bottom animation

            if (Position == POSITION.In_The_Bottom)
            {
                tmrOpen.Tick += new EventHandler(tmrOpenDown_Tick);
                tmrClose.Tick += new EventHandler(tmrCloseUp_Tick);
            }

            // Timers enable/disable
            tmrOpen.Enabled = false;
            tmrClose.Enabled = false;
        }

        // Check max animation size
        private void Check_MaxSize()
        {
            if (_size > _maxSize)
                _size = _maxSize;
        }

        // Set margins
        private void SetPosition()
        {
            // Left animation
            if (Position == POSITION.In_The_Left)
            {
                if (Is_Opened)
                {
                    if (_stopPosition > Object.Location.X + Object.Width)
                        _minLimit = Object.Location.X + Object.Width;
                    else
                        _minLimit = _stopPosition;
                }
                else
                {
                    _maxLimit = _stopPosition;
                }

                _openBearingPosition = MaxLimit + OpenBearingSize;
                _closeBearingPosition = MinLimit - CloseBearingSize;
            }

            // Right animation
            if (Position == POSITION.In_The_Right)
            {
                if (Is_Opened)
                {
                    if (_stopPosition < Object.Location.X)
                        _minLimit = Object.Location.X;
                    else
                        _minLimit = _stopPosition;
                }
                else
                {
                    _maxLimit = _stopPosition;
                }

                _openBearingPosition = MaxLimit - OpenBearingSize;
                _closeBearingPosition = MinLimit + CloseBearingSize;
            }

            // Top Aniamtion
            if (Position == POSITION.In_The_Top)
            {
                if (Is_Opened)
                {
                    if (_stopPosition > Object.Location.Y + Object.Height)
                        _minLimit = Object.Location.Y + Object.Height;
                    else
                        _minLimit = _stopPosition;
                }
                else
                {
                    _maxLimit = _stopPosition;
                }

                _openBearingPosition = MaxLimit + OpenBearingSize;
                _closeBearingPosition = MinLimit - CloseBearingSize;
            }

            // Bottom animation
            if (Position == POSITION.In_The_Bottom)
            {
                if (Is_Opened)
                {
                    if (_stopPosition < Object.Location.Y)
                        _minLimit = Object.Location.Y;
                    else
                        _minLimit = _stopPosition;
                }
                else
                {
                    _maxLimit = _stopPosition;
                }

                _openBearingPosition = MaxLimit - OpenBearingSize;
                _closeBearingPosition = MinLimit + CloseBearingSize;
            }
        }

        // Define margins (max/min open/close limit) when refreshing
        private void Set_Margins()
        {
            // Left Animation
            if (Position == POSITION.In_The_Left)
            {
                if (Is_Opened)
                {
                    _maxSize = Object.Width;
                    Check_MaxSize();
                    _minLimit = Object.Location.X + Size;
                    _maxLimit = Object.Location.X;
                }
                else
                {
                    _maxSize = Size;
                    _maxLimit = Object.Location.X - Size;
                    _minLimit = Object.Location.X;
                }

                _openBearingPosition = MaxLimit + OpenBearingSize;
                _closeBearingPosition = MinLimit - CloseBearingSize;
            }

            // Right Animation
            if (Position == POSITION.In_The_Right)
            {
                if (Is_Opened)
                {
                    _maxSize = Object.Width;
                    Check_MaxSize();
                    _minLimit = Object.Location.X + Object.Width - Size;
                    _maxLimit = Object.Location.X + Object.Width;
                }
                else
                {
                    _maxSize = Size;
                    _maxLimit = Object.Location.X + Object.Width + Size;
                    _minLimit = Object.Location.X + Object.Width;
                }

                _openBearingPosition = MaxLimit - OpenBearingSize;
                _closeBearingPosition = MinLimit + CloseBearingSize;
            }

            // Top Animation
            if (Position == POSITION.In_The_Top)
            {
                if (Is_Opened)
                {
                    _maxSize = Object.Height;
                    Check_MaxSize();
                    _minLimit = Object.Location.Y + Size;
                    _maxLimit = Object.Location.Y;
                }
                else
                {
                    _maxSize = Size;
                    _maxLimit = Object.Location.Y - Size;
                    _minLimit = Object.Location.Y;
                }

                _openBearingPosition = MaxLimit + OpenBearingSize;
                _closeBearingPosition = MinLimit - CloseBearingSize;
            }

            // Bottom animation
            if (Position == POSITION.In_The_Bottom)
            {

                if (Is_Opened)
                {
                    _maxSize = Object.Height;
                    Check_MaxSize();
                    _minLimit = Object.Location.Y + Object.Height - Size;
                    _maxLimit = Object.Location.Y + Object.Height;
                }
                else
                {
                    _maxSize = Size;
                    _maxLimit = Object.Location.Y + Object.Height + Size;
                    _minLimit = Object.Location.Y + Object.Height;
                }

                _openBearingPosition = MaxLimit - OpenBearingSize;
                _closeBearingPosition = MinLimit + CloseBearingSize;
            }
        }

        // Check animation status (opened/closed)
        private void Check_Status()
        {
            if (OnLoad == ON_LOAD.Is_Opened)
            {
                Is_Opened = true;
                Is_Opening = false;
            }
            else
            {
                Is_Opened = false;
                Is_Closing = false;
            }
        }

        // Set margins when refresh
        public void RefreshEl()
        {
            if (Object != null)
            {
                Set_Margins();
            }
        }

        private void DinamicLocation(object sender, EventArgs e)
        {
            Set_Margins();
        }

        private void DinamicSize(object sender, EventArgs e)
        {

            /*
            if (Position == POSITION.In_The_Left)
            {
                diff = Object.Width - Width;                
            }

            // Pour l'animation a droit
            // Per l'animazione a destra

            if (Position == POSITION.In_The_Right)
            {
                diff = Object.Width - Width;
            }

            // Pour l'animation a haut
            // Per l'animazione in alto

            if (Position == POSITION.In_The_Top)
            {
                diff = Object.Height - Height - Size;
            }

            // Pour l'animation en bas
            // Per l'animazione in basso

            if (Position == POSITION.In_The_Bottom)
            {
                diff = Object.Height - Height;
            }

            Set_Margins();
            Set_Timers();*/
        }

        // Start open animation with params
        public Boolean OpenPanel(int Position)
        {
            // Set pair timers and open step size
            if ((Is_Opening == false) || (Is_Opened == false))
            {
                tmrOpen.Interval = OpenSpeed;
                _walkStep = OpenStep;
                StopPosition = Position;
                //Is_Opened = true;
                Is_Operating = true;
                Is_Opening = true;
                Is_Closing = false;
                if (_object != null)
                {
                    tmrOpen.Enabled = true;
                    tmrClose.Enabled = false;
                }
            }
            return true;
        }

        // Start close animation with params
        public Boolean ClosePanel(int Position)
        {
            // Set pair timers and close step size
            if ((Is_Closing == false) && (Is_Opened == true))
            {
                _walkStep = CloseStep;
                tmrClose.Interval = CloseSpeed;
                StopPosition = Position;
                //Is_Opened = false;
                Is_Operating = true;
                Is_Opening = false;
                Is_Closing = true;
                if (_object != null)
                {
                    tmrClose.Enabled = true;
                    tmrOpen.Enabled = false;
                }
            }

            return true;
        }

        // Start open animation
        public Boolean OpenPanel() 
        {
            // Set pair timers and open step size
            tmrOpen.Interval = OpenSpeed;
            _walkStep = OpenStep;
            if ((Is_Opening == false) || (Is_Opened == false))
            {
                //Is_Opened = true;
                Is_Operating = true;
                Is_Opening = true;
                Is_Closing = false;
                if (_object != null)
                {
                    tmrClose.Enabled = false;
                    tmrOpen.Enabled = true;
                }
            }
            return true;
        }

        // Start close animation
        public Boolean ClosePanel()
        {
            // Set pair timers and close step size
            _walkStep = CloseStep;
            tmrClose.Interval = CloseSpeed;
            if ((Is_Closing == false) && (Is_Opened == true))
            {
                //Is_Opened = false;
                Is_Opening = false;
                Is_Closing = true;
                Is_Operating = true;
                if (_object != null)
                {
                    tmrOpen.Enabled = false;
                    tmrClose.Enabled = true;
                }
            }
            return true;
        }

        #endregion

        #region Timer Settings

        // Timers Animation Setings
        // HeventEndler 

        #region Open Timers

        private void OpenEndAnimation()
        {
            tmrOpen.Enabled = false;
            Is_Operating = false;
            Is_Opened = true;
            Is_Opening = false;
            Is_Closing = false;
            //tmrOpen.Dispose();
            this.Dispose(true);
            if (Magnetic)
            {
                OldAnhor = Object.Anchor;
                switch (Position)
                {
                    case POSITION.In_The_Left: Object.Anchor = (Object.Anchor | AnchorStyles.Left); break;
                    case POSITION.In_The_Right: Object.Anchor = (Object.Anchor | AnchorStyles.Right); break;
                    case POSITION.In_The_Top: Object.Anchor = (Object.Anchor | AnchorStyles.Top); break;
                    case POSITION.In_The_Bottom: Object.Anchor = (Object.Anchor | AnchorStyles.Bottom); break;
                }
            }
        }

        private void CloseEndAnimation()
        {
            tmrClose.Enabled = false;
            Is_Operating = false;
            Is_Opened = false;
            Is_Closing = false;
            Is_Opening = false;
            //tmrClose.Dispose();
            this.Dispose(true);
            if (Magnetic)
            {
                Object.Anchor = OldAnhor;
            }
        }

        private void tmrOpenDown_Tick(object sender, EventArgs e)
        {
            if ((MaxLimit - Object.Location.Y + Object.Height) <= OpenStep) // Update step
                _walkStep = MaxLimit - (Object.Location.Y + Object.Height);

            if (Object.Location.Y + Object.Height >= MaxLimit) // main exit condition
                OpenEndAnimation();
            else
                if (Object.Location.Y + Object.Height >= OpenBearingPosition) // bearing condition
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width, Object.Size.Height + 1); // last open animation
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width, Object.Size.Height + WalkStep); // normal open animation part
        }

        private void tmrOpenUp_Tick(object sender, EventArgs e)
        {
            if ((Object.Location.Y - OpenBearingPosition) <= OpenStep) // update step
                _walkStep = Object.Location.Y - OpenBearingPosition;

            if (Object.Location.Y <= MaxLimit) // main exit condition
                OpenEndAnimation();
            else
                if (Object.Location.Y <= OpenBearingPosition) // bearing condition
                Object.SetBounds(Object.Location.X, Object.Location.Y - 1, Object.Size.Width, Object.Size.Height + 1); // last open animation part
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y - WalkStep, Object.Size.Width, Object.Size.Height + WalkStep); // normal open animation part
        }

        private void tmrOpenLeft_Tick(object sender, EventArgs e)
        {
            if ((Object.Location.X - OpenBearingPosition) <= OpenStep) // update step
                _walkStep = Object.Location.X - OpenBearingPosition;

            if (Object.Location.X <= MaxLimit) // main exit condition
                OpenEndAnimation();
            else
                if (Object.Location.X <= OpenBearingPosition) // bearing condition
                Object.SetBounds(Object.Location.X - 1, Object.Location.Y, Object.Size.Width + 1, Object.Size.Height); // last open animation part
            else
                Object.SetBounds(Object.Location.X - WalkStep, Object.Location.Y, Object.Size.Width + WalkStep, Object.Size.Height); // normal open animation part
        }

        private void tmrOpenRight_Tick(object sender, EventArgs e)
        {
            if ((OpenBearingPosition - Object.Location.X + Object.Width) < OpenStep) // update step
                _walkStep = OpenBearingPosition - (Object.Location.X + Object.Width);

            if (Object.Location.X + Object.Width >= MaxLimit) // main exit condition
                OpenEndAnimation();
            else
                if (Object.Location.X + Object.Width >= OpenBearingPosition) // bearing condition
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width + 1, Object.Size.Height); // last open animation part
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width + WalkStep, Object.Size.Height); // normal open animation part
        }

        #endregion

        #region Close Timers

        private void tmrCloseDown_Tick(object sender, EventArgs e)
        {
            if ((CloseBearingPosition - Object.Location.Y) <= CloseStep) // update step
                _walkStep = CloseBearingPosition - Object.Location.Y;

            if (Object.Location.Y >= MinLimit) // main exit condition
                CloseEndAnimation();
            else
                if (Object.Location.Y >= CloseBearingPosition) // bearing condition
                Object.SetBounds(Object.Location.X, Object.Location.Y + 1, Object.Size.Width, Object.Size.Height - 1); // last open animation part
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y + WalkStep, Object.Size.Width, Object.Size.Height - WalkStep); // normal open animation part 
        }

        private void tmrCloseUp_Tick(object sender, EventArgs e)
        {
            if ((Object.Location.Y + Object.Height - CloseBearingPosition) <= CloseStep) // update step
                _walkStep = Object.Location.Y + Object.Height - CloseBearingPosition;

            if (Object.Location.Y + Object.Height <= MinLimit) // main exit condition
                CloseEndAnimation();
            else
                if (Object.Location.Y + Object.Height <= CloseBearingPosition) // bearing condition
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width, Object.Size.Height - 1); // last open animation part
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width, Object.Size.Height - WalkStep); // normal open animation part 
        }

        private void tmrCloseLeft_Tick(object sender, EventArgs e)
        {
            if ((Object.Location.X + Object.Width - CloseBearingPosition) < CloseStep) // update step
                _walkStep = Object.Location.X + Object.Width - CloseBearingPosition;

            if (Object.Location.X + Object.Width <= MinLimit) // main exit condition
                CloseEndAnimation();
            else
                if (Object.Location.X + Object.Width <= CloseBearingPosition) // bearing condition
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width - 1, Object.Size.Height); // last open animation part
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width - WalkStep, Object.Size.Height); // normal open animation part 
        }

        private void tmrCloseRight_Tick(object sender, EventArgs e)
        {
            if ((CloseBearingPosition - Object.Location.X) <= CloseStep) // update step
                _walkStep = CloseBearingPosition - Object.Location.X;

            if (Object.Location.X >= MinLimit) // main exit condition
                CloseEndAnimation();
            else
                if (Object.Location.X >= CloseBearingPosition) // bearing condition
                Object.SetBounds(Object.Location.X + 1, Object.Location.Y, Object.Size.Width - 1, Object.Size.Height); // last open animation part
            else
                Object.SetBounds(Object.Location.X + WalkStep, Object.Location.Y, Object.Size.Width - WalkStep, Object.Size.Height); // Ouverture normal 
        }
        #endregion

        #endregion

        #region Init Class

        // Init animation class

        public AnimationComponents()
        {
            // Create Open/Close timers

            //tmrOpen = new System.Windows.Forms.Timer();
            //tmrClose = new System.Windows.Forms.Timer();
        }

        public AnimationComponents(System.Windows.Forms.Control control, OPTIONS options)
        {

            // Animated object
            Object = control;

            // Create Open/Close timers
            Set_Timers();

            // Animation options
            Position = options.Position;
            OnLoad = options.OnLoad;
            OpenSpeed = options.OpenSpeed;
            OpenStep = options.OpenStep;
            OpenBearingSize = options.OpenBearingSize;
            CloseSpeed = options.CloseSpeed;
            CloseStep = options.CloseStep;
            CloseBearingSize = options.CloseBearingSize;
            Size = options.Size;
            Magnetic = options.Magnetic;
            Animation = options.Animation;
            // Set margins according the animation options
            Set_Margins();
        }

        #endregion
    }

    #endregion

    #region Move Object

    /// <summary>
    /// Move a component in all region
    /// Movement availables: left<->right and top<->down
    /// Normally the movement consists in ten frame. Each frame is a mouvement
    /// </summary>

    public class MoveOBject : Component
    {

        #region Variables

        static readonly int FRAME_NUMBER = 10;
        public event EventHandler OnStartMoving;
        public event EventHandler OnFinishMoving;
        private Boolean _isOperating = false;

        public struct OPTIONS
        {
            public int Interval;
            public TYPE Type;
        }

        private TYPE _type;
        private int _interval;

        public enum TYPE
        {
            Vertical,
            Horizontal
        }

        private System.Windows.Forms.Control _object = null;

        public System.Windows.Forms.Control Object
        {
            get { return _object; }
            set { _object = value; this.SetDoubleBuffering(_object, true); }
        }

        public Boolean IsOperating
        {
            get { return _isOperating; }
            set
            {
                _isOperating = value;
                if (value) OnStart(EventArgs.Empty);
                else OnFinish(EventArgs.Empty);
            }
        }

        public int Interval // define speed
        {
            get { return _interval; }
            set
            {
                if (value == 0)
                {
                    _interval = 100;
                    tmr_AnimationLine.Interval = 100;
                }
                else
                {
                    _interval = value;
                    tmr_AnimationLine.Interval = value;
                }
            }
        }
        private int New_Position;  // end position
        private int Step;          // step size
        private int Frame;         // frame counter     
        private bool Desc = false; // direction left/right or top/down
        public TYPE Type           // type (Vertical / Horizontal)
        {
            get { return _type; }
            set { _type = value; Set_Timer(); }
        }
        private Color New_Color;    // line color

        // Animation's Timers

        public System.Windows.Forms.Timer tmr_AnimationLine;

        #endregion

        #region Functions       

        protected virtual void OnStart(EventArgs e)
        {
            EventHandler handler = OnStartMoving;
            handler?.Invoke(this, e);
        }

        protected virtual void OnFinish(EventArgs e)
        {
            EventHandler handler = OnFinishMoving;
            handler?.Invoke(this, e);
        }

        private void SetDoubleBuffering(System.Windows.Forms.Control control, bool value)
        {
            //Enable double buffeing for ListView Img_List to prevent the flikering 
            System.Reflection.PropertyInfo controlProperty = typeof(System.Windows.Forms.Control)
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            controlProperty.SetValue(control, value, null);
        }

        public void Init_AnimationObject(OPTIONS options)
        {
            // Init animation
            Interval = options.Interval;
            Type = options.Type;
        }

        private void Set_Timer()
        {
            // Timer EventHandler
            tmr_AnimationLine.Dispose();
            if (Type == TYPE.Vertical) // Vertical
                tmr_AnimationLine.Tick += new EventHandler(tmrAnimation_Object_Vertical);
            else // Horizontal
                tmr_AnimationLine.Tick += new EventHandler(tmrAnimation_Object_Horizontal);
        }

        public void MoveObject(int Position, Color color)
        {
            New_Position = Position;
            IsOperating = true;
            Desc = Desc_Found();                 // Define direction (left, right)
            Start_Step();                        // Find step 
            New_Color = color;                   // Color
            tmr_AnimationLine.Enabled = true;    // Animation start
        }

        public void MoveObject(int Position)
        {
            New_Position = Position;
            IsOperating = true;
            Desc = Desc_Found();                // Define direction (left, right)
            Start_Step();                       // Find step
            New_Color = Object.BackColor;       // Color
            tmr_AnimationLine.Enabled = true;   // Animation start
        }

        public void Refresh(System.Windows.Forms.Control Object)
        {
            this.Object = Object;
            Set_Timer();
        }

        private Boolean Desc_Found()
        {
            // Define direction
            if (Type == TYPE.Vertical) // Vertical
            {
                if (New_Position > Object.Location.Y)
                    return true;
                else
                    return false;
            }
            else // Horizontal
            {
                if (New_Position > Object.Location.X)
                    return true;
                else
                    return false;
            }
        }

        private void End_Step()
        {
            // Calculate end step transaction
            if (Desc) // For the descending or left-right phase
            {
                if (Type == TYPE.Vertical) // Final vertical step
                {
                    if ((New_Position - Object.Location.Y) <= Step) // step update
                        Step = New_Position - Object.Location.Y;
                    else
                        Step = (Object.Height / 20);
                }
                else  // Final horizontal step
                {
                    if ((New_Position - Object.Location.X) <= Step) // step update
                        Step = New_Position - Object.Location.X;
                    else
                        Step = (Object.Width / 20);
                }
            }
            else //  For the ascending phase
            {
                if (Type == TYPE.Vertical) // Final vertical step
                {
                    if ((Object.Location.Y - New_Position) <= Step) // step update
                        Step = Object.Location.Y - New_Position;
                    else
                        Step = (Object.Height / 20);
                }
                else // Final horizontal step
                {
                    if ((Object.Location.X - New_Position) <= Step) // step update
                        Step = Object.Location.X - New_Position;
                    else
                        Step = (Object.Width / 20);
                }
            }
        }

        private void Start_Step()
        {
            // Find step according of the animation size 
            Frame = FRAME_NUMBER;

            if ((Desc)) // For the descending or left-right phase
            {
                if (Type == TYPE.Vertical) // Vertical step
                    Step = (New_Position - Object.Location.Y) / FRAME_NUMBER;
                else
                    Step = (New_Position - Object.Location.X) / FRAME_NUMBER;
            }
            else // For the ascending or left-right phase
            {
                if (Type == TYPE.Vertical) // Horizontal step
                    Step = (Object.Location.Y - New_Position) / FRAME_NUMBER;
                else
                    Step = (Object.Location.X - New_Position) / FRAME_NUMBER;
            }
        }

        #endregion

        #region Event's Timer

        // Vertical timer
        private void tmrAnimation_Object_Vertical(object sender, EventArgs e)
        {
            if (New_Position == Object.Location.Y) // exit condition
            {
                tmr_AnimationLine.Enabled = false;
                IsOperating = false;
                Object.BackColor = New_Color; // update color
            }
            else // Move management
                if (Desc) // Descending phase
            {
                if (Frame == 1) // end part
                    End_Step();
                else
                    Frame--;
                Object.SetBounds(Object.Location.X, Object.Location.Y + Step, Object.Width, Object.Height); // Mouve
            }
            else // Ascending phase
            {
                if (Frame == 1) // end part
                    End_Step();
                else
                    Frame--;
                Object.SetBounds(Object.Location.X, Object.Location.Y - Step, Object.Width, Object.Height); // Mouve
            }
        }

        // Horizontal timer
        private void tmrAnimation_Object_Horizontal(object sender, EventArgs e)
        {
            if (New_Position == Object.Location.X) // exit condition
            {
                tmr_AnimationLine.Enabled = false;
                IsOperating = false;
                Object.BackColor = New_Color; // update color
            }
            else // Move management
                if (Desc) // Right
            {
                if (Frame == 1) // last part
                    End_Step();
                else
                    Frame--;
                Object.SetBounds(Object.Location.X + Step, Object.Location.Y, Object.Width, Object.Height); // Mouve

            }
            else // Left
            {
                if (Frame == 1)  // last part
                    End_Step();
                else
                    Frame--;
                Object.SetBounds(Object.Location.X - Step, Object.Location.Y, Object.Width, Object.Height); // Mouve
            }
        }

        #endregion        

        #region Init Class

        public MoveOBject(OPTIONS options)
        {
            //
            // Timer
            //
            tmr_AnimationLine = new System.Windows.Forms.Timer();
            Interval = options.Interval;
            Type = options.Type;
        }

        public MoveOBject()
        {
            tmr_AnimationLine = new System.Windows.Forms.Timer();
        }

        #endregion

    }

    #endregion

    #region Animation Line

    /// <summary>
    /// Make and move a line in your favorite position
    /// You can mouve in two way: left <-> right and top <-> down
    /// Normally the movement consists in ten frame. Each frame is a mouvement
    /// </summary>

    public class AnimationLine : UserControl
    {

        #region Variables

        // Frame animation nnumber
        static readonly int FRAME_NUMBER = 10;

        public struct OPTIONS
        {
            public int Interval;
            public TYPE Type;
        }

        private TYPE _type;
        private int _interval;

        public enum TYPE
        {
            Vertical,
            Horizontal
        }

        public int Interval // Define speed
        {
            get { return _interval; }
            set
            {
                if (value == 0)
                {
                    _interval = 100;
                    tmr_AnimationLine.Interval = 100;
                }
                else
                {
                    _interval = value;
                    tmr_AnimationLine.Interval = value;
                }
            }
        }
        private int New_Position;       // line point end
        private int Step_AnimationLine; // step animation
        private int Frame;              // frame counter   
        private bool Desc = false;      // Direction (left/right top/down)
        public TYPE Type                
        {
            get { return _type; }
            set { _type = value; Set_Timer(); }
        }
        private Color New_Color;        // line color

        // Animation timer
        public System.Windows.Forms.Timer tmr_AnimationLine;

        #endregion

        #region Fonctions        

        public void Init_AnimationLine(OPTIONS options)
        {
            // Init
            Interval = options.Interval;
            Type = options.Type;
        }

        private void Set_Timer()
        {
            // Make new timer EventHandler

            if (Type == TYPE.Vertical) // Vertical
                tmr_AnimationLine.Tick += new EventHandler(tmrAnimatioLine_Vertical);
            else // Horizontal
                tmr_AnimationLine.Tick += new EventHandler(tmrAnimatioLine_Horizontal);
        }

        private void SetDoubleBuffering(System.Windows.Forms.Control control, bool value)
        {
            //Enable double buffeing for ListView Img_List to prevent the flikering 
            System.Reflection.PropertyInfo controlProperty = typeof(System.Windows.Forms.Control)
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            controlProperty.SetValue(control, value, null);
        }

        public void MoveLine(System.Drawing.Point Position, Color color)
        {
            // Extrapolation of the position from the Variable Position according to the selected mode (Veticale / Orizzontale)
            if (Type == TYPE.Vertical)
                New_Position = Position.Y;
            else
                New_Position = Position.X;
            Desc = Desc_Found();              // Direction
            Start_Step();                     // Find step animation
            New_Color = color;                // Color
            tmr_AnimationLine.Enabled = true; // Animation start
        }

        public void MoveLine(System.Drawing.Point Position)
        {
            // Extrapolation of the position from the Variable Position according to the selected mode (Veticale / Orizzontale)
            if (Type == TYPE.Vertical)
                New_Position = Position.Y;
            else
                New_Position = Position.X;
            Desc = Desc_Found();              // Direction
            Start_Step();                     // Find step animation
            New_Color = this.BackColor;       // Color
            tmr_AnimationLine.Enabled = true; // Animation start
        }

        private Boolean Desc_Found()
        {
            // Check direction (left/right or top/down)
            if (Type == TYPE.Vertical) // Vertical
            {
                if (New_Position > this.Location.Y)
                    return true;
                else
                    return false;
            }
            else // Horizontal
            {
                if (New_Position > this.Location.X)
                    return true;
                else
                    return false;
            }
        }

        private void End_Step()
        {
            // Find the step size for the last animation part
            if (Desc) // For the descending or left-right phase
            {
                if (Type == TYPE.Vertical) // vertical final step
                {
                    if ((New_Position - this.Location.Y) < Step_AnimationLine) // update step
                        Step_AnimationLine = New_Position - this.Location.Y;
                    else
                        Step_AnimationLine = (this.Height / 20);
                }
                else  // horizontal final step
                {
                    if ((New_Position - this.Location.X) < Step_AnimationLine) // update step
                        Step_AnimationLine = New_Position - this.Location.X;
                    else
                        Step_AnimationLine = (this.Width / 20);
                }
            }
            else // For the mounting phase
            {
                if (Type == TYPE.Vertical) // final vertical step
                {
                    if ((this.Location.Y - New_Position) < Step_AnimationLine) // update step
                        Step_AnimationLine = this.Location.Y - New_Position;
                    else
                        Step_AnimationLine = (this.Height / 20);
                }
                else // final horizontal step
                {
                    if ((this.Location.X - New_Position) < Step_AnimationLine) // update step
                        Step_AnimationLine = this.Location.X - New_Position;
                    else
                        Step_AnimationLine = (this.Width / 20);
                }
            }
        }

        private void Start_Step()
        {
            // Find the step size according the animation size
            Frame = FRAME_NUMBER;

            if ((Desc)) // For the descending or left-right phase
            {
                if (Type == TYPE.Vertical) // Vertical step
                    Step_AnimationLine = (New_Position - this.Location.Y) / FRAME_NUMBER;
                else
                    Step_AnimationLine = (New_Position - this.Location.X) / FRAME_NUMBER;
            }
            else // For the ascending or left-right phase
            {
                if (Type == TYPE.Vertical) // Pas horizontal // Passo orizzontale
                    Step_AnimationLine = (this.Location.Y - New_Position) / FRAME_NUMBER;
                else
                    Step_AnimationLine = (this.Location.X - New_Position) / FRAME_NUMBER;
            }
        }

        #endregion

        #region Event Timer

        private void tmrAnimatioLine_Vertical(object sender, EventArgs e)
        {
            // Vertical timer
            if (New_Position == this.Location.Y) // Exit condition
            {
                tmr_AnimationLine.Enabled = false;
                this.BackColor = New_Color; // update color
            }
            else
                if (Desc) // Right or Top
                {
                    if (Frame == 1) // last animation part
                        End_Step();
                    else
                        Frame--;
                    this.SetBounds(this.Location.X, this.Location.Y + Step_AnimationLine, this.Width, this.Height); // Mouve
                }
                else // Left or Down
                {
                    if (Frame == 1) // last animation part
                        End_Step();
                    else
                        Frame--;
                    this.SetBounds(this.Location.X, this.Location.Y - Step_AnimationLine, this.Width, this.Height); // Mouve
                }
        }

        private void tmrAnimatioLine_Horizontal(object sender, EventArgs e)
        {
            // Horizontal timer
            if (New_Position == this.Location.X) // Exit condition
            {
                tmr_AnimationLine.Enabled = false;
                this.BackColor = New_Color; // update color
            }
            else // Mouve
                if (Desc) // Right or Top
                {
                    if (Frame == 1) // last animation part
                        End_Step();
                    else
                        Frame--;
                    this.SetBounds(this.Location.X + Step_AnimationLine, this.Location.Y, this.Width, this.Height); // Mouve

                }
                else // Left or Down
                {
                    if (Frame == 1)  // last animation part
                        End_Step();
                    else
                        Frame--;
                    this.SetBounds(this.Location.X - Step_AnimationLine, this.Location.Y, this.Width, this.Height); // Mouve
                }
        }

        #endregion        

        #region Init Class

        public AnimationLine(OPTIONS options)
        {
            this.SuspendLayout();
            // 
            // Animation Region
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            //this.Name = "AnimationLine";
            this.Size = new System.Drawing.Size(3, 40);
            this.ResumeLayout(false);
            //
            // Timer
            //
            tmr_AnimationLine = new System.Windows.Forms.Timer();
            Interval = options.Interval;
            Type = options.Type;
            this.SetDoubleBuffering(this, true);
        }

        public AnimationLine()
        {
            this.SuspendLayout();
            // 
            // Animation Region
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Name = "AnimationLine";
            this.Size = new System.Drawing.Size(206, 193);
            this.ResumeLayout(false);
            tmr_AnimationLine = new System.Windows.Forms.Timer();
            this.SetDoubleBuffering(this, true);
        }

        #endregion

    }

    #endregion

    #region Chart Circle

    /// <summary>
    /// Make a animated circle
    /// </summary>

    public class ChartCircle : UserControl
    {

        #region Variables

        private Color _fillColor = Color.Black;     // Fill color
        private Color _backColor = Color.LightGray; // Background color
        private int _thickness = 5;                 // Thickness size
        private int _perimeter = 90;                // Perimeter size
        private int? _fillSize = 25;                 // Fill size
        private Boolean _activated = true;          
        private Boolean _textVisible = true;        

        public System.Windows.Forms.Label lbl_Info;

        public Boolean Activated
        {
            get { return _activated; }
            set { _activated = value; this.Refresh(); }
        }

        public Boolean TextVisible
        {
            get { return _textVisible; }
            set
            {
                _textVisible = value;

                lbl_Info.Visible = value;

                this.Refresh();
            }
        }

        public Color CircleFillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; this.Refresh(); }
        }

        public Color CircleBackColor
        {
            get { return _backColor; }
            set { _backColor = value; this.Refresh(); }
        }

        public int Thickness
        {
            get { return _thickness; }
            set
            {
                if (value <= 0) _thickness = 1;
                else
                    if (value >= this.Width / 2)
                    _thickness = this.Width / 2 - 1;
                else
                    _thickness = value;
                this.Refresh(); // Mis à jour;
            }
        }

        public int? FillSize
        {
            get { return _fillSize; }
            set
            {
                // Check entry values
                if ((value <= 0) || (value == null))
                {
                    _fillSize = 0;
                    _perimeter = 0;
                }
                else
                {
                    if (value >= 100)
                    {
                        _fillSize = 100;
                        _perimeter = 360;
                    }
                    else
                    {
                        _fillSize = value;
                        _perimeter = Convert.ToUInt16(value * 3.6); // Radiants convertion
                    }
                }
                this.Refresh();
            }
        }

        private int Perimeter
        {
            get { return _perimeter; }
            set { _perimeter = Convert.ToInt16(FillSize * 3.6); }
        }

        #endregion

        #region Fonctions

        public void Draw_Cirlce(int size, Color FillColor, Color BackColor)
        {
            this.CircleFillColor = FillColor;
            this.CircleBackColor = BackColor;
            FillSize = size;
        }

        public void Draw_Cirlce()
        {
            this.Refresh();
        }

        public void Draw_Cirlce(int size)
        {
            FillSize = size;
        }

        public void Draw_Cirlce(int FillSize, int Thickness)
        {
            this.Thickness = Thickness;
            this.FillSize = FillSize;
        }

        public void Draw_Cirlce(int FillSize, int Thickness, Color FillColor, Color BackColor)
        {
            this.CircleFillColor = FillColor;
            this.CircleBackColor = BackColor;
            this.Thickness = Thickness;
            this.FillSize = FillSize;
        }

        private void SetDoubleBuffering(System.Windows.Forms.Control control, bool value)
        {
            //Enable double buffeing for ListView Img_List to prevent the flikering 
            System.Reflection.PropertyInfo controlProperty = typeof(System.Windows.Forms.Control)
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            controlProperty.SetValue(control, value, null);
        }

        private void Circle_Paint(object sender, PaintEventArgs e)
        {
            // Quality
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.CompositingMode = CompositingMode.SourceOver;
            e.Graphics.InterpolationMode = InterpolationMode.High;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //
            // Circles
            //
            if (!Activated)
            {
                this._backColor = Color.LightGray;
                this._perimeter = 0;
            }
            e.Graphics.FillPie(new SolidBrush(CircleFillColor), 1, 1, (this.Size.Width - 2), (this.Size.Height - 2), -90, Perimeter);
            e.Graphics.FillPie(new SolidBrush(CircleBackColor), 1, 1, (this.Size.Width - 2), (this.Size.Height - 2), -90 + Perimeter, 360 - Perimeter);
            e.Graphics.FillPie(new SolidBrush(this.Parent.BackColor), Thickness, Thickness, (this.Size.Width) - (Thickness * 2), (this.Size.Height) - (Thickness * 2), 0, 360);
            //
            //Text
            //
            if (TextVisible)
            {
                lbl_Info.SetBounds((this.Width / 2) - (lbl_Info.Width / 2), (this.Height / 2) - (lbl_Info.Height / 2), lbl_Info.Width, lbl_Info.Height);
                lbl_Info.Text = FillSize.ToString() + "%";
            }
        }

        #endregion

        #region Init Class

        public ChartCircle()
        {
            this.lbl_Info = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbl_Info
            // 
            this.lbl_Info.AutoSize = true;
            this.lbl_Info.BackColor = System.Drawing.Color.Transparent;
            this.lbl_Info.Location = new System.Drawing.Point(37, 43);
            this.lbl_Info.Name = "lbl_Info";
            this.lbl_Info.Size = new System.Drawing.Size(27, 13);
            this.lbl_Info.TabIndex = 0;
            // 
            // Circle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbl_Info);
            this.Name = "Circle";
            this.Size = new System.Drawing.Size(100, 100);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Circle_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

            this.SetDoubleBuffering(this, true);
        }

        #endregion

    }
    #endregion

    #region Chart Bar

    /// <summary>
    /// Make a animated bar
    /// </summary>

    public class ChartBar : UserControl
    {

        #region Variables

        private System.Windows.Forms.Label lbl_Info;
        private Color _fillColor = Color.Black;      // Fill color
        private Color _backColor = Color.LightGray;  // Background color
        private int? _fillSize = 25;                  // Fill size
        private STYLE _style = STYLE.Horizontal;
        private Boolean _activated = true;
        private ContentType _chartText;
        private String _customText;
       
        public Boolean Activated
        {
            get { return _activated; }
            set { _activated = value; this.Refresh(); }
        }
        public string CustomText 
        {
            get { return _customText; }
            set { _customText = value; this.Refresh(); }
        }
        public ContentAlignment Alignment 
        {
            get { return lbl_Info.TextAlign; }
            set { lbl_Info.TextAlign = value; this.Refresh(); }
        }

        public ContentType ChartText
        {
            get { return _chartText; }
            set { 
                _chartText = value;
                UpdateText();
                this.Refresh(); 
                }
        }
        public enum ContentType
        {
            None,
            FillSize,
            CustomText
        }
        public enum STYLE
        {
            Vertical,
            Horizontal
        }
        public STYLE Style
        {
            get { return _style; }
            set { _style = value; this.Refresh(); }
        }
        public int? BarFillSize
        {
            get { return _fillSize; }
            set
            {
                if ((value <= 0) || (value == null))
                {
                    _fillSize = 0;
                }
                else
                {
                    if (value >= 100)
                    {
                        _fillSize = 100;
                    }
                    else
                    {
                        _fillSize = value;
                    }
                }
                this.Refresh();
            }
        }
        public Color TextColor
        {
            get { return lbl_Info.ForeColor; }
            set { lbl_Info.ForeColor = value; this.Refresh(); }
        }
        public Color BarFillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; this.Refresh(); }
        }
        public Color BarBackColor
        {
            get { return _backColor; }
            set { _backColor = value; this.Refresh(); }
        }
        public Font TextFont
        {
            get { return this.lbl_Info.Font; }
            set 
            {
                this.lbl_Info.Font = value;
                this.Refresh();
            }
        }
        private int FillSize
        {
            get
            {

                if (Style == STYLE.Horizontal)
                    return (Convert.ToInt16((this.Width * _fillSize) / 100));
                else
                    return (Convert.ToInt16((this.Height * _fillSize) / 100));
            }
            set
            {
                // Check entry values
                if (value <= 0)
                {
                    _fillSize = 0;
                }
                else
                {
                    if (value >= 100)
                    {
                        _fillSize = 100;
                    }
                    else
                    {
                        _fillSize = value;
                    }
                }
                this.Refresh();
            }
        }

        #endregion

        #region Fonctions

        public void Draw_ChartBar()
        {
            this.Refresh();
        }

        public void Draw_ChartBar(int Size)
        {
            this.FillSize = Size;
        }

        public void Draw_ChartBar(int Size, Color FillColor, Color BackColor)
        {
            this.BarFillColor = FillColor;
            this.BarBackColor = BackColor;
            this.FillSize = Size;
        }

        private void SetDoubleBuffering(System.Windows.Forms.Control control, bool value)
        {
            //Enable double buffeing for ListView Img_List to prevent the flikering 
            System.Reflection.PropertyInfo controlProperty = typeof(System.Windows.Forms.Control)
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            controlProperty.SetValue(control, value, null);
        }

        public void UpdateText()
        {
            lbl_Info.Visible = !(_chartText == ContentType.None);
            if (_chartText == ContentType.FillSize) lbl_Info.Text = BarFillSize.ToString() + "%";
            if (_chartText == ContentType.CustomText) lbl_Info.Text = CustomText;
        }
        private void Bar_Paint(object sender, PaintEventArgs e)
        {
            // Quality
            
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            e.Graphics.CompositingQuality = CompositingQuality.AssumeLinear;
           
            //
            // Bar
            //      
            if (!Activated)
            {
                this._backColor = Color.LightGray;
                _fillSize = 0;
            }
            if (Style == STYLE.Horizontal) // horizontal bar
            {
                e.Graphics.FillRectangle(new SolidBrush(BarBackColor), FillSize, 0, this.Width, this.Height); // parti droite
                e.Graphics.FillRectangle(new SolidBrush(BarFillColor), 0, 0, FillSize, this.Height); // parti gauche
            }
            else // vertical bar
            {
                e.Graphics.FillRectangle(new SolidBrush(BarFillColor), 0, this.Height - FillSize, this.Width, this.Height); // parti inferior
                e.Graphics.FillRectangle(new SolidBrush(BarBackColor), 0, 0, this.Width, this.Height - FillSize); // parti supeirior
            }
            //
            // Text
            //
            UpdateText();
            /*if (ShowContent == TEXT_SHOW.FillSize)
                TextRenderer.DrawText(e.Graphics, this.BarFillSize.ToString() + "%", this.TextFont, this.lbl_Info.Location, this.TextColor);
            if (ShowContent == TEXT_SHOW.CustomText)
                TextRenderer.DrawText(e.Graphics, this.CustomText, this.TextFont, this.lbl_Info.Location, this.TextColor);*/
        }

        #endregion

        #region Init Class

        public ChartBar()
        {
            this.lbl_Info = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbl_Info
            // 
            this.lbl_Info.AutoSize = false;
            this.lbl_Info.Dock = DockStyle.Fill;
            this.lbl_Info.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_Info.BackColor = Color.Transparent;
            this.lbl_Info.Name = "lbl_Info";
            this.lbl_Info.Visible = true;
            this.lbl_Info.ForeColor = Color.White;
            this.lbl_Info.Font = this.Font;
            // 
            // BarChart
            // 
            this.TextColor = Color.White;
            this.Alignment = ContentAlignment.MiddleCenter;
            this.ChartText = ContentType.FillSize;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbl_Info);
            this.Name = "BarChart";
            this.Size = new System.Drawing.Size(100, 100);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Bar_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();
            this.SetDoubleBuffering(this, true);
        }

        #endregion
    }



    #endregion

    #region ThreadSystem

    public class ThreadSystem
    {
        private Action action;
        private Thread th;
        private int delay;
        
        public event EventHandler OnStart;
        public event EventHandler OnAbort;

        protected virtual void OnStartTh(EventArgs e)
        {
            EventHandler handler = OnStart;
            handler?.Invoke(this, e);
        }

        protected virtual void OnAbortTh(EventArgs e)
        {
            EventHandler handler = OnAbort;
            handler?.Invoke(this, e);
        }

        private void execute()
        {
            try
            {
                while (true)
                {
                    action.Invoke();
                    Thread.Sleep(delay);
                }
            }
            catch (ThreadAbortException e)
            {
                OnAbortTh(EventArgs.Empty);
            }
        }

        private void Start()
        {
            if (th == null) createTh();
            th.Start();
            OnStartTh(EventArgs.Empty);
        }

        private void Abort()
        {
            if (th == null) return;
            th.Abort();
            th = null;
        }

        private void createTh()
        {
            th = new Thread(new ThreadStart(execute));
        }
        public ThreadSystem(Action ActionToRun)
        {
            action = ActionToRun;
        }
    }

    #endregion
}
