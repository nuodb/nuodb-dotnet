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
using System.Globalization;

namespace NuoDb.VisualStudio.DataTools.Editors
{
    public partial class SQLEditor : UserControl,
        IVsWindowPane,
        IOleCommandTarget,
        IOleServiceProvider,
        IVsPersistDocData,              //to Enable persistence functionality for document data
        IPersistFileFormat,             //to enable the programmatic loading or saving of an object 
                                        //in a format specified by the user.
        IVsFindTarget,                  //to implement find and replace capabilities within the editor
        IVsTextViewEvents,
        IVsTextLinesEvents,
        IVsFileChangeEvents,            //to notify the client when file changes on disk
        IVsDocDataFileChangeControl     //to Determine whether changes to files made outside 
                                        //of the editor should be ignored
    {
        private const string MyExtension = ".nuosql";
        private IOleServiceProvider _vsServiceProvider;
        private DbProviderFactory _providerFactory;
        private DbConnection _connection;
        private IVsTextView _activeTextView = null;
        private string _fileName;
        private int _isDirty = 0;
        private string Server;
        private string User;

        // data members to implement notification of file changes occurred outside of our editor
        private int ignoreFileChangeLevel;
        private bool fileChangedTimerSet;
        private Timer FileChangeTrigger = new Timer();
        private IVsFileChangeEx vsFileChangeEx;
        private uint vsFileChangeCookie;
        private bool loading;

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

        //
        // Summary:
        //     Returns an object that represents a service provided by the System.ComponentModel.Component
        //     or by its System.ComponentModel.Container.
        //
        // Parameters:
        //   service:
        //     A service provided by the System.ComponentModel.Component.
        //
        // Returns:
        //     An System.Object that represents a service provided by the System.ComponentModel.Component,
        //     or null if the System.ComponentModel.Component does not provide the specified
        //     service.
        protected override object GetService(Type service)
        {
            if (_vsServiceProvider != null)
            {
                ServiceProvider sp = new ServiceProvider(_vsServiceProvider);
                return sp.GetService(service);
            }
            return null;
        }

        public IVsWindowFrame editorFrame
        {
            get
            {
                return GetService(typeof(SVsWindowFrame)) as IVsWindowFrame;
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
            SetFileChangeNotification(null, false);
            return VSConstants.S_OK;
        }

        public int GetGuidEditorType(out Guid pClassID)
        {
            return ((IPersistFileFormat)this).GetClassID(out pClassID);
        }

        public int IsDocDataDirty(out int pfDirty)
        {
            return ((IPersistFileFormat)this).IsDirty(out pfDirty);
        }

        public int IsDocDataReloadable(out int pfReloadable)
        {
            pfReloadable = 1;
            return VSConstants.S_OK;
        }

        public int LoadDocData(string pszMkDocument)
        {
            return ((IPersistFileFormat)this).Load(pszMkDocument, 0, 0);
        }

        public int OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
        {
            return VSConstants.S_OK;
        }

        public int ReloadDocData(uint grfFlags)
        {
            return ((IPersistFileFormat)this).Load(_fileName, grfFlags, 0);
        }

        public int RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            _fileName = pszMkDocumentNew;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Saves the document data. Before actually saving the file, we first need to indicate to the environment
        /// that a file is about to be saved. This is done through the "SVsQueryEditQuerySave" service. We call the
        /// "QuerySaveFile" function on the service instance and then proceed depending on the result returned as follows:
        /// If result is QSR_SaveOK - We go ahead and save the file and the file is not read only at this point.
        /// If result is QSR_ForceSaveAs - We invoke the "Save As" functionality which will bring up the Save file name 
        ///                                dialog 
        /// If result is QSR_NoSave_Cancel - We cancel the save operation and indicate that the document could not be saved
        ///                                by setting the "pfSaveCanceled" flag
        /// If result is QSR_NoSave_Continue - Nothing to do here as the file need not be saved
        /// </summary>
        /// <param name="dwSave">Flags which specify the file save options:
        /// VSSAVE_Save        - Saves the current file to itself.
        /// VSSAVE_SaveAs      - Prompts the User for a filename and saves the file to the file specified.
        /// VSSAVE_SaveCopyAs  - Prompts the user for a filename and saves a copy of the file with a name specified.
        /// VSSAVE_SilentSave  - Saves the file without prompting for a name or confirmation.  
        /// </param>
        /// <param name="pbstrMkDocumentNew">Pointer to the path to the new document</param>
        /// <param name="pfSaveCanceled">value 1 if the document could not be saved</param>
        /// <returns></returns>
        public int SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            pbstrMkDocumentNew = null;
            pfSaveCanceled = 0;
            int hr = VSConstants.S_OK;

            switch (dwSave)
            {
                case VSSAVEFLAGS.VSSAVE_Save:
                case VSSAVEFLAGS.VSSAVE_SilentSave:
                    {
                        IVsQueryEditQuerySave2 queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

                        // Call QueryEditQuerySave
                        uint result = 0;
                        hr = queryEditQuerySave.QuerySaveFile(
                                _fileName,        // filename
                                0,    // flags
                                null,            // file attributes
                                out result);    // result
                        if (ErrorHandler.Failed(hr))
                            return hr;

                        // Process according to result from QuerySave
                        switch ((tagVSQuerySaveResult)result)
                        {
                            case tagVSQuerySaveResult.QSR_NoSave_Cancel:
                                // Note that this is also case tagVSQuerySaveResult.QSR_NoSave_UserCanceled because these
                                // two tags have the same value.
                                pfSaveCanceled = ~0;
                                break;

                            case tagVSQuerySaveResult.QSR_SaveOK:
                                {
                                    // Call the shell to do the save for us
                                    IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(dwSave, (IPersistFileFormat)this, _fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                                    if (ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_ForceSaveAs:
                                {
                                    // Call the shell to do the SaveAS for us
                                    IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(VSSAVEFLAGS.VSSAVE_SaveAs, (IPersistFileFormat)this, _fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                                    if (ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_NoSave_Continue:
                                // In this case there is nothing to do.
                                break;

                            default:
                                throw new NotSupportedException("Unsupported result from QEQS");
                        }
                        break;
                    }
                case VSSAVEFLAGS.VSSAVE_SaveAs:
                case VSSAVEFLAGS.VSSAVE_SaveCopyAs:
                    {
                        // Make sure the file name as the right extension
                        if (String.Compare(MyExtension, System.IO.Path.GetExtension(_fileName), true, CultureInfo.CurrentCulture) != 0)
                        {
                            _fileName += MyExtension;
                        }
                        // Call the shell to do the save for us
                        IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                        hr = uiShell.SaveDocDataToFile(dwSave, (IPersistFileFormat)this, _fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                        if (ErrorHandler.Failed(hr))
                            return hr;
                        break;
                    }
                default:
                    throw new ArgumentException("Unsupported Save flag");
            };

            return VSConstants.S_OK;
        }

        public int SetUntitledDocPath(string pszDocDataPath)
        {
            return ((IPersistFileFormat)this).InitNew(0);
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


        #region IPersistFileFormat Members

        public int GetClassID(out Guid pClassID)
        {
            pClassID = GuidList.guidNuoDBObjectFactoryService;
            return VSConstants.S_OK;
        }

        public int GetCurFile(out string ppszFilename, out uint pnFormatIndex)
        {
            ppszFilename = _fileName;
            pnFormatIndex = 0;
            return VSConstants.S_OK;
        }

        public int GetFormatList(out string ppszFormatList)
        {
            ppszFormatList = "NuoDB SQL Query\n*"+MyExtension+"\n\n";
            return VSConstants.S_OK;
        }

        public int InitNew(uint nFormatIndex)
        {
            CommandWindow.Text = "";
            // until someone change the file, we can consider it not dirty as
            // the user would be annoyed if we prompt him to save an empty file
            _isDirty = 0;
            return VSConstants.S_OK;
        }

        public int IsDirty(out int pfIsDirty)
        {
            pfIsDirty = _isDirty;
            return VSConstants.S_OK;
        }

        public new int Load(string pszFilename, uint grfMode, int fReadOnly)
        {
            if (pszFilename == null)
                return VSConstants.E_INVALIDARG;

            loading = true;
            int hr = VSConstants.S_OK;
            try
            {
                // Show the wait cursor while loading the file
                IVsUIShell VsUiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                if (VsUiShell != null)
                {
                    // Note: we don't want to throw or exit if this call fails, so
                    // don't check the return code.
                    hr = VsUiShell.SetWaitCursor();
                }

                // Load the file
                StreamReader streamReader = new StreamReader(pszFilename);
                string text = streamReader.ReadToEnd();
                streamReader.Close();
                CommandWindow.Text = text;
                _isDirty = 0;

                //Determine if the file is read only on the file system
                FileAttributes fileAttrs = File.GetAttributes(pszFilename);

                int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;

                //Set readonly if either the file is readonly for the user or on the file system
                if (0 == isReadOnly && 0 == fReadOnly)
                    SetReadOnly(false);
                else
                    SetReadOnly(true);

                // Hook up to file change notifications
                if (String.IsNullOrEmpty(_fileName) || 0 != String.Compare(_fileName, pszFilename, true, CultureInfo.CurrentCulture))
                {
                    _fileName = pszFilename;
                    SetFileChangeNotification(pszFilename, true);

                    // Notify the load or reload
                    NotifyDocChanged();
                }
            }
            finally
            {
                loading = false;
            }
            return VSConstants.S_OK;
        }

        public int Save(string pszFilename, int fRemember, uint nFormatIndex)
        {
            int hr = VSConstants.S_OK;
            bool doingSaveOnSameFile = false;
            // If file is null or same --> SAVE
            if (pszFilename == null || pszFilename == _fileName)
            {
                fRemember = 1;
                doingSaveOnSameFile = true;
            }

            //Suspend file change notifications for only Save since we don't have notifications setup
            //for SaveAs and SaveCopyAs (as they are different files)
            if (doingSaveOnSameFile)
                this.SuspendFileChangeNotification(pszFilename, 1);

            try
            {
                string text = CommandWindow.Text;
                StreamWriter writer = new StreamWriter(pszFilename);
                writer.Write(text);
                writer.Close();
            }
            catch (IOException)
            {
                hr = VSConstants.E_FAIL;
            }
            finally
            {
                //restore the file change notifications
                if (doingSaveOnSameFile)
                    this.SuspendFileChangeNotification(pszFilename, 0);
            }

            if (VSConstants.E_FAIL == hr)
                return hr;

            if (fRemember != 0)
            {
                if (null != pszFilename && !_fileName.Equals(pszFilename))
                {
                    SetFileChangeNotification(_fileName, false); //remove notification from old file
                    SetFileChangeNotification(pszFilename, true); //add notification for new file
                    _fileName = pszFilename;     //cache the new file name
                }
                _isDirty = 0;
                SetReadOnly(false);             //set read only to false since you were successfully able
            }
            return VSConstants.S_OK;
        }

        public int SaveCompleted(string pszFilename)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsDocDataFileChangeControl Members

        /// <summary>
        /// Used to determine whether changes to DocData in files should be ignored or not
        /// </summary>
        /// <param name="fIgnore">a non zero value indicates that the file changes should be ignored
        /// </param>
        /// <returns></returns>
        public int IgnoreFileChanges(int fIgnore)
        {
            if (fIgnore != 0)
            {
                ignoreFileChangeLevel++;
            }
            else
            {
                if (ignoreFileChangeLevel > 0)
                    ignoreFileChangeLevel--;

                // We need to check here if our file has changed from "Read Only"
                // to "Read/Write" or vice versa while the ignore level was non-zero.
                // This may happen when a file is checked in or out under source
                // code control. We need to check here so we can update our caption.
                FileAttributes fileAttrs = File.GetAttributes(_fileName);
                int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;
                SetReadOnly(isReadOnly != 0);
            }
            return VSConstants.S_OK;
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
            Regex regex = new Regex(@"([^;'""]+|""[^""]*""|'[^']*'|`[^`]*`)*[;]");
            MatchCollection coll = regex.Matches(sql);
            foreach (Match m in coll)
            {
                string cmd = m.Value.TrimEnd(new char[] { ';' }).Trim();
                if (cmd.Length == 0)
                    continue;

                // the command could be preceded by comments (between -- and a new line)
                cmd = cmd.Replace("\r", "");
                string[] lines = cmd.Split(new char[] { '\n' });
                StringBuilder sqlCmd = new StringBuilder(cmd.Length);
                foreach (string line in lines)
                {
                    StringBuilder sqlString = new StringBuilder(line.Length);
                    int state = 0;
                    bool inSingleQuotes = false, inDoubleQuotes = false, inSmartQuotes = false;
                    foreach (char c in line)
                    {
                        if (c == '\'' && !(inDoubleQuotes || inSmartQuotes))
                        {
                            inSingleQuotes = !inSingleQuotes;
                            state = 0;
                            sqlString.Append(c);
                            continue;
                        }
                        else if (c == '\"' && !(inSingleQuotes || inSmartQuotes))
                        {
                            inDoubleQuotes = !inDoubleQuotes;
                            state = 0;
                            sqlString.Append(c);
                            continue;
                        }
                        else if (c == '`' && !(inSingleQuotes || inDoubleQuotes))
                        {
                            inSmartQuotes = !inSmartQuotes;
                            state = 0;
                            sqlString.Append(c);
                            continue;
                        }
                        if (inSingleQuotes || inDoubleQuotes || inSmartQuotes)
                        {
                            sqlString.Append(c);
                            continue;
                        }

                        if (state == 0 && c == '-')
                            state = 1;
                        else if (state == 1)
                        {
                            if (c == '-')
                            {
                                // comment found
                                state = 2;
                                break;
                            }
                            else
                            {
                                sqlString.Append('-');
                                sqlString.Append(c);
                                state = 0;
                            }
                        }
                        else
                            sqlString.Append(c);
                    }
                    if (state == 1)
                        sqlString.Append('-');

                    sqlCmd.AppendLine(sqlString.ToString());
                }
                cmd = sqlCmd.ToString().Trim();
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

        #region IVsFileChangeEvents Members

        /// <summary>
        /// Notify the editor of the changes made to a directory
        /// </summary>
        /// <param name="pszDirectory">Name of the directory that has changed</param>
        /// <returns></returns>
        public int DirectoryChanged(string pszDirectory)
        {
            //Nothing to do here
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notify the editor of the changes made to one or more files
        /// </summary>
        /// <param name="cChanges">Number of files that have changed</param>
        /// <param name="rgpszFile">array of the files names that have changed</param>
        /// <param name="rggrfChange">Array of the flags indicating the type of changes</param>
        /// <returns></returns>
        public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            //check the different parameters
            if (0 == cChanges || null == rgpszFile || null == rggrfChange)
                return VSConstants.E_INVALIDARG;

            //ignore file changes if we are in that mode
            if (ignoreFileChangeLevel != 0)
                return VSConstants.S_OK;

            for (uint i = 0; i < cChanges; i++)
            {
                if (!String.IsNullOrEmpty(rgpszFile[i]) && String.Compare(rgpszFile[i], _fileName, true, CultureInfo.CurrentCulture) == 0)
                {
                    // if the readonly state (file attributes) have changed we can immediately update
                    // the editor to match the new state (either readonly or not readonly) immediately
                    // without prompting the user.
                    if (0 != (rggrfChange[i] & (int)_VSFILECHANGEFLAGS.VSFILECHG_Attr))
                    {
                        FileAttributes fileAttrs = File.GetAttributes(_fileName);
                        int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;
                        SetReadOnly(isReadOnly != 0);
                    }
                    // if it looks like the file contents have changed (either the size or the modified
                    // time has changed) then we need to prompt the user to see if we should reload the
                    // file. it is important to not syncronisly reload the file inside of this FilesChanged
                    // notification. first it is possible that there will be more than one FilesChanged 
                    // notification being sent (sometimes you get separate notifications for file attribute
                    // changing and file size/time changing). also it is the preferred UI style to not
                    // prompt the user until the user re-activates the environment application window.
                    // this is why we use a timer to delay prompting the user.
                    if (0 != (rggrfChange[i] & (int)(_VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Size)))
                    {
                        if (!fileChangedTimerSet)
                        {
                            FileChangeTrigger = new Timer();
                            fileChangedTimerSet = true;
                            FileChangeTrigger.Interval = 1000;
                            FileChangeTrigger.Tick += new EventHandler(this.OnFileChangeEvent);
                            FileChangeTrigger.Enabled = true;
                        }
                    }
                }
            }

            return VSConstants.S_OK;
        }

        #endregion

        #region File Change Notification Helpers

        /// <summary>
        /// In this function we inform the shell when we wish to receive 
        /// events when our file is changed or we inform the shell when 
        /// we wish not to receive events anymore.
        /// </summary>
        /// <param name="pszFileName">File name string</param>
        /// <param name="fStart">TRUE indicates advise, FALSE indicates unadvise.</param>
        /// <returns>Result of teh operation</returns>
        private int SetFileChangeNotification(string pszFileName, bool fStart)
        {
            int result = VSConstants.E_FAIL;

            //Get the File Change service
            if (null == vsFileChangeEx)
                vsFileChangeEx = (IVsFileChangeEx)GetService(typeof(SVsFileChangeEx));
            if (null == vsFileChangeEx)
                return VSConstants.E_UNEXPECTED;

            // Setup Notification if fStart is TRUE, Remove if fStart is FALSE.
            if (fStart)
            {
                if (vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                {
                    //Receive notifications if either the attributes of the file change or 
                    //if the size of the file changes or if the last modified time of the file changes
                    result = vsFileChangeEx.AdviseFileChange(pszFileName,
                        (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Attr | _VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Time),
                        (IVsFileChangeEvents)this,
                        out vsFileChangeCookie);
                    if (vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                        return VSConstants.E_FAIL;
                }
            }
            else
            {
                if (vsFileChangeCookie != VSConstants.VSCOOKIE_NIL)
                {
                    result = vsFileChangeEx.UnadviseFileChange(vsFileChangeCookie);
                    vsFileChangeCookie = VSConstants.VSCOOKIE_NIL;
                }
            }
            return result;
        }

        /// <summary>
        /// In this function we suspend receiving file change events for
        /// a file or we reinstate a previously suspended file depending
        /// on the value of the given fSuspend flag.
        /// </summary>
        /// <param name="pszFileName">File name string</param>
        /// <param name="fSuspend">TRUE indicates that the events needs to be suspended</param>
        /// <returns></returns>

        private int SuspendFileChangeNotification(string pszFileName, int fSuspend)
        {
            if (null == vsFileChangeEx)
                vsFileChangeEx = (IVsFileChangeEx)GetService(typeof(SVsFileChangeEx));
            if (null == vsFileChangeEx)
                return VSConstants.E_UNEXPECTED;

            if (0 == fSuspend)
            {
                // we are transitioning from suspended to non-suspended state - so force a
                // sync first to avoid asynchronous notifications of our own change
                if (vsFileChangeEx.SyncFile(pszFileName) == VSConstants.E_FAIL)
                    return VSConstants.E_FAIL;
            }

            //If we use the VSCOOKIE parameter to specify the file, then pszMkDocument parameter 
            //must be set to a null reference and vice versa 
            return vsFileChangeEx.IgnoreFile(vsFileChangeCookie, null, fSuspend);
        }
        #endregion
        
        /// <summary>
        /// This event is triggered when one of the files loaded into the environment has changed outside of the
        /// editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileChangeEvent(object sender, System.EventArgs e)
        {
            //Disable the timer
            FileChangeTrigger.Enabled = false;

            string message = this.GetResourceString("FileChangedOutsideIDE");    //get the message string from the resource
            IVsUIShell VsUiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            int result = 0;
            Guid tempGuid = Guid.Empty;
            if (VsUiShell != null)
            {
                //Show up a message box indicating that the file has changed outside of VS environment
                ErrorHandler.ThrowOnFailure(VsUiShell.ShowMessageBox(0, ref tempGuid, _fileName, message, null, 0,
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_QUERY, 0, out result));
            }
            //if the user selects "Yes", reload the current file
            if (result == (int)DialogResult.Yes)
            {
                ErrorHandler.ThrowOnFailure(((IVsPersistDocData)this).ReloadDocData(0));
            }

            fileChangedTimerSet = false;
        }

        /// <summary>
        /// This method loads a localized string based on the specified resource.
        /// </summary>
        /// <param name="resourceName">Resource to load</param>
        /// <returns>String loaded for the specified resource</returns>
        internal string GetResourceString(string resourceName)
        {
            string resourceValue;
            IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
            }
            Guid packageGuid = new Guid(GuidList.guidNuoDBVSPackagePkgString);
            int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            return resourceValue;
        }

        /// <summary>
        /// Used to ReadOnly property for the Rich TextBox and correspondingly update the editor caption
        /// </summary>
        /// <param name="_isFileReadOnly">Indicates whether the file loaded is Read Only or not</param>
        private void SetReadOnly(bool _isFileReadOnly)
        {
            //this.editorControl.RichTextBoxControl.ReadOnly = _isFileReadOnly;

            //update editor caption with "[Read Only]" or "" as necessary
            string editorCaption = "";
            if (_isFileReadOnly)
                editorCaption = this.GetResourceString("ReadOnlyMarker");
            ErrorHandler.ThrowOnFailure(editorFrame.SetProperty((int)__VSFPROPID.VSFPROPID_EditorCaption, editorCaption));
            //backupObsolete = true;
        }

        /// <summary>
        /// Gets an instance of the RunningDocumentTable (RDT) service which manages the set of currently open 
        /// documents in the environment and then notifies the client that an open document has changed
        /// </summary>
        private void NotifyDocChanged()
        {
            // Make sure that we have a file name
            if (_fileName.Length == 0)
                return;

            // Get a reference to the Running Document Table
            IVsRunningDocumentTable runningDocTable = (IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable));

            // Lock the document
            uint docCookie;
            IVsHierarchy hierarchy;
            uint itemID;
            IntPtr docData;
            int hr = runningDocTable.FindAndLockDocument(
                (uint)_VSRDTFLAGS.RDT_ReadLock,
                _fileName,
                out hierarchy,
                out itemID,
                out docData,
                out docCookie
            );
            ErrorHandler.ThrowOnFailure(hr);

            // Send the notification
            hr = runningDocTable.NotifyDocumentChanged(docCookie, (uint)__VSRDTATTRIB.RDTA_DocDataReloaded);

            // Unlock the document.
            // Note that we have to unlock the document even if the previous call failed.
            ErrorHandler.ThrowOnFailure(runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, docCookie));

            // Check ff the call to NotifyDocChanged failed.
            ErrorHandler.ThrowOnFailure(hr);
        }


        #region IVsFindTarget Members

        public int Find(string pszSearch, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out uint pResult)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).Find(pszSearch, grfOptions, fResetStartPoint, pHelper, out pResult);
        }

        public int GetCapabilities(bool[] pfImage, uint[] pgrfOptions)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).GetCapabilities(pfImage, pgrfOptions);
        }

        public int GetCurrentSpan(TextSpan[] pts)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).GetCurrentSpan(pts);
        }

        public int GetFindState(out object ppunk)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).GetFindState(out ppunk);
        }

        public int GetMatchRect(RECT[] prc)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).GetMatchRect(prc);
        }

        public int GetProperty(uint propid, out object pvar)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).GetProperty(propid, out pvar);
        }

        public int GetSearchImage(uint grfOptions, IVsTextSpanSet[] ppSpans, out IVsTextImage ppTextImage)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).GetSearchImage(grfOptions, ppSpans, out ppTextImage);
        }

        public int MarkSpan(TextSpan[] pts)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).MarkSpan(pts);
        }

        public int NavigateTo(TextSpan[] pts)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).NavigateTo(pts);
        }

        public int NotifyFindTarget(uint notification)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).NotifyFindTarget(notification);
        }

        public int Replace(string pszSearch, string pszReplace, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out int pfReplaced)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).Replace(pszSearch, pszReplace, grfOptions, fResetStartPoint, pHelper, out pfReplaced);
        }

        public int SetFindState(object pUnk)
        {
            return (CommandWindow.VsCodeWindow as IVsFindTarget).SetFindState(pUnk);
        }

        #endregion
    }
}
