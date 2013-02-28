using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Common;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;

using IServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Microsoft.VisualStudio;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.Data;
using System.Text.RegularExpressions;
using System.Data;

namespace NuoDb.VisualStudio.DataTools.Editors
{
    public partial class SQLEditor : UserControl,
        IVsWindowPane,
        IOleCommandTarget,
        IOleServiceProvider,
        IVsPersistDocData2,
        IPersistFileFormat,
        IVsTextViewEvents,
        IVsTextLinesEvents,
        IVsFileChangeEx,
        IVsDocDataFileChangeControl,
        IVsFindTarget,
        IVsFindTarget2
    {
        private IOleServiceProvider _vsServiceProvider;
        private DbProviderFactory _providerFactory;
        private DbConnection _connection;
        private IVsTextView _activeTextView = null;
        private string _fileName;
        private int _isReadOnly = 0;
        private int _isDirty = 0;
        private string Server;
        private string User;

        public string ConnectionString
        {
            get { return _connection.ConnectionString; }
            set { _connection.ConnectionString = value; }
        }

        public SQLEditor()
        {
            InitializeComponent();
            _providerFactory = DbProviderFactories.GetFactory("NuoDb.Data.Client");
            if (_providerFactory == null)
                throw new Exception("NuoDB Data Provider is not correctly registered");
            _connection = _providerFactory.CreateConnection();
        }

        public SQLEditor(IOleServiceProvider sp)
            : this()
        {
            _vsServiceProvider = sp;
        }

        public IVsWindowFrame editorFrame
        {
            get
            {
                IVsWindowFrame windowFrame = null;
                if (_vsServiceProvider != null)
                {
                    ServiceProvider sp = new ServiceProvider(_vsServiceProvider);
                    windowFrame = sp.GetService(typeof(SVsWindowFrame)) as IVsWindowFrame;
                }
                return windowFrame;
            }
        }

        #region IOleCommandTarget Members

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == GuidList.guidNuoDBVSPackageCmdSet)
            {
                if (nCmdID == PkgCmdIDList.cmdExecuteCommand)
                {
                    ExecuteSQL();
                    return VSConstants.S_OK;
                }
                else if (nCmdID == PkgCmdIDList.cmdConnectionClose)
                {
                    Disconnect();
                    return VSConstants.S_OK;
                }
                else if (nCmdID == PkgCmdIDList.cmdConnectionOpen)
                {
                    Connect();
                    return VSConstants.S_OK;
                }
            }

            int hr = (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
            if (_activeTextView != null)
            {
                IOleCommandTarget cmdTarget = (IOleCommandTarget)_activeTextView;
                hr = cmdTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            return hr;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == GuidList.guidNuoDBVSPackageCmdSet)
            {
                for (uint i = 0; i < cCmds; i++)
                {
                    if (prgCmds[i].cmdID == PkgCmdIDList.cmdConnectionOpen)
                    {
                        // 'Open' is enabled if the connection is not already opened
                        prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | (_connection.State == System.Data.ConnectionState.Open ? 0 : OLECMDF.OLECMDF_ENABLED));
                    }
                    else if (prgCmds[i].cmdID == PkgCmdIDList.cmdConnectionClose || prgCmds[i].cmdID == PkgCmdIDList.cmdExecuteCommand)
                    {
                        // 'Close' and 'Execute' are enabled if the connection is not already opened
                        prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | (_connection.State == System.Data.ConnectionState.Open ? OLECMDF.OLECMDF_ENABLED : 0));
                    }
                }
                return VSConstants.S_OK;
            }

            int hr = (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
            if (_activeTextView != null)
            {
                IOleCommandTarget cmdTarget = (IOleCommandTarget)_activeTextView;
                hr = cmdTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }

            return hr;
        }

        #endregion

        #region IVsWindowPane Members

        public int ClosePane()
        {
            Disconnect();
            return VSConstants.S_OK;
        }

        public int CreatePaneWindow(IntPtr hwndParent, int x, int y, int cx, int cy, out IntPtr hwnd)
        {
            Win32Methods.SetParent(Handle, hwndParent);
            hwnd = Handle;
            Size = new Size(cx - x, cy - y);
            return VSConstants.S_OK;
        }

        public int GetDefaultSize(SIZE[] pSize)
        {
            if (pSize.Length >= 1)
            {
                pSize[0].cx = 300;
                pSize[0].cy = 200;
            }
            return VSConstants.S_OK;
        }

        public int LoadViewState(IStream pStream)
        {
            return VSConstants.S_OK;
        }

        public int SaveViewState(IStream pStream)
        {
            return VSConstants.S_OK;
        }

        public int SetSite(IOleServiceProvider psp)
        {
            _vsServiceProvider = psp;

            int hr = editorFrame.SetGuidProperty((int)__VSFPROPID.VSFPROPID_InheritKeyBindings, ref GuidList.guidCmdUI_TextEditor);

            return VSConstants.S_OK;
        }

        public int TranslateAccelerator(MSG[] lpmsg)
        {
            // defer to active code window
            if (_activeTextView != null)
            {
                IVsWindowPane vsWindowPane = (IVsWindowPane)_activeTextView;
                return vsWindowPane.TranslateAccelerator(lpmsg);
            }

            switch (lpmsg[0].message)
            {
                case Win32.WM_KEYDOWN:
                case Win32.WM_SYSKEYDOWN:
                case Win32.WM_CHAR:
                case Win32.WM_SYSCHAR:
                    {
                        Message msg = new Message();
                        msg.HWnd = lpmsg[0].hwnd;
                        msg.Msg = (int)lpmsg[0].message;
                        msg.LParam = lpmsg[0].lParam;
                        msg.WParam = lpmsg[0].wParam;

                        Control ctrl = Control.FromChildHandle(msg.HWnd);
                        if (ctrl != null && ctrl.PreProcessMessage(ref msg))
                            return VSConstants.S_OK;
                    }
                    break;

                default:
                    break;
            }

            return VSConstants.S_FALSE;
        }

        #endregion

        #region IOleServiceProvider Members

        public int QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject)
        {
            if (_vsServiceProvider != null)
                return _vsServiceProvider.QueryService(ref guidService, ref riid, out ppvObject);
            else
            {
                ppvObject = IntPtr.Zero;
                return VSConstants.E_NOINTERFACE;
            }
        }

        #endregion

        #region IVsPersistDocData Members

        public int Close()
        {
            return VSConstants.S_OK;
        }

        public int GetGuidEditorType(out Guid pClassID)
        {
            pClassID = GuidList.guidNuoDBObjectFactoryService;
            return VSConstants.S_OK;
        }

        public int IsDocDataDirty(out int pfDirty)
        {
            pfDirty = _isDirty;
            return VSConstants.S_OK;
        }

        public int IsDocDataReloadable(out int pfReloadable)
        {
            pfReloadable = 0;
            return VSConstants.S_OK;
        }

        public int LoadDocData(string pszMkDocument)
        {
            StreamReader streamReader = new StreamReader(pszMkDocument);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            CommandWindow.Text = text;

            _isDirty = 0;
            this._fileName = pszMkDocument;
            return VSConstants.S_OK;
        }

        public int OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
        {
            return VSConstants.S_OK;
        }

        public int ReloadDocData(uint grfFlags)
        {
            LoadDocData(_fileName);
            return VSConstants.S_OK;
        }

        public int RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            _fileName = pszMkDocumentNew;
            return VSConstants.S_OK;
        }

        public int SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            string text = CommandWindow.Text;

            StreamWriter writer = new StreamWriter(_fileName);
            writer.Write(text);
            writer.Close();

            _isDirty = 0;
            pbstrMkDocumentNew = _fileName;
            pfSaveCanceled = 0;
            return VSConstants.S_OK;
        }

        public int SetUntitledDocPath(string pszDocDataPath)
        {
            _fileName = pszDocDataPath;
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsTextViewEvents Members

        public void OnChangeCaretLine(IVsTextView pView, int iNewLine, int iOldLine)
        {
        }

        public void OnChangeScrollInfo(IVsTextView pView, int iBar, int iMinUnit, int iMaxUnits, int iVisibleUnits, int iFirstVisibleUnit)
        {
        }

        public void OnKillFocus(IVsTextView pView)
        {
            _activeTextView = null;
        }

        public void OnSetBuffer(IVsTextView pView, IVsTextLines pBuffer)
        {
        }

        public void OnSetFocus(IVsTextView pView)
        {
            _activeTextView = pView;
        }

        #endregion


        #region IVsPersistDocData2 Members


        public int IsDocDataReadOnly(out int pfReadOnly)
        {
            pfReadOnly = _isReadOnly;
            return VSConstants.S_OK;
        }

        public int SetDocDataDirty(int fDirty)
        {
            _isDirty = fDirty;
            return VSConstants.S_OK;
        }

        public int SetDocDataReadOnly(int fReadOnly)
        {
            _isReadOnly = fReadOnly;
            return VSConstants.S_OK;
        }

        #endregion

        #region IPersistFileFormat Members

        public int GetClassID(out Guid pClassID)
        {
            throw new NotImplementedException();
        }

        public int GetCurFile(out string ppszFilename, out uint pnFormatIndex)
        {
            ppszFilename = _fileName;
            pnFormatIndex = 0;
            return VSConstants.S_OK;
        }

        public int GetFormatList(out string ppszFormatList)
        {
            ppszFormatList = "NuoDB SQL Query\n*.nuosql\n\n";
            return VSConstants.S_OK;
        }

        public int InitNew(uint nFormatIndex)
        {
            CommandWindow.Text = "";
            return VSConstants.S_OK;
        }

        public int IsDirty(out int pfIsDirty)
        {
            pfIsDirty = _isDirty;
            return VSConstants.S_OK;
        }

        public new int Load(string pszFilename, uint grfMode, int fReadOnly)
        {
            StreamReader streamReader = new StreamReader(pszFilename);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            CommandWindow.Text = text;
            _isDirty = 0;
            _isReadOnly = fReadOnly;
            _fileName = pszFilename;

            return VSConstants.S_OK;
        }

        public int Save(string pszFilename, int fRemember, uint nFormatIndex)
        {
            //IVsQueryEditQuerySave2 sccProvider;
            //int hr = QueryService(SVsQueryEditQuerySave, IVsQueryEditQuerySave2, out sccProvider);
            //QueryEditFiles
            //QuerySaveFile
            string text = CommandWindow.Text;

            StreamWriter writer = new StreamWriter(pszFilename);
            writer.Write(text);
            writer.Close();

            _isDirty = 0;
            if (fRemember != 0)
                _fileName = pszFilename;
            return VSConstants.S_OK;
        }

        public int SaveCompleted(string pszFilename)
        {
            return VSConstants.S_OK;
        }

        #endregion
    
        #region IVsFileChangeEx Members

        public int  AdviseDirChange(string pszDir, int fWatchSubDir, IVsFileChangeEvents pFCE, out uint pvsCookie)
        {
 	        throw new NotImplementedException();
        }

        public int  AdviseFileChange(string pszMkDocument, uint grfFilter, IVsFileChangeEvents pFCE, out uint pvsCookie)
        {
 	        throw new NotImplementedException();
        }

        public int  IgnoreFile(uint VSCOOKIE, string pszMkDocument, int fIgnore)
        {
 	        throw new NotImplementedException();
        }

        public int  SyncFile(string pszMkDocument)
        {
 	        throw new NotImplementedException();
        }

        public int  UnadviseDirChange(uint VSCOOKIE)
        {
 	        throw new NotImplementedException();
        }

        public int  UnadviseFileChange(uint VSCOOKIE)
        {
 	        throw new NotImplementedException();
        }

        #endregion

        #region IVsDocDataFileChangeControl Members

        public int IgnoreFileChanges(int fIgnore)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVsFindTarget2 Members

        public int NavigateTo2(IVsTextSpanSet pSpans, TextSelMode iSelMode)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVsFindTarget Members

        public int Find(string pszSearch, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out uint pResult)
        {
            throw new NotImplementedException();
        }

        public int GetCapabilities(bool[] pfImage, uint[] pgrfOptions)
        {
            throw new NotImplementedException();
        }

        public int GetCurrentSpan(TextSpan[] pts)
        {
            throw new NotImplementedException();
        }

        public int GetFindState(out object ppunk)
        {
            throw new NotImplementedException();
        }

        public int GetMatchRect(RECT[] prc)
        {
            throw new NotImplementedException();
        }

        public int GetProperty(uint propid, out object pvar)
        {
            throw new NotImplementedException();
        }

        public int GetSearchImage(uint grfOptions, IVsTextSpanSet[] ppSpans, out IVsTextImage ppTextImage)
        {
            throw new NotImplementedException();
        }

        public int MarkSpan(TextSpan[] pts)
        {
            throw new NotImplementedException();
        }

        public int NavigateTo(TextSpan[] pts)
        {
            throw new NotImplementedException();
        }

        public int NotifyFindTarget(uint notification)
        {
            throw new NotImplementedException();
        }

        public int Replace(string pszSearch, string pszReplace, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out int pfReplaced)
        {
            throw new NotImplementedException();
        }

        public int SetFindState(object pUnk)
        {
            throw new NotImplementedException();
        }

        #endregion

        internal void Connect()
        {
            Disconnect();
            if (ConnectionString == null)
            {
                NewConnectionDialog dlg = new NewConnectionDialog();
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                ConnectionString = dlg.ConnectionString;
                Server = dlg.Server;
                User = dlg.User;
            }
            _connection.Open();
            connectionStatus.Text = "Connected to <"+Server+"> as user <"+User+">";
        }

        internal void Disconnect()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
                _connection.Close();
            connectionStatus.Text = "Disconnected";
        }

        internal void ExecuteSQL()
        {
            ResultsWindow.Clear();
            string sql = CommandWindow.Text.Trim();
            if (sql.Length == 0)
                return;
            // ensure that the command is properly terminated, or the last statement will be ignored by the regex
            if (!sql.EndsWith(";"))
                sql += ";";
            
#if DEBUG
            System.Diagnostics.Trace.WriteLine("SQLEditor.ExecuteSQL: " + sql );
#endif
            Regex regex = new Regex(@"([^;'""]+|""[^""]*""|'[^']*')*[;]");
            MatchCollection coll = regex.Matches(sql);
            foreach (Match m in coll)
            {
                string cmd = m.Value.TrimEnd(new char[] { ';' }).Trim();
                if (cmd.Length == 0)
                    continue;
                CommandWindow.SetCaret(m.Index);
#if DEBUG
                System.Diagnostics.Trace.WriteLine(cmd);
#endif
                DbCommand dbCmd = _connection.CreateCommand();
                dbCmd.CommandText = cmd;
                if (cmd.Substring(0, 6).ToUpper().Equals("SELECT"))
                {
                    DbDataReader reader = dbCmd.ExecuteReader();
                    DataTable schema = reader.GetSchemaTable();

                    int lineLength = 0;
                    bool[] columnIsRightAligned = new bool[reader.FieldCount];
                    int[] columnWidths = new int[reader.FieldCount];
                    string[] columnTitles = new string[reader.FieldCount];
                    foreach (DataRow row in schema.Rows)
                    {
                        int colNumber = (int)row["ColumnOrdinal"];
                        bool rightAlign = true;
                        int length = 0;
                        DbType type = (DbType)row["ProviderType"];
                        switch (type)
                        {
                            case DbType.Boolean:
                                length = 5; // False
                                break;
                            case DbType.Byte:
                                length = 4; // -128
                                break;
                            case DbType.Int16:
                                length = 6; // -32767
                                break;
                            case DbType.Int32:
                                length = 10; // -134217728
                                break;
                            case DbType.Int64:
                                length = 19; // -576460752303423488
                                break;
                            case DbType.Single:
                                length = 14; // -3.4028234E-38
                                break;
                            case DbType.Double:
                                length = 25; // -2.2250738585072009E−308
                                break;
                            case DbType.Decimal:
                                length = (int)row["NumericPrecision"];
                                break;
                            case DbType.Date:
                                length = 10; // 2000-12-31
                                break;
                            case DbType.Time:
                                length = 18; // 10:00:00.234GMT+11
                                break;
                            case DbType.DateTime:
                                length = 29; // 2000-12-31T10:00:00.234GMT+11
                                break;
                            default:
                                rightAlign = false;
                                length = (int)row["ColumnSize"];
                                break;
                        }
                        length = Math.Min(length, 80);

                        string label = row["ColumnName"].ToString();
                        if (label == null || label.Length == 0)
                            label = row["BaseColumnName"].ToString();
                        if (label == null || label.Length == 0)
                            label = String.Format("Column{0}", row["ColumnOrdinal"]);
                        length = Math.Max(length, label.Length);
                        columnWidths[colNumber] = length;
                        string padLeft = String.Format("{0," + ((length - label.Length) / 2) + "}", "");
                        string padRight = String.Format("{0," + (length - padLeft.Length - label.Length) + "}", "");
                        columnTitles[colNumber] = padLeft + label + padRight;
                        columnIsRightAligned[colNumber] = rightAlign;

                        lineLength += length + 1;
                    }
                    StringBuilder outputRow = new StringBuilder(lineLength);
                    for (int i = 0; i < columnTitles.Length; i++)
                    {
                        outputRow.Append(columnTitles[i]).Append(" ");
                    }
                    outputRow.AppendLine();
                    ResultsWindow.AppendText(outputRow.ToString());
                    outputRow.Clear();
                    for (int i = 0; i < columnWidths.Length; i++)
                    {
                        outputRow.Append(String.Format("{0," + columnWidths[i] + "}", "").Replace(' ', '-')).Append(" ");
                    }
                    outputRow.AppendLine();
                    ResultsWindow.AppendText(outputRow.ToString());
                    while (reader.Read())
                    {
                        outputRow.Clear();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string col = reader.GetString(i);
                            if (col == null)
                                col = "<null>";
                            if (col.Length > columnWidths[i])
                                if(columnWidths[i] > 5)
                                    col = col.Substring(0, columnWidths[i] - 5) + "[...]";
                                else
                                    col = col.Substring(0, columnWidths[i]);
                            outputRow.Append(String.Format("{0," + (columnIsRightAligned[i] ? "" : "-") + columnWidths[i] + "}", col)).Append(" ");
                        }
                        outputRow.AppendLine();
                        ResultsWindow.AppendText(outputRow.ToString());
                    }
                    reader.Close();
                }
                else
                {
                    int affectedRows = dbCmd.ExecuteNonQuery();
                    StringBuilder outputRow = new StringBuilder();
                    outputRow.AppendLine(String.Format("{0} row(s) updated", affectedRows));
                    ResultsWindow.AppendText(outputRow.ToString());
                }
            }
        }

        #region IVsTextLinesEvents Members

        public void OnChangeLineAttributes(int iFirstLine, int iLastLine)
        {
        }

        public void OnChangeLineText(TextLineChange[] pTextLineChange, int fLast)
        {
            _isDirty = 1;
        }

        #endregion
    }
}
