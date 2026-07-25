using FluentValidation;
using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Model.Validators;

public class CreateExperimentDtoValidator : AbstractValidator<CreateExperimentDto>
{
    public CreateExperimentDtoValidator()
    {
        RuleFor(x => x.FarmId)
            .NotEmpty().WithMessage("FarmId is required.");

        RuleFor(x => x.ExperimentCode)
            .NotEmpty().WithMessage("ExperimentCode is required.")
            .MaximumLength(50).WithMessage("ExperimentCode cannot exceed 50 characters.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Objective)
            .NotEmpty().WithMessage("Objective is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
    }
}

public class UpdateExperimentDtoValidator : AbstractValidator<UpdateExperimentDto>
{
    public UpdateExperimentDtoValidator()
    {
        RuleFor(x => x.ExperimentCode)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.ExperimentCode))
            .WithMessage("ExperimentCode cannot exceed 50 characters.");

        RuleFor(x => x.Title)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
    }
}

public class CreateExperimentStageDtoValidator : AbstractValidator<CreateExperimentStageDto>
{
    public CreateExperimentStageDtoValidator()
    {
        RuleFor(x => x.StageName)
            .NotEmpty().WithMessage("StageName is required.")
            .MaximumLength(100).WithMessage("StageName cannot exceed 100 characters.");

        RuleFor(x => x.StageOrder)
            .GreaterThan(0).WithMessage("StageOrder must be greater than 0.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
    }
}

public class UpdateExperimentStageDtoValidator : AbstractValidator<UpdateExperimentStageDto>
{
    public UpdateExperimentStageDtoValidator()
    {
        RuleFor(x => x.StageName)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.StageName))
            .WithMessage("StageName cannot exceed 100 characters.");

        RuleFor(x => x.StageOrder)
            .GreaterThan(0).When(x => x.StageOrder.HasValue)
            .WithMessage("StageOrder must be greater than 0.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
    }
}

public class CreateCareScheduleDtoValidator : AbstractValidator<CreateCareScheduleDto>
{
    public CreateCareScheduleDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters.")
            .MaximumLength(150).WithMessage("Title cannot exceed 150 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate.HasValue)
            .WithMessage("EndDate must be greater than or equal to StartDate.");

        RuleFor(x => x.FrequencyDays)
            .GreaterThan(0).When(x => x.FrequencyDays.HasValue)
            .WithMessage("FrequencyDays must be greater than 0.");
    }
}

public class UpdateCareScheduleDtoValidator : AbstractValidator<UpdateCareScheduleDto>
{
    public UpdateCareScheduleDtoValidator()
    {
        RuleFor(x => x.Title)
            .MinimumLength(3).When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Title must be at least 3 characters.")
            .MaximumLength(150).When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Title cannot exceed 150 characters.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate must be greater than or equal to StartDate.");

        RuleFor(x => x.FrequencyDays)
            .GreaterThan(0).When(x => x.FrequencyDays.HasValue)
            .WithMessage("FrequencyDays must be greater than 0.");
    }
}

public class CreateMeasurementDefinitionDtoValidator : AbstractValidator<CreateMeasurementDefinitionDto>
{
    public CreateMeasurementDefinitionDtoValidator()
    {
        RuleFor(x => x.MetricName)
            .NotEmpty().WithMessage("MetricName is required.")
            .MaximumLength(100).WithMessage("MetricName cannot exceed 100 characters.");

        RuleFor(x => x.Unit)
            .MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Unit))
            .WithMessage("Unit cannot exceed 30 characters.");
    }
}
