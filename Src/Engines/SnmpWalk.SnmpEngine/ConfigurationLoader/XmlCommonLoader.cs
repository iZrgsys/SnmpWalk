using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using SnmpWalk.Common.DataModel.Snmp;

namespace SnmpWalk.Engines.SnmpEngine.ConfigurationLoader
{
    internal class XmlCommonLoader
    {
        private const string ConfMain = "oids_common";
        private const string OidFileIdentifier = "oids_";
        private const string CodesFileIdentifier = "codes_";
        private const string ConfDir = "conf";
        private const string CodesDir = "codes";
        private const string OidAttr = "oid";
        private const string RootNodename = "oid-tree";
        private const string DecimalAttr = "Decimal";
        private const string DeviceModelFileName = "dev_models";
        private const string NameAttr = "Name";
        private const string DescAttr = "Description";
        private const string Additions = "Additional";

        private static List<FileInfo> _commoInfos;
        private static List<FileInfo> _codesInfo;

        private static readonly string CurrentDir = Directory.GetCurrentDirectory();
        private static readonly Lazy<XmlCommonLoader> CommonInstance = new Lazy<XmlCommonLoader>(() => new XmlCommonLoader());
        private static readonly List<Oid> ConfOids = new List<Oid>();
        private static readonly Hashtable CodesTable = new Hashtable();
        private static readonly Hashtable BrandHashtable = new Hashtable();

        public static XmlCommonLoader Instance
        {
            get
            {
                return CommonInstance.Value;
            }
        }

        public List<Oid> Oids
        {
            get { return ConfOids; }
        }

        public Hashtable AdditionalCodeTable
        {
            get { return CodesTable; }
        }

        public Hashtable BrandNameTable
        {
            get { return BrandHashtable; }
        }

        private static void Initialize()
        {
            var confPath = Path.Combine(CurrentDir, ConfDir);
            var codesPath = Path.Combine(CurrentDir, ConfDir, CodesDir);

            if (!Directory.Exists(confPath) && !Directory.Exists(codesPath)) return;

            var dirInfo = new DirectoryInfo(confPath);
            _commoInfos = dirInfo.GetFiles("*.xml").Where(file => file.Name.Contains(OidFileIdentifier)).ToList();

            var codesDirInfo = new DirectoryInfo(codesPath);
            _codesInfo = codesDirInfo.GetFiles("*.xml").Where(file => file.Name.Contains(CodesFileIdentifier)).ToList();

            if (!_commoInfos.Any() || !_commoInfos.All(file => file.Name.Contains(ConfMain))) return;

            foreach (var file in _commoInfos)
            {
                var xml = XDocument.Load(file.OpenRead());

                if (xml.Root == null) continue;
                var rootNode = xml.Root;

                if (!ValidateOidFile(rootNode)) continue;

                var subNode = (XElement)rootNode.FirstNode;

                if (string.IsNullOrEmpty(subNode.FirstAttribute.Name.LocalName) || subNode.FirstAttribute.Name.LocalName != OidAttr) continue;

                var rootOid = new Oid(subNode.FirstAttribute.Value, subNode.Name.LocalName, subNode.Name.LocalName);

                var oids = subNode.Elements();

                var childOids = oids.Select(oid => new Oid(oid.FirstAttribute.Value, oid.Name.LocalName, string.Concat(rootOid.Name, ".", oid.Name.LocalName))).ToList();

                rootOid.ChildOids = InitializeCodes(childOids);

                ConfOids.Add(rootOid);
            }

            var brandNameInfos = dirInfo.GetFiles("*.xml").Where(file => file.Name.Contains(DeviceModelFileName)).ToList();
            var bnInfo = DeserializeCodes(brandNameInfos.First());

            foreach (var code in bnInfo.Code)
            {
                BrandHashtable.Add(code.Decimal, code.Name);
            }
        }

        private static bool ValidateOidFile(XElement rootNode)
        {
            return rootNode.Name.LocalName.Equals(RootNodename);
        }

        private static bool ValidateCodeFile(XElement rootNode)
        {
            return rootNode.Name.LocalName.Equals(CodesDir);
        }

        private static List<Oid> InitializeCodes(List<Oid> oids)
        {
            for (var i = 0; i < oids.Count; i++)
            {
                if (_codesInfo.Any(file => file.Name.Contains(oids[i].Name)) && !_codesInfo.Any(file => file.Name.Contains(oids[i].Name+Additions)))
                {
                    oids[i] = InitializeCode(oids[i], _codesInfo.First(file => file.Name.Contains(oids[i].Name)));
                }

                if (_codesInfo.Any(file => file.Name.Contains(oids[i].Name + Additions)))
                {
                    if (CodesTable.ContainsKey(oids[i].Name)) continue;
                    var value = DeserializeCodes(_codesInfo.First(file => file.Name.Contains(oids[i].Name + Additions)));
                    CodesTable.Add(oids[i].Name, value);
                    oids[i].HasAdditionalCodes = true;
                }
            }

            return oids;
        }

        private static Codes DeserializeCodes(FileInfo fileInfo)
        {
            Codes codes = null;

            var serializer = new XmlSerializer(typeof(Codes));

            using (var reader = new StreamReader(fileInfo.OpenRead()))
            {
                codes = (Codes) serializer.Deserialize(reader);
            }

            return codes;
        }

        private static Oid InitializeCode(Oid oid, FileInfo file)
        {
            var childoids = new List<Oid>();

            var xml = XDocument.Load(file.OpenRead());

            if (xml.Root == null) return oid;
            var rootNode = xml.Root;

            if (!ValidateCodeFile(rootNode)) return oid;

            var codesElements = rootNode.Elements();

            foreach (var element in codesElements)
            {
                var decimalElement = element.Element(DecimalAttr);
                string objId = null;
                string name = null;
                string fullName = null;
                string decsr = null;

                if (decimalElement != null)
                {
                    var decVal = decimalElement.Value;
                    objId = CreateOid(oid.Value, decVal);
                }

                var nameElement = element.Element(NameAttr);

                if (nameElement != null)
                {
                    name = nameElement.Value;
                    fullName = CreateOid(oid.FullName, name);
                }

                var descriptionElement = element.Element(DescAttr);

                if (descriptionElement != null)
                {
                    decsr = descriptionElement.Value;
                }

                childoids.Add(new Oid(objId, name, fullName) { Description = decsr });
            }

            if (childoids.Any())
            {
                oid.ChildOids = InitializeCodes(childoids);
            }

            return oid;
        }

        private static string CreateOid(string oid, string index)
        {
            return string.Concat(oid, ".", index);
        }

        private XmlCommonLoader()
        {
            Initialize();
        }
    }
}
