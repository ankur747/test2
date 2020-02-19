using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test2
{
    public class SimpleJobService
    {
        IServiceProvider _serviceProvider;


        public SimpleJobService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void AddMsg()
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            using testContext context = scope.ServiceProvider.GetRequiredService<testContext>();
            var lastId = context.Alpha.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).ToList();
            Alpha a = new Alpha
            {
                Msg = $"Hello {lastId[0]}"
            };
            context.Add(a);
            context.SaveChanges();

        }

    }
}
