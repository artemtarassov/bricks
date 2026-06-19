// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("qvLpYD82xhLYOxPYZXq1wWynC7sLfF+STTGvqbESBuo59pm1hbk6WQF0wx5OydjiSXZ9lbUCt/vwSw/26bFLaVcv5W1cUPoso9c/9B00phkWRQRpMT1+TTWNJn5+v86WmAEHfhBPD3fiDOGSFnJhULIdkMmFIKa/FackBxUoIywPo22j0igkJCQgJSa2DM7lSAWwZPclXGKkc3zkXumxBy5ZoxqYMuV/ZyoJ99dPDW6f9eQq3qyNPDij8QxZytbHUetgWPlpO0G1WsMgV+8uWXlLaOhNxMQJu0fstr48OCSsZDvvmPGdbrLGsrjHlHXhpyQqJRWnJC8npyQkJeFlWKZAG3BaDd0Yul2Z6zyBnsixPXhrmjbfCU72OSsVQsPkmCcmJCUk");
        private static int[] order = new int[] { 2,12,11,6,7,7,11,9,8,9,10,13,12,13,14 };
        private static int key = 37;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
