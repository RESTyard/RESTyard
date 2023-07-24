using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using Benchmarking.Hcos;
using RESTyard.Client;
using RESTyard.Client.Extensions.NewtonsoftJson;
using RESTyard.Client.Extensions.SystemTextJson;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Reader;

namespace Benchmarking
{
    [MemoryDiagnoser()]
    [HtmlExporter]
    public class StringReaderPlusExportAsString
    {
        private string JsonString;
        private byte[] JsonBuffer;
        private Stream asMemoryStream;
        private HttpContent httpContent;
        private IHypermediaReader sirenReader;

        [ParamsSource(nameof(StringParsers))]
        public IStringParser Parser { get; set; }

        public StringReaderPlusExportAsString()
        {
        }

        private class Config : ManualConfig
        {
            public Config()
            {
                this.AddColumnProvider(new ParserColumnProvider());
            }

            private class ParserColumnProvider : IColumnProvider
            {
                public IEnumerable<IColumn> GetColumns(Summary summary)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public static IEnumerable<IStringParser> StringParsers() => new IStringParser[]
        {
            new NewtonsoftJsonStringParser(),
            new SystemTextJsonStringParser(),
        };

        [GlobalSetup]
        public void GlobalSetup()
        {
            this.JsonString = BenchmarkPayload.GetJsonPayload();
            this.JsonBuffer = System.Text.Encoding.UTF8.GetBytes(this.JsonString);
        }

        [IterationSetup]
        public void Setup()
        {
            this.asMemoryStream = new MemoryStream();
            this.asMemoryStream.Write(this.JsonBuffer, 0, this.JsonBuffer.Length);
            this.asMemoryStream.Seek(0, SeekOrigin.Begin);
            this.httpContent = new HttpContentMock(this.asMemoryStream);
            var register = new HypermediaObjectRegister();
            register.RegisterAllClassesDeriving<HypermediaClientObject>(typeof(QuarterlyReportingHco).Assembly);
            //register.Register
            this.sirenReader = new SirenHypermediaReader(register, this.Parser);
        }

        [Benchmark]
        public async Task ParseFromStringAsync()
        {
            var asString = await this.httpContent.ReadAsStringAsync();
            var result = this.sirenReader.Read(asString, null);
        }

        [Benchmark]
        public async Task ParseFromStreamAsync()
        {
            var asStream = await this.httpContent.ReadAsStreamAsync();
            var result = await this.sirenReader.ReadAsync(asStream, null);
        }

        [Benchmark]
        public async Task ParseFromStreamAndExportAsync()
        {
            var asStream = await this.httpContent.ReadAsStreamAsync();
            var result = await this.sirenReader.ReadAndSerializeAsync(asStream, null);
        }

        private class HttpContentMock : HttpContent
        {
            private readonly Stream contentStream;

            public HttpContentMock(Stream contentStream)
            {
                this.contentStream = contentStream;
            }

            protected override async Task SerializeToStreamAsync(
                Stream stream,
                TransportContext? context)
            {
                await this.contentStream.CopyToAsync(stream);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = this.contentStream.Length;
                return true;
            }

            protected override Stream CreateContentReadStream(CancellationToken cancellationToken)
            {
                return this.contentStream;
            }

            protected override Task<Stream> CreateContentReadStreamAsync()
            {
                return Task.FromResult(this.contentStream);
            }

            protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(this.contentStream);
            }
        }
    }
}
