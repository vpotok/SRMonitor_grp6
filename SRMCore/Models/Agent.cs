namespace SRMCore.Models;

public class Agent
{
    public Guid Uuid { get; set; }  // ✅ Primärschlüssel (automatisch generiert)
    
    public string AuthToken { get; set; } = null!; // ✅ Geheimtoken – unique index, aber kein PK
    public bool Enabled { get; set; }

    public int ComId { get; set; } // 🔁 FK zur Firma
    public Company Company { get; set; } = null!;
}