using System.Net;
using ExpertEase.Application.Errors;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.Application.Responses;

/// <summary>
/// This class contains methods for controllers to set the response objects and status codes in a easier way, inherit it in the controller instead of the plain ControllerBase.
/// </summary>
public abstract class ResponseController : ControllerBase
{
    /// <summary>
    /// Notice that the following methods adapt the responses or errors to a ActionResult with a status code that will be serialized into the HTTP response body.
    /// </summary>
    protected ActionResult<RequestResponse> CreateErrorMessageResult(ServerException serverException) =>
        StatusCode((int)serverException.Status, RequestResponse.CreateErrorResponse(ErrorMessage.FromException(serverException))); // The StatusCode method of the controller base will
                                                                                                                                // set the given HTTP status code in the response and will serialize
                                                                                                                                // the response object.

    protected ActionResult<RequestResponse> CreateErrorMessageResult(ErrorMessage? errorMessage = null) =>
        StatusCode((int)(errorMessage?.Status ?? HttpStatusCode.InternalServerError), RequestResponse.CreateErrorResponse(errorMessage));

    protected ActionResult<RequestResponse<T>> CreateErrorMessageResult<T>(ErrorMessage? errorMessage = null) =>
        StatusCode((int)(errorMessage?.Status ?? HttpStatusCode.InternalServerError), RequestResponse<T>.CreateErrorResponse(errorMessage));

    protected ActionResult<RequestResponse> CreateRequestResponseFromServiceResponse(ServiceResponse response) =>
        response.Error == null ? Ok(RequestResponse.OkRequestResponse) : CreateErrorMessageResult(response.Error); // The Ok method of the controller base will set the
                                                                                                               // HTTP status code in the response to 200 Ok and will
                                                                                                               // serialize the response object.

    protected ActionResult<RequestResponse<T>> CreateRequestResponseFromServiceResponse<T>(ServiceResponse<T> response) =>
        response.Error == null ? Ok(RequestResponse<T>.CreateRequestResponseFromServiceResponse(response)) : CreateErrorMessageResult<T>(response.Error);

    protected ActionResult<RequestResponse> OkRequestResponse() => Ok(RequestResponse.OkRequestResponse);
}