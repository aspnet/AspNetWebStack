// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using System.Web.Http.Validation;
using Newtonsoft.Json;

namespace System.Web.Http
{
    public abstract class ApiController : IHttpController, IDisposable
    {
        private HttpActionContext _actionContext = new HttpActionContext();
        private bool _initialized;

        /// <summary>Gets the configuration.</summary>
        /// <remarks>The setter is intended for unit testing purposes only.</remarks>
        public HttpConfiguration Configuration
        {
            get { return ControllerContext.Configuration; }
            set { ControllerContext.Configuration = value; }
        }

        /// <summary>Gets the controller context.</summary>
        /// <remarks>The setter is intended for unit testing purposes only.</remarks>
        public HttpControllerContext ControllerContext
        {
            get
            {
                // unit test only
                if (ActionContext.ControllerContext == null)
                {
                    ActionContext.ControllerContext = new HttpControllerContext
                    {
                        RequestContext = new RequestBackedHttpRequestContext()
                    };
                }
                return ActionContext.ControllerContext;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                ActionContext.ControllerContext = value;
            }
        }

        /// <summary>Gets the action context.</summary>
        /// <remarks>The setter is intended for unit testing purposes only.</remarks>
        public HttpActionContext ActionContext
        {
            get { return _actionContext; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _actionContext = value;
            }
        }

        /// <summary>
        /// Gets model state after the model binding process. This ModelState will be empty before model binding happens.
        /// </summary>
        /// <remarks>The setter is intended for unit testing purposes only.</remarks>
        public ModelStateDictionary ModelState
        {
            get
            {
                return ActionContext.ModelState;
            }
        }

        /// <summary>Gets or sets the HTTP request message.</summary>
        /// <remarks>The setter is intended for unit testing purposes only.</remarks>
        public HttpRequestMessage Request
        {
            get
            {
                return ControllerContext.Request;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                HttpRequestContext contextOnRequest = value.GetRequestContext();
                HttpRequestContext contextOnController = RequestContext;

                if (contextOnRequest != null && contextOnRequest != contextOnController)
                {
                    // Prevent unit testers from setting conflicting requests contexts.
                    throw new InvalidOperationException(SRResources.RequestContextConflict);
                }

                ControllerContext.Request = value;
                value.SetRequestContext(contextOnController);

                RequestBackedHttpRequestContext requestBackedContext =
                    contextOnController as RequestBackedHttpRequestContext;

                if (requestBackedContext != null)
                {
                    requestBackedContext.Request = value;
                }
            }
        }

        /// <summary>Gets the request context.</summary>
        /// <remarks>The setter is intended for unit testing purposes only.</remarks>
        public HttpRequestContext RequestContext
        {
            get
            {
                return ControllerContext.RequestContext;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                HttpRequestContext oldContext = ControllerContext.RequestContext;
                HttpRequestMessage request = Request;

                if (request != null)
                {
                    HttpRequestContext contextOnRequest = request.GetRequestContext();

                    if (contextOnRequest != null && contextOnRequest != oldContext && contextOnRequest != value)
                    {
                        // Prevent unit testers from setting conflicting requests contexts.
                        throw new InvalidOperationException(SRResources.RequestContextConflict);
                    }

                    request.SetRequestContext(value);
                }

                ControllerContext.RequestContext = value;
            }
        }

        /// <summary>Gets a factory used to generate URLs to other APIs.</summary>
        /// <remarks>The setter is intended for unit testing purposes only.</remarks>
        public UrlHelper Url
        {
            get { return RequestContext.Url; }
            set { RequestContext.Url = value; }
        }

        /// <summary>Gets or sets the current principal associated with this request.</summary>
        /// <remarks>The setter is intended for unit testing purposes only.</remarks>
        public IPrincipal User
        {
            get { return RequestContext.Principal; }
            set { RequestContext.Principal = value; }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This method is a coordinator, so this coupling is expected.")]
        public virtual Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            if (_initialized)
            {
                // if user has registered a controller factory which produces the same controller instance, we should throw here
                throw Error.InvalidOperation(SRResources.CannotSupportSingletonInstance, typeof(ApiController).Name, typeof(IHttpControllerActivator).Name);
            }

            Initialize(controllerContext);

            // We can't be reused, and we know we're disposable, so make sure we go away when
            // the request has been completed.
            if (Request != null)
            {
                Request.RegisterForDispose(this);
            }

            HttpControllerDescriptor controllerDescriptor = controllerContext.ControllerDescriptor;
            ServicesContainer controllerServices = controllerDescriptor.Configuration.Services;

            HttpActionDescriptor actionDescriptor = controllerServices.GetActionSelector().SelectAction(controllerContext);
            ActionContext.ActionDescriptor = actionDescriptor;
            if (Request != null)
            {
                Request.SetActionDescriptor(actionDescriptor);
            }

            FilterGrouping filterGrouping = actionDescriptor.GetFilterGrouping();

            IActionFilter[] actionFilters = filterGrouping.ActionFilters;
            IAuthenticationFilter[] authenticationFilters = filterGrouping.AuthenticationFilters;
            IAuthorizationFilter[] authorizationFilters = filterGrouping.AuthorizationFilters;
            IExceptionFilter[] exceptionFilters = filterGrouping.ExceptionFilters;

            IHttpActionResult result = new ActionFilterResult(actionDescriptor.ActionBinding, ActionContext,
                controllerServices, actionFilters);
            if (authorizationFilters.Length > 0)
            {
                result = new AuthorizationFilterResult(ActionContext, authorizationFilters, result);
            }
            if (authenticationFilters.Length > 0)
            {
                result = new AuthenticationFilterResult(ActionContext, this, authenticationFilters, result);
            }
            if (exceptionFilters.Length > 0)
            {
                IExceptionLogger exceptionLogger = ExceptionServices.GetLogger(controllerServices);
                IExceptionHandler exceptionHandler = ExceptionServices.GetHandler(controllerServices);
                result = new ExceptionFilterResult(ActionContext, exceptionFilters, exceptionLogger, exceptionHandler,
                    result);
            }

            return result.ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Validates the given entity and adds the validation errors to the <see cref="ApiController.ModelState"/>
        /// under the empty prefix, if any.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be validated.</typeparam>
        /// <param name="entity">The entity being validated.</param>
        public void Validate<TEntity>(TEntity entity)
        {
            Validate(entity, keyPrefix: String.Empty);
        }

        /// <summary>
        /// Validates the given entity and adds the validation errors to the <see cref="ApiController.ModelState"/>, if any.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be validated.</typeparam>
        /// <param name="entity">The entity being validated.</param>
        /// <param name="keyPrefix">
        /// The key prefix under which the model state errors would be added in the <see cref="ApiController.ModelState"/>.
        /// </param>
        public void Validate<TEntity>(TEntity entity, string keyPrefix)
        {
            if (Configuration == null)
            {
                throw Error.InvalidOperation(SRResources.TypePropertyMustNotBeNull, typeof(ApiController).Name, "Configuration");
            }

            IBodyModelValidator validator = Configuration.Services.GetBodyModelValidator();
            if (validator != null)
            {
                ModelMetadataProvider metadataProvider = Configuration.Services.GetModelMetadataProvider();
                Contract.Assert(metadataProvider != null, "GetModelMetadataProvider throws on null.");

                validator.Validate(entity, typeof(TEntity), metadataProvider, ActionContext, keyPrefix);
            }
        }

        /// <summary>Creates a <see cref="BadRequestResult"/> (400 Bad Request).</summary>
        /// <returns>A <see cref="BadRequestResult"/>.</returns>
        protected internal virtual BadRequestResult BadRequest()
        {
            return new BadRequestResult(this);
        }

        /// <summary>
        /// Creates a <see cref="BadRequestErrorMessageResult"/> (400 Bad Request) with the specified error message.
        /// </summary>
        /// <param name="message">The user-visible error message.</param>
        /// <returns>A <see cref="BadRequestErrorMessageResult"/> with the specified error message.</returns>
        protected internal virtual BadRequestErrorMessageResult BadRequest(string message)
        {
            return new BadRequestErrorMessageResult(message, this);
        }

        /// <summary>
        /// Creates an <see cref="InvalidModelStateResult"/> (400 Bad Request) with the specified model state.
        /// </summary>
        /// <param name="modelState">The model state to include in the error.</param>
        /// <returns>An <see cref="InvalidModelStateResult"/> with the specified model state.</returns>
        protected internal virtual InvalidModelStateResult BadRequest(ModelStateDictionary modelState)
        {
            return new InvalidModelStateResult(modelState, this);
        }

        /// <summary>Creates a <see cref="ConflictResult"/> (409 Conflict).</summary>
        /// <returns>A <see cref="ConflictResult"/>.</returns>
        protected internal virtual ConflictResult Conflict()
        {
            return new ConflictResult(this);
        }

        /// <summary>Creates a <see cref="NegotiatedContentResult{T}"/> with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="value">The content value to negotiate and format in the entity body.</param>
        /// <returns>A <see cref="NegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal virtual NegotiatedContentResult<T> Content<T>(HttpStatusCode statusCode, T value)
        {
            return new NegotiatedContentResult<T>(statusCode, value, this);
        }

        /// <summary>Creates a <see cref="FormattedContentResult{T}"/> with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <param name="formatter">The formatter to use to format the content.</param>
        /// <returns>A <see cref="FormattedContentResult{T}"/> with the specified values.</returns>
        protected internal FormattedContentResult<T> Content<T>(HttpStatusCode statusCode, T value,
            MediaTypeFormatter formatter)
        {
            return Content(statusCode, value, formatter, (MediaTypeHeaderValue)null);
        }

        /// <summary>Creates a <see cref="FormattedContentResult{T}"/> with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <param name="formatter">The formatter to use to format the content.</param>
        /// <param name="mediaType">The value for the Content-Type header.</param>
        /// <returns>A <see cref="FormattedContentResult{T}"/> with the specified values.</returns>
        protected internal FormattedContentResult<T> Content<T>(HttpStatusCode statusCode, T value,
            MediaTypeFormatter formatter, string mediaType)
        {
            return Content(statusCode, value, formatter, new MediaTypeHeaderValue(mediaType));
        }

        /// <summary>Creates a <see cref="FormattedContentResult{T}"/> with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="statusCode">The HTTP status code for the response message.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <param name="formatter">The formatter to use to format the content.</param>
        /// <param name="mediaType">
        /// The value for the Content-Type header, or <see langword="null"/> to have the formatter pick a default
        /// value.
        /// </param>
        /// <returns>A <see cref="FormattedContentResult{T}"/> with the specified values.</returns>
        protected internal virtual FormattedContentResult<T> Content<T>(HttpStatusCode statusCode, T value,
            MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType)
        {
            return new FormattedContentResult<T>(statusCode, value, formatter, mediaType, this);
        }

        /// <summary>
        /// Creates a <see cref="CreatedNegotiatedContentResult{T}"/> (201 Created) with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="location">
        /// The location at which the content has been created. Must be a relative or absolute URL.
        /// </param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <returns>A <see cref="CreatedNegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal CreatedNegotiatedContentResult<T> Created<T>(string location, T content)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            return Created<T>(new Uri(location, UriKind.RelativeOrAbsolute), content);
        }

        /// <summary>
        /// Creates a <see cref="CreatedNegotiatedContentResult{T}"/> (201 Created) with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="location">The location at which the content has been created.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <returns>A <see cref="CreatedNegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal virtual CreatedNegotiatedContentResult<T> Created<T>(Uri location, T content)
        {
            return new CreatedNegotiatedContentResult<T>(location, content, this);
        }

        /// <summary>
        /// Creates a <see cref="CreatedAtRouteNegotiatedContentResult{T}"/> (201 Created) with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <returns>A <see cref="CreatedAtRouteNegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal CreatedAtRouteNegotiatedContentResult<T> CreatedAtRoute<T>(string routeName,
            object routeValues, T content)
        {
            return CreatedAtRoute<T>(routeName, new HttpRouteValueDictionary(routeValues), content);
        }

        /// <summary>
        /// Creates a <see cref="CreatedAtRouteNegotiatedContentResult{T}"/> (201 Created) with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <returns>A <see cref="CreatedAtRouteNegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal virtual CreatedAtRouteNegotiatedContentResult<T> CreatedAtRoute<T>(string routeName,
            IDictionary<string, object> routeValues, T content)
        {
            return new CreatedAtRouteNegotiatedContentResult<T>(routeName, routeValues, content, this);
        }

        /// <summary>Creates an <see cref="InternalServerErrorResult"/> (500 Internal Server Error).</summary>
        /// <returns>A <see cref="InternalServerErrorResult"/>.</returns>
        protected internal virtual InternalServerErrorResult InternalServerError()
        {
            return new InternalServerErrorResult(this);
        }

        /// <summary>
        /// Creates an <see cref="ExceptionResult"/> (500 Internal Server Error) with the specified exception.
        /// </summary>
        /// <param name="exception">The exception to include in the error.</param>
        /// <returns>An <see cref="ExceptionResult"/> with the specified exception.</returns>
        protected internal virtual ExceptionResult InternalServerError(Exception exception)
        {
            return new ExceptionResult(exception, this);
        }

        /// <summary>Creates a <see cref="JsonResult{T}"/> (200 OK) with the specified value.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="content">The content value to serialize in the entity body.</param>
        /// <returns>A <see cref="JsonResult{T}"/> with the specified value.</returns>
        protected internal JsonResult<T> Json<T>(T content)
        {
            return Json<T>(content, new JsonSerializerSettings());
        }

        /// <summary>Creates a <see cref="JsonResult{T}"/> (200 OK) with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="content">The content value to serialize in the entity body.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <returns>A <see cref="JsonResult{T}"/> with the specified values.</returns>
        protected internal JsonResult<T> Json<T>(T content, JsonSerializerSettings serializerSettings)
        {
            return Json<T>(content, serializerSettings, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true));
        }

        /// <summary>Creates a <see cref="JsonResult{T}"/> (200 OK) with the specified values.</summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="content">The content value to serialize in the entity body.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <param name="encoding">The content encoding.</param>
        /// <returns>A <see cref="JsonResult{T}"/> with the specified values.</returns>
        protected internal virtual JsonResult<T> Json<T>(T content, JsonSerializerSettings serializerSettings,
            Encoding encoding)
        {
            return new JsonResult<T>(content, serializerSettings, encoding, this);
        }

        /// <summary>Creates a <see cref="NotFoundResult"/> (404 Not Found).</summary>
        /// <returns>A <see cref="NotFoundResult"/>.</returns>
        protected internal virtual NotFoundResult NotFound()
        {
            return new NotFoundResult(this);
        }

        /// <summary>Creates an <see cref="OkResult"/> (200 OK).</summary>
        /// <returns>An <see cref="OkResult"/>.</returns>
        protected internal virtual OkResult Ok()
        {
            return new OkResult(this);
        }

        /// <summary>
        /// Creates an <see cref="OkNegotiatedContentResult{T}"/> (200 OK) with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of content in the entity body.</typeparam>
        /// <param name="content">The content value to negotiate and format in the entity body.</param>
        /// <returns>An <see cref="OkNegotiatedContentResult{T}"/> with the specified values.</returns>
        protected internal virtual OkNegotiatedContentResult<T> Ok<T>(T content)
        {
            return new OkNegotiatedContentResult<T>(content, this);
        }

        /// <summary>Creates a <see cref="RedirectResult"/> (302 Found) with the specified value.</summary>
        /// <param name="location">The location to which to redirect.</param>
        /// <returns>A <see cref="RedirectResult"/> with the specified value.</returns>
        protected internal virtual RedirectResult Redirect(string location)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            return Redirect(new Uri(location));
        }

        /// <summary>Creates a <see cref="RedirectResult"/> (302 Found) with the specified value.</summary>
        /// <param name="location">The location to which to redirect.</param>
        /// <returns>A <see cref="RedirectResult"/> with the specified value.</returns>
        protected internal virtual RedirectResult Redirect(Uri location)
        {
            return new RedirectResult(location, this);
        }

        /// <summary>Creates a <see cref="RedirectToRouteResult"/> (302 Found) with the specified values.</summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <returns>A <see cref="RedirectToRouteResult"/> with the specified values.</returns>
        protected internal RedirectToRouteResult RedirectToRoute(string routeName, object routeValues)
        {
            return RedirectToRoute(routeName, new HttpRouteValueDictionary(routeValues));
        }

        /// <summary>Creates a <see cref="RedirectToRouteResult"/> (302 Found) with the specified values.</summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <returns>A <see cref="RedirectToRouteResult"/> with the specified values.</returns>
        protected internal virtual RedirectToRouteResult RedirectToRoute(string routeName,
            IDictionary<string, object> routeValues)
        {
            return new RedirectToRouteResult(routeName, routeValues, this);
        }

        /// <summary>Creates a <see cref="ResponseMessageResult"/> with the specified response.</summary>
        /// <param name="response">The HTTP response message.</param>
        /// <returns>A <see cref="ResponseMessageResult"/> for the specified response.</returns>
        protected internal virtual ResponseMessageResult ResponseMessage(HttpResponseMessage response)
        {
            return new ResponseMessageResult(response);
        }

        /// <summary>Creates a <see cref="StatusCodeResult"/> with the specified status code.</summary>
        /// <param name="status">The HTTP status code for the response message</param>
        /// <returns>A <see cref="StatusCodeResult"/> with the specified status code.</returns>
        protected internal virtual StatusCodeResult StatusCode(HttpStatusCode status)
        {
            return new StatusCodeResult(status, this);
        }

        /// <summary>
        /// Creates an <see cref="UnauthorizedResult"/> (401 Unauthorized) with the specified values.
        /// </summary>
        /// <param name="challenges">The WWW-Authenticate challenges.</param>
        /// <returns>An <see cref="UnauthorizedResult"/> with the specified values.</returns>
        protected internal UnauthorizedResult Unauthorized(params AuthenticationHeaderValue[] challenges)
        {
            return Unauthorized((IEnumerable<AuthenticationHeaderValue>)challenges);
        }

        /// <summary>
        /// Creates an <see cref="UnauthorizedResult"/> (401 Unauthorized) with the specified values.
        /// </summary>
        /// <param name="challenges">The WWW-Authenticate challenges.</param>
        /// <returns>An <see cref="UnauthorizedResult"/> with the specified values.</returns>
        protected internal virtual UnauthorizedResult Unauthorized(IEnumerable<AuthenticationHeaderValue> challenges)
        {
            return new UnauthorizedResult(challenges, this);
        }

        protected virtual void Initialize(HttpControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            _initialized = true;
            ControllerContext = controllerContext;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion IDisposable
    }
}