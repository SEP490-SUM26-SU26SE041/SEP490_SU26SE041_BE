namespace SmartFarmSEP490.Model.Exceptions;

public class TaskValidationException : ArgumentException
{
    public string ErrorCode { get; }
    public Dictionary<string, string[]> Errors { get; }

    public TaskValidationException(string message, string errorCode = "VALIDATION_ERROR") 
        : base(message)
    {
        ErrorCode = errorCode;
        Errors = new Dictionary<string, string[]>();
    }

    public TaskValidationException(Dictionary<string, string[]> errors, string errorCode = "VALIDATION_ERROR")
        : base("One or more validation errors occurred.")
    {
        ErrorCode = errorCode;
        Errors = errors;
    }
}

public class ConcurrencyException : InvalidOperationException
{
    public string ErrorCode { get; }

    public ConcurrencyException(string message) 
        : base(message)
    {
        ErrorCode = "CONCURRENCY_CONFLICT";
    }
}

public class ResourceNotFoundException : KeyNotFoundException
{
    public string ResourceType { get; }
    public object ResourceId { get; }

    public ResourceNotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} with id '{resourceId}' was not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

public class BusinessRuleException : InvalidOperationException
{
    public string ErrorCode { get; }

    public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
