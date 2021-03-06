﻿using System.IO;
using System.Text;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;

namespace BenchmarkDotNet.Exporters.Xml
{
    public abstract class XmlExporterBase : ExporterBase
    {
        protected override string FileExtension => "xml";

        private readonly bool indentXml;
        private readonly bool excludeMeasurements;

        public XmlExporterBase(bool indentXml = false, bool excludeMeasurements = false)
        {
            this.indentXml = indentXml;
            this.excludeMeasurements = excludeMeasurements;
        }

        public override void ExportToLog(Summary summary, ILogger logger)
        {
            IXmlSerializer serializer = BuildSerializer(summary);

            // Use custom UTF-8 stringwriter because the default writes UTF-16
            var stringBuilder = new StringBuilder();
            using (var textWriter = new Utf8StringWriter(stringBuilder))
            {
                using (var writer = new SimpleXmlWriter(textWriter, indentXml))
                {
                    serializer.Serialize(writer, new SummaryDto(summary));
                }
            }

            logger.WriteLine(stringBuilder.ToString());
        }

        private IXmlSerializer BuildSerializer(Summary summary)
        {
            XmlSerializer.XmlSerializerBuilder builder =
                XmlSerializer.GetBuilder(typeof(SummaryDto))
                               .WithRootName(nameof(Summary))
                               .WithCollectionItemName(nameof(BenchmarkReportDto.Measurements),
                                                       nameof(Measurement))
                               .WithCollectionItemName(nameof(SummaryDto.Benchmarks),
                                                       nameof(BenchmarkReport.Benchmark))
                               .WithCollectionItemName(nameof(Statistics.Outliers), "Outlier");

            if (!summary.Config.HasMemoryDiagnoser())
            {
                builder.WithExcludedProperty(nameof(BenchmarkReportDto.Memory));
            }

            if (excludeMeasurements)
            {
                builder.WithExcludedProperty(nameof(BenchmarkReportDto.Measurements));
            }

            return builder.Build();
        }
    }

    internal class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public Utf8StringWriter(StringBuilder builder) :base(builder) { }
    }
}
