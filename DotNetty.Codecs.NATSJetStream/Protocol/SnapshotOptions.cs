using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
	public class SnapshotOptions
	{
		public string dir { get; set; }
		public string metaFile { get; set; }
		public string dataFile { get; set; }
		//	scb           func(SnapshotProgress)
		//rcb           func(RestoreProgress)
		public bool debug { get; set; }
		public bool consumers { get; set; }
		public bool jsck { get; set; }
		public int chunkSz { get; set; }
		public JetStreamConfig restoreConfig { get; set; }
	}
}
