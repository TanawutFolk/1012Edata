using System;
using System.Runtime.InteropServices;

namespace EdataManagement.Utilities
{
    /// <summary>
    /// COM message filter for the thread that drives Outlook (EmailCls.SendEmail): when Outlook is busy - still taking
    /// 250 attachments in, showing a dialog - it rejects calls ("application is busy" / "call was rejected by callee");
    /// with this filter COM waits and retries the call instead of failing it, for up to RETRY_FOR_MS.
    /// Register on a single-threaded-apartment thread only (CoRegisterMessageFilter fails on an MTA thread);
    /// Dispose puts the previous filter back. The pattern of Microsoft's "Fixing 'Application is Busy' and 'Call was
    /// Rejected By Callee' errors" for Office automation.
    /// </summary>
    public sealed class OutlookMessageFilter : IOleMessageFilter, IDisposable
    {
        private const int SERVERCALL_ISHANDLED = 0;
        private const int SERVERCALL_RETRYLATER = 2;
        private const int PENDINGMSG_WAITDEFPROCESS = 2;

        private const int RETRY_EVERY_MS = 100;
        private const int RETRY_FOR_MS = 120000;

        [DllImport("Ole32.dll")]
        private static extern int CoRegisterMessageFilter(IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);

        private IOleMessageFilter _previous;
        private bool _registered;

        public static OutlookMessageFilter Register()
        {
            OutlookMessageFilter filter = new OutlookMessageFilter();
            filter._registered = CoRegisterMessageFilter(filter, out filter._previous) == 0;
            return filter;
        }

        public void Dispose()
        {
            if (!_registered) return;

            IOleMessageFilter ours;
            CoRegisterMessageFilter(_previous, out ours);
            _registered = false;
        }

        int IOleMessageFilter.HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo)
        {
            return SERVERCALL_ISHANDLED;
        }

        /// <summary>Busy: try again in RETRY_EVERY_MS, until RETRY_FOR_MS have gone by (-1 = give up, the call fails).</summary>
        int IOleMessageFilter.RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType)
        {
            return dwRejectType == SERVERCALL_RETRYLATER && dwTickCount < RETRY_FOR_MS ? RETRY_EVERY_MS : -1;
        }

        int IOleMessageFilter.MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType)
        {
            return PENDINGMSG_WAITDEFPROCESS;
        }
    }

    [ComImport, Guid("00000016-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOleMessageFilter
    {
        [PreserveSig]
        int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo);

        [PreserveSig]
        int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType);

        [PreserveSig]
        int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType);
    }
}
