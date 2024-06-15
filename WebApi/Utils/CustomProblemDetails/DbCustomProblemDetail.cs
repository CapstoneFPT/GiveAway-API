using Microsoft.AspNetCore.Mvc;

namespace WebApi.Utils.CustomProblemDetails;

public class DbCustomProblemDetail : ProblemDetails
{
    public string AdditionalInfo { get; set; }
}