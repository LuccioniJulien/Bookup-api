using System;

namespace BaseApi.Helper {
    public class AmazonCreditential {
        private string _id = Environment.GetEnvironmentVariable ("AMAZON_ID");
        private string _key = Environment.GetEnvironmentVariable ("AMAZON_KEY");
        public void Deconstruct (out string id, out string key) {
            id = _id;
            key = _key;
        }
    }
}