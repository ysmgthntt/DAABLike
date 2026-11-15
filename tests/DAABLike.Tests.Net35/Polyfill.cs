#pragma warning disable IDE0130
namespace DAABLike.Tests
{
    internal static class AssertExtensions
    {
        extension(Assert)
        {
            public static T Throws<T>(string? paramName, Assert.ThrowsDelegate testCode)
                where T : ArgumentException
            {
                var ex = Assert.Throws<T>(testCode);
                Assert.Equal(paramName, ex.ParamName);
                return ex;
            }

            public static T ThrowsAny<T>(Assert.ThrowsDelegate testCode)
                where T : Exception
            {
                try
                {
                    testCode();
                }
                catch (Exception ex)
                {
                    if (ex is T exception)
                        return exception;
                }
                Assert.Fail();
                return null!;
            }

            public static void Fail() => throw new Xunit.Sdk.AssertException("Assert.Fail() Failure");
        }
    }
    internal static class TaskExtensions
    {
        extension(Task)
        {
            public static Task Run(Action action, CancellationToken _) => TaskEx.Run(action);

            public static Task WhenAll(Task[] tasks) => TaskEx.WhenAll(tasks);
        }
    }
    internal sealed class TestContext
    {
        public static TestContext Current = new();

        public CancellationToken CancellationToken => CancellationToken.None;
    }
}
