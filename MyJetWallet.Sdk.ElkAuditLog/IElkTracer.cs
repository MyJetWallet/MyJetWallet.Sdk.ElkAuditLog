using MyJetWallet.Sdk.Service;

namespace MyJetWallet.Sdk.ElkAuditLog
{
    public interface IElkTracer
    {
        void Init(LogElkSettings config, string name, string source);
        void Trace(string id, object entity, string prefix = "_");
        object GetFromTodayIndex(string id);
    }
}