using System.Text.Json;

using Amazon.SQS;
using Amazon.SQS.Model;

using Customers.Consumer.Messages;
using Customers.Consumer.Messaging;

using Microsoft.Extensions.Options;

namespace Customers.Consumer
{
    public class QueueConsumerService : BackgroundService
    {
        private readonly IAmazonSQS _amazonSQS;
        private readonly QueueSettings _queueSettings;

        public QueueConsumerService(IAmazonSQS amazonSQS,
            IOptions<QueueSettings> queueSettings)
        {
            _amazonSQS = amazonSQS;
            _queueSettings = queueSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueUrl = await _amazonSQS.GetQueueUrlAsync(new GetQueueUrlRequest()
            {
                QueueName = _queueSettings.Name
            }, stoppingToken);

            var receiveMessageRequest = new ReceiveMessageRequest()
            {
                QueueUrl = queueUrl.QueueUrl,
                AttributeNames = new List<string>() { "All" },
                MessageAttributeNames = new List<string>() { "All" },
                MaxNumberOfMessages = 1
            };

            while(!stoppingToken.IsCancellationRequested)
            {
                var response = await _amazonSQS.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                foreach(var message in response.Messages)
                {
                    var messageType = message.MessageAttributes["MessageType"].StringValue;

                    switch (messageType)
                    {
                        case nameof(CustomerCreated):
                            var customerCreated = JsonSerializer.Deserialize<CustomerCreated>(message.Body);
                            break;
                        case nameof(CustomerUpdated):
                            break;
                        case nameof(CustomerDeleted):
                            break;
                    }

                    await _amazonSQS.DeleteMessageAsync(queueUrl.QueueUrl, message.ReceiptHandle, stoppingToken);

                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
