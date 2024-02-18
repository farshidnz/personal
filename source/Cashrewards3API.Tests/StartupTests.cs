using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests
{
    [TestFixture]
    public class StartupTests
    {
        private Mock<IConfiguration> configuration;
        private Mock<IServiceCollection> serviceCollection;

        [SetUp]
        public void Setup()
        {
            configuration = new Mock<IConfiguration>();
            serviceCollection = new Mock<IServiceCollection>();
        } 

        public Startup SUT()
        {
            return new Startup(configuration.Object);
        }

        [Test]
        public void ShouldRegisterOpenTelemetry()
        {
            SUT().AddOpenTelementry(serviceCollection.Object);
            var x = serviceCollection.Invocations;
            serviceCollection.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => 
                sd.Lifetime == ServiceLifetime.Singleton &&
                sd.ServiceType == typeof(TracerProvider)
            )));
        }
    }
}
