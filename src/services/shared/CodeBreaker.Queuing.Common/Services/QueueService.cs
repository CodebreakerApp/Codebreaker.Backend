using System.Text.Json;
using Azure.Storage.Queues;
using CodeBreaker.Queuing.Common.Exceptions;
using CodeBreaker.Queuing.Common.Models;
using Microsoft.Extensions.Logging;

namespace CodeBreaker.Queuing.Common.Services;

public abstract class QueueService<TMessage>
{
    private readonly ILogger _logger;

    private readonly QueueServiceClient _queueServiceClient;

    private readonly string _queueName;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueService{TMessage}"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="queueServiceClient">The queue service client. Typically registered in the DI-container using Microsoft.Extensions.Azure.</param>
    /// <param name="queueName">Name of the queue to use in this service.</param>
    public QueueService(ILogger logger, QueueServiceClient queueServiceClient, string queueName)
    {
        _logger = logger;
        _queueServiceClient = queueServiceClient;
        _queueName = queueName;
    }

    /// <summary>
    /// Enqueues the <paramref name="message"/> to the queue asynchronously.
    /// </summary>
    /// <param name="message">The message to enqueue.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task EnqueueMessageAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        var queueClient = await CreateQueueClient();
        var messageBody = JsonSerializer.Serialize(message);
        _logger.EnqueuedItem(messageBody);
        await queueClient.SendMessageAsync(messageBody, cancellationToken);
    }

    /// <inheritdoc cref="QueueService{TMessage}.DequeueMessageAsync(TimeSpan, CancellationToken)" />
    /// <remarks>
    /// The message will be visible in the queue again after 30 seconds.
    /// </remarks>
    public Task<QueueMessage<TMessage>> DequeueMessageAsync(CancellationToken cancellationToken = default) =>
        DequeueMessageAsync(TimeSpan.FromSeconds(30), cancellationToken);

    /// <summary>
    /// Dequeues a message from the queue asynchronously.<br />
    /// The message will not be removed from the queue.
    /// </summary>
    /// <remarks>
    /// The message will be visible in the queue again after the <paramref name="visibilityTimeout"/> elapses.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dequeued message.</returns>
    /// <exception cref="CodeBreaker.Queuing.Common.Exceptions.NoItemException">
    /// No item is in the queue.
    /// or
    /// The dequeued item is empty.
    /// </exception>
    public async Task<QueueMessage<TMessage>> DequeueMessageAsync(TimeSpan visibilityTimeout, CancellationToken cancellationToken = default)
    {
        var queueClient = await CreateQueueClient();
        cancellationToken.ThrowIfCancellationRequested();
        var result = await queueClient.ReceiveMessageAsync(visibilityTimeout, cancellationToken);
        var message = result.Value;

        if (message == null)
        {
            _logger.DequeuedEmptyItem();
            throw new NoItemException($"No item in \"{_queueName}\"");
        }

        cancellationToken.ThrowIfCancellationRequested();
        _logger.DequeuedItem();
        var messageBody = JsonSerializer.Deserialize<TMessage>(message.Body.ToStream()) ?? throw new NoItemException($"Empty item in \"{_queueName}\"");
        return new(message.MessageId, message.PopReceipt, messageBody);
    }

    /// <summary>
    /// Dequeues <b><u>and removes</u></b> a message from the queue asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dequeued message.</returns>
    /// <exception cref="CodeBreaker.Queuing.Common.Exceptions.NoItemException">
    /// No item is in the queue.
    /// or
    /// The dequeued item is empty.
    /// </exception>
    public async Task<QueueMessage<TMessage>> DequeueAndRemoveMessageAsync(CancellationToken cancellationToken = default)
    {
        var message = await DequeueMessageAsync(cancellationToken);
        await RemoveMessageAsync(message);
        return message;
    }

    /// <summary>
    /// Removes the message from the queue asynchronously.
    /// </summary>
    /// <param name="message">The message to remove.</param>
    public Task RemoveMessageAsync(QueueMessage<TMessage> message, CancellationToken cancellationToken = default) =>
        RemoveMessageAsync(message.Id, message.PopReceipt, cancellationToken);

    /// <summary>
    /// Removes the message from the queue asynchronously.
    /// </summary>
    /// <param name="messageId">The identifier of the message to remove.</param>
    /// <param name="popReceipt">The pop receipt.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task RemoveMessageAsync(string messageId, string popReceipt, CancellationToken cancellationToken = default)
    {
        var queueClient = await CreateQueueClient();
        await queueClient.DeleteMessageAsync(messageId, popReceipt, cancellationToken);
        _logger.RemovedItem(messageId);
    }

    private async Task<QueueClient> CreateQueueClient()
    {
        var client = _queueServiceClient.GetQueueClient(_queueName);
        await client.CreateIfNotExistsAsync();
        return client;
    }
}
