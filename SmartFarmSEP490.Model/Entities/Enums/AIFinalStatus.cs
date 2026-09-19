namespace SmartFarmSEP490.Model.Enums;

/// <summary>
/// Kết quả cuối cùng từ AI. Tách riêng giá trị cho từng provider.
/// </summary>
public enum AIFinalStatus
{
    Unknown = 0,

    // ----- Tomato Leaf Disease -----
    NotTomatoLeaf = 1,          // gate: out_of_domain
    NoLeafDetected = 2,        // gate pass, YOLO không thấy box
    TomatoLeafClassified = 3,   // gate pass + có detection + classify bệnh

    // ----- Argo Pest -----
    NoPest = 4,                // gate: non_pest (is_pest=false)
    NoPestDetected = 5,        // gate: pest nhưng YOLO không thấy box
    PestClassified = 6         // gate: pest + có detection + classify loài
}
