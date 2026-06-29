namespace SmartFarmSEP490.Model.Enums;

public enum ExperimentStatus
{
    Active = 1,
    Completed = 2
}
// chỉ cần 2 trạng thái Active và Completed
// vì khi được duyệt request thì sẽ tự động chuyển sang trạng thái Active và khi hoàn thành thì sẽ tự động chuyển sang trạng thái Completed
// và không cần trạng thái Draft, Approved và Cancelled
