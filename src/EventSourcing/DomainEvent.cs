namespace EventSourcing;

public abstract record DomainEvent(string Id);

public sealed record DomainEvent<TData>(string Id, TData Data) : DomainEvent(Id)
    where TData : notnull
{
    public static DomainEvent<TData> From(string id, TData data) =>
        new(id, data);
}
