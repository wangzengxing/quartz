using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quartz.Api
{
    public class JobHostedService : IHostedService
    {
        private ISchedulerFactory _schedulerFactory;
        private IScheduler _scheduler;

        public JobHostedService(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //1、通过调度工厂获得调度器
            _scheduler = await _schedulerFactory.GetScheduler();
            //2、开启调度器
            await _scheduler.Start(cancellationToken);

            var assembly = Assembly.GetExecutingAssembly();
            var jobs = assembly.DefinedTypes.Where(r => r.IsClass && typeof(IJob).IsAssignableFrom(r));

            foreach (var job in jobs)
            {
                var jobAttribute = job.GetCustomAttribute<JobAttribute>();
                if (jobAttribute != null)
                {
                    //3、创建一个触发器
                    var builder = TriggerBuilder.Create();
                    if (string.IsNullOrEmpty(jobAttribute.CornExpression))
                    {
                        builder.WithSimpleSchedule();
                    }
                    else
                    {
                        builder.WithCronSchedule(jobAttribute.CornExpression);
                    }
                    var trigger = builder
                                    .Build();
                    //4、创建任务
                    var jobDetail = JobBuilder.Create(job.AsType())
                                    .WithIdentity(jobAttribute.Name, jobAttribute.Group)
                                    .Build();
                    //5、将触发器和任务器绑定到调度器中
                    await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown(cancellationToken);
        }
    }
}
