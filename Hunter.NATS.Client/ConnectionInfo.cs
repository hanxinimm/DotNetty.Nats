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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Hunter.NATS.Client
{

    
    [DataContract]
    internal class ConnectionInfo
    {
        bool isVerbose;
        bool isPedantic;
        string clientUser;
        string clientPass;
        bool sslRequired;
        string clientName;
        string clientLang = DefaultsOptions.LangString;
        string clientVersion = DefaultsOptions.Version;
        int protocolVersion = (int)ClientProtcolVersion.ClientProtoInfo;
        string authToken = null;

        [DataMember]
        public bool verbose
        {
            get { return isVerbose; }
            set { isVerbose = value; }
        }

        [DataMember]
        public bool pedantic
        {
            get { return isPedantic; }
            set { isPedantic = value; }
        }

        [DataMember]
        public string user
        {
            get { return clientUser; }
            set { clientUser = value; }
        }

        [DataMember]
        public string pass
        {
            get { return clientPass; }
            set { clientPass = value; }
        }

        [DataMember]
        public bool ssl_required
        {
            get { return sslRequired; }
            set { sslRequired = value; }
        }

        [DataMember]
        public string name
        {
            get { return clientName; }
            set { clientName = value; }
        }

        [DataMember]
        public string auth_token
        {
            get { return authToken; }
            set { authToken = value; }
        }

        [DataMember]
        public string lang
        {
            get { return clientLang; }
            set { clientLang = value; }
        }

        [DataMember]
        public string version
        {
            get { return clientVersion; }
            set { clientVersion = value; }
        }

        [DataMember]
        public int protocol
        {
            get { return protocolVersion; }
            set { protocolVersion = value; }
        }

        internal ConnectionInfo(bool verbose, bool pedantic, string user, string pass,
            string token, bool secure, string name)
        {
            isVerbose = verbose;
            isPedantic = pedantic;
            clientUser = user;
            clientPass = pass;
            sslRequired = secure;
            clientName = name;
            authToken = token;
        }

        internal string ToJson()
        {
            var serializer = new DataContractJsonSerializer(typeof(ConnectionInfo));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, this);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
