using System.Collections.Generic;

namespace howto_image_hash
{
    public class VPNode<T>
    {
        public bool linear;

        public int threshold;
        public T vantage;
        public VPNode<T> near;
        public VPNode<T> far;

        public uint count;
        public List<T> keys;
    }
}
