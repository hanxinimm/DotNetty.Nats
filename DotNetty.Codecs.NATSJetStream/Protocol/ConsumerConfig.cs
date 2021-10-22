using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class ConsumerConfig
    {

        [JsonProperty("ack_policy")]
        public AckPolicy AckPolicy { get; private set; }
        [JsonProperty("ack_wait")]
        public long AckWait { get; private set; }
        [JsonProperty("deliver_policy")]
        public DeliverPolicy DeliverPolicy { get; private set; }
        [JsonProperty("deliver_subject")]
        public string DeliverSubject { get; private set; }

        [JsonProperty("deliver_group")]
        public string DeliverGroup { get; private set; }
        [JsonProperty("durable_name")]
        public string DurableName { get; private set; }
        [JsonProperty("filter_subject")]
        public string FilterSubject { get; private set; }
        [JsonProperty("flow_control")]
        public bool? FlowControl { get; private set; }
        [JsonProperty("idle_heartbeat")]
        public long? Heartbeat { get; private set; }
        [JsonProperty("max_ack_pending")]
        public long? MaxAckPending { get; private set; }
        [JsonProperty("max_deliver")]
        public long? MaxDeliver { get; private set; }

        [JsonProperty("max_waiting")]
        public long? MaxWaiting { get; private set; }
        [JsonProperty("opt_start_seq")]
        public long? StartSequence  { get; private set; }
        [JsonProperty("opt_start_time")]
        public DateTime? StartTime { get; private set; }
        [JsonProperty("rate_limit_bps")]
        public long? RateLimit { get; private set; }

        [JsonProperty("replay_policy")]
        public ReplayPolicy ReplayPolicy { get; private set; }

        [JsonProperty("sample_freq")]
        public string SampleFrequency { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("headers_only")]
        public string HeadersOnly { get; private set; }

        private ConsumerConfig() { }

        // For the builder
        private ConsumerConfig(string durableName,
            DeliverPolicy deliverPolicy,
            long? startSequence,
            DateTime? startTime,
            AckPolicy ackPolicy,
            long ackWait,
            long? maxDeliver,
            string filterSubject,
            ReplayPolicy replayPolicy,
            string sampleFrequency,
            long? rateLimit,
            string deliverSubject,
            string deliverGroup,
            long? maxAckPending,
            long? heartbeat,
            bool? flowControl)
        {
            this.DurableName = durableName;
            this.DeliverPolicy = deliverPolicy;
            this.StartSequence = startSequence;
            this.StartTime = startTime;
            this.AckPolicy = ackPolicy;
            this.AckWait = ackWait;
            this.MaxDeliver = maxDeliver;
            this.FilterSubject = filterSubject;
            this.ReplayPolicy = replayPolicy;
            this.SampleFrequency = sampleFrequency;
            this.RateLimit = rateLimit;
            this.DeliverSubject = deliverSubject;
            this.DeliverGroup = deliverGroup;
            this.MaxAckPending = maxAckPending;
            this.Heartbeat = heartbeat;
            this.FlowControl = flowControl;
        }

        /**
         * Creates a builder for the publish options.
         * @return a publish options builder
         */
        public static ConsumerConfigBuilder Builder()
        {
            return new ConsumerConfigBuilder();
        }

        /**
         * Creates a builder for the publish options.
         * @param cc the consumer configuration
         * @return a publish options builder
         */
        public static ConsumerConfigBuilder Builder(ConsumerConfig consumerConfig)
        {
            return consumerConfig == null ? new ConsumerConfigBuilder() : new ConsumerConfigBuilder(consumerConfig);
        }

        /**
         * ConsumerConfiguration is created using a Builder. The builder supports chaining and will
         * create a default set of options if no methods are calls.
         * 
         * <p>{@code new ConsumerConfiguration.Builder().build()} will create a default ConsumerConfiguration.
         * 
         */
        public class ConsumerConfigBuilder
        {
            private string DurableName;
            private DeliverPolicy DeliverPolicy;
            private long? StartSequence;
            private DateTime? StartTime;
            private AckPolicy AckPolicy;
            private long AckWait = 30;
            private long? MaxDeliver;
            private string FilterSubject;
            private ReplayPolicy ReplayPolicy;
            private string SampleFrequency;
            private long? RateLimit;
            private string DeliverSubject;
            private string DeliverGroup;
            private long? MaxAckPending;
            private long? Heartbeat;
            private bool? FlowControl;

            public ConsumerConfigBuilder() { }

            public ConsumerConfigBuilder(ConsumerConfig consumerConfig)
            {
                this.DurableName = consumerConfig.DurableName;
                this.DeliverPolicy = consumerConfig.DeliverPolicy;
                this.StartSequence = consumerConfig.StartSequence;
                this.StartTime = consumerConfig.StartTime;
                this.AckPolicy = consumerConfig.AckPolicy;
                this.AckWait = consumerConfig.AckWait;
                this.MaxDeliver = consumerConfig.MaxDeliver;
                this.FilterSubject = consumerConfig.FilterSubject;
                this.ReplayPolicy = consumerConfig.ReplayPolicy;
                this.SampleFrequency = consumerConfig.SampleFrequency;
                this.RateLimit = consumerConfig.RateLimit;
                this.DeliverSubject = consumerConfig.DeliverSubject;
                this.DeliverGroup = consumerConfig.DeliverGroup;
                this.MaxAckPending = consumerConfig.MaxAckPending;
                this.Heartbeat = consumerConfig.Heartbeat;
                this.FlowControl = consumerConfig.FlowControl;
            }

            /**
             * Sets the name of the durable subscription.
             * @param durable name of the durable subscription.
             * @return the builder
             */
            public ConsumerConfigBuilder SetDurable(string durable)
            {
                this.DurableName = durable;
                return this;
            }

            /**
             * Get the name of the durable subscription.
             * @param durable name of the durable subscription.
             * @return the builder
             */
            public string GetDurable()
            {
                return this.DurableName;
            }

            /**
             * Sets the delivery policy of the ConsumerConfiguration.
             * @param policy the delivery policy.
             * @return Builder
             */
            public ConsumerConfigBuilder SetDeliverPolicy(DeliverPolicy policy)
            {
                this.DeliverPolicy = policy;
                return this;
            }


            /**
             * Sets the delivery policy of the ConsumerConfiguration.
             * @param policy the delivery policy.
             * @return Builder
             */
            public ConsumerConfigBuilder SetDeliverGroup(string deliverGroup)
            {
                this.DeliverGroup = deliverGroup;
                return this;
            }

            /**
             * Sets the subject to deliver messages to.
             * @param subject the delivery subject.
             * @return the builder
             */
            public ConsumerConfigBuilder SetDeliverSubject(string subject)
            {
                this.DeliverSubject = subject;
                return this;
            }

            /**
             * Sets the start sequence of the ConsumerConfiguration.
             * @param sequence the start sequence
             * @return Builder
             */
            public ConsumerConfigBuilder SetStartSequence(long sequence)
            {
                this.StartSequence = sequence;
                return this;
            }

            /**
             * Sets the start time of the ConsumerConfiguration.
             * @param startTime the start time
             * @return Builder
             */
            public ConsumerConfigBuilder SetStartTime(DateTime startTime)
            {
                this.StartTime = startTime;
                return this;
            }

            /**
             * Sets the acknowledgement policy of the ConsumerConfiguration.
             * @param policy the acknowledgement policy.
             * @return Builder
             */
            public ConsumerConfigBuilder SetAckPolicy(AckPolicy policy)
            {
                this.AckPolicy = policy;
                return this;
            }

            /**
             * Sets the acknowledgement wait duration of the ConsumerConfiguration.
             * @param timeout the wait timeout
             * @return Builder
             */
            public ConsumerConfigBuilder SetAckWait(TimeSpan timeout)
            {
                this.AckWait = NATSJetStreamDuration.OfSeconds(((long)timeout.TotalSeconds)).Nanos;
                return this;
            }

            /**
             * Sets the maximum delivery amount of the ConsumerConfiguration.
             * @param maxDeliver the maximum delivery amount
             * @return Builder
             */
            public ConsumerConfigBuilder SetMaxDeliver(long maxDeliver)
            {
                this.MaxDeliver = maxDeliver;
                return this;
            }

            /**
             * Sets the filter subject of the ConsumerConfiguration.
             * @param filterSubject the filter subject
             * @return Builder
             */
            public ConsumerConfigBuilder SetFilterSubject(string filterSubject)
            {
                this.FilterSubject = filterSubject;
                return this;
            }

            /**
             * Sets the replay policy of the ConsumerConfiguration.
             * @param policy the replay policy.
             * @return Builder
             */
            public ConsumerConfigBuilder SetReplayPolicy(ReplayPolicy policy)
            {
                this.ReplayPolicy = policy;
                return this;
            }

            /**
             * Sets the sample frequency of the ConsumerConfiguration.
             * @param frequency the frequency
             * @return Builder
             */
            public ConsumerConfigBuilder SetSampleFrequency(string frequency)
            {
                this.SampleFrequency = frequency;
                return this;
            }

            /**
             * Set the rate limit of the ConsumerConfiguration.
             * @param msgsPerSecond messages per second to deliver
             * @return Builder
             */
            public ConsumerConfigBuilder SetRateLimit(int msgsPerSecond)
            {
                this.RateLimit = msgsPerSecond;
                return this;
            }

            /**
             * Sets the maximum ack pending.
             * @param maxAckPending maximum pending acknowledgements.
             * @return Builder
             */
            public ConsumerConfigBuilder SetMaxAckPending(long maxAckPending)
            {
                this.MaxAckPending = maxAckPending;
                return this;
            }

            /**
             * sets the idle heart beat wait time
             * @param idleHeartbeat the idle heart beat duration
             * @return Builder
             */
            public ConsumerConfigBuilder SetHeartbeat(TimeSpan heartbeat)
            {
                this.Heartbeat = NATSJetStreamDuration.OfSeconds(((long)heartbeat.TotalSeconds)).Nanos;
                return this;
            }

            /**
             * set the flow control mode
             * @param flowControl the flow control mode flag
             * @return Builder
             */
            public ConsumerConfigBuilder SetFlowControl(bool flowControl)
            {
                this.FlowControl = flowControl;
                return this;
            }

            /**
             * Builds the ConsumerConfiguration
             * @return a consumer configuration.
             */
            public ConsumerConfig Build()
            {

                return new ConsumerConfig(
                        DurableName,
                        DeliverPolicy,
                        StartSequence,
                        StartTime,
                        AckPolicy,
                        AckWait,
                        MaxDeliver,
                        FilterSubject,
                        ReplayPolicy,
                        SampleFrequency,
                        RateLimit,
                        DeliverSubject,
                        DeliverGroup,
                        MaxAckPending,
                        Heartbeat,
                        FlowControl
                );
            }
        }

        public override string ToString()
        {
            return "ConsumerConfiguration{" +
                    "durable='" + DurableName + '\'' +
                    ", deliverPolicy=" + DeliverPolicy +
                    ", deliverSubject='" + DeliverSubject + '\'' +
                    ", deliverGroup='" + DeliverGroup + '\'' +
                    ", startSeq=" + StartSequence +
                    ", startTime=" + StartTime +
                    ", ackPolicy=" + AckPolicy +
                    ", ackWait=" + AckWait +
                    ", maxDeliver=" + MaxDeliver +
                    ", filterSubject='" + FilterSubject + '\'' +
                    ", replayPolicy=" + ReplayPolicy +
                    ", sampleFrequency='" + SampleFrequency + '\'' +
                    ", rateLimit=" + RateLimit +
                    ", maxAckPending=" + MaxAckPending +
                    ", idleHeartbeat=" + Heartbeat +
                    ", flowControl=" + FlowControl +
                    '}';
        }
    }
}
