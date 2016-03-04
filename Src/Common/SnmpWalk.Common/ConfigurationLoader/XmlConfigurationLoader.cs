using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SnmpWalk.Common.DataModel.Snmp;

namespace SnmpWalk.Common.ConfigurationLoader
{
    internal class XmlConfigurationLoader
    {
        private const string ConfMain = "oids_common";
        private const string OidFileIdentifier = "oids_";
        private const string ConfDir = "conf";
        private const string OidAttr = "oid";
        private const string RootNodename = "oid-tree";


        private string _currentDir;
        private static readonly Lazy<XmlConfigurationLoader> Instance = new Lazy<XmlConfigurationLoader>(() => new XmlConfigurationLoader());
        private readonly Hashtable _oidTrees = new Hashtable();

        public XmlConfigurationLoader InstanceLoader
        {
            get
            {
                Initialize();
                return Instance.Value;
            }
        }

        public Hashtable Oids
        {
            get { return _oidTrees; }
        }

        private void Initialize()
        {
            _currentDir = Directory.GetCurrentDirectory();
            var confPath = Path.Combine(_currentDir, ConfDir);

            if (!Directory.Exists(confPath)) return;

            var dirInfo = new DirectoryInfo(confPath);
            var files = dirInfo.GetFiles("*.xml").Where(file => file.Name.Contains(OidFileIdentifier)).ToList();

            if (files.Any() && files.Any(file => file.Name == ConfMain))
            {
                foreach (var file in files)
                {
                    var xml = XDocument.Load(file.OpenRead());

                    if(xml.Root == null) continue;
                    var rootNode = xml.Root;

                    if (!ValidateOidFile(rootNode)) continue;

                    var subNode = (XElement)rootNode.FirstNode;

                    if (string.IsNullOrEmpty(subNode.FirstAttribute.Name.LocalName) || subNode.FirstAttribute.Name.LocalName != OidAttr) continue;

                    var rootOid = new Oid
                    {
                        Value = subNode.FirstAttribute.Value,
                        Name = subNode.Name.LocalName,
                        IsRoot = true
                    };

                    var oids = subNode.Elements();

                    var subOids = oids.Select(oid => new Oid
                    {
                        Value = oid.FirstAttribute.Value,
                        Name = oid.Name.LocalName
                    }).ToList();

                    _oidTrees.Add(rootOid, subOids);
                }
            }
        }

        private bool ValidateOidFile(XElement rootNode)
        {
            return rootNode.Name.LocalName.Equals(RootNodename);
        }
    }
}
