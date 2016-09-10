using System;
using System.IO;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Nest;
using Newtonsoft.Json;

namespace Elasticsearch.ESClient
{
    internal class AWSCredentials
    {
        public string Key { get; set; }
        public string Secret { get; set; }
        public string Region { get; set; }
    }

    public class Client
    {
        private ElasticClient _client;

        public Client( )
        {
            AWSCredentials credentials;
            string json = File.ReadAllText( string.Format( @"{0}aws.config.json", AppDomain.CurrentDomain.BaseDirectory ) );

            credentials = JsonConvert.DeserializeObject< AWSCredentials >( json );
                
            if (credentials == null) 
                throw new Exception("Whoops...");

            var httpConnection = new AwsHttpConnection(new AwsSettings
            {
                AccessKey = credentials.Key,
                SecretKey = credentials.Secret,
                Region = credentials.Region
            });


            // create uri and connect to the service
            var pool = new SingleNodeConnectionPool( new Uri( "http://search-slayingdragons-rjkkghkxvgnzkzzcm6b4savkme.us-west-2.es.amazonaws.com" ) );
            var config = new ConnectionSettings( pool, httpConnection );
            _client = new ElasticClient( config );
        }
    }
}