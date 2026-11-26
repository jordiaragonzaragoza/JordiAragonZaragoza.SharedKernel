namespace JordiAragonZaragoza.SharedKernel.UnitTests.Application.Pipelines
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using AwesomeAssertions;
    using JordiAragonZaragoza.SharedKernel.Application.Attributes;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Application.Pipelines;
    using Xunit;

    public sealed class RequestAuthorizationServiceTests
    {
        [Fact]
        public async Task TryAuthorizeAsync_WhenRolesInOneAttribute_ShouldBeAlternativesOfOneRequirement()
        {
            var authorization = new CapturingAuthorizationService();
            var sut = new RequestAuthorizationService<SingleAttributeRequest>(authorization);

            await sut.TryAuthorizeAsync(new SingleAttributeRequest());

            var requirement = authorization.Captured.Should().ContainSingle().Subject;
            requirement.AnyOfRoles.Should().BeEquivalentTo("Admin", "Manager");
            requirement.AllOfPermissions.Should().BeEquivalentTo("a:x", "b:x");
            requirement.AllOfPolicies.Should().BeEquivalentTo("P1");
        }

        [Fact]
        public async Task TryAuthorizeAsync_WhenSeveralAttributes_ShouldProduceOneRequirementEach()
        {
            var authorization = new CapturingAuthorizationService();
            var sut = new RequestAuthorizationService<TwoAttributesRequest>(authorization);

            await sut.TryAuthorizeAsync(new TwoAttributesRequest());

            authorization.Captured.Should().HaveCount(2);
            authorization.Captured.Select(requirement => requirement.AnyOfRoles.Single())
                .Should().BeEquivalentTo("Admin", "Auditor");
        }

        [Fact]
        public async Task TryAuthorizeAsync_WhenNoAttribute_ShouldSucceedWithoutCallingTheService()
        {
            var authorization = new CapturingAuthorizationService();
            var sut = new RequestAuthorizationService<NoAttributeRequest>(authorization);

            var result = await sut.TryAuthorizeAsync(new NoAttributeRequest());

            result.IsSuccess.Should().BeTrue();
            authorization.Captured.Should().BeEmpty();
        }

        [Fact]
        public async Task TryAuthorizeAsync_WhenValuesHaveSpacesOrEmptyEntries_ShouldTrimAndDiscardThem()
        {
            var authorization = new CapturingAuthorizationService();
            var sut = new RequestAuthorizationService<MessyAttributeRequest>(authorization);

            await sut.TryAuthorizeAsync(new MessyAttributeRequest());

            var requirement = authorization.Captured.Should().ContainSingle().Subject;
            requirement.AllOfPermissions.Should().BeEquivalentTo("a:x", "b:x");
        }

        [Authorize(Roles = "Admin, Manager", Permissions = "a:x,b:x", Policies = "P1")]
        private sealed record SingleAttributeRequest;

        [Authorize(Roles = "Admin")]
        [Authorize(Roles = "Auditor")]
        private sealed record TwoAttributesRequest;

        [Authorize(Permissions = " a:x , ,b:x ,")]
        private sealed record MessyAttributeRequest;

        private sealed record NoAttributeRequest;

        private sealed class CapturingAuthorizationService : IAuthorizationService
        {
            public IReadOnlyCollection<AuthorizationRequirement> Captured { get; private set; } = [];

            public Task<Result> ValidateScopeAsync(Guid userId, ScopeContext scope, CancellationToken cancellationToken = default)
                => Task.FromResult(Result.Success());

            public Task<Result> AuthorizeAsync(
                IReadOnlyCollection<AuthorizationRequirement> requirements,
                Guid? resourceId,
                CancellationToken cancellationToken = default)
            {
                this.Captured = requirements;
                return Task.FromResult(Result.Success());
            }
        }
    }
}
