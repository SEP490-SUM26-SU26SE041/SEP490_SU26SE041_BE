using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string? ProfileDescription { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<AitaskAssignmentSuggestion> AitaskAssignmentSuggestionReviewedByNavigations { get; set; } = new List<AitaskAssignmentSuggestion>();

    public virtual ICollection<AitaskAssignmentSuggestion> AitaskAssignmentSuggestionSuggestedUsers { get; set; } = new List<AitaskAssignmentSuggestion>();

    public virtual ICollection<ExperimentReport> ExperimentReports { get; set; } = new List<ExperimentReport>();

    public virtual ICollection<ExperimentRequest> ExperimentRequests { get; set; } = new List<ExperimentRequest>();

    public virtual ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();

    public virtual ICollection<Farm> Farms { get; set; } = new List<Farm>();

    public virtual ICollection<KnowledgeDocument> KnowledgeDocuments { get; set; } = new List<KnowledgeDocument>();

    public virtual ICollection<MeasurementRecord> MeasurementRecords { get; set; } = new List<MeasurementRecord>();

    public virtual ICollection<PlantHealthAssessment> PlantHealthAssessments { get; set; } = new List<PlantHealthAssessment>();

    public virtual ICollection<PlantImage> PlantImages { get; set; } = new List<PlantImage>();

    public virtual ICollection<ProcedureTemplate> ProcedureTemplates { get; set; } = new List<ProcedureTemplate>();

    public virtual ICollection<RequestReview> RequestReviews { get; set; } = new List<RequestReview>();

    public virtual ICollection<Task> TaskAssignedToNavigations { get; set; } = new List<Task>();

    public virtual ICollection<TaskAssignment> TaskAssignmentAssignedByNavigations { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<TaskAssignment> TaskAssignmentAssignees { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<Task> TaskCreatedByNavigations { get; set; } = new List<Task>();

    public virtual ICollection<TaskReport> TaskReports { get; set; } = new List<TaskReport>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();

    public virtual ICollection<SmartFarmSEP490.Model.Entities.Notification> NotificationRecipients { get; set; } = new List<SmartFarmSEP490.Model.Entities.Notification>();

    public virtual ICollection<SmartFarmSEP490.Model.Entities.Notification> NotificationSenders { get; set; } = new List<SmartFarmSEP490.Model.Entities.Notification>();
}
