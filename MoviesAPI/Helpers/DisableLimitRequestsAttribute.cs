using System;

namespace MoviesAPI.Helpers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class DisableLimitRequestsAttribute : Attribute
    {
    }
}
