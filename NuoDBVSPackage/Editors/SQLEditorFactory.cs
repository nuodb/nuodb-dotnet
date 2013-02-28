using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace NuoDb.VisualStudio.DataTools.Editors
{
    [Guid(GuidList.guidNuoDBSQLEditorFactoryString)]
    class SQLEditorFactory : IVsEditorFactory
    {
        private IOleServiceProvider _ServiceProvider;
        #region IVsEditorFactory Members

        public int Close()
        {
            return VSConstants.S_OK;
        }

        public int CreateEditorInstance(uint grfCreateDoc, string pszMkDocument, string pszPhysicalView, IVsHierarchy pvHier, uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView, out IntPtr ppunkDocData, out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
        {
            // --- Initialize to null
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pguidCmdUI = GetType().GUID;
            pgrfCDW = 0;
            pbstrEditorCaption = null;
            // --- Validate inputs
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }
            if (punkDocDataExisting != IntPtr.Zero)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }
            // --- Create the Document (editor)
            SQLEditor newEditor = new SQLEditor(_ServiceProvider);
            ppunkDocView = Marshal.GetIUnknownForObject(newEditor);
            ppunkDocData = Marshal.GetIUnknownForObject(newEditor);
            pbstrEditorCaption = "";
            return VSConstants.S_OK;
        }

        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null;
            if (VSConstants.LOGVIEWID_Primary == rguidLogicalView)
            {
                // --- Primary view uses null as physicalView
                return VSConstants.S_OK;
            }
            else
            {
                // --- You must return E_NOTIMPL for any unrecognized logicalView values
                return VSConstants.E_NOTIMPL;
            }
        }

        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
        {
            _ServiceProvider = serviceProvider;
            return VSConstants.S_OK;
        }

        #endregion

    }
}
