using System;
using Unity.Jobs;

namespace Blox.CommonNS
{
    public readonly struct JobDescriptor<T> where T : IJob, IDisposable
    {
        public readonly string name;
        public readonly T job;
        public readonly JobHandle handle;

        public JobDescriptor(string name, T job, JobHandle handle)
        {
            this.name = name;
            this.job = job;
            this.handle = handle;
        }
    }
}