namespace Blox.Environment.Jobs
{
    public interface IJob : Unity.Jobs.IJob
    {
        public void Dispose();
    }
}