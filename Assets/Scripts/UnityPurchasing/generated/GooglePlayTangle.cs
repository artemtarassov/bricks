// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("dsRHZHZLQE9swA7AsUtHR0dDRkVPhAob4eafnY8abpyaXbs+zSrHlPQ2xJFd+1+6ZunmRbReThcy9A7SxEdJRnbER0xExEdHRoyWqSu+TldAOKxj/WrerUl26sgswU27Vw6E6K2e7e3yufZ6TAHlrDy6WYBvBLMs+wICk0U2L9Vyf0T1WquQag3LjchYKxYC/W6m0uvRIilTdOGJpzALmqydNku3ZskS+aOIVpU+gQLlFEucFHYc3NKR4AI4yKx2hwF3k2N0SUmOCgHXquKry+CutOFqUjZoLtv38SF0EqCnjFT3vxT/6dAzWeQcDDh76sSeSoFF40h8pbAQDcFaKdJnRsFERcQV32PHtNekUYw61SmfIQxW2VopyjAXH9PXsURFR0ZH");
        private static int[] order = new int[] { 0,9,6,9,5,11,11,11,11,12,11,12,13,13,14 };
        private static int key = 70;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
