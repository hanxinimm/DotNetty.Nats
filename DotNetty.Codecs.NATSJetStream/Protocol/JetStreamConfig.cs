using DotNetty.Codecs.NATSJetStream.JetStream;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class JetStreamConfig
    {
        // see builder for defaults
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("subjects")]
        public List<string> Subjects { get; private set; }
        [JsonProperty("retention")]
        public RetentionPolicy RetentionPolicy { get; private set; }
        [JsonProperty("max_consumers")]
        public long MaxConsumers { get; private set; }
        [JsonProperty("max_msgs")]
        public long MaxMsgs { get; private set; }
        [JsonProperty("max_bytes")]
        public long MaxBytes { get; private set; }
        [JsonProperty("max_age")]
        public TimeSpan MaxAge { get; private set; }
        [JsonProperty("max_msgs_per_subject")]
        public long MaxMsgPer { get; private set; }
        [JsonProperty("max_msg_size")]
        public long MaxMsgSize { get; private set; }
        [JsonProperty("discard")]
        public DiscardPolicy DiscardPolicy { get; private set; }
        [JsonProperty("storage")]
        public StorageType StorageType { get; private set; }
        [JsonProperty("num_replicas")]
        public int Replicas { get; private set; }
        [JsonProperty("no_ack")]
        public bool NoAck { get; private set; }
        [JsonProperty("template_owner")]
        public string TemplateOwner { get; private set; }
        [JsonProperty("duplicate_window")]
        public TimeSpan DuplicateWindow { get; private set; }
        [JsonProperty("placement")]
        public Placement Placement { get; private set; }
        [JsonProperty("mirror")]
        public StreamSource Mirror { get; private set; }
        [JsonProperty("sources")]
        public List<StreamSource> Sources { get; private set; }

        private JetStreamConfig(
            string name, 
            List<string> subjects, 
            RetentionPolicy retentionPolicy,
            long MaxConsumers,
            long MaxMsgs, 
            long MaxBytes,
            TimeSpan MaxAge,
            long MaxMsgSize,
            StorageType storageType,
            int replicas, 
            bool noAck, 
            string templateOwner,
            DiscardPolicy discardPolicy, 
            TimeSpan duplicateWindow,
            Placement placement,
            StreamSource mirror,
            List<StreamSource> sources)
        {
            this.Name = name;
            this.Subjects = subjects;
            this.RetentionPolicy = retentionPolicy;
            this.MaxConsumers = MaxConsumers;
            this.MaxMsgs = MaxMsgs;
            this.MaxBytes = MaxBytes;
            this.MaxAge = MaxAge;
            this.MaxMsgSize = MaxMsgSize;
            this.StorageType = storageType;
            this.Replicas = replicas;
            this.NoAck = noAck;
            this.TemplateOwner = templateOwner;
            this.DiscardPolicy = discardPolicy;
            this.DuplicateWindow = duplicateWindow;
            this.Placement = placement;
            this.Mirror = mirror;
            this.Sources = sources;
        }


        public override string ToString()
        {
            return "NATSJetStreamConfig{" +
                    "name='" + Name + '\'' +
                    ", subjects=" + Subjects +
                    ", retentionPolicy=" + RetentionPolicy +
                    ", MaxConsumers=" + MaxConsumers +
                    ", MaxMsgs=" + MaxMsgs +
                    ", MaxBytes=" + MaxBytes +
                    ", MaxAge=" + MaxAge +
                    ", MaxMsgSize=" + MaxMsgSize +
                    ", storageType=" + StorageType +
                    ", replicas=" + Replicas +
                    ", noAck=" + NoAck +
                    ", template='" + TemplateOwner + '\'' +
                    ", discardPolicy=" + DiscardPolicy +
                    ", duplicateWindow=" + DuplicateWindow +
                    ", " + Mirror +
                    ", " + Placement +
                    ", sources=" + Sources +
                    '}';
        }

        /**
         * Creates a builder for the stream configuration.
         * @return a stream configuration builder
         */
        public static NATSJetStreamConfigBuilder Builder()
        {
            return new NATSJetStreamConfigBuilder();
        }

        /**
         * Creates a builder to copy the stream configuration.
         * @param jetStreamConfig an existing NATSJetStreamConfig
         * @return a stream configuration builder
         */
        public static NATSJetStreamConfigBuilder Builder(JetStreamConfig jetStreamConfig)
        {
            return new NATSJetStreamConfigBuilder(jetStreamConfig);
        }

        /**
         * NATSJetStreamConfig is created using a Builder. The builder supports chaining and will
         * create a default set of options if no methods are calls.
         * 
         * <p>{@code new NATSJetStreamConfig.Builder().build()} will create a new ConsumerConfiguration.
         * 
         */
        public class NATSJetStreamConfigBuilder
        {

            private string Name;
            private readonly List<string> Subjects = new List<string>();
            private RetentionPolicy RetentionPolicy;
            private long MaxConsumers = -1;
            private long MaxMsgs = -1;
            private long MaxBytes = -1;
            private TimeSpan MaxAge = TimeSpan.Zero;
            private long MaxMsgSize = -1;
            private StorageType StorageType;
            private int Replicas = 1;
            private bool NoAck = false;
            private string TemplateOwner;
            private DiscardPolicy DiscardPolicy;
            private TimeSpan DuplicateWindow = TimeSpan.Zero;
            private Placement Placement;
            private StreamSource Mirror;
            private readonly List<StreamSource> Sources = new List<StreamSource>();

            /**
             * Default Builder
             */
            internal NATSJetStreamConfigBuilder() { }

            /**
             * Update Builder, useful if you need to update a configuration
             * @param sc the configuration to copy
             */
            internal NATSJetStreamConfigBuilder(JetStreamConfig jetStreamConfig)
            {
                if (jetStreamConfig != null)
                {
                    Name = jetStreamConfig.Name;
                    SetSubjects(jetStreamConfig.Subjects);
                    this.RetentionPolicy = jetStreamConfig.RetentionPolicy;
                    this.MaxConsumers = jetStreamConfig.MaxConsumers;
                    this.MaxMsgs = jetStreamConfig.MaxMsgs;
                    this.MaxBytes = jetStreamConfig.MaxBytes;
                    this.MaxAge = jetStreamConfig.MaxAge;
                    this.MaxMsgSize = jetStreamConfig.MaxMsgSize;
                    this.StorageType = jetStreamConfig.StorageType;
                    this.Replicas = jetStreamConfig.Replicas;
                    this.NoAck = jetStreamConfig.NoAck;
                    this.TemplateOwner = jetStreamConfig.TemplateOwner;
                    this.DiscardPolicy = jetStreamConfig.DiscardPolicy;
                    this.DuplicateWindow = jetStreamConfig.DuplicateWindow;
                    this.Placement = jetStreamConfig.Placement;
                    this.Mirror = jetStreamConfig.Mirror;
                    SetSources(jetStreamConfig.Sources);
                }
            }

            /**
             * Sets the name of the stream.
             * @param name name of the stream.
             * @return the builder
             */
            public NATSJetStreamConfigBuilder SetName(string name)
            {
                this.Name = name;
                return this;
            }

            /**
             * Sets the subjects in the NATSJetStreamConfig.
             * @param subjects the stream's subjects
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetSubjects(params string[] subjects)
            {
                this.Subjects.Clear();
                return AddSubjects(subjects);
            }

            /**
             * Sets the subjects in the NATSJetStreamConfig.
             * @param subjects the stream's subjects
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetSubjects(IEnumerable<string> subjects)
            {
                this.Subjects.Clear();
                return AddSubjects(subjects);
            }

            /**
             * Sets the subjects in the NATSJetStreamConfig.
             * @param subjects the stream's subjects
             * @return Builder
             */
            public NATSJetStreamConfigBuilder AddSubjects(params string[] subjects)
            {
                if (subjects != null)
                {
                    return AddSubjects(subjects);
                }
                return this;
            }

            /**
             * Sets the subjects in the NATSJetStreamConfig.
             * @param subjects the stream's subjects
             * @return Builder
             */
            public NATSJetStreamConfigBuilder AddSubjects(IEnumerable<string> subjects)
            {
                if (subjects != null)
                {
                    foreach (var subject in subjects)
                    {
                        if (subject != null && !this.Subjects.Contains(subject))
                        {
                            this.Subjects.Add(subject);
                        }
                    }
                }
                return this;
            }

            /**
             * Sets the retention policy in the NATSJetStreamConfig.
             * @param policy the retention policy of the NATSJetStreamConfig
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetRetentionPolicy(RetentionPolicy policy)
            {
                this.RetentionPolicy = policy;
                return this;
            }

            /**
             * Sets the Maximum number of consumers in the NATSJetStreamConfig.
             * @param MaxConsumers the Maximum number of consumers
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetMaxConsumers(long maxConsumers)
            {
                this.MaxConsumers = NATSJetStreamValidator.ValidateMaxConsumers(maxConsumers);
                return this;
            }

            /**
             * Sets the Maximum number of consumers in the NATSJetStreamConfig.
             * @param MaxMsgs the Maximum number of messages
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetMaxMessages(long maxMsgs)
            {
                this.MaxMsgs = NATSJetStreamValidator.ValidateMaxMessages(maxMsgs);
                return this;
            }

            /**
             * Sets the Maximum number of bytes in the NATSJetStreamConfig.
             * @param MaxBytes the Maximum number of bytes
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetMaxBytes(long maxBytes)
            {
                this.MaxBytes = NATSJetStreamValidator.ValidateMaxBytes(maxBytes);
                return this;
            }

            /**
             * Sets the Maximum age in the NATSJetStreamConfig.
             * @param MaxAge the Maximum message age
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetMaxAge(TimeSpan maxAge)
            {
                this.MaxAge = NATSJetStreamValidator.ValidateTimeSpanNotRequiredGtOrEqZero(maxAge);
                return this;
            }

            /**
             * Sets the Maximum message size in the NATSJetStreamConfig.
             * @param MaxMsgSize the Maximum message size
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetMaxMsgSize(long maxMsgSize)
            {
                this.MaxMsgSize = NATSJetStreamValidator.ValidateMaxMessageSize(maxMsgSize);
                return this;
            }

            /**
             * Sets the storage type in the NATSJetStreamConfig.
             * @param storageType the storage type
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetStorageType(StorageType storageType)
            {
                this.StorageType = storageType;
                return this;
            }

            /**
             * Sets the number of replicas a message must be stored on in the NATSJetStreamConfig.
             * @param replicas the number of replicas to store this message on
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetReplicas(int replicas)
            {
                this.Replicas = NATSJetStreamValidator.ValidateNumberOfReplicas(replicas);
                return this;
            }

            /**
             * Sets the acknowledgement mode of the NATSJetStreamConfig.  if no acknowledgements are
             * set, then acknowledgements are not sent back to the client.  The default is false.
             * @param noAck true to disable acknowledgements.
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetNoAck(bool noAck)
            {
                this.NoAck = noAck;
                return this;
            }

            /**
             * Sets the template a stream in the form of raw JSON.
             * @param templateOwner the stream template of the stream.
             * @return the builder
             */
            public NATSJetStreamConfigBuilder SetTemplateOwner(string templateOwner)
            {
                if (string.IsNullOrEmpty(templateOwner)) throw new ArgumentNullException(nameof(templateOwner));
                this.TemplateOwner = templateOwner;
                return this;
            }

            /**
             * Sets the discard policy in the NATSJetStreamConfig.
             * @param policy the discard policy of the NATSJetStreamConfig
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetDiscardPolicy(DiscardPolicy policy)
            {
                this.DiscardPolicy = policy;
                return this;
            }

            /**
             * Sets the duplicate checking window in the the NATSJetStreamConfig.  A TimeSpan.Zero
             * disables duplicate checking.  Duplicate checking is disabled by default.
             * @param window duration to hold message ids for duplicate checking.
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetDuplicateWindow(TimeSpan window)
            {
                this.DuplicateWindow = NATSJetStreamValidator.ValidateTimeSpanNotRequiredGtOrEqZero(window);
                return this;
            }

            /**
             * Sets the placement directive object
             * @param placement the placement directive object
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetPlacement(Placement placement)
            {
                this.Placement = placement;
                return this;
            }

            /**
             * Sets the mirror  object
             * @param mirror the mirror object
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetMirror(StreamSource mirror)
            {
                this.Mirror = mirror;
                return this;
            }

            /**
             * Sets the sources in the NATSJetStreamConfig.
             * @param sources the stream's sources
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetSources(params StreamSource[] sources)
            {
                this.Sources.Clear();
                return AddSources(sources);
            }

            /**
             * Sets the sources in the NATSJetStreamConfig.
             * @param sources the stream's sources
             * @return Builder
             */
            public NATSJetStreamConfigBuilder SetSources(IEnumerable<StreamSource> sources)
            {
                this.Sources.Clear();
                return AddSources(sources);
            }

            /**
             * Sets the sources in the NATSJetStreamConfig.
             * @param sources the stream's sources
             * @return Builder
             */
            public NATSJetStreamConfigBuilder AddSources(params StreamSource[] sources)
            {
                return AddSources(sources);
            }

            /**
             * Sets the sources in the NATSJetStreamConfig.
             * @param sources the stream's sources
             * @return Builder
             */
            public NATSJetStreamConfigBuilder AddSources(IEnumerable<StreamSource> sources)
            {
                if (sources != null)
                {
                    foreach (var source in sources)
                    {
                        if (source != null && !this.Sources.Contains(source))
                        {
                            this.Sources.Add(source);
                        }
                    }
                }
                return this;
            }

            /**
             * Builds the ConsumerConfiguration
             * @return a consumer configuration.
             */
            public JetStreamConfig Build()
            {
                return new JetStreamConfig(
                        Name,
                        Subjects,
                        RetentionPolicy,
                        MaxConsumers,
                        MaxMsgs,
                        MaxBytes,
                        MaxAge,
                        MaxMsgSize,
                        StorageType,
                        Replicas,
                        NoAck,
                        TemplateOwner,
                        DiscardPolicy,
                        DuplicateWindow,
                        Placement,
                        Mirror,
                        Sources
                );
            }

        }
    }
}
