namespace HashtagMeta.Test.Models {
    internal class SignatureTestModel {
        public string Comment { get; set; }
        public string MessageBase64 { get; set; }
        public string Algorithm { get; set; }
        public string DidDocSuite { get; set; }
        public string PublicKeyDid { get; set; }
        public string PublicKeyMultibase { get; set; }
        public string SignatureBase64 { get; set; }
        public bool ValidSignature { get; set; }
        public List<string> Tags { get; set; }
    }
}
