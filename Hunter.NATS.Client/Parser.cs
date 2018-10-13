// Copyright 2015-2018 The NATS Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    

    internal sealed class Parser : IDisposable
    {
        NATSConnection conn;
        NATSOptions _options;

        readonly internal object mu = new Object();

        byte[] argBufBase = new byte[DefaultsOptions.DefaultBufSize];
        MemoryStream argBufStream = null;

        byte[] msgBufBase = new byte[DefaultsOptions.DefaultBufSize];
        MemoryStream msgBufStream = null;

        private ConcurrentDictionary<Int64, ISubscription> _subscriptionDictionary =
            new ConcurrentDictionary<Int64, ISubscription>();
        private ServerPool srvPool = new ServerPool();
        MessageArgs msgArgs = new MessageArgs();

        private Queue<SingleUseChannel<bool>> pongs = new Queue<SingleUseChannel<bool>>();

        CallbackScheduler callbackScheduler = new CallbackScheduler();

        int pout = 0;

        internal Parser(NATSConnection conn,NATSOptions options)
        {
            argBufStream = new MemoryStream(argBufBase);
            msgBufStream = new MemoryStream(msgBufBase);

            this.conn = conn;
        }

        internal int state = 0;

        private const int MAX_CONTROL_LINE_SIZE = 1024;

        // For performance declare these as consts - they'll be
        // baked into the IL code (thus faster).  An enum would
        // be nice, but we want speed in this critical section of
        // message handling.
        private const int OP_START         = 0;
        private const int OP_PLUS          = 1;
        private const int OP_PLUS_O        = 2;
	    private const int OP_PLUS_OK       = 3;
	    private const int OP_MINUS         = 4;
	    private const int OP_MINUS_E       = 5;
	    private const int OP_MINUS_ER      = 6;
	    private const int OP_MINUS_ERR     = 7;
	    private const int OP_MINUS_ERR_SPC = 8;
	    private const int MINUS_ERR_ARG    = 9;
	    private const int OP_C             = 10;
	    private const int OP_CO            = 11;
	    private const int OP_CON           = 12;
	    private const int OP_CONN          = 13;
	    private const int OP_CONNE         = 14;
	    private const int OP_CONNEC        = 15;
	    private const int OP_CONNECT       = 16;
	    private const int CONNECT_ARG      = 17; 
	    private const int OP_M             = 18;
	    private const int OP_MS            = 19;
	    private const int OP_MSG           = 20; 
	    private const int OP_MSG_SPC       = 21;
	    private const int MSG_ARG          = 22; 
	    private const int MSG_PAYLOAD      = 23;
	    private const int MSG_END          = 24;
	    private const int OP_P             = 25;
	    private const int OP_PI            = 26;
	    private const int OP_PIN           = 27;
	    private const int OP_PING          = 28;
	    private const int OP_PO            = 29;
	    private const int OP_PON           = 30;
	    private const int OP_PONG          = 31;
        private const int OP_I             = 32;
        private const int OP_IN            = 33;
        private const int OP_INF           = 34;
        private const int OP_INFO          = 35;
        private const int OP_INFO_SPC      = 36;
        private const int INFO_ARG         = 37;

        private void parseError(byte[] buffer, int position)
        {
            throw new NATSException(string.Format("Parse Error [{0}], {1}", state, buffer));
        }

        internal void parse(byte[] buffer, int len)
        {
            int i;
            char b;

            for (i = 0; i < len; i++)
            {
                b = (char)buffer[i];

                switch (state)
                {
                    case OP_START:
                        switch (b)
                        {
                            case 'M':
                            case 'm':
                                state = OP_M;
                                break;
                            case 'C':
                            case 'c':
                                state = OP_C;
                                break;
                            case 'P':
                            case 'p':
                                state = OP_P;
                                break;
                            case '+':
                                state = OP_PLUS;
                                break;
                            case '-':
                                state = OP_MINUS;
                                break;
                            case 'i':
                            case 'I':
                                state = OP_I;
                                break;
                            default:
                                parseError(buffer,i);
                                break;
                        }
                        break;
                    case OP_M:
                        switch (b)
                        {
                            case 'S':
                            case 's':
                                state = OP_MS;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_MS:
                        switch (b)
                        {
                            case 'G':
                            case 'g':
                                state = OP_MSG;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_MSG:
                        switch (b)
                        {
                            case ' ':
                            case '\t':
                                state = OP_MSG_SPC;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_MSG_SPC:
                        switch (b)
                        {
                            case ' ':
                                break;
                            case '\t':
                                break;
                            default:
                                state = MSG_ARG;
                                i--;
                                break;
                        }
                        break;
                    case MSG_ARG:
                        switch (b)
                        {
                            case '\r':
                                break;
                            case '\n':
                                processMsgArgs(argBufBase, argBufStream.Position);
                                argBufStream.Position = 0;
                                if (msgArgs.Size > msgBufBase.Length)
                                {
                                    // Add 2 to account for the \r\n
                                    msgBufBase = new byte[msgArgs.Size + 2];
                                    msgBufStream = new MemoryStream(msgBufBase);
                                }
                                state = MSG_PAYLOAD;
                                break;
                            default:
                                argBufStream.WriteByte((byte)b);
                                break;
                        }
                        break;
                    case MSG_PAYLOAD:
                        int  msgSize  = msgArgs.Size;
                        if (msgSize == 0)
                        {
                            processMsg(msgBufBase, msgSize);
                            state = MSG_END;
                        }
                        else
                        {
                            long position = msgBufStream.Position;
                            int writeLen = msgSize - (int)position;
                            int avail = len - i;

                            if (avail < writeLen)
                            {
                                writeLen = avail;
                            }

                            msgBufStream.Write(buffer, i, writeLen);
                            i += (writeLen - 1);

                            if ((position + writeLen) >= msgSize)
                            {
                                processMsg(msgBufBase, msgSize);
                                msgBufStream.Position = 0;
                                state = MSG_END;
                            }
                        }
                        break;
                    case MSG_END:
                        switch (b)
                        {
                            case '\n':
                                state = OP_START;
                                break;
                            default:
                                continue;
                        }
                        break;
                    case OP_PLUS:
                        switch (b)
                        {
                            case 'O':
                            case 'o':
                                state = OP_PLUS_O;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_PLUS_O:
                        switch (b)
                        {
                            case 'K':
                            case 'k':
                                state = OP_PLUS_OK;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_PLUS_OK:
                        switch (b)
                        {
                            case '\n':
                                processOK();
                                state = OP_START;
                                break;
                        }
                        break;
                    case OP_MINUS:
                        switch (b)
                        {
                            case 'E':
                            case 'e':
                                state = OP_MINUS_E;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_MINUS_E:
                        switch (b)
                        {
                            case 'R':
                            case 'r':
                                state = OP_MINUS_ER;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_MINUS_ER:
                        switch (b)
                        {
                            case 'R':
                            case 'r':
                                state = OP_MINUS_ERR;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_MINUS_ERR:
                        switch (b)
                        {
                            case ' ':
                            case '\t':
                                state = OP_MINUS_ERR_SPC;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_MINUS_ERR_SPC:
                        switch (b)
                        {
                            case ' ':
                            case '\t':
                                state = OP_MINUS_ERR_SPC;
                                break;
                            default:
                                state = MINUS_ERR_ARG;
                                i--;
                                break;
                        }
                        break;
                    case MINUS_ERR_ARG:
                        switch (b)
                        {
                            case '\r':
                                break;
                            case '\n':
                                processErr(argBufStream);
                                argBufStream.Position = 0;
                                state = OP_START;
                                break;
                            default:
                                argBufStream.WriteByte((byte)b);
                                break;
                        }
                        break;
                    case OP_P:
                        switch (b)
                        {
                            case 'I':
                            case 'i':
                                state = OP_PI;
                                break;
                            case 'O':
                            case 'o':
                                state = OP_PO;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_PO:
                        switch (b)
                        {
                            case 'N':
                            case 'n':
                                state = OP_PON;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_PON:
                        switch (b)
                        {
                            case 'G':
                            case 'g':
                                state = OP_PONG;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_PONG:
                        switch (b)
                        {
                            case '\r':
                                break;
                            case '\n':
                                processPong();
                                state = OP_START;
                                break;
                        }
                        break;
                    case OP_PI:
                        switch (b)
                        {
                            case 'N':
                            case 'n':
                                state = OP_PIN;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_PIN:
                        switch (b)
                        {
                            case 'G':
                            case 'g':
                                state = OP_PING;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_PING:
                        switch (b)
                        {
                            case '\r':
                                break;
                            case '\n':
                                processPing();
                                state = OP_START;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_I:
                        switch (b)
                        {
                            case 'N':
                            case 'n':
                                state = OP_IN;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_IN:
                        switch (b)
                        {
                            case 'F':
                            case 'f':
                                state = OP_INF;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_INF:
                        switch (b)
                        {
                            case 'O':
                            case 'o':
                                state = OP_INFO;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_INFO:
                        switch (b)
                        {
                            case ' ':
                            case '\t':
                                state = OP_INFO_SPC;
                                break;
                            default:
                                parseError(buffer, i);
                                break;
                        }
                        break;
                    case OP_INFO_SPC:
                        switch (b)
                        {
                            case ' ':
                            case '\t':
                                break;
                            default:
                                argBufStream.Position = 0;
                                state = INFO_ARG;
                                i--;
                                break;
                        }
                        break;
                    case INFO_ARG:
                        switch (b)
                        {
                            case '\r':
                                break;
                            case '\n':
                                processAsyncInfo(argBufBase, (int)argBufStream.Position);
								argBufStream.Position = 0;
                                state = OP_START;
                                break;
                            default:
                                argBufStream.WriteByte((byte)b);
                                break;
                        }
                        break;
                    default:
                        throw new NATSException("Unable to parse.");
                } // switch(state)

            }  // for
     
        } // parse


        // processOK is a placeholder for processing OK messages.
        internal void processOK()
        {
            // NOOP;
            return;
        }

        // processPing will send an immediate pong protocol response to the
        // server. The server uses this mechanism to detect dead clients.
        internal void processPing()
        {
            sendProto(PublishProtocol.PONG_P_BYTES, PublishProtocol.PONG_P_BYTES_LEN);
        }

        private void sendProto(byte[] value, int length)
        {
            lock (mu)
            {
                //重新发送指令
                //分离协议发送
                //bw.Write(value, 0, length);
            }
        }

        // processInfo is used to parse the info messages sent
        // from the server.
        // Caller must lock.
        internal void processInfo(string json, bool notifyOnServerAddition)
        {
            if (json == null || NATSConstants._EMPTY_.Equals(json))
            {
                return;
            }

            var info = ServerInfo.CreateFromJson(json);
            var servers = info.ClusterRoutes;
            if (servers != null)
            {
                if (!_options.NoRandomize && servers.Length > 1)
                {
                    // If randomization is allowed, shuffle the received array, 
                    // not the entire pool. We want to preserve the pool's
                    // order up to this point (this would otherwise be 
                    // problematic for the (re)connect loop).
                    servers = (string[])info.ClusterRoutes.Clone();
                    ServerPool.shuffle<string>(servers);
                }

                var serverAdded = srvPool.Add(servers, true);
                if (notifyOnServerAddition && serverAdded)
                {
                    scheduleConnEvent(_options.ServerDiscoveredEventHandler);
                }
            }
        }

        // Schedules a connection event (connected/disconnected/reconnected)
        // if non-null.
        // Caller must lock.
        private void scheduleConnEvent(EventHandler<ConnectionEventArgs> connEvent)
        {
            if (connEvent == null)
                return;

            // Schedule a reference to the event handler.
            EventHandler<ConnectionEventArgs> eh = connEvent;
            callbackScheduler.Add(
                new Task(() => { eh(this, new ConnectionEventArgs(conn)); })
            );

        }

        internal void processAsyncInfo(byte[] jsonBytes, int length)
        {
            lock (mu)
            {
                processInfo(Encoding.UTF8.GetString(jsonBytes, 0, length), true);
            }
        }

        // processPong is used to process responses to the client's ping
        // messages. We use pings for the flush mechanism as well.
        internal void processPong()
        {
            SingleUseChannel<bool> ch = null;
            lock (mu)
            {
                if (pongs.Count > 0)
                    ch = pongs.Dequeue();

                pout = 0;
            }

            if (ch != null)
            {
                ch.add(true);
            }
        }

        private Queue<SingleUseChannel<bool>> createPongs()
        {
            return new Queue<SingleUseChannel<bool>>();
        }

        // processErr processes any error messages from the server and
        // sets the connection's lastError.
        internal void processErr(MemoryStream errorStream)
        {
            bool invokeDelegates = false;
            Exception ex = null;

            string s = getNormalizedError(errorStream);

            if (NATSConstants.STALE_CONNECTION.Equals(s))
            {
                processOpError(new NATSStaleConnectionException());
            }
            else if (NATSConstants.AUTH_TIMEOUT.Equals(s))
            {
                // Protect against a timing issue where an authoriztion error
                // is handled before the connection close from the server.
                // This can happen in reconnect scenarios.
                processOpError(new NATSConnectionException(NATSConstants.AUTH_TIMEOUT));
            }
            else
            {
                ex = new NATSException("Error from processErr(): " + s);
               
                //处理异常并且关闭链接

                //lock (mu)
                //{
                //    lastEx = ex;

                //    if (status != ConnectionState.CONNECTING)
                //    {
                //        invokeDelegates = true;
                //    }
                //}

                //close(ConnectionState.CLOSED, invokeDelegates);
            }
        }

        // getNormalizedError extracts a string from a MemoryStream, then normalizes it
        // by removing leading and trailing spaces and quotes, and converting to lowercase
        private string getNormalizedError(MemoryStream errorStream)
        {
            string s = Encoding.UTF8.GetString(errorStream.ToArray(), 0, (int)errorStream.Position);
            return s.Trim('\'', '"', ' ', '\t', '\r', '\n').ToLower();
        }

        private void processOpError(Exception e)
        {
            bool disconnected = false;

            //lock (mu)
            //{
                  //处于三个状态不处理错误
            //    if (isConnecting() || isClosed() || isReconnecting())
            //    {
            //        return;
            //    }

                  //允许重新连接并且状态等于已经连接尝试重新连接
            //    if (_options.AllowReconnect && status == ConnectionState.Connected)
            //    {
            //        processReconnect();
            //    }
            //    else
            //    {
                   // 断开连接释放资源
            //        processDisconnect();
            //        disconnected = true;
            //        lastEx = e;
            //    }
            //}

            //if (disconnected)
            //{
            //    Close();
            //}
        }

        // Roll our own fast conversion - we know it's the right
        // encoding. 
        char[] convertToStrBuf = new char[DefaultsOptions.ScratchSize];

        private int[] argEnds = new int[4];

        // Caller must ensure thread safety.
        private string convertToString(byte[] buffer, long length)
        {
            // expand if necessary
            if (length > convertToStrBuf.Length)
            {
                convertToStrBuf = new char[length];
            }

            for (int i = 0; i < length; i++)
            {
                convertToStrBuf[i] = (char)buffer[i];
            }

            // This is the copy operation for msg arg strings.
            return new string(convertToStrBuf, 0, (int)length);
        }

        // processMsg is called by parse and will place the msg on the
        // appropriate channel for processing. All subscribers have their
        // their own channel. If the channel is full, the connection is
        // considered a slow subscriber.
        internal void processMsg(byte[] msg, long length)
        {
            bool maxReached = false;
            ISubscription s;

            lock (mu)
            {
                //TODO:消息字节流统计计数
                //stats.inMsgs++;
                //stats.inBytes += length;

                // In regular message processing, the key should be present,
                // so optimize by using an an exception to handle a missing key.
                // (as opposed to checking with Contains or TryGetValue)
                try
                {
                    s = _subscriptionDictionary[msgArgs.SubscriptionId];
                }
                catch (Exception)
                {
                    // this can happen when a subscriber is unsubscribing.
                    return;
                }

                //获取判断统计消息是否达到上限
                //lock (s.mu)
                //{
                //    maxReached = s.tallyMessage(length);
                //    if (maxReached == false)
                //    {
                //        s.addMessage(new Message(msgArgs, s, msg, length), _options.subChanLen);
                //    } // maxreached == false

                //} // lock s.mu

            } // lock mu

            if (maxReached)
                removeSub(s);
        }

        internal void removeSub(ISubscription s)
        {
            ISubscription o;

            _subscriptionDictionary.TryRemove(s.Id, out o);

            //TODO:分析移除队列后所做的操作
            //if (s.mch != null)
            //{
            //    if (s.ownsChannel)
            //        s.mch.close();

            //    s.mch = null;
            //}

            //s.conn = null;
            //s.closed = true;
        }


        // Here we go ahead and convert the message args into
        // strings, numbers, etc.  The msgArg object is a temporary
        // place to hold them, until we create the message.
        //
        // These strings, once created, are never copied.
        internal void processMsgArgs(byte[] buffer, long length)
        {



            int argCount = setMsgArgsAryOffsets(buffer, length);

            switch (argCount)
            {
                case 3:
                    msgArgs.subject = new string(convertToStrBuf, 0, argEnds[0]);
                    msgArgs.SubscriptionId = ToInt64(buffer, argEnds[0] + 1, argEnds[1]);
                    msgArgs.reply = null;
                    msgArgs.Size = (int)ToInt64(buffer, argEnds[1] + 1, argEnds[2]);
                    break;
                case 4:
                    msgArgs.subject = new string(convertToStrBuf, 0, argEnds[0]);
                    msgArgs.SubscriptionId = ToInt64(buffer, argEnds[0] + 1, argEnds[1]);
                    msgArgs.reply = new string(convertToStrBuf, argEnds[1] + 1, argEnds[2] - argEnds[1] - 1);
                    msgArgs.Size = (int)ToInt64(buffer, argEnds[2] + 1, argEnds[3]);
                    break;
                default:
                    throw new NATSException("Unable to parse message arguments: " + Encoding.UTF8.GetString(buffer, 0, (int)length));
            }

            if (msgArgs.Size < 0)
            {
                throw new NATSException("Invalid Message - Bad or Missing Size: " + Encoding.UTF8.GetString(buffer, 0, (int)length));
            }
            if (msgArgs.SubscriptionId < 0)
            {
                throw new NATSException("Invalid Message - Bad or Missing Sid: " + Encoding.UTF8.GetString(buffer, 0, (int)length));
            }
        }

        private int setMsgArgsAryOffsets(byte[] buffer, long length)
        {
            if (convertToStrBuf.Length < length)
            {
                convertToStrBuf = new char[length];
            }

            int count = 0;
            int i = 0;

            // We only support 4 elements in this protocol version
            for (; i < length && count < 4; i++)
            {
                convertToStrBuf[i] = (char)buffer[i];
                if (buffer[i] == ' ')
                {
                    argEnds[count] = i;
                    count++;
                }
            }

            argEnds[count] = i;
            count++;

            return count;
        }

        static long ToInt64(byte[] buffer, int start, int end)
        {
            int length = end - start;
            switch (length)
            {
                case 0:
                    return 0;
                case 1:
                    return buffer[start] - '0';
                case 2:
                    return 10 * (buffer[start] - '0')
                         + (buffer[start + 1] - '0');
                case 3:
                    return 100 * (buffer[start] - '0')
                         + 10 * (buffer[start + 1] - '0')
                         + (buffer[start + 2] - '0');
                case 4:
                    return 1000 * (buffer[start] - '0')
                         + 100 * (buffer[start + 1] - '0')
                         + 10 * (buffer[start + 2] - '0')
                         + (buffer[start + 3] - '0');
                case 5:
                    return 10000 * (buffer[start] - '0')
                         + 1000 * (buffer[start + 1] - '0')
                         + 100 * (buffer[start + 2] - '0')
                         + 10 * (buffer[start + 3] - '0')
                         + (buffer[start + 4] - '0');
                case 6:
                    return 100000 * (buffer[start] - '0')
                         + 10000 * (buffer[start + 1] - '0')
                         + 1000 * (buffer[start + 2] - '0')
                         + 100 * (buffer[start + 3] - '0')
                         + 10 * (buffer[start + 4] - '0')
                         + (buffer[start + 5] - '0');
                default:
                    if (length < 0)
                        throw new ArgumentOutOfRangeException("end");
                    break;
            }

            long value = 0L;

            int i = start;
            while (i < end)
            {
                value *= 10L;
                value += (buffer[i++] - '0');
            }

            return value;
        }

        public void Dispose()
        {
            argBufStream.Dispose();
            msgBufStream.Dispose();
        }
    }
}