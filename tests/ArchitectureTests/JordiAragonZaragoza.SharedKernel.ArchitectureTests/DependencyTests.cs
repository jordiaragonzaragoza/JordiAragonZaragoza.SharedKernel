namespace JordiAragonZaragoza.SharedKernel.ArchitectureTests
{
    using JordiAragonZaragoza.SharedKernel;
    using JordiAragonZaragoza.SharedKernel.Application;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration;
    using JordiAragonZaragoza.SharedKernel.Contracts;
    using JordiAragonZaragoza.SharedKernel.Domain;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts;
    using JordiAragonZaragoza.SharedKernel.Infrastructure;
    ////using JordiAragonZaragoza.SharedKernel.Infrastructure.MongoDb;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts;
    using JordiAragonZaragoza.SharedKernel.Presentation.Integration;
    using JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts;
    using AwesomeAssertions;
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
        private readonly string applicationContractsIntegrationNamespace = ApplicationContractsIntegrationAssemblyReference.Assembly.GetName().Name!;
        private readonly string infrastructureNamespace = InfrastructureAssemblyReference.Assembly.GetName().Name!;
        ////private readonly string infrastructureMongoDbNamespace = InfrastructureMongoDbAssemblyReference.Assembly.GetName().Name!;
        private readonly string infrastructureMongoDbNamespace = string.Empty;
        private readonly string httpRestfulApiNamespace = HttpRestfulApiAssemblyReference.Assembly.GetName().Name!;
        private readonly string httpRestfulApiContractsNamespace = HttpRestfulApiContractsAssemblyReference.Assembly.GetName().Name!;
        private readonly string integrationNamespace = IntegrationAssemblyReference.Assembly.GetName().Name!;
        private readonly string integrationContractsNamespace = IntegrationContractsAssemblyReference.Assembly.GetName().Name!;
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
                this.applicationContractsIntegrationNamespace,
                this.infrastructureNamespace,
                this.infrastructureMongoDbNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
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
                this.applicationContractsIntegrationNamespace,
                this.infrastructureNamespace,
                this.infrastructureMongoDbNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
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
                this.applicationContractsIntegrationNamespace,
                this.infrastructureNamespace,
                this.infrastructureMongoDbNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
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
                this.applicationContractsIntegrationNamespace,
                this.infrastructureNamespace,
                this.infrastructureMongoDbNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
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
                this.applicationContractsIntegrationNamespace,
                this.infrastructureNamespace,
                this.infrastructureMongoDbNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
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
                this.infrastructureMongoDbNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
            };

            var dependencies = new[]
            {
                this.applicationContractsIntegrationNamespace,
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
            var assembly = ApplicationContractsIntegrationAssemblyReference.Assembly;

            var otherProjects = new[]
            {
                this.sharedKernelNamespace,
                this.sharedKernelContractsNamespace,
                this.domainNamespace,
                this.domainContractsNamespace,
                this.applicationNamespace,
                this.applicationContractsNamespace,
                this.infrastructureNamespace,
                this.infrastructureMongoDbNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(otherProjects)
                .Or()
                .HaveDependencyOn(this.applicationContractsIntegrationNamespace)
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
                this.infrastructureMongoDbNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
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
                this.infrastructureMongoDbNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
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

        /*[Fact]
        public void InfrastructureMongoDb_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = InfrastructureMongoDbAssemblyReference.Assembly;

            var forbiddenDependencies = new[]
            {
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
            };

            var dependencies = new[]
            {
                this.infrastructureNamespace,
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
                .HaveDependencyOn(this.infrastructureMongoDbNamespace)
                .Or()
                .HaveDependencyOnAny(dependencies)
                .Or()
                .NotHaveDependencyOnAny(this.allProjects)
                .GetResult();

            // Assert.
            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }*/

        [Fact]
        public void Integration_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange.
            var assembly = IntegrationAssemblyReference.Assembly;

            var forbiddenDependencies = new[]
            {
                this.domainNamespace,
                this.applicationNamespace,
                this.infrastructureNamespace,
                this.infrastructureMongoDbNamespace,
                this.httpRestfulApiNamespace,
                this.httpRestfulApiContractsNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
            };

            var dependencies = new[]
            {
                this.applicationContractsIntegrationNamespace,
                this.applicationContractsNamespace,
                this.domainContractsNamespace,
            };

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(forbiddenDependencies)
                .Or()
                .HaveDependencyOn(this.integrationNamespace)
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
                this.applicationContractsIntegrationNamespace,
                this.infrastructureNamespace,
                this.infrastructureMongoDbNamespace,
                this.httpRestfulApiNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
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
                this.infrastructureMongoDbNamespace,
                this.integrationNamespace,
                this.integrationContractsNamespace,
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