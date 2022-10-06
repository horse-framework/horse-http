using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Horse.Core;
using Horse.Core.Protocols;
using Horse.Mvc.Auth;
using Horse.Mvc.Controllers;
using Horse.Mvc.Errors;
using Horse.Mvc.Filters;
using Horse.Mvc.Middlewares;
using Horse.Mvc.Results;
using Horse.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Horse.Protocols.Http;

namespace Horse.Mvc
{
    /// <summary>
    /// HTTP Request Handler implementation of Horse.Server for Horse.Mvc project.
    /// All HTTP Requests starts in here in Request method.
    /// </summary>
    internal class MvcConnectionHandler : IProtocolConnectionHandler<SocketBase, HttpMessage>
    {
        #region Properties

        /// <summary>
        /// Horse.Mvc Facade object
        /// </summary>
        public HorseMvc Mvc { get; }

        internal MvcAppBuilder App { get; }

        public MvcConnectionHandler(HorseMvc mvc, MvcAppBuilder app)
        {
            Mvc = mvc;
            App = app;
        }

        #endregion

        #region Implementations

        /// <summary>
        /// HTTP Protocol does not support piped connections
        /// </summary>
        public Task<SocketBase> Connected(IHorseServer server, IConnectionInfo connection, ConnectionData data)
        {
            return Task.FromResult((SocketBase) null);
        }

        /// <summary>
        /// Triggered when handshake is completed and the connection is ready to communicate 
        /// </summary>
        public Task Ready(IHorseServer server, SocketBase client)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// HTTP Protocol does not support piped connections
        /// </summary>
        public Task Disconnected(IHorseServer server, SocketBase client)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Triggered when an HTTP request is received
        /// </summary>
        public Task Received(IHorseServer server, IConnectionInfo info, SocketBase client, HttpMessage message)
        {
            return RequestAsync(server, message.Request, message.Response);
        }

        #endregion

        #region Execution

        /// <summary>
        /// Triggered when a non-websocket request available.
        /// </summary>
        private async Task RequestAsync(IHorseServer server, HttpRequest request, HttpResponse response)
        {
            IServiceScope scope = Mvc.ServiceProvider.CreateScope();

            try
            {
                if (App.Descriptors.Count > 0)
                {
                    MiddlewareRunner runner = new MiddlewareRunner(scope);
                    await runner.RunSequence(App, request, response);
                    if (runner.LastResult != null)
                    {
                        WriteResponse(response, runner.LastResult);
                        return;
                    }
                }

                await RequestMvc(server, request, response, scope);
            }
            catch (Exception ex)
            {
                if (Mvc.IsDevelopment)
                {
                    IErrorHandler handler = new DevelopmentErrorHandler();
                    IActionResult result = await handler.Error(request, ex);
                    WriteResponse(request.Response, result);
                }
                else if (Mvc.ErrorHandler != null)
                {
                    IActionResult result = await Mvc.ErrorHandler.Error(request, ex);
                    WriteResponse(request.Response, result);
                }
                else
                    WriteResponse(request.Response, StatusCodeResult.InternalServerError());

                if (request.Response.StreamSuppressed && request.Response.ResponseStream != null)
                    GC.ReRegisterForFinalize(request.Response.ResponseStream);
            }
            finally
            {
                scope.Dispose();
            }
        }

        /// <summary>
        /// Handles the request in MVC pattern
        /// </summary>
        private async Task RequestMvc(IHorseServer server, HttpRequest request, HttpResponse response, IServiceScope scope)
        {
            //find file route
            if (Mvc.FileRoutes.Count > 0)
            {
                IActionResult fileResult = Mvc.RouteFinder.FindFile(Mvc.FileRoutes, request);
                if (fileResult != null)
                {
                    response.SuppressContentEncoding = true;
                    WriteResponse(response, fileResult);
                    return;
                }
            }

            if (Mvc.ActionRoutes.Count > 0)
            {
                ActionRoute actionRoute = Mvc.RouteFinder.FindAction(Mvc.ActionRoutes, request);
                if (actionRoute != null)
                {
                    IActionResult actionResult = null;
                    if (actionRoute.Action != null)
                        actionResult = actionRoute.Action(request);
                    else if (actionRoute.AsyncAction != null)
                        actionResult = await actionRoute.AsyncAction(request);

                    if (actionResult != null)
                    {
                        WriteResponse(response, actionResult);
                        return;
                    }
                }
            }

            //find controller route
            RouteMatch match = Mvc.RouteFinder.Find(Mvc.Routes, request);
            if (match?.Route == null)
            {
                WriteResponse(response, Mvc.NotFoundResult);
                return;
            }

            //read user token
            ClaimsPrincipal user = null;
            if (Mvc.ClaimsPrincipalValidator != null)
            {
                try
                {
                    user = Mvc.ClaimsPrincipalValidator.Get(request);
                }
                catch (Exception e)
                {
                    Mvc.ErrorHandler?.Error(request, e);
                }
            }

            FilterContext context = new FilterContext
                                    {
                                        Server = server,
                                        Request = request,
                                        Response = response,
                                        Result = null,
                                        User = user
                                    };

            if (!CheckControllerAuthority(match, context, response))
                return;

            if (!await ExecuteBeforeControllerFilters(match, context, response))
                return;

            HorseController controller = Mvc.ControllerFactory.CreateInstance(Mvc, match.Route.ControllerType, request, response, scope);
            if (controller == null)
            {
                WriteResponse(response, Mvc.NotFoundResult);
                return;
            }

            controller.User = user;

            if (!await ExecuteAfterControllerFilters(match, context, response, controller))
                return;

            //fill action descriptor
            ActionDescriptor descriptor = new ActionDescriptor
                                          {
                                              Controller = controller,
                                              Action = match.Route.ActionType,
                                              Parameters = await FillParameters(request, match)
                                          };

            if (!CheckActionAuthority(match, context, response, descriptor))
                return;

            if (!await CheckActionExecutingFilters(match, context, response, controller, descriptor))
                return;

            await controller.CallActionExecuting(descriptor, context);
            if (context.Result != null)
            {
                WriteResponse(response, context.Result);
                return;
            }

            await ExecuteAction(match, context, controller, descriptor, response);
        }

        private async Task ExecuteAction(RouteMatch match, FilterContext context, HorseController controller, ActionDescriptor descriptor, HttpResponse response)
        {
            if (match.Route.IsAsyncMethod)
            {
                Task<IActionResult> task = (Task<IActionResult>) match.Route.ActionType.Invoke(controller, descriptor.Parameters.Select(x => x.Value).ToArray());
                // ReSharper disable once PossibleNullReferenceException
                await task;
                await CompleteActionExecution(match, context, response, controller, descriptor, task.Result);
            }
            else
            {
                TaskCompletionSource<bool> source = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                ThreadPool.QueueUserWorkItem(async t =>
                {
                    try
                    {
                        IActionResult ar = (IActionResult) match.Route.ActionType.Invoke(controller, descriptor.Parameters.Select(x => x.Value).ToArray());
                        await CompleteActionExecution(match, context, response, controller, descriptor, ar);
                        source.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        source.SetException(e.InnerException ?? e);
                    }
                });

                await source.Task;
            }
        }

        /// <summary>
        /// Writes the action result to the response
        /// </summary>
        public void WriteResponse(HttpResponse response, IActionResult result)
        {
            //disable content encoding for file download responses
            if (!response.SuppressContentEncoding && result is FileResult)
                response.SuppressContentEncoding = true;

            //if there is no body content for the result
            //check status code results to find a body
            if (result.Stream == null)
            {
                IActionResult statusAction;
                bool found = Mvc.StatusCodeResults.TryGetValue(result.Code, out statusAction);
                if (found && statusAction != null)
                    result = statusAction;
            }

            response.StatusCode = result.Code;
            response.ContentType = result.ContentType;

            //if result has headers, add these headers to response
            if (result.Headers != null)
                if (response.AdditionalHeaders == null)
                    response.AdditionalHeaders = result.Headers;
                else
                {
                    foreach (var header in result.Headers)
                        if (response.AdditionalHeaders.ContainsKey(header.Key))
                            response.AdditionalHeaders[header.Key] = header.Value;
                        else
                            response.AdditionalHeaders.Add(header.Key, header.Value);
                }

            //set stream if result has stream
            if (result.Stream != null && result.Stream.Length > 0)
                response.SetStream(result.Stream, true, true);
        }

        /// <summary>
        /// Creates parameter list and sets values for the specified request to the specified route.
        /// </summary>
        internal async Task<List<ParameterValue>> FillParameters(HttpRequest request, RouteMatch route)
        {
            List<ParameterValue> values = new List<ParameterValue>();
            foreach (ActionParameter ap in route.Route.Parameters)
            {
                ParameterValue paramValue = new ParameterValue
                                            {
                                                Name = ap.ParameterName,
                                                Type = ap.ParameterType,
                                                Source = ap.Source
                                            };

                //by source find the value of the parameter and set it to "value" local variable
                switch (ap.Source)
                {
                    case ParameterSource.None:
                    case ParameterSource.Route:
                        paramValue.Value = ChangeType(route.Values[ap.FromName], ap.ParameterType, ap.Nullable);
                        break;

                    case ParameterSource.Body:
                    {
                        string content = Encoding.UTF8.GetString(request.ContentStream.ToArray());
                        if (ap.FromName == "json")
                        {
                            if (Mvc.JsonInputOptions.UseNewtonsoft)
                            {
                                paramValue.Value = Mvc.JsonInputOptions.NewtonsoftOptions != null
                                                       ? Newtonsoft.Json.JsonConvert.DeserializeObject(content, ap.ParameterType, Mvc.JsonInputOptions.NewtonsoftOptions)
                                                       : Newtonsoft.Json.JsonConvert.DeserializeObject(content, ap.ParameterType);
                            }
                            else
                                paramValue.Value = System.Text.Json.JsonSerializer.Deserialize(content, ap.ParameterType, Mvc.JsonInputOptions.SystemTextOptions);
                        }
                        else if (ap.FromName == "xml")
                        {
                            await using MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
                            XmlSerializer serializer = new XmlSerializer(ap.ParameterType);
                            paramValue.Value = serializer.Deserialize(ms);
                        }

                        break;
                    }

                    case ParameterSource.Form:
                        if (ap.IsClass)
                        {
                            object obj = Activator.CreateInstance(ap.ParameterType);
                            string start = $"{ap.FromName}.";
                            var props = request.Form.Where(x => x.Key.StartsWith(start, StringComparison.InvariantCultureIgnoreCase));
                            foreach (var kv in props)
                            {
                                string propName = kv.Key.Substring(start.Length);
                                PropertyInfo propInfo;
                                ap.ClassProperties.TryGetValue(propName, out propInfo);
                                if (propInfo != null)
                                    propInfo.SetValue(obj, ChangeType(kv.Value, propInfo.PropertyType, Nullable.GetUnderlyingType(propInfo.PropertyType) != null));
                            }

                            paramValue.Value = obj;
                        }
                        else if (request.Form.ContainsKey(ap.FromName))
                            paramValue.Value = ChangeType(request.Form[ap.FromName], ap.ParameterType, ap.Nullable);

                        break;

                    case ParameterSource.QueryString:
                        if (request.QueryString.ContainsKey(ap.FromName))
                            paramValue.Value = ChangeType(request.QueryString[ap.FromName], ap.ParameterType, ap.Nullable);
                        break;

                    case ParameterSource.Header:
                        if (request.Headers.ContainsKey(ap.FromName))
                            paramValue.Value = ChangeType(request.Headers[ap.FromName], ap.ParameterType, ap.Nullable);
                        break;
                }

                //return value
                values.Add(paramValue);
            }

            return values;
        }

        /// <summary>
        /// Converts string value to action parameter type
        /// </summary>
        private static object ChangeType(object value, Type type, bool nullable)
        {
            //if the value is null, parameter may be missing
            if (value == null)
                return null;

            //if the value and parameter types same, do nothing
            if (value.GetType() == type)
                return value;

            //need casting. parameter and value types are different

            //for nullable types, we need extra check (because we need to cast underlying type)
            if (nullable)
            {
                Type utype = Nullable.GetUnderlyingType(type);
                return Convert.ChangeType(value, utype);
            }

            if (type.IsEnum)
                return Enum.Parse(type, value.ToString());

            //directly cast
            return Convert.ChangeType(value, type);
        }

        #endregion

        #region Filters

        /// <summary>
        /// Checks controller authority, returns false if unauthorized
        /// </summary>
        private bool CheckControllerAuthority(RouteMatch match, FilterContext context, HttpResponse response)
        {
            if (!match.Route.HasControllerAuthorizeFilter)
                return true;

            AuthorizeAttribute filter = (AuthorizeAttribute) match.Route.ControllerType.GetCustomAttribute(typeof(AuthorizeAttribute));
            if (filter != null)
            {
                filter.VerifyAuthority(Mvc, null, context);
                if (context.Result != null)
                {
                    WriteResponse(response, context.Result);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Executes before controller filters. Returns true, if execution can resume.
        /// </summary>
        private Task<bool> ExecuteBeforeControllerFilters(RouteMatch match, FilterContext context, HttpResponse response)
        {
            if (!match.Route.HasControllerBeforeFilter)
                return Task.FromResult(true);

            //find controller filters
            IBeforeControllerFilter[] filters = (IBeforeControllerFilter[]) match.Route.ControllerType.GetCustomAttributes(typeof(IBeforeControllerFilter), true);

            //call BeforeCreated methods of controller attributes
            return CallFilters(response, context, filters, async filter => await filter.OnBefore(context));
        }

        /// <summary>
        /// Executes after controller filters. Returns true, if execution can resume.
        /// </summary>
        private Task<bool> ExecuteAfterControllerFilters(RouteMatch match, FilterContext context, HttpResponse response, IController controller)
        {
            if (!match.Route.HasControllerAfterFilter)
                return Task.FromResult(true);

            //find controller filters
            IAfterControllerFilter[] filters = (IAfterControllerFilter[]) match.Route.ControllerType.GetCustomAttributes(typeof(IAfterControllerFilter), true);

            //call AfterCreated methods of controller attributes
            return CallFilters(response, context, filters, async filter => await filter.OnAfter(controller, context));
        }

        /// <summary>
        /// Checks action authority, returns false if unauthorized
        /// </summary>
        private bool CheckActionAuthority(RouteMatch match, FilterContext context, HttpResponse response, ActionDescriptor descriptor)
        {
            if (!match.Route.HasActionAuthorizeFilter)
                return true;

            //check action authorize attribute
            AuthorizeAttribute filter = (AuthorizeAttribute) match.Route.ActionType.GetCustomAttribute(typeof(AuthorizeAttribute));
            if (filter != null)
            {
                filter.VerifyAuthority(Mvc, descriptor, context);
                if (context.Result != null)
                {
                    WriteResponse(response, context.Result);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Calls controller and action executing filters
        /// </summary>
        private async Task<bool> CheckActionExecutingFilters(RouteMatch match, FilterContext context, HttpResponse response, IController controller, ActionDescriptor descriptor)
        {
            if (match.Route.HasControllerExecutingFilter)
            {
                //find controller filters
                IActionExecutingFilter[] filters = (IActionExecutingFilter[]) match.Route.ControllerType.GetCustomAttributes(typeof(IActionExecutingFilter), true);

                //call BeforeCreated methods of controller attributes
                bool resume = await CallFilters(response, context, filters, async filter => await filter.OnExecuting(controller, descriptor, context));
                if (!resume)
                    return false;
            }

            if (match.Route.HasActionExecutingFilter)
            {
                //find controller filters
                IActionExecutingFilter[] filters = (IActionExecutingFilter[]) match.Route.ActionType.GetCustomAttributes(typeof(IActionExecutingFilter), true);

                //call BeforeCreated methods of controller attributes
                bool resume = await CallFilters(response, context, filters, async filter => await filter.OnExecuting(controller, descriptor, context));
                if (!resume)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Completes action execution, calls after actions and writes response
        /// </summary>
        private async Task CompleteActionExecution(RouteMatch match,
                                                   FilterContext context,
                                                   HttpResponse response,
                                                   HorseController controller,
                                                   ActionDescriptor descriptor,
                                                   IActionResult actionResult)
        {
            if (actionResult == null) return;

            //IActionResult actionResult = match.Route.ActionType.Invoke(controller, descriptor.Parameters.Select(x => x.Value).ToArray()) as IActionResult;
            context.Result = actionResult;

            if (match.Route.HasActionExecutedFilter)
            {
                //find controller filters
                IActionExecutedFilter[] filters = (IActionExecutedFilter[]) match.Route.ActionType.GetCustomAttributes(typeof(IActionExecutedFilter), true);

                //call AfterCreated methods of controller attributes
                foreach (IActionExecutedFilter filter in filters)
                    await filter.OnExecuted(controller, descriptor, actionResult, context);
            }

            if (match.Route.HasControllerExecutedFilter)
            {
                //find controller filters
                IActionExecutedFilter[] filters = (IActionExecutedFilter[]) match.Route.ControllerType.GetCustomAttributes(typeof(IActionExecutedFilter), true);

                //call AfterCreated methods of controller attributes
                foreach (IActionExecutedFilter filter in filters)
                    await filter.OnExecuted(controller, descriptor, actionResult, context);
            }

            await controller.CallActionExecuted(descriptor, context, actionResult);

            WriteResponse(response, actionResult);
        }


        /// <summary>
        /// Calls the action method (from parameter) for the specified response, context for each filter items (from parameter).
        /// Filter parameter action calling is used many times in Request method.
        /// CallFilters method is created to avoid to type this code many times
        /// </summary>
        private async Task<bool> CallFilters<TFilter>(HttpResponse response,
                                                      FilterContext context,
                                                      IEnumerable<TFilter> items,
                                                      Func<TFilter, Task> action,
                                                      bool skipResultChanges = false)
        {
            foreach (TFilter item in items)
            {
                await action(item);

                if (skipResultChanges)
                    continue;

                if (context.Result != null)
                {
                    WriteResponse(response, context.Result);
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}