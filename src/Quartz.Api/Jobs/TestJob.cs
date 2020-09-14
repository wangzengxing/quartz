using System;
using System.Threading.Tasks;

namespace Quartz.Api.Jobs
{
    [Job(nameof(TestJob), GroupNames.Default)]
    public class TestJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("正在执行任务！");
            return Task.CompletedTask;
        }
    }
}