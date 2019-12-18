using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Labels
    {
        List<int> labels = new List<int>();
        
        public int New()
        {
            labels.Add(-1);
            return labels.Count - 1;
        }
        
        public void Set(int label, int offset)
        {
            labels[label] = offset;
        }
 
        public int Offset(int label)
        {
            return labels[label];
        }
    }
}