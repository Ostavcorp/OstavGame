using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ostav
{
    public interface IOstavModuleLifecycle
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }

    public interface IOstavPlatformHost
    {
        IOstavPlatformRuntime Runtime { get; }
        IReadOnlyCollection<IOstavModuleHealth> ModuleHealth { get; }
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }

    public sealed class OstavPlatformHost : IOstavPlatformHost
    {
        private readonly List<HostedModuleHealth> health = new List<HostedModuleHealth>();
        private bool started;

        public OstavPlatformHost(IOstavPlatformRuntime runtime)
        {
            Runtime = runtime ?? throw new ArgumentNullException("runtime");
            foreach (IOstavModuleProvider provider in runtime.ModuleCatalog.Providers)
                health.Add(new HostedModuleHealth(provider));
        }

        public IOstavPlatformRuntime Runtime { get; private set; }
        public IReadOnlyCollection<IOstavModuleHealth> ModuleHealth
        {
            get { return health.ToArray(); }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (started) return;
            foreach (HostedModuleHealth item in health)
            {
                cancellationToken.ThrowIfCancellationRequested();
                item.SetState(OstavModuleState.Starting);
                try
                {
                    IOstavModuleLifecycle lifecycle = item.Provider as IOstavModuleLifecycle;
                    if (lifecycle != null) await lifecycle.StartAsync(cancellationToken);
                    item.SetState(OstavModuleState.Ready);
                }
                catch (OperationCanceledException)
                {
                    item.SetState(OstavModuleState.Unavailable);
                    throw;
                }
                catch
                {
                    item.SetState(OstavModuleState.Unavailable);
                    throw;
                }
            }
            started = true;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int index = health.Count - 1; index >= 0; index--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                HostedModuleHealth item = health[index];
                IOstavModuleLifecycle lifecycle = item.Provider as IOstavModuleLifecycle;
                if (lifecycle != null) await lifecycle.StopAsync(cancellationToken);
                item.SetState(OstavModuleState.Stopped);
            }
            started = false;
        }

        private sealed class HostedModuleHealth : IOstavModuleHealth
        {
            public HostedModuleHealth(IOstavModuleProvider provider)
            {
                Provider = provider;
                State = OstavModuleState.Registered;
            }
            public IOstavModuleProvider Provider { get; private set; }
            public string ModuleId { get { return Provider.Module.Id; } }
            public OstavModuleState State { get; private set; }
            public bool IsCapabilityAvailable(string capabilityId)
            {
                if (State != OstavModuleState.Ready && State != OstavModuleState.Degraded)
                    return false;
                foreach (string capability in Provider.Module.Capabilities)
                    if (string.Equals(capability, capabilityId, StringComparison.Ordinal)) return true;
                return false;
            }
            public void SetState(OstavModuleState state) { State = state; }
        }
    }
}
