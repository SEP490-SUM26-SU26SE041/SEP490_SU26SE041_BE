using FluentValidation;

namespace SmartFarmSEP490.Model.Validators;

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
            .Must(d => !d.HasValue || d.Value >= DateTime.UtcNow)
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
            .Must(d => !d.HasValue || d.Value >= DateTime.UtcNow)
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
            .Must(m => !m.HasValue || m.Value <= DateTime.UtcNow.AddMinutes(5))
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
            .Must(m => !m.HasValue || m.Value <= DateTime.UtcNow.AddMinutes(5))
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
