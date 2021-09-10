using System.Collections.Generic;

namespace MyJetWallet.Sdk.ElkAuditLog
{
    public class TraceBufferItem
    {
        public string Index { get; set; }
        public string Id { get; set; }
        public Dictionary<string, object> Entity { get; set; }
    }
}