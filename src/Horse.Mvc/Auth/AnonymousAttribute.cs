using System;

namespace Horse.Mvc.Auth
{
    /// <summary>
    /// Authorization attribute for Horse MVC.
    /// Can be used for controllers and actions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowAnonymousAttribute : Attribute
    { }
}
