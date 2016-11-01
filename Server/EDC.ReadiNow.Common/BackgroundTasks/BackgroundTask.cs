using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using ProtoBuf;
using ProtoBuf.Meta;
using StackExchange.Redis;
using System;
using System.IO;
using System.Linq;

namespace EDC.ReadiNow.BackgroundTasks
{
    [ProtoContract]
    public class BackgroundTask
    {
        /// <summary>
        /// The key for the handler for this task
        /// </summary>
		[ProtoMember(1)]
        public string HandlerKey { get; set; }


        /// <summary>
        /// The data for the task
        /// </summary>
        [ProtoMember(3)]
        public byte[] SerializedData { get; set; }

        /// <summary>
        /// Teh data for the task
        /// </summary>
        [ProtoMember(4)]
        public byte[] SerializedContext { get; set; }

        public T GetData<T>()
        {
            return DeserializeData<T>(SerializedData);
        }

        [ProtoIgnore]
        public RequestContextData Context { get { return DeserializeData<BackgroundTaskContext>(SerializedContext).RequestContext; } }


        public BackgroundTask()
        {

        }

        static BackgroundTask()
        {
            // Register all the parameters with ProtBuf
            var type = typeof(IWorkflowQueuedEvent);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p != typeof(IWorkflowQueuedEvent));

            var metaType = RuntimeTypeModel.Default.Add(typeof(IWorkflowQueuedEvent), true);

            var subtype = 50;
            foreach (var t in types)
            {
                //RuntimeTypeModel.Default.Add(t, false)
                //    .SetSurrogate(t);

                metaType.AddSubType(subtype++, t);
            }
        }

        /// <summary>
        /// Create a background task using a handler, data and the current RequestContext
        /// </summary>
        /// <param name="handlerKey"></param>
        /// <param name="data"></param>
        public static BackgroundTask Create<T>(string handlerKey, T data) where T: IWorkflowQueuedEvent, new()
        {
            var result = new BackgroundTask();
            result.HandlerKey = handlerKey;
            result.SerializedData = SerializeData<T>(data);
            result.SerializedContext = SerializeData<BackgroundTaskContext>(new BackgroundTaskContext(RequestContext.GetContext()));

            return result;
        }

        static byte[] SerializeData<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, value);

                byte[] bytes = stream.ToArray();

                return bytes;
            }
        }

        static T DeserializeData<T>(byte[] bytes)
        {
            T result;

            using (var stream = new MemoryStream(bytes))
            {
                result = ProtoBuf.Serializer.Deserialize<T>(stream);
            }

            return result;
        }
    }

    [ProtoContract]
    public class BackgroundTaskContext
    {
        [ProtoMember(1)]
        public long TenantId { get; set;}

        [ProtoMember(2)]
        public long IdentityId { get; set; }

        [ProtoMember(3)]
        public string IdentityName { get; set; }

        [ProtoMember(4)]
        public long SecondaryIdentityId { get; set; }

        [ProtoMember(5)]
        public string SecondaryIdentityName { get; set; }


        [ProtoMember(6)]
        public string Culture { get; set; }

        [ProtoMember(7)]
        public string TimeZone { get; set; }

        public BackgroundTaskContext()
        { }

        public BackgroundTaskContext(RequestContext requestContext)
        {
            TenantId = requestContext.Tenant.Id;

            if (requestContext.Identity != null)
            {
                IdentityId = requestContext.Identity.Id;
                IdentityName = requestContext.Identity.Name;
            }

            if (requestContext.SecondaryIdentity != null)
            { 
                SecondaryIdentityId = requestContext.SecondaryIdentity.Id;
                SecondaryIdentityName = requestContext.SecondaryIdentity.Name;
            }

            Culture = requestContext.Culture;
            TimeZone = requestContext.TimeZone;
        }

        public RequestContextData RequestContext
        {
            get
            {
                var identity = new Security.IdentityInfo(IdentityId, IdentityName);
                var secondaryIdentity = SecondaryIdentityId != 0 ? new Security.IdentityInfo(SecondaryIdentityId, SecondaryIdentityName) : null;

                return new RequestContextData(
                    identity,
                    new Metadata.Tenants.TenantInfo(TenantId),
                    Culture,
                    TimeZone,
                    secondaryIdentity);
            }
        }

    }
}