using Unity.Netcode;
using Unity.Collections;
using System;

namespace TheButton.Network
{
    /// <summary>
    /// Fixed-size string struct for NetworkList compatibility
    /// Uses Unity's FixedString64Bytes (no unsafe code needed!)
    /// Max 61 UTF-8 bytes
    /// </summary>
    public struct NetworkString : INetworkSerializable, IEquatable<NetworkString>
    {
        private FixedString64Bytes value;

        public NetworkString(string str = "")
        {
            value = new FixedString64Bytes();
            if (!string.IsNullOrEmpty(str))
            {
                value = str;  // Implicit conversion
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref value);
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public bool Equals(NetworkString other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkString other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public static implicit operator string(NetworkString ns) => ns.ToString();
        public static implicit operator NetworkString(string s) => new NetworkString(s);
        
        public static bool operator ==(NetworkString a, NetworkString b) => a.Equals(b);
        public static bool operator !=(NetworkString a, NetworkString b) => !a.Equals(b);
    }
}

