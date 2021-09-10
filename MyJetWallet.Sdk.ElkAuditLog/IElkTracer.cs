using MyJetWallet.Sdk.Service;

namespace MyJetWallet.Sdk.ElkAuditLog
{
    public interface IElkTracer
    {
        void Trace(string id, object entity, string prefix = "_");
    }
}