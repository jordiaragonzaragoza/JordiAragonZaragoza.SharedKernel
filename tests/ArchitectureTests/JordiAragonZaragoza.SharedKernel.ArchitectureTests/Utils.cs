namespace JordiAragonZaragoza.SharedKernel.ArchitectureTests
{
    using System;
    using NetArchTest.Rules;

    public static class Utils
    {
        public static string GetFailingTypes(TestResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            return result.FailingTypeNames != null ?
                string.Join(", ", result.FailingTypeNames) :
                string.Empty;
        }
    }
}