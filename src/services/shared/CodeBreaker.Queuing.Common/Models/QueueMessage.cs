namespace CodeBreaker.Queuing.Common.Models;

public class QueueMessage<TBody> : IEquatable<QueueMessage<TBody>>
{
    public QueueMessage(string id, string popReceipt, TBody body)
    {
        Id = id;
        PopReceipt = popReceipt;
        Body = body;
    }

    public string Id { get; init; }

    public string PopReceipt { get; init; }

    public TBody Body { get; init; }

    public bool Equals(QueueMessage<TBody>? other) =>
        Id == other?.Id;

    public override bool Equals(object? obj) =>
        Equals(obj as QueueMessage<TBody>);

    public override int GetHashCode() =>
        Id.GetHashCode();

    public static bool operator ==(QueueMessage<TBody> a, QueueMessage<TBody> b) =>
        a.Equals(b);

    public static bool operator !=(QueueMessage<TBody> a, QueueMessage<TBody> b) =>
        !a.Equals(b);
}
