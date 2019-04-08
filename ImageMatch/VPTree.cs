using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace howto_image_hash
{
    public partial class VPTree<T>
    {

        public delegate int dist_func(T key1, T key2);

        private dist_func _distFunc;

        public VPTree(dist_func func)
        {
            _distFunc = func;
        }

        private const int MAX_DISTANCE = 2 * 64;
        private const int max_linear = 100;

        public VPNode<T> make_vp(List<T> keys)
        {
            int n = keys.Count;

            VPNode<T> root = new VPNode<T>();
            if (n <= max_linear || n <= 1)
            {
                root.linear = true;
                root.count = (uint)n;
                root.keys = new List<T>();
                root.keys.AddRange(keys);
                return root;
            }

            T rootkey = keys[0];
            root.linear = false;
            root.threshold = 0;
            root.vantage = rootkey;

            // count keys inside the given ball
            int [] dcnt = new int[MAX_DISTANCE + 1];
            for (int i = 0; i <= MAX_DISTANCE; i++)
                dcnt[i] = 0;
            for (int i = 0; i < n; i++)
            {
                int dist = _distFunc(rootkey, keys[i]);
                dcnt[dist]++;
            }
            int a = 0;
            for (int i = 0; i <= MAX_DISTANCE; i++)
            {
                a += dcnt[i];
                dcnt[i] = a;
            }
            Debug.Assert(a == n);

            int median = dcnt[0] + (n - dcnt[0]) / 2;
            int k = 1;
            for (; k <= MAX_DISTANCE; k++)
                if (dcnt[k] > median)
                    break;
            if (k != 1 && ((median - dcnt[k - 1]) <= (dcnt[k] - median)))
                k--;
            int nnear = dcnt[k] - dcnt[0];
            int nfar = n - dcnt[k];

            // Sort keys into near and far sets
            List<T> nearKeys = new List<T>();
            List<T> farKeys = new List<T>();
            for (int i = 0; i < n; i++)
            {
                if (keys[i].Equals(rootkey))
                    continue;
                if (_distFunc(rootkey, keys[i]) <= k)
                    nearKeys.Add(keys[i]);
                else
                    farKeys.Add(keys[i]);
            }

            Debug.Assert(nearKeys.Count == nnear);
            Debug.Assert(farKeys.Count == nfar);

            root.threshold = k;
            if (nnear > 0)
                root.near = make_vp(nearKeys);
            if (nfar > 0)
                root.far = make_vp(farKeys);

            return root;
        }

        public void query_vp(VPNode<T> root, T who, int maxd, List<T> ret)
        {
            if (root.linear)
            {
                for (int i = 0; i < root.count; i++)
                    if (_distFunc(who, root.keys[i]) <= maxd)
                        ret.Add(root.keys[i]);
                return;
            }

            int d = _distFunc(root.vantage, who);
            int thr = root.threshold;
            if (d <= (maxd + thr))
            {
                if (root.near != null)
                    query_vp(root.near, who, maxd, ret);
                if (d < maxd)
                    ret.Add(root.vantage);
            }
            if ((d + maxd) > thr && root.far != null)
                query_vp(root.far, who, maxd, ret);
        }
    }
}
