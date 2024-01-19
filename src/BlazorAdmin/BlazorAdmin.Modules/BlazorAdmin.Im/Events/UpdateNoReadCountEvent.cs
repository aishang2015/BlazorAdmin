namespace BlazorAdmin.Im.Events
{
    public class UpdateNoReadCountEvent
    {
        public UpdateNoCountEventType Type { get; set; }

        public int Count { get; set; }
    }

    public enum UpdateNoCountEventType
    {
        Add,
        Sub,
        Refresh,
    }
}
