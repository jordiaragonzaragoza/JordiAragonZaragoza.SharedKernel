namespace JordiAragonZaragoza.SharedKernel.UnitTests.Application.Contracts
{
    using System.Collections.Generic;
    using AwesomeAssertions;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using Xunit;

    public sealed class AuthorizationRequirementTests
    {
        [Fact]
        public void IsRoleRequirementSatisfied_WhenNoRoleIsRequired_ShouldBeSatisfied()
        {
            var requirement = new AuthorizationRequirement([], [], []);

            requirement.IsRoleRequirementSatisfied(new HashSet<string>()).Should().BeTrue();
        }

        [Fact]
        public void IsRoleRequirementSatisfied_WhenHoldsAnyOfTheRoles_ShouldBeSatisfied()
        {
            var requirement = new AuthorizationRequirement(["Admin", "Manager"], [], []);

            requirement.IsRoleRequirementSatisfied(new HashSet<string> { "Manager" }).Should().BeTrue();
        }

        [Fact]
        public void IsRoleRequirementSatisfied_WhenHoldsNoneOfTheRoles_ShouldNotBeSatisfied()
        {
            var requirement = new AuthorizationRequirement(["Admin", "Manager"], [], []);

            requirement.IsRoleRequirementSatisfied(new HashSet<string> { "Viewer" }).Should().BeFalse();
        }

        [Fact]
        public void GetMissingPermissions_WhenHoldsOnlySome_ShouldReturnTheRest()
        {
            var requirement = new AuthorizationRequirement([], ["a:x", "b:x", "c:x"], []);

            var missing = requirement.GetMissingPermissions(new HashSet<string> { "a:x" });

            missing.Should().BeEquivalentTo("b:x", "c:x");
        }

        [Fact]
        public void GetMissingPermissions_WhenHoldsAll_ShouldBeEmpty()
        {
            var requirement = new AuthorizationRequirement([], ["a:x", "b:x"], []);

            requirement.GetMissingPermissions(new HashSet<string> { "a:x", "b:x", "z:x" }).Should().BeEmpty();
        }
    }
}
