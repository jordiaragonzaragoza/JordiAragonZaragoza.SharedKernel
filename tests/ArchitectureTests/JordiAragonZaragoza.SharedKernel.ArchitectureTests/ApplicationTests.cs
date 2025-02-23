namespace JordiAragonZaragoza.SharedKernel.ArchitectureTests
{
    using System;
    using FluentAssertions;
    using JordiAragonZaragoza.SharedKernel.Application;
    using NetArchTest.Rules;
    using Xunit;

    public class ApplicationTests
    {
        [Fact]
        public void CommandHandlers_Should_Have_DependencyOnArdalisResult()
        {
            // Arrange.
            var assembly = ApplicationAssemblyReference.Assembly;

            // Act.
            var testResult = Types
                .InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Handler", StringComparison.InvariantCulture)
                .And().DoNotHaveNameEndingWith("EventHandler", StringComparison.InvariantCulture)
                .And().DoNotHaveNameEndingWith("NotificationHandler", StringComparison.InvariantCulture)
                .Should()
                .HaveDependencyOn("Ardalis.Result")
                .GetResult();

            // Assert.
            _ = testResult.IsSuccessful.Should().BeTrue(Utils.GetFailingTypes(testResult));
        }
    }
}