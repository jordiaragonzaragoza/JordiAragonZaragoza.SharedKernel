namespace JordiAragonZaragoza.SharedKernel.ArchitectureTests
{
    using FluentAssertions;
    using JordiAragonZaragoza.SharedKernel;
    using JordiAragonZaragoza.SharedKernel.Application;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.IntegrationMessages;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using JordiAragonZaragoza.SharedKernel.Domain;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts;
    using JordiAragonZaragoza.SharedKernel.Infrastructure;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts;
    using NetArchTest.Rules;
    using Xunit;

    public class DependencyTests
    {
        private readonly string sharedKernelNamespace = SharedKernelAssemblyReference.Assembly.GetName().Name!;
        private readonly string sharedKernelContractsNamespace = SharedKernelContractsAssemblyReference.Assembly.GetName().Name!;
        private readonly string domainNamespace = DomainAssemblyReference.Assembly.GetName().Name!;
        private readonly string domainContractsNamespace = DomainContractsAssemblyReference.Assembly.GetName().Name!;
        private readonly string applicationNamespace = ApplicationAssemblyReference.Assembly.GetName().Name!;
        private readonly string applicationContractsNamespace = ApplicationContractsAssemblyReference.Assembly.GetName().Name!;
        private readonly string applicationContractsIntegrationMessagesNamespace = ApplicationContractsIntegrationMessagesAssemblyReference.Assembly.GetName().Name!;
        private readonly string infrastructureNamespace = InfrastructureAssemblyReference.Assembly.GetName().Name!;
        private readonly string infrastructureEntityFrameworkNamespace = InfrastructureEntityFrameworkAssemblyReference.Assembly.GetName().Name!;
        private readonly string infrastructureEventStoreNamespace = InfrastructureEventStoreAssemblyReference.Assembly.GetName().Name!;
        private readonly string httpRestfulApiNamespace = HttpRestfulApiAssemblyReference.Assembly.GetName().Name!;
        private readonly string httpRestfulApiContractsNamespace = HttpRestfulApiContractsAssemblyReference.Assembly.GetName().Name!;
        private readonly string[] allProjects;

        public DependencyTests()
        {
            this.allProjects =
            [
                this.sharedKernelNamespace,
                this.sharedKernelContractsNamespace,
                this.domainNamespace,
                this.domainContractsNamespace,
                this.applicationNamespace,
                this.applicationContractsNamespace,
                this.applicationContractsIntegrationMessagesNamespace,
                this.infrastructureNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.infrastructureEventStoreNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
            ];
        }

        [Fact]
        public void SharedKernelContracts_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = SharedKernelContractsAssemblyReference.Assembly;

            var otherProjects = new[]
            {
                this.sharedKernelNamespace,
                this.domainNamespace,
                this.domainContractsNamespace,
                this.applicationNamespace,
                this.applicationContractsNamespace,
                this.applicationContractsIntegrationMessagesNamespace,
                this.infrastructureNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.infrastructureEventStoreNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(otherProjects)
                .Or()
                .HaveDependencyOn(this.sharedKernelContractsNamespace)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            // Assert.
            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }

        [Fact]
        public void SharedKernel_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = SharedKernelAssemblyReference.Assembly;

            var otherProjects = new[]
            {
                this.domainNamespace,
                this.domainContractsNamespace,
                this.applicationNamespace,
                this.applicationContractsNamespace,
                this.applicationContractsIntegrationMessagesNamespace,
                this.infrastructureNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.infrastructureEventStoreNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(otherProjects)
                .Or()
                .HaveDependencyOn(this.sharedKernelContractsNamespace)
                .Or()
                .HaveDependencyOn(this.sharedKernelNamespace)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            // Assert.
            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }

        [Fact]
        public void DomainContracts_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = DomainContractsAssemblyReference.Assembly;

            var otherProjects = new[]
            {
                this.domainNamespace,
                this.applicationNamespace,
                this.applicationContractsNamespace,
                this.applicationContractsIntegrationMessagesNamespace,
                this.infrastructureNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.infrastructureEventStoreNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(otherProjects)
                .Or()
                .HaveDependencyOn(this.domainContractsNamespace)
                .Or()
                .HaveDependencyOn(this.sharedKernelContractsNamespace)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }

        [Fact]
        public void Domain_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = DomainAssemblyReference.Assembly;

            var forbiddenDependencies = new[]
            {
                this.applicationNamespace,
                this.applicationContractsNamespace,
                this.applicationContractsIntegrationMessagesNamespace,
                this.infrastructureNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.infrastructureEventStoreNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
            };

            var dependencies = new[]
            {
                this.sharedKernelNamespace,
                this.domainContractsNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(forbiddenDependencies)
                .Or()
                .HaveDependencyOn(this.domainNamespace)
                .Or()
                .HaveDependencyOnAny(dependencies)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            // Assert.
            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }

        [Fact]
        public void ApplicationContracts_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = ApplicationContractsAssemblyReference.Assembly;

            var forbiddenDependencies = new[]
            {
                this.sharedKernelNamespace,
                this.domainNamespace,
                this.applicationNamespace,
                this.applicationContractsNamespace,
                this.infrastructureNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.infrastructureEventStoreNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
            };

            var dependencies = new[]
            {
                this.applicationContractsIntegrationMessagesNamespace,
                this.domainContractsNamespace,
                this.sharedKernelContractsNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(forbiddenDependencies)
                .Or()
                .HaveDependencyOn(this.applicationContractsNamespace)
                .Or()
                .HaveDependencyOnAny(dependencies)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            // Assert.
            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }

        [Fact]
        public void ApplicationContractsIntegrationMessages_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = ApplicationContractsIntegrationMessagesAssemblyReference.Assembly;

            var otherProjects = new[]
            {
                this.sharedKernelNamespace,
                this.sharedKernelContractsNamespace,
                this.domainNamespace,
                this.domainContractsNamespace,
                this.applicationNamespace,
                this.applicationContractsNamespace,
                this.infrastructureNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.infrastructureEventStoreNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(otherProjects)
                .Or()
                .HaveDependencyOn(this.applicationContractsIntegrationMessagesNamespace)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            // Assert.
            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }

        [Fact]
        public void Application_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = ApplicationAssemblyReference.Assembly;

            var forbiddenDependencies = new[]
            {
                this.infrastructureNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.infrastructureEventStoreNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
            };

            var otherProjects = new[]
            {
                this.applicationContractsNamespace,
                this.domainNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(forbiddenDependencies)
                .Or()
                .HaveDependencyOn(this.applicationNamespace)
                .Or()
                .HaveDependencyOnAny(otherProjects)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            // Assert.
            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }

        [Fact]
        public void Infrastructure_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = InfrastructureAssemblyReference.Assembly;

            var forbiddenDependencies = new[]
            {
                this.domainNamespace,
                this.applicationNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.infrastructureEventStoreNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
            };

            var otherProjects = new[]
            {
                this.sharedKernelNamespace,
                this.applicationContractsNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(forbiddenDependencies)
                .Or()
                .HaveDependencyOn(this.infrastructureNamespace)
                .Or()
                .HaveDependencyOnAny(otherProjects)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            // Assert.
            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }

        [Fact]
        public void InfrastructureEntityFramework_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = InfrastructureEntityFrameworkAssemblyReference.Assembly;

            var forbiddenDependencies = new[]
            {
                this.infrastructureNamespace,
                this.infrastructureEventStoreNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
            };

            var dependencies = new[]
            {
                this.applicationContractsNamespace,
                this.sharedKernelNamespace,
                this.domainNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(forbiddenDependencies)
                .Or()
                .HaveDependencyOn(this.infrastructureEntityFrameworkNamespace)
                .Or()
                .HaveDependencyOnAny(dependencies)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            // Assert.
            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }

        [Fact]
        public void InfrastructureEventStore_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = InfrastructureEventStoreAssemblyReference.Assembly;

            var forbiddenDependencies = new[]
            {
                this.infrastructureNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
            };

            var dependencies = new[]
            {
                this.applicationContractsNamespace,
                this.sharedKernelNamespace,
                this.domainNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(forbiddenDependencies)
                .Or()
                .HaveDependencyOn(this.infrastructureEventStoreNamespace)
                .Or()
                .HaveDependencyOnAny(dependencies)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            // Assert.
            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }

        [Fact]
        public void HttpRestfulApiContracts_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = HttpRestfulApiContractsAssemblyReference.Assembly;

            var otherProjects = new[]
            {
                this.sharedKernelNamespace,
                this.sharedKernelContractsNamespace,
                this.domainNamespace,
                this.domainContractsNamespace,
                this.applicationNamespace,
                this.applicationContractsNamespace,
                this.applicationContractsIntegrationMessagesNamespace,
                this.infrastructureNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.infrastructureEventStoreNamespace,
                this.httpRestfulApiNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(otherProjects)
                .Or()
                .HaveDependencyOn(this.httpRestfulApiContractsNamespace)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }

        [Fact]
        public void HttpRestfulApi_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = HttpRestfulApiAssemblyReference.Assembly;

            var forbiddenDependencies = new[]
            {
                this.domainNamespace,
                this.domainContractsNamespace,
                this.applicationNamespace,
                this.infrastructureNamespace,
                this.infrastructureEntityFrameworkNamespace,
                this.infrastructureEventStoreNamespace,
            };

            var dependencies = new[]
            {
                this.applicationContractsNamespace,
                this.sharedKernelNamespace,
                this.httpRestfulApiContractsNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(forbiddenDependencies)
                .Or()
                .HaveDependencyOn(this.httpRestfulApiNamespace)
                .Or()
                .HaveDependencyOnAny(dependencies)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }
    }
}