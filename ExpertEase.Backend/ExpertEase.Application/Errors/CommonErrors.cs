﻿using System.Net;

namespace ExpertEase.Application.Errors;

/// <summary>
/// Common error messages that may be reused in various places in the code.
/// </summary>
public static class CommonErrors
{
    public static ErrorMessage NotAllowed => new(HttpStatusCode.Forbidden, "You are not allowed to do this action!");
    public static ErrorMessage UserNotFound => new(HttpStatusCode.NotFound, "User doesn't exist!", ErrorCodes.EntityNotFound);
    public static ErrorMessage EntityNotFound => new(HttpStatusCode.NotFound, "Entity doesn't exist!", ErrorCodes.EntityNotFound);
    public static ErrorMessage FileNotFound => new(HttpStatusCode.NotFound, "File not found on disk!", ErrorCodes.PhysicalFileNotFound);
    public static ErrorMessage TechnicalSupport => new(HttpStatusCode.InternalServerError, "An unknown error occurred, contact the technical support!", ErrorCodes.TechnicalError);
}
