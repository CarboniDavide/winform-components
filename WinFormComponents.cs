///
/// Davide Carboni
/// winform components for visual studio

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace winform_components
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

        //deplacememt de la fênetre

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

        // effet d'ombre

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

        Color _titleBarColor = TITLEBAR_COLOR; // Title Bar Color
        Color _containerColor = CONTAINER_COLOR; // Container Color
        Color _titleColor = TITLE_COLOR; // Title Text Color
        string _titleText = "Titre"; // Text of te Title
        Font _titleFont; // Font of the Title
        Image _titleImage; // Image for the icon
        ImageLayout _titleImageLayout; // Layout for the Icon

        Boolean _titleVisible = true; // Title enabled/disbled
        Boolean _titleIconVisible = true; //Icon enabled/disbled
        Boolean _buttonMinimizeVisible = true; // Button Close  enabled/disbled
        Boolean _buttonMaximizeVisible = true; // Button Maximize  enabled/disbled
        Boolean _buttonCloseVisible = true; // Button Minimize  enabled/disbled

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

        #region Form's Variables

        static readonly Color BORDER_COLOR = Color.FromArgb(24, 131, 215);
        private Boolean Is_MaxSize = false;

        class ReSize
        {

            private bool Above, Right, Under, Left, Right_above, Right_under, Left_under, Left_above;


            int Thickness = 6;  //Thickness of border  u can cheang it
            int Area = 18;     //Thickness of Angle border 


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
        /*
        public override Size MinimumSize
        {
            get { return base.MinimumSize; }
            set { base.MinimumSize = new Size(179, 51); }
        }*/

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

            //Events Timers
            tmrCloseLeave.Tick += new EventHandler(tmrCloseLeave_Tick);
            tmrMaximizeLeave.Tick += new EventHandler(tmrMaximizeLeave_Tick);
            tmrMinimizeLeave.Tick += new EventHandler(tmrMinimizeLeave_Tick);
        }
        #endregion

        #region Timers

        private void tmrCloseLeave_Tick(object sender, EventArgs e)
        {
            // Change the color of button close when mouse leave

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
            // Change the color of button maximize when mouse leave

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
            // Change the color of button minimize when mouse leave

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
            Form.ActiveForm.Close(); // Close the Window
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
            // Windows State Control and change

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
            // Change the buttons position int the title bar

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
        /// <summary>
        /// Make a windows 10 form template using a user control
        /// </summary>

        #region Variables

        public System.Windows.Forms.Panel cmdClose;
        public System.Windows.Forms.Panel cmdMaximize;
        public System.Windows.Forms.Panel cmdMinimize;
        public System.Windows.Forms.Label lblTitre;
        public System.Windows.Forms.PictureBox picIcon;

        private const int WM_NCHITTEST = 0x84;          // variables for dragging the form
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        Boolean Is_Enter = false;
        Boolean Is_Leave = false;
        Boolean Is_Base = true;
        Boolean Is_Down = false;
        public Boolean Is_MaxSize = false;

        private Point diff;

        private System.ComponentModel.IContainer components;

        Color _titleBarColor = TITLEBAR_COLOR; // Title Bar Color
        Color _containerColor = CONTAINER_COLOR; // Container Color
        Color _titleColor = TITLE_COLOR; // Title Text Color
        string _titleText = "Titre"; // Text of te Title
        Font _titleFont; // Font of the Title
        Image _titleImage; // Image for the icon
        ImageLayout _titleImageLayout; // Layout for the Icon

        Boolean _titleVisible = true; // Title enabled/disbled
        Boolean _titleIconVisible = true; //Icon enabled/disbled
        Boolean _buttonMinimizeVisible = true; // Button Close  enabled/disbled
        Boolean _buttonMaximizeVisible = true; // Button Maximize  enabled/disbled
        Boolean _buttonCloseVisible = true; // Button Minimize  enabled/disbled

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
            // Initit Timers Buttons Animation Color

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
            // Change the color of button close when mouse leave

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
            // Change the color of button maximize when mouse leave

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
            // Change the color of button minimize when mouse leave

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
            Form.ActiveForm.Close(); // Close the Window
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

        // Object to use during the animation

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

        public enum ANIMATION
        {
            Static,
            Dinamic
        }

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

        private ON_LOAD _onload; // Option du panneau au démarrage(Ouverte, Férme) // Opzione all'avvio (Aperto o chisuo)
        private POSITION _position; // Position de l'animation  (Gauche, Droit; Bas, Haut)// Posizione dell'animazione (Destra, Sinistra, in Alto, in Basso)
        private ANIMATION _animation;
        private int _closeStep; // Pas d'ouverture // Passo di apertura
        private int _closeSpeed; // vitesse de l'animation en fermeture
        private int _openStep; // Pas de fermeture // Passo di chiusura
        private int _openSpeed; // vitesse de l'animation à l'ouverture
        private int _openBearingSize; // Taille d'overture du bearing // Dimensione di chiusura del cuscinetto 
        private int _closeBearingSize; // Taille de fermeture du bearing // Dimensione di chiusura del cuscinetto 
        private int _size; // Taille d'ouverture et fermeture // Dimensione d'apertura e chiusura del pannello
        private int _maxLimit; // Limite MAX de l'animation // Limite Massimo dell'animazione
        private int _minLimit; // Limite MIN de l'animation // Limite Minimo dell'animazione
        private int _maxSize;
        private int _openBearingPosition;  // Position du bearing a l'ouverture // Posizione del cuscinetto in apertura
        private int _closeBearingPosition; // Position du bearing à la fermeture//Posizione del cuscinetto in chiusura
        private int _walkStep; // Valeur du pas qui peut change en cours d'execution // Valore del passo che in fase di esecuzione puo cambiare
        private int _stopPosition;
        private Boolean _magnetic;
        private Boolean _isOperating;
        private Boolean _isOpened;
        private AnchorStyles OldAnhor;

        // Dimension original du panneau // Dimensione originale del pannello

        public int LocationX;
        public int LocationY;
        public int Width;
        public int Height;

        /// <summary>
        /// Return the max size of the animation [ only in closed mode]
        /// </summary>
        public int MaxSize { get { return _maxSize; } }
        /// <summary>
        /// Return the max position where the open animation will be finished
        /// </summary>
        public int MaxLimit { get { return _maxLimit; } }
        /// <summary>
        /// Return the min position where the close animatin will be finished
        /// </summary>
        public int MinLimit { get { return _minLimit; } }
        /// <summary>
        /// Return the start entry poistion of the Bearing in the open animation
        /// </summary>
        public int OpenBearingPosition { get { return _openBearingPosition; } }
        /// <summary>
        /// Return the start entry position of the Bearing in the close animation
        /// </summary>
        public int CloseBearingPosition { get { return _closeBearingPosition; } }
        /// <summary>
        /// Return the step of the animation when open or close
        /// </summary>
        public int WalkStep { get { return _walkStep; } }
        /// <summary>
        /// Return True if the Container is Opened
        /// </summary>
        public Boolean Is_Opened
        {
            get { return _isOpened; }
            set { _isOpened = value; }
        }
        public Boolean Is_Opening;
        public Boolean Is_Closing;

        /// <summary>
        /// Return True if the Animation is working
        /// </summary>
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
        /// <summary>
        /// Define where the animation will be developed[Left, Right, Top, Bottom]
        /// </summary>
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
        /// <summary>
        /// Define if the animation is closed or opened at the first time
        /// </summary>
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
        /// <summary>
        /// Define if the animation is Static[the size and position not change with the control] or Dinamic[the size and position change with the control] 
        /// </summary>
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
        /// <summary>
        /// The size of the animation
        /// </summary>
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
        /// <summary>
        /// Define the step of the animation when open
        /// </summary>
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
        /// <summary>
        /// Define the step of the animation when close
        /// </summary>
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
        /// <summary>
        /// Define the speed animation when open
        /// </summary>
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
        /// <summary>
        /// Define the speed animation when close
        /// </summary>
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
        /// <summary>
        /// Define the size of the bearing when animation is opening
        /// </summary>
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
        /// <summary>
        /// Define the size of the bearing when animation is closing
        /// </summary>
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
        /// <summary>
        /// Define the position (X or Y) when using the function OpenPanel(position) where the animation will be finisched
        /// </summary>
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
        /// <summary>
        /// Define if the anchor will be added at the end of the animation in the left,right,top or bottom
        /// </summary>
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

        private void Init()
        {
            // Prise de la dimmensione de l'objet
            // Passaggio della dimmensione dell'oggetto

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

        private void Set_Timers()
        {
            // Initialisation des Events pour la couple des timers en fonction de la zone qui devra etre animée
            // Inizializazzione degli Event per la coppia dei timer in funzione della zona dell'animazione

            // Animation a gauche // Animazione a sinistra

            tmrOpen = new System.Windows.Forms.Timer();
            tmrClose = new System.Windows.Forms.Timer();

            if (Position == POSITION.In_The_Left)
            {
                tmrOpen.Tick += new EventHandler(tmrOpenLeft_Tick);
                tmrClose.Tick += new EventHandler(tmrCloseRight_Tick);
            }

            // Animation a droit // Animazione a destra

            if (Position == POSITION.In_The_Right)
            {
                tmrOpen.Tick += new EventHandler(tmrOpenRight_Tick);
                tmrClose.Tick += new EventHandler(tmrCloseLeft_Tick);
            }

            // Animation à haut // Animazione in alto

            if (Position == POSITION.In_The_Top)
            {
                tmrOpen.Tick += new EventHandler(tmrOpenUp_Tick);
                tmrClose.Tick += new EventHandler(tmrCloseDown_Tick);
            }

            // Animation en bas // Animazione in basso

            if (Position == POSITION.In_The_Bottom)
            {
                tmrOpen.Tick += new EventHandler(tmrOpenDown_Tick);
                tmrClose.Tick += new EventHandler(tmrCloseUp_Tick);
            }

            // Désactivation des timers
            // Disattivazione dei timer

            tmrOpen.Enabled = false;
            tmrClose.Enabled = false;
        }

        private void Check_MaxSize()
        {
            // Verifica la dimensione massima dell'animazione 
            if (_size > _maxSize)
                _size = _maxSize;
        }

        private void SetPosition()
        {
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

            // Pour l'animation a droit
            // Per l'animazione a destra

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

            // Pour l'animation a haut
            // Per l'animazione in alto

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

        private void Set_Margins()
        {
            // Définition des margins de travaille (margins) de l'animation. Limite max et min d'ouverture et/ou férmeture en fonction de la zone établi
            // Définizione della zona di lavoro (margini) dell animazione. Limite massimo e minimo di apetura e/o chiusura in funzione della zona scelta

            // Pour l'animation a gauche
            // Per l'animazione a sinistra

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

            // Pour l'animation a droit
            // Per l'animazione a destra

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

            // Pour l'animation a haut
            // Per l'animazione in alto

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

            // Pour l'animation en bas
            // Per l'animazione in basso

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

        /// <summary>
        /// Start the animation/Open the control at the Position
        /// </summary>
        public Boolean OpenPanel(int Position)
        {
            // Reglages de la couple de Timeur et du pas d'uoverture // Regolazione della coppia dei timeurs e del passo d'apertura
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

        /// <summary>
        /// Start the animation/Close the control at the Position
        /// </summary>
        public Boolean ClosePanel(int Position)
        {
            // Reglages de la couple de Timeur et du pas d'ouverture // Regolazione della coppia dei timer e del passo d'apertura
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

        /// <summary>
        /// Start the animation/Open the control with the defined size 
        /// </summary>
        public Boolean OpenPanel() // Fonction d'ouverture du panneau
        {
            // Reglages de la couple de Timeur et du pas d'uoverture // Regolazione della coppia dei timeurs e del passo d'apertura
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

        /// <summary>
        /// Start the animation/Close the control with the defined size
        /// </summary>
        public Boolean ClosePanel() // Fonction de fermeture du panneau
        {
            // Reglages de la couple de Timeur et du pas d'ouverture // Regolazione della coppia dei timer e del passo d'apertura
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

        //
        // Timers Animation Setings
        //

        // Gestion des HeventEndler pour chaque animation // Gestion dei HeventHendler per ogni timer

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
            if ((MaxLimit - Object.Location.Y + Object.Height) <= OpenStep) // Mis a Jour de la valeur du Pas
                _walkStep = MaxLimit - (Object.Location.Y + Object.Height);

            if (Object.Location.Y + Object.Height >= MaxLimit) // Condition de Sortie
                OpenEndAnimation();
            else
                if (Object.Location.Y + Object.Height >= OpenBearingPosition) // Condition du bearing
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width, Object.Size.Height + 1); // Fin d'ouverture
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width, Object.Size.Height + WalkStep); // Ouverture normal               
        }

        private void tmrOpenUp_Tick(object sender, EventArgs e)
        {
            if ((Object.Location.Y - OpenBearingPosition) <= OpenStep) // Mis a Jour de la valeur du Pas
                _walkStep = Object.Location.Y - OpenBearingPosition;

            if (Object.Location.Y <= MaxLimit) // Condition de Sortie
                OpenEndAnimation();
            else
                if (Object.Location.Y <= OpenBearingPosition) // Condition du bearing
                Object.SetBounds(Object.Location.X, Object.Location.Y - 1, Object.Size.Width, Object.Size.Height + 1); // Fin d'ouverture
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y - WalkStep, Object.Size.Width, Object.Size.Height + WalkStep); // Ouverture normal 
        }

        private void tmrOpenLeft_Tick(object sender, EventArgs e)
        {
            if ((Object.Location.X - OpenBearingPosition) <= OpenStep) // Mis a Jour de la valeur du Pas
                _walkStep = Object.Location.X - OpenBearingPosition;

            if (Object.Location.X <= MaxLimit) // Condition de Sortie
                OpenEndAnimation();
            else
                if (Object.Location.X <= OpenBearingPosition) // Condition du bearing
                Object.SetBounds(Object.Location.X - 1, Object.Location.Y, Object.Size.Width + 1, Object.Size.Height); // Fin d'ouverture
            else
                Object.SetBounds(Object.Location.X - WalkStep, Object.Location.Y, Object.Size.Width + WalkStep, Object.Size.Height); // Ouverture normal 
        }

        private void tmrOpenRight_Tick(object sender, EventArgs e)
        {
            if ((OpenBearingPosition - Object.Location.X + Object.Width) < OpenStep) // Mis a Jour de la valeur du Pas
                _walkStep = OpenBearingPosition - (Object.Location.X + Object.Width);

            if (Object.Location.X + Object.Width >= MaxLimit) // Condition de Sortie
                OpenEndAnimation();
            else
                if (Object.Location.X + Object.Width >= OpenBearingPosition) // Gestion partie finalde la transation
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width + 1, Object.Size.Height); // Fin d'ouverture
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width + WalkStep, Object.Size.Height); // Ouverture 
        }

        #endregion

        #region Close Timers

        private void tmrCloseDown_Tick(object sender, EventArgs e)
        {
            if ((CloseBearingPosition - Object.Location.Y) <= CloseStep) // Mis a Jour de la valeur du Pas
                _walkStep = CloseBearingPosition - Object.Location.Y;

            if (Object.Location.Y >= MinLimit) // Condition de Sortie
                CloseEndAnimation();
            else
                if (Object.Location.Y >= CloseBearingPosition) // Condition du bearing
                Object.SetBounds(Object.Location.X, Object.Location.Y + 1, Object.Size.Width, Object.Size.Height - 1); // Fin d'ouverture
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y + WalkStep, Object.Size.Width, Object.Size.Height - WalkStep); // Ouverture normal 
        }

        private void tmrCloseUp_Tick(object sender, EventArgs e)
        {
            if ((Object.Location.Y + Object.Height - CloseBearingPosition) <= CloseStep) // Mis a Jour de la valeur du Pas
                _walkStep = Object.Location.Y + Object.Height - CloseBearingPosition;

            if (Object.Location.Y + Object.Height <= MinLimit) // Condition de Sortie
                CloseEndAnimation();
            else
                if (Object.Location.Y + Object.Height <= CloseBearingPosition) // Condition du bearing
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width, Object.Size.Height - 1); // Fin d'ouverture
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width, Object.Size.Height - WalkStep); // Ouverture normal 
        }

        private void tmrCloseLeft_Tick(object sender, EventArgs e)
        {
            if ((Object.Location.X + Object.Width - CloseBearingPosition) < CloseStep) // Mis a Jour de la valeur du Pas
                _walkStep = Object.Location.X + Object.Width - CloseBearingPosition;

            if (Object.Location.X + Object.Width <= MinLimit) // Condition de Sortie
                CloseEndAnimation();
            else
                if (Object.Location.X + Object.Width <= CloseBearingPosition) // Gestion partie finalde la transation
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width - 1, Object.Size.Height); // Fin d'ouverture
            else
                Object.SetBounds(Object.Location.X, Object.Location.Y, Object.Size.Width - WalkStep, Object.Size.Height); // Ouverture normal 
        }

        private void tmrCloseRight_Tick(object sender, EventArgs e)
        {
            if ((CloseBearingPosition - Object.Location.X) <= CloseStep) // Mis a Jour de la valeur du Pas
                _walkStep = CloseBearingPosition - Object.Location.X;

            if (Object.Location.X >= MinLimit) // Condition de Sortie
                CloseEndAnimation();
            else
                if (Object.Location.X >= CloseBearingPosition) // Condition du bearing
                Object.SetBounds(Object.Location.X + 1, Object.Location.Y, Object.Size.Width - 1, Object.Size.Height); // Fin d'ouverture
            else
                Object.SetBounds(Object.Location.X + WalkStep, Object.Location.Y, Object.Size.Width - WalkStep, Object.Size.Height); // Ouverture normal 
        }
        #endregion

        #endregion

        #region Init Class

        // Creation et Initialisation de la classe Animation
        // Creazione e inizializzazione della classe Animazione

        public AnimationComponents()
        {
            // Creation des timers Open et Close
            // Creazione dei timer di gestion Open e Close

            //tmrOpen = new System.Windows.Forms.Timer();
            //tmrClose = new System.Windows.Forms.Timer();
        }

        public AnimationComponents(System.Windows.Forms.Control control, OPTIONS options)
        {

            // Passage du relative User Control pour lequel il faut utiliser l'animation
            // Passagio dell'oggetto User Control per il quale si vuole l'animazione

            Object = control;

            // Creation des timers Open et Close
            // Creazione dei timer di gestion Open e Close

            Set_Timers();

            // Passage de paramatres a l'animation
            // Passagio dei parametri a l'animazione

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

            // Initialisation des timers et passage des "EventHandler" aux timeurs utilisés a chaque tick du timeur
            // Inizializazzione dei timers et passaggio degli "EventHendler" ai timer che sono utilizzati a ogni occorenza dei timer  

            // Animation a gauche // Animazione a sinistra

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
            set { _object = value; }
        }

        public Boolean Is_Operating = false;

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

        public void Init_AnimationObject(OPTIONS options)
        {
            // Parametrs d'entreés pour l'initialization
            // Parametri di partenza per inizializzare l'animazione

            Interval = options.Interval;
            Type = options.Type;
        }

        private void Set_Timer()
        {
            // Creation du EventHandler // Creazione dell'evento EventHandler per il timer
            tmr_AnimationLine.Dispose();
            if (Type == TYPE.Vertical) // Vertical // Verticale
                tmr_AnimationLine.Tick += new EventHandler(tmrAnimation_Object_Vertical);
            else // Horizontal // Orizzontale
                tmr_AnimationLine.Tick += new EventHandler(tmrAnimation_Object_Horizontal);
        }

        public void MoveObject(int Position, Color color)
        {
            New_Position = Position;
            Is_Operating = true;
            Desc = Desc_Found(); // Définition de la direction du dèplacement (vers le bas/droite ou la gauche/sinistra)
            Start_Step(); // Calcule de la valeur du pas // Calcolo del valore del passo
            New_Color = color; // Passage de la couleur choisi // Passaggio del colore scelto
            tmr_AnimationLine.Enabled = true; // Démarrage de l'animation // Inizio della animazione
        }

        public void MoveObject(int Position)
        {
            New_Position = Position;
            Is_Operating = true;
            Desc = Desc_Found(); // Définition de la direction du dèplacement (vers le bas/droite ou la gauche/sinistra)
            Start_Step(); // Calcule de la valeur du pas // Calcolo del valore del passo
            New_Color = Object.BackColor; // Passage de la couleur choisi // Passaggio del colore scelto
            tmr_AnimationLine.Enabled = true; // Démarrage de l'animation // Inizio della animazione
        }

        public void Refresh(System.Windows.Forms.Control Object)
        {
            this.Object = Object;
            Set_Timer();
        }

        private Boolean Desc_Found()
        {
            // Définition de la direction du dèplacement (vers le bas/droite ou la gauche/sinistra)
            // Definizione della direzione dello spostamento (verso il basso/destra o la sinistra/alto)

            if (Type == TYPE.Vertical) // Vertical // Verticale
            {
                if (New_Position > Object.Location.Y)
                    return true;
                else
                    return false;
            }
            else // Horizontal // Orizzontale
            {
                if (New_Position > Object.Location.X)
                    return true;
                else
                    return false;
            }
        }

        private void End_Step()
        {
            // Calcule de la valeur du pas final à utiliser dans la derniere transaction
            // Calcolo del valore del passo da utilizzare nell'ultima transazione

            if (Desc) // Per la phase descedant  ou droite gauche // Per la fase discendente  o sinistra destra
            {
                if (Type == TYPE.Vertical) // Pas final vertical // Passo finale verticale
                {
                    if ((New_Position - Object.Location.Y) <= Step) // mise à jour du pas
                        Step = New_Position - Object.Location.Y;
                    else
                        Step = (Object.Height / 20);
                }
                else  // Pas final horizontal // Passo finalel orizzontale
                {
                    if ((New_Position - Object.Location.X) <= Step) // mise à jour du pas
                        Step = New_Position - Object.Location.X;
                    else
                        Step = (Object.Width / 20);
                }
            }
            else // Pour la phase montant // Per la fase montante
            {
                if (Type == TYPE.Vertical) // Pas final vertical // Passo finale verticale
                {
                    if ((Object.Location.Y - New_Position) <= Step) // mise à jour du pas
                        Step = Object.Location.Y - New_Position;
                    else
                        Step = (Object.Height / 20);
                }
                else // Pas final horizontal // Passo finalel orizzontale
                {
                    if ((Object.Location.X - New_Position) <= Step)// mise à jour du pas
                        Step = Object.Location.X - New_Position;
                    else
                        Step = (Object.Width / 20);
                }
            }
        }

        private void Start_Step()
        {
            // Calcule de la valeur du pas en fonction de la longeur de la transaction 
            // Calcolo del valore del passo in funzione della lunghezza della transazione 

            Frame = FRAME_NUMBER;

            if ((Desc)) // Pour la phase descedant  ou droite gauche // Per la fase discendente  o sinistra destra
            {
                if (Type == TYPE.Vertical) // Pas vertical // Passo verticale
                    Step = (New_Position - Object.Location.Y) / FRAME_NUMBER;
                else
                    Step = (New_Position - Object.Location.X) / FRAME_NUMBER;
            }
            else // Pour la phase montant ou droite gauche // Per la fase montante o sinistra destra
            {
                if (Type == TYPE.Vertical) // Pas horizontal // Passo orizzontale
                    Step = (Object.Location.Y - New_Position) / FRAME_NUMBER;
                else
                    Step = (Object.Location.X - New_Position) / FRAME_NUMBER;
            }
        }

        #endregion

        #region Event's Timer

        private void tmrAnimation_Object_Vertical(object sender, EventArgs e)
        {
            // Timeur de géstion du déplacemt Vertical
            // Timer per la gestione dello spostamento Verticale 

            if (New_Position == Object.Location.Y) // Condition de sortie
            {
                tmr_AnimationLine.Enabled = Is_Operating = false;
                Object.BackColor = New_Color; // Mise a jour de la couleur
            }
            else // Gestion du déplacement
                if (Desc) // Pour la phase descendant // Per la fase in discesa
            {
                if (Frame == 1) // Géstion de la dérniere partie
                    End_Step();
                else
                    Frame--;
                Object.SetBounds(Object.Location.X, Object.Location.Y + Step, Object.Width, Object.Height); // Déplacement
            }
            else // Pour la phase montant // Per la fase in salita
            {
                if (Frame == 1) // Géstion de la dérniere partie
                    End_Step();
                else
                    Frame--;
                Object.SetBounds(Object.Location.X, Object.Location.Y - Step, Object.Width, Object.Height); // Déplacement
            }
        }

        private void tmrAnimation_Object_Horizontal(object sender, EventArgs e)
        {
            // Timeur de géstion du déplacemt Horizontal
            // Timer per la gestione dello spostamento Orizzontale

            if (New_Position == Object.Location.X) // Condition de sortie
            {
                tmr_AnimationLine.Enabled = Is_Operating = false;
                Object.BackColor = New_Color; // Mise a jour de la couleur
            }
            else // Géstion du déplacement
                if (Desc) // Pour la phase à droite // Per la fase a destra
            {
                if (Frame == 1) // Géstion de la dérniere partie
                    End_Step();
                else
                    Frame--;
                Object.SetBounds(Object.Location.X + Step, Object.Location.Y, Object.Width, Object.Height); // Déplacement

            }
            else // Pour la phase à gauche // Per la fase a sinistra
            {
                if (Frame == 1)  // Géstion de la dérniere partie
                    End_Step();
                else
                    Frame--;
                Object.SetBounds(Object.Location.X - Step, Object.Location.Y, Object.Width, Object.Height); // Déplacement
            }
        }

        #endregion        

        #region Init Class

        public MoveOBject(OPTIONS options)
        {
            //
            // Timer
            //
            tmr_AnimationLine = new Timer();
            Interval = options.Interval;
            Type = options.Type;
        }

        public MoveOBject()
        {
            tmr_AnimationLine = new Timer();
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

        // Nombre de parties dans laquelle l'animation et composée ou numero des sequence de l'animation
        // Numero dei segmenti in cui viene divisa l'animazione o numero delle sequenze

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
        private int New_Position; // Cible de la ligne ou point d'arrivé // Punto di destinazione della line
        private int Step_AnimationLine; // Pas de déplacement de la ligne// Passo dello spostamento della linea
        private int Frame; // Contateur qui tien compte de la sequence des déplacement     
        private bool Desc = false; // Direction du déplament. Ver les bas/droit ou haut/gauche
        public TYPE Type // Type dé deplacement (Vertical / Horizontal) // Tipo dello spostamento (Veticale / Orizzontale)
        {
            get { return _type; }
            set { _type = value; Set_Timer(); }
        }
        private Color New_Color; // Couleur de la ligne // Colore della linea

        // Timeur utilisé pour l'animation
        // Timer per l'animazione

        public System.Windows.Forms.Timer tmr_AnimationLine;

        #endregion

        #region Fonctions        

        public void Init_AnimationLine(OPTIONS options)
        {
            // Parametrs d'entreés pour l'initialization
            // Parametri di partenza per inizializzare l'animazione

            Interval = options.Interval;
            Type = options.Type;
        }

        private void Set_Timer()
        {
            // Creation du EventHandler // Creazione dell'evento EventHandler per il timer

            if (Type == TYPE.Vertical) // Vertical // Verticale
                tmr_AnimationLine.Tick += new EventHandler(tmrAnimatioLine_Vertical);
            else // Horizontal // Orizzontale
                tmr_AnimationLine.Tick += new EventHandler(tmrAnimatioLine_Horizontal);
        }

        public void MoveLine(System.Drawing.Point Position, Color color)
        {
            // Extrapolation de la position depuis la varible Position en fonction de la modalité ( Vertical / Horizontal)
            // Estrapolazione della posizione dalla varibile Position in funzione della modalita scelta ( Veticale / Orizzontale)

            if (Type == TYPE.Vertical)
                New_Position = Position.Y;
            else
                New_Position = Position.X;
            Desc = Desc_Found(); // Définition de la direction du dèplacement (vers le bas/droite ou la gauche/sinistra)
            Start_Step(); // Calcule de la valeur du pas // Calcolo del valore del passo
            New_Color = color; // Passage de la couleur choisi // Passaggio del colore scelto
            tmr_AnimationLine.Enabled = true; // Démarrage de l'animation // Inizio della animazione
        }

        public void MoveLine(System.Drawing.Point Position)
        {
            // Extrapolation de la position depuis la varible Position en fonction de la modalité ( Vertical / Horizontal)
            // Estrapolazione della posizione dalla varibile Position in funzione della modalita scelta ( Veticale / Orizzontale)

            if (Type == TYPE.Vertical)
                New_Position = Position.Y;
            else
                New_Position = Position.X;
            Desc = Desc_Found(); // Définition de la direction du dèplacement (vers le bas/droite ou la gauche/sinistra)
            Start_Step(); // Calcule de la valeur du pas // Calcolo del valore del passo
            New_Color = this.BackColor; // Passage de la couleur choisi // Passaggio del colore scelto
            tmr_AnimationLine.Enabled = true; // Démarrage de l'animation // Inizio della animazione
        }

        private Boolean Desc_Found()
        {
            // Définition de la direction du dèplacement (vers le bas/droite ou la gauche/sinistra)
            // Definizione della direzione dello spostamento (verso il basso/destra o la sinistra/alto)

            if (Type == TYPE.Vertical) // Vertical // Verticale
            {
                if (New_Position > this.Location.Y)
                    return true;
                else
                    return false;
            }
            else // Horizontal // Orizzontale
            {
                if (New_Position > this.Location.X)
                    return true;
                else
                    return false;
            }
        }

        private void End_Step()
        {
            // Calcule de la valeur du pas final à utiliser dans la derniere transaction
            // Calcolo del valore del passo da utilizzare nell'ultima transazione

            if (Desc) // Per la phase descedant  ou droite gauche // Per la fase discendente  o sinistra destra
            {
                if (Type == TYPE.Vertical) // Pas final vertical // Passo finale verticale
                {
                    if ((New_Position - this.Location.Y) < Step_AnimationLine) // mise à jour du pas
                        Step_AnimationLine = New_Position - this.Location.Y;
                    else
                        Step_AnimationLine = (this.Height / 20);
                }
                else  // Pas final horizontal // Passo finalel orizzontale
                {
                    if ((New_Position - this.Location.X) < Step_AnimationLine) // mise à jour du pas
                        Step_AnimationLine = New_Position - this.Location.X;
                    else
                        Step_AnimationLine = (this.Width / 20);
                }
            }
            else // Pour la phase montant // Per la fase montante
            {
                if (Type == TYPE.Vertical) // Pas final vertical // Passo finale verticale
                {
                    if ((this.Location.Y - New_Position) < Step_AnimationLine) // mise à jour du pas
                        Step_AnimationLine = this.Location.Y - New_Position;
                    else
                        Step_AnimationLine = (this.Height / 20);
                }
                else // Pas final horizontal // Passo finalel orizzontale
                {
                    if ((this.Location.X - New_Position) < Step_AnimationLine)// mise à jour du pas
                        Step_AnimationLine = this.Location.X - New_Position;
                    else
                        Step_AnimationLine = (this.Width / 20);
                }
            }
        }

        private void Start_Step()
        {
            // Calcule de la valeur du pas en fonction de la longeur de la transaction 
            // Calcolo del valore del passo in funzione della lunghezza della transazione 

            Frame = FRAME_NUMBER;

            if ((Desc)) // Pour la phase descedant  ou droite gauche // Per la fase discendente  o sinistra destra
            {
                if (Type == TYPE.Vertical) // Pas vertical // Passo verticale
                    Step_AnimationLine = (New_Position - this.Location.Y) / FRAME_NUMBER;
                else
                    Step_AnimationLine = (New_Position - this.Location.X) / FRAME_NUMBER;
            }
            else // Pour la phase montant ou droite gauche // Per la fase montante o sinistra destra
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
            // Timeur de géstion du déplacemt Vertical
            // Timer per la gestione dello spostamento Verticale 

            if (New_Position == this.Location.Y) // Condition de sortie
            {
                tmr_AnimationLine.Enabled = false;
                this.BackColor = New_Color; // Mise a jour de la couleur
            }
            else // Gestion du déplacement
                if (Desc) // Pour la phase descendant // Per la fase in discesa
            {
                if (Frame == 1) // Géstion de la dérniere partie
                    End_Step();
                else
                    Frame--;
                this.SetBounds(this.Location.X, this.Location.Y + Step_AnimationLine, this.Width, this.Height); // Déplacement
            }
            else // Pour la phase montant // Per la fase in salita
            {
                if (Frame == 1) // Géstion de la dérniere partie
                    End_Step();
                else
                    Frame--;
                this.SetBounds(this.Location.X, this.Location.Y - Step_AnimationLine, this.Width, this.Height); // Déplacement
            }
        }

        private void tmrAnimatioLine_Horizontal(object sender, EventArgs e)
        {
            // Timeur de géstion du déplacemt Horizontal
            // Timer per la gestione dello spostamento Orizzontale

            if (New_Position == this.Location.X) // Condition de sortie
            {
                tmr_AnimationLine.Enabled = false;
                this.BackColor = New_Color; // Mise a jour de la couleur
            }
            else // Géstion du déplacement
                if (Desc) // Pour la phase à droite // Per la fase a destra
            {
                if (Frame == 1) // Géstion de la dérniere partie
                    End_Step();
                else
                    Frame--;
                this.SetBounds(this.Location.X + Step_AnimationLine, this.Location.Y, this.Width, this.Height); // Déplacement

            }
            else // Pour la phase à gauche // Per la fase a sinistra
            {
                if (Frame == 1)  // Géstion de la dérniere partie
                    End_Step();
                else
                    Frame--;
                this.SetBounds(this.Location.X - Step_AnimationLine, this.Location.Y, this.Width, this.Height); // Déplacement
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
            tmr_AnimationLine = new Timer();
            Interval = options.Interval;
            Type = options.Type;
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
            tmr_AnimationLine = new Timer();
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

        private Color _fillColor = Color.Black; // Couleur de remplissage // Colore di riempimento
        private Color _backColor = Color.LightGray; // Couleur du fond // Colore di sfondo
        private int _thickness = 5; // Valeur du  épaisseur // Valore dello spessore
        private int _perimeter = 90; // Valeur du périmètre // Valore del perimetro
        private int _fillSize = 25; // Diménsion du circle à dessiner // Dimmensione del cerchio da disegnar
        private Boolean _activated = true; // activation ou déactivation du graphique // attivazione o disattivazione del grafico
        private Boolean _textVisible = true; // text active ou no // testo attivo o no

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

        public int FillSize
        {
            get { return _fillSize; }
            set
            {
                // Vérification des valeurs d'entrées (les valeurs péermis sont 0..100)
                // Controllo dei valori d'ingresso (valori consentiti sono 0..100)
                if (value <= 0)
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
                        _perimeter = Convert.ToUInt16(value * 3.6); // Conversion en radiants // Conversione in radianti
                    }
                }
                this.Refresh(); // Mise à jour de l'ovverride // Richiama l'ovveride per il controllo
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

        private void Circle_Paint(object sender, PaintEventArgs e)
        {
            // Définition de la qualité // Definizione della qualità
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

        public System.Windows.Forms.Label lbl_Info;

        private Color _fillColor = Color.Black; // Couleur de remplissage // Colore di riempimento
        private Color _backColor = Color.LightGray; // Couleur du fond // Colore di sfondo
        private Color _textColor = Color.White; // Couleur du text // Colore del testo
        private int _fillSize = 25; // Diménsion du circle à dessiner // Dimmensione del cerchio da disegnare
        private Boolean _TextVisible = true;
        private STYLE _style = STYLE.Horizontal;
        private Boolean _activated = true;

        public Boolean Activated
        {
            get { return _activated; }
            set { _activated = value; this.Refresh(); }
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

        public int BarFillSize
        {
            get { return _fillSize; }
            set
            {
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

        public Boolean TextVisible
        {
            get { return _TextVisible; }
            set { _TextVisible = value; this.Refresh(); }
        }

        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; this.Refresh(); }
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
                // Vérification des valeurs d'entrées (les valeurs péermis sont 0..100)
                // Controllo dei valori d'ingresso (valori consentiti sono 0..100)
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
                this.Refresh(); // Mise à jour de l'ovverride // Richiama l'ovveride per il controllo
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

        private void Bar_Paint(object sender, PaintEventArgs e)
        {
            // Définition de la qualité // Definizione della qualità
            /*
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.High;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            */
            //
            // Bar
            //      
            if (!Activated)
            {
                this._backColor = Color.LightGray;
                _fillSize = 0;
            }
            if (Style == STYLE.Horizontal) // bar horizontal
            {
                e.Graphics.FillRectangle(new SolidBrush(BarBackColor), FillSize, 0, this.Width, this.Height); // parti droite
                e.Graphics.FillRectangle(new SolidBrush(BarFillColor), 0, 0, FillSize, this.Height); // parti gauche
            }
            else // bar vertical
            {
                e.Graphics.FillRectangle(new SolidBrush(BarFillColor), 0, this.Height - FillSize, this.Width, this.Height); // parti inferior
                e.Graphics.FillRectangle(new SolidBrush(BarBackColor), 0, 0, this.Width, this.Height - FillSize); // parti supeirior
            }
            //
            // Text
            //
            if (TextVisible)
                TextRenderer.DrawText(e.Graphics, BarFillSize.ToString() + "%", this.Font, new Point((this.Width / 2) - (lbl_Info.Width / 2) - 2, (this.Height / 2) - (lbl_Info.Height / 2)), this.TextColor);
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
            this.lbl_Info.AutoSize = true;
            this.lbl_Info.BackColor = System.Drawing.Color.Transparent;
            this.lbl_Info.Location = new System.Drawing.Point(37, 43);
            this.lbl_Info.Name = "lbl_Info";
            this.lbl_Info.Size = new System.Drawing.Size(27, 13);
            this.lbl_Info.Text = _fillSize.ToString();
            this.lbl_Info.Visible = false;
            // 
            // BarChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbl_Info);
            this.Name = "BarChart";
            this.Size = new System.Drawing.Size(100, 100);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Bar_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }



    #endregion  

}
