using FluentValidation;
using System;

namespace SmartFarmSEP490.Model.Validators;

/// <summary>
/// Convert 1 DateTime (giả định theo giờ VN nếu Kind=Unspecified) sang UTC rồi so sánh với UtcNow.
/// Validator nằm trong project Model nên không thể reference VietnamTime — tính inline.
/// ICT = UTC+7, cố định quanh năm (VN không có DST).
/// </summary>
internal static class ValidatorTime
{
    private const int VietnamUtcOffsetHours = 7;

    public static bool IsNotInPast(DateTime value)
    {
        DateTime asUtc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            // Kind=Unspecified → mặc định coi như giờ VN, trừ 7h để ra UTC
            _ => DateTime.SpecifyKind(value.AddHours(-VietnamUtcOffsetHours), DateTimeKind.Utc)
        };
        return asUtc >= DateTime.UtcNow;
    }

    public static bool IsWithinFutureSkew(DateTime value, int skewMinutes)
    {
        DateTime asUtc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.AddHours(-VietnamUtcOffsetHours), DateTimeKind.Utc)
        };
        return asUtc <= DateTime.UtcNow.AddMinutes(skewMinutes);
    }
}

public class CreateTaskDtoValidator : AbstractValidator<DTOs.CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.ExperimentId)
            .NotEmpty().WithMessage("ExperimentId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters.")
            .MaximumLength(150).WithMessage("Title cannot exceed 150 characters.");

        RuleFor(x => x.TaskType)
            .NotEmpty().WithMessage("TaskType is required.");

        RuleFor(x => x.DueDate)
            .Must(d => !d.HasValue || ValidatorTime.IsNotInPast(d.Value))
            .WithMessage("DueDate cannot be in the past.");
    }
}

public class UpdateTaskDtoValidator : AbstractValidator<DTOs.UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .MinimumLength(3).When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Title must be at least 3 characters.");

        RuleFor(x => x.DueDate)
            .Must(d => !d.HasValue || ValidatorTime.IsNotInPast(d.Value))
            .When(x => x.DueDate.HasValue)
            .WithMessage("DueDate cannot be in the past.");
    }
}

public class CreateTaskReportDtoValidator : AbstractValidator<DTOs.CreateTaskReportDto>
{
    public CreateTaskReportDtoValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");
    }
}

public class UpdateTaskReportDtoValidator : AbstractValidator<DTOs.UpdateTaskReportDto>
{
    public UpdateTaskReportDtoValidator()
    {
    }
}

public class CreateMeasurementRecordDtoValidator : AbstractValidator<DTOs.CreateMeasurementRecordDto>
{
    public CreateMeasurementRecordDtoValidator()
    {
        RuleFor(x => x.ExperimentId)
            .NotEmpty().WithMessage("ExperimentId is required.");

        RuleFor(x => x.BatchId)
            .NotEmpty().WithMessage("BatchId is required.");

        RuleFor(x => x)
            .Must(dto => dto.Value.HasValue || !string.IsNullOrWhiteSpace(dto.TextValue))
            .WithMessage("Must provide either Value or TextValue.");

        RuleFor(x => x)
            .Must(dto => !(dto.Value.HasValue && !string.IsNullOrWhiteSpace(dto.TextValue)))
            .WithMessage("Cannot provide both Value and TextValue.");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0).When(x => x.Value.HasValue)
            .WithMessage("Value must be non-negative.");

        RuleFor(x => x.MeasuredAt)
            .Must(m => !m.HasValue || ValidatorTime.IsWithinFutureSkew(m.Value, 5))
            .When(x => x.MeasuredAt.HasValue)
            .WithMessage("MeasuredAt cannot be more than 5 minutes in the future.");
    }
}

public class UpdateMeasurementRecordDtoValidator : AbstractValidator<DTOs.UpdateMeasurementRecordDto>
{
    public UpdateMeasurementRecordDtoValidator()
    {
        RuleFor(x => x)
            .Must(dto => !(dto.Value.HasValue && !string.IsNullOrWhiteSpace(dto.TextValue)))
            .When(x => x.Value.HasValue || !string.IsNullOrWhiteSpace(x.TextValue))
            .WithMessage("Cannot provide both Value and TextValue.");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0).When(x => x.Value.HasValue)
            .WithMessage("Value must be non-negative.");

        RuleFor(x => x.MeasuredAt)
            .Must(m => !m.HasValue || ValidatorTime.IsWithinFutureSkew(m.Value, 5))
            .When(x => x.MeasuredAt.HasValue)
            .WithMessage("MeasuredAt cannot be more than 5 minutes in the future.");
    }
}

public class AssignTaskDtoValidator : AbstractValidator<DTOs.AssignTaskDto>
{
    public AssignTaskDtoValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");

        RuleFor(x => x.AssigneeId)
            .NotEmpty().WithMessage("AssigneeId is required.");
    }
}

public class ReassignTaskDtoValidator : AbstractValidator<DTOs.ReassignTaskDto>
{
    public ReassignTaskDtoValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");

        RuleFor(x => x.NewAssigneeId)
            .NotEmpty().WithMessage("NewAssigneeId is required.");
    }
}

public class UpdateTaskAssignmentStatusDtoValidator : AbstractValidator<DTOs.UpdateTaskAssignmentStatusDto>
{
    public UpdateTaskAssignmentStatusDtoValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("AssignmentId is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => new[] { "Assigned", "Reassigned", "Resigned", "Completed", "Cancelled" }.Contains(s))
            .WithMessage("Status must be one of: Assigned, Reassigned, Resigned, Completed, Cancelled.");
    }
}
