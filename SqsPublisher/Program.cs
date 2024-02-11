using System.Text.Json;

using Amazon.SQS;
using Amazon.SQS.Model;

using SqsPublisher;

var sqsClient = new AmazonSQSClient();

var customer = new CustomerCreated {
    Id = new Guid(),
    DateOfBirth = DateTime.UtcNow,
    Email = "imizyk09@gmail.com1",
    Fullname = "I Mizyk",
    GitHubUsername = "im4everok"
};

var queueUrlResponse = await sqsClient.GetQueueUrlAsync("customers");

var sendMessageRequest = new SendMessageRequest
{
    QueueUrl = queueUrlResponse.QueueUrl,
    MessageBody = JsonSerializer.Serialize(customer),
    MessageAttributes = new Dictionary<string, MessageAttributeValue> {
        {
            "MessageType", new MessageAttributeValue
            {
                DataType = "String",
                StringValue = nameof(CustomerCreated)
            }
        } 
    }
};

var response = await sqsClient.SendMessageAsync(sendMessageRequest);

Console.WriteLine("Sent");