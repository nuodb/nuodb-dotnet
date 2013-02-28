using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;

namespace NuoDb.VisualStudio.DataTools.Editors
{
    public partial class CodeWindow : UserControl
    {
        private SQLEditor _editor;
        private IVsTextBuffer _vsTextBuffer;

        public void SetCaret(int position)
        {
            IVsTextLines lines = (IVsTextLines)_vsTextBuffer;
            int piLine, piColumn;
            lines.GetLineIndexOfPosition(position, out piLine, out piColumn);
            IVsTextView view;
            _vsCodeWindow.GetPrimaryView(out view);
            view.SetCaretPos(piLine, piColumn);
        }

        public override string Text
        {
            get
            {
                if (_vsTextBuffer == null)
                    return "";
                
                IVsTextLines lines = (IVsTextLines)_vsTextBuffer;
                string text;
                int endLine, endIndex, endLineIndex;
                lines.GetLastLineIndex(out endLine, out endIndex);
                lines.GetLengthOfLine(endLine, out endLineIndex);
                lines.GetLineText(0, 0, endLine, endLineIndex, out text);
                return text;
            }
            set
            {
                if (_vsTextBuffer != null)
                {
                    IVsTextLines lines = (IVsTextLines)_vsTextBuffer;
                    int endLine, endIndex, endLineIndex;
                    lines.GetLastLineIndex(out endLine, out endIndex);
                    lines.GetLengthOfLine(endLine, out endLineIndex);
                    GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
                    try
                    {
                        TextSpan[] span = new TextSpan[1];
                        IntPtr textPtr = handle.AddrOfPinnedObject();
                        int textLen = string.IsNullOrEmpty(value) ? 0 : value.Length;
                        int hr = lines.ReplaceLines(0, 0, endLine, endLineIndex, textPtr, textLen, span);
                    }
                    finally
                    {
                        if ((null != handle) && (handle.IsAllocated))
                        {
                            handle.Free();
                        }
                    }
                }
            }
        }

        private IVsCodeWindow _vsCodeWindow;
        private IntPtr _hWndCodeWindow = IntPtr.Zero;
        private uint cookie = 0;

        public IVsCodeWindow VsCodeWindow
        {
            get
            {
                return _vsCodeWindow;
            }
        }

        public IVsWindowPane VsWindowPane
        {
            get
            {
                return _vsCodeWindow as IVsWindowPane;
            }
        }

        public IOleCommandTarget VsCommandTarget
        {
            get
            {
                return _vsCodeWindow as IOleCommandTarget;
            }
        }

        public CodeWindow(UserControl editor)
        {
            _editor = editor as SQLEditor;
            InitializeComponent();
        }

        public CodeWindow()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_editor != null)
                CreateVsCodeWindow();
        }

        private void CreateVsCodeWindow()
        {
            int hr = VSConstants.S_OK;
            Guid clsidVsCodeWindow = typeof(VsCodeWindowClass).GUID;
            Guid iidVsCodeWindow = typeof(IVsCodeWindow).GUID;
            Guid clsidVsTextBuffer = typeof(VsTextBufferClass).GUID;
            Guid iidVsTextLines = typeof(IVsTextLines).GUID;

            // create/site a VsTextBuffer object
            _vsTextBuffer = (IVsTextBuffer)NuoDbVSPackagePackage.Instance.CreateInstance(ref clsidVsTextBuffer, ref iidVsTextLines, typeof(IVsTextBuffer));
            IObjectWithSite ows = (IObjectWithSite)_vsTextBuffer;
            ows.SetSite(_editor);
            //string strSQL = "select * from sometable";
            //hr = _vsTextBuffer.InitializeContent(strSQL, strSQL.Length);
            //_vsTextBuffer.SetStateFlags((uint)BUFFERSTATEFLAGS.BSF_USER_READONLY);
            hr = _vsTextBuffer.SetLanguageServiceID(ref GuidList.guidSQLLangSvc);

            // create/initialize/site a VsCodeWindow object
            _vsCodeWindow = (IVsCodeWindow)NuoDbVSPackagePackage.Instance.CreateInstance(ref clsidVsCodeWindow, ref iidVsCodeWindow, typeof(IVsCodeWindow));

            INITVIEW[] initView = new INITVIEW[1];
            initView[0].fSelectionMargin = 0;
            initView[0].fWidgetMargin = 0;
            initView[0].fVirtualSpace = 0;
            initView[0].fDragDropMove = 1;
            initView[0].fVirtualSpace = 0;
            IVsCodeWindowEx vsCodeWindowEx = (IVsCodeWindowEx)_vsCodeWindow;
            hr = vsCodeWindowEx.Initialize((uint)_codewindowbehaviorflags.CWB_DISABLEDROPDOWNBAR | (uint)_codewindowbehaviorflags.CWB_DISABLESPLITTER,
                0, null, null,
                (uint)TextViewInitFlags.VIF_SET_WIDGET_MARGIN |
                (uint)TextViewInitFlags.VIF_SET_SELECTION_MARGIN |
                (uint)TextViewInitFlags.VIF_SET_VIRTUAL_SPACE |
                (uint)TextViewInitFlags.VIF_SET_DRAGDROPMOVE |
                (uint)TextViewInitFlags2.VIF_SUPPRESS_STATUS_BAR_UPDATE |
                (uint)TextViewInitFlags2.VIF_SUPPRESSBORDER |
                (uint)TextViewInitFlags2.VIF_SUPPRESSTRACKCHANGES |
                (uint)TextViewInitFlags2.VIF_SUPPRESSTRACKGOBACK,
                initView);

            hr = _vsCodeWindow.SetBuffer((IVsTextLines)_vsTextBuffer);
            IVsWindowPane vsWindowPane = (IVsWindowPane)_vsCodeWindow;
            hr = vsWindowPane.SetSite(_editor);
            hr = vsWindowPane.CreatePaneWindow(this.Handle, 0, 0, this.Parent.Size.Width, this.Parent.Size.Height, out _hWndCodeWindow);

            IVsTextView vsTextView;
            hr = _vsCodeWindow.GetPrimaryView(out vsTextView);

            // sink IVsTextViewEvents, so we can determine when a VsCodeWindow object actually has the focus.
            IConnectionPointContainer connptCntr = (IConnectionPointContainer)vsTextView;
            Guid riid = typeof(IVsTextViewEvents).GUID;
            IConnectionPoint cp;
            connptCntr.FindConnectionPoint(ref riid, out cp);
            cp.Advise(_editor, out cookie);

            // sink IVsTextLinesEvents, so we can determine when a VsCodeWindow text has been changed.
            connptCntr = (IConnectionPointContainer)_vsTextBuffer;
            riid = typeof(IVsTextLinesEvents).GUID;
            connptCntr.FindConnectionPoint(ref riid, out cp);
            cp.Advise(_editor, out cookie);
            
        }

        private void CodeWindow_SizeChanged(object sender, EventArgs e)
        {
            Win32.SetWindowPos(_hWndCodeWindow, IntPtr.Zero,
                0, 0,
                this.Width,
                this.Height, 0);
        }

    }
}
