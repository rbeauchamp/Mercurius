//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Mercurius
//{
//    public sealed class MessagePublisher : IMessagePublisher
//    {
//        private readonly IGuidGenerator _guidGenerator;
//        private readonly CloudQueue _queue;

//        public MessagePublisher(string queueName, string storageConnectionStringName, IGuidGenerator guidGenerator)
//        {
//            _guidGenerator = guidGenerator;
//            _queue = GetQueue(queueName, storageConnectionStringName);
//        }

//        public async Task PublishAsync(IMessage message)
//        {
//            await AddMessageToQueue(message);
//        }

//        internal void InitializeMessage(IMessage message)
//        {
//            message.Id = _guidGenerator.Generate();
//            message.DatePublished = DateTime.UtcNow;
//            message.PrincipalId = GetPrincipalId();
//        }

//        private async Task AddMessageToQueue(IMessage message)
//        {
//            var content = InitializeAndSerializeContent(message);
//            var cloudQueueMessage = new CloudQueueMessage(content);

//            await _queue.AddMessageAsync(cloudQueueMessage);
//        }

//        internal byte[] InitializeAndSerializeContent(IMessage message)
//        {
//            InitializeMessage(message);

//            Validate(message);

//            return new MessageEnvelope(message).Serialize();
//        }

//        internal static CloudQueue GetQueue(string queueName, string connectionStringName)
//        {
//            // Retrieve storage account from connection string
//            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(connectionStringName));

//            // Create the queue client
//            var queueClient = storageAccount.CreateCloudQueueClient();

//            // Retrieve a reference to the queue
//            // Note the queue name requirements! See https://coderwall.com/p/g2xeua
//            var queue = queueClient.GetQueueReference(queueName);

//            // Create the queue if it doesn't already exist
//            queue.CreateIfNotExists();

//            return queue;
//        }

//        private static Guid? GetPrincipalId()
//        {
//            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
//            {
//                throw new AuthenticationException("A message can only be published by an authenticated principal.");
//            }

//            var userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Id);

//            if (userId != null && !userId.Value.IsNullOrEmpty())
//            {
//                return new Guid(userId.Value);
//            }

//            throw new InvalidOperationException(string.Format("The current principal must have an {0} claim.", ClaimTypes.Id));
//        }

//        private static void Validate(IMessage message)
//        {
//            var validationResults = new List<ValidationResult>();

//            Validator.TryValidateObject(message, new ValidationContext(message, null, null), validationResults, true);

//            if (validationResults.Any())
//            {
//                throw new ArgumentException(FormatValidationMessages(validationResults));
//            }
//        }

//        private static string FormatValidationMessages(IEnumerable<ValidationResult> validationResults)
//        {
//            var validationMessages = new StringBuilder();

//            foreach (var validationResult in validationResults)
//            {
//                validationMessages.AppendLine(validationResult.ErrorMessage);
//            }

//            return validationMessages.ToString();
//        }
//    }
//}