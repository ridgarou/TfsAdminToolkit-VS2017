using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using System.Windows;
using System.Windows.Controls;


namespace mskold.TfsAdminToolKit
{
    class VSIntegration
    {
        public static void OpenDocumentAndNavigateTo(string path, int line, int column)
        {
            IVsUIShellOpenDocument openDoc =
                Package.GetGlobalService(typeof(IVsUIShellOpenDocument))
                        as IVsUIShellOpenDocument;
            if (openDoc == null)
            {
                return;
            }
            IVsWindowFrame frame;
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp;
            IVsUIHierarchy hier;
            uint itemid;
            Guid logicalView = VSConstants.LOGVIEWID_Code;
            if (ErrorHandler.Failed(
                openDoc.OpenDocumentViaProject(path, ref logicalView, out sp, out hier, out itemid, out frame))
                || frame == null)
            {
                return;
            }
            object docData;
            frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out docData);

            // Get the VsTextBuffer  
            VsTextBuffer buffer = docData as VsTextBuffer;
            if (buffer == null)
            {
                IVsTextBufferProvider bufferProvider = docData as IVsTextBufferProvider;
                if (bufferProvider != null)
                {
                    IVsTextLines lines;
                    ErrorHandler.ThrowOnFailure(bufferProvider.GetTextBuffer(out lines));
                    buffer = lines as VsTextBuffer;
                    Debug.Assert(buffer != null, "IVsTextLines does not implement IVsTextBuffer");
                    if (buffer == null)
                    {
                        return;
                    }
                }
            }
            // Finally, perform the navigation.  
            IVsTextManager mgr = Package.GetGlobalService(typeof(VsTextManagerClass))
                 as IVsTextManager;
            if (mgr == null)
            {
                return;
            }
            mgr.NavigateToLineAndColumn(buffer, ref logicalView, line, column, line, column);
        }
    }

  
}
