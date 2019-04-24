using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;

namespace BaseApi.Helper {
    public class AmazonS3Helper {
        private static readonly string _id = Environment.GetEnvironmentVariable ("AMAZON_ID");
        private static readonly string _key = Environment.GetEnvironmentVariable ("AMAZON_KEY");

        public static async Task<string> SaveImageToBucket (IFormFile file) {
            try {
                using (var client = new AmazonS3Client (_id, _key, RegionEndpoint.EUWest3)) {
                    using (var newMemoryStream = new MemoryStream ()) {
                        var name = Guid.NewGuid ().ToString () + ".png";
                        file.CopyTo (newMemoryStream);
                        var uploadRequest = new TransferUtilityUploadRequest {
                            InputStream = newMemoryStream,
                            Key = name,
                            BucketName = "bookupstorapeapi",
                            CannedACL = S3CannedACL.PublicRead
                        };
                        var fileTransferUtility = new TransferUtility (client);
                        await fileTransferUtility.UploadAsync (uploadRequest);
                        return name;
                    }
                }
            } catch (Exception e) {

                return null;
            }

        }
    }
}