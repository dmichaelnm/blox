using Unity.Jobs;

namespace Blox.EnvironmentNS.JobsNS
{
    /// <summary>
    /// This struct contains informations about a job.
    /// </summary>
    /// <typeparam name="T">The type of the job struct</typeparam>
    public readonly struct JobData<T> where T : IJob
    {
        /// <summary>
        /// The job itself.
        /// </summary>
        public readonly T Job;

        /// <summary>
        /// The handle of the scheduled job.
        /// </summary>
        public readonly JobHandle Handle;

        /// <summary>
        /// Constructor for a scheduled job.
        /// </summary>
        /// <param name="job">The job itself</param>
        /// <param name="handle">The handle of the scheduled job</param>
        public JobData(T job, JobHandle handle)
        {
            Job = job;
            Handle = handle;
        }
    }
}